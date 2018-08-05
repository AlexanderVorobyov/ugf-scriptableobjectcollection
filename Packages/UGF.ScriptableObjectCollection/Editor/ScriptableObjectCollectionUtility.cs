using System;
using System.Collections.Generic;
using System.Reflection;
using UGF.ScriptableObjectCollection.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UGF.ScriptableObjectCollection.Editor
{
    internal static class ScriptableObjectCollectionUtility
    {
        private static readonly Dictionary<string, Type> m_types = new Dictionary<string, Type>();
        private static ScriptableObject m_copyBuffer;
        
        public static Type GetElementType(SerializedProperty serializedProperty)
        {
            var targetType = serializedProperty.serializedObject.targetObject.GetType();
            var fieldInfo = GetFieldInfo(targetType, serializedProperty.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var type = fieldInfo.FieldType;
            
            return type.IsArray ? type.GetElementType() : type.GenericTypeArguments[0];
        }

        public static bool CanPasteAsNew(Type type)
        {
            return m_copyBuffer != null && type.IsInstanceOfType(m_copyBuffer);
        }

        public static bool CanPasteValues(UnityEditor.Editor editor)
        {
            return m_copyBuffer != null && m_copyBuffer.GetType() == editor.target.GetType();
        }
        
        public static void Copy(ScriptableObject scriptableObject)
        {
            if (m_copyBuffer != null)
            {
                Object.DestroyImmediate(m_copyBuffer);
            }

            m_copyBuffer = Object.Instantiate(scriptableObject);
            m_copyBuffer.name = scriptableObject.name;
        }
        
        public static void PasteAsNew(SerializedProperty serializedProperty, int index)
        {
            if (m_copyBuffer != null)
            {
                var scriptableObject = Object.Instantiate(m_copyBuffer);

                scriptableObject.name = m_copyBuffer.name;

                AddElement(serializedProperty, scriptableObject, index);
            }
        }

        public static void PasteValues(UnityEditor.Editor editor)
        {
            if (m_copyBuffer != null)
            {
                for (int i = 0; i < editor.targets.Length; i++)
                {
                    var target = editor.targets[i];
                    
                    Undo.RecordObject(target, $"Paste Values {target.name}");
                    EditorUtility.CopySerialized(m_copyBuffer, target);
                }
            }
        }
        
        public static void Reset(UnityEditor.Editor editor)
        {
            for (int i = 0; i < editor.targets.Length; i++)
            {
                var target = editor.targets[i];
                var copy = ScriptableObject.CreateInstance(target.GetType());

                copy.name = target.name;
                
                Undo.RecordObject(target, $"Reset {target.name}");
                EditorUtility.CopySerialized(copy, target);

                Object.DestroyImmediate(copy);
            }
        }
        
        public static void EditScript(UnityEditor.Editor editor)
        {
            var propertyScript = editor.serializedObject.FindProperty("m_Script");
            string path = AssetDatabase.GetAssetPath(propertyScript.objectReferenceValue);
            
            if (!string.IsNullOrEmpty(path))
            {
                InternalEditorUtility.OpenFileAtLineExternal(path, 0);
            }
        }
        
        public static void AddElement(SerializedProperty serializedProperty, ScriptableObject scriptableObject)
        {
            int index = serializedProperty.arraySize > 0 ? serializedProperty.arraySize - 1 : 0;

            AddElement(serializedProperty, scriptableObject, index);
        }

        public static void AddElement(SerializedProperty serializedProperty, ScriptableObject scriptableObject, int index)
        {
            int next = serializedProperty.arraySize > 0 ? index + 1 : index;

            AssetDatabase.AddObjectToAsset(scriptableObject, serializedProperty.serializedObject.targetObject);
            Undo.RegisterCreatedObjectUndo(scriptableObject, $"Add {scriptableObject.name}");

            serializedProperty.InsertArrayElementAtIndex(index);
            serializedProperty.GetArrayElementAtIndex(next).objectReferenceValue = scriptableObject;
            serializedProperty.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
        }
        
        public static void RemoveElement(SerializedProperty serializedProperty, int index)
        {
            var propertyElement = serializedProperty.GetArrayElementAtIndex(index);
            var scriptableObject = propertyElement.objectReferenceValue;
            
            RemoveNestedElements(serializedProperty, index);

            serializedProperty.DeleteArrayElementAtIndex(index);
            serializedProperty.DeleteArrayElementAtIndex(index);
            serializedProperty.serializedObject.ApplyModifiedProperties();
            
            Undo.DestroyObjectImmediate(scriptableObject);

            EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
        }

        public static GenericMenu CreateScriptableObjectMenu(GenericMenu.MenuFunction2 function, Type type)
        {
            if (m_types.Count == 0)
            {
                GetTypes(m_types);
            }

            var menu = new GenericMenu();

            foreach (var pair in m_types)
            {
                if (type.IsAssignableFrom(pair.Value))
                {
                    menu.AddItem(new GUIContent(pair.Key), false, function, pair.Value);
                }
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("None"));
            }

            return menu;
        }
        
        private static void RemoveNestedElements(SerializedProperty serializedProperty, int index)
        {
            var propertyElement = serializedProperty.GetArrayElementAtIndex(index);
            var collection = propertyElement.objectReferenceValue as ScriptableObjectCollectionBase;

            if (collection != null)
            {
                Undo.RegisterCompleteObjectUndo(collection, $"Remove {collection.name}");

                var serializedObject = new SerializedObject(collection);
                var propertyCollection = serializedObject.FindProperty("m_collection");

                for (int i = 0; i < propertyCollection.arraySize; i++)
                {
                    var propertyCollectionElement = propertyCollection.GetArrayElementAtIndex(i);

                    if (propertyCollectionElement.objectReferenceValue != null)
                    {
                        Undo.DestroyObjectImmediate(propertyCollectionElement.objectReferenceValue);
                    }
                }

                propertyCollection.ClearArray();
                serializedObject.ApplyModifiedProperties();
                serializedObject.Dispose();
            }
        }

        private static void GetTypes(Dictionary<string, Type> dictionary)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                GetTypes(dictionary, assemblies[i].GetTypes());
            }
        }

        private static void GetTypes(Dictionary<string, Type> dictionary, Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                
                if (IsValidType(type))
                {
                    string path = GetCreatePath(type);

                    dictionary[path] = type;
                }
            }
        }

        private static bool IsValidType(Type type)
        {
            return !type.IsAbstract && type.IsSubclassOf(typeof(ScriptableObject))
                && (type.IsDefined(typeof(CreateAssetMenuAttribute)) || type.IsDefined(typeof(ScriptableObjectCollectionCreateAttribute)));
        }

        private static string GetCreatePath(Type type)
        {
            string path = string.Empty;

            if (type.IsDefined(typeof(CreateAssetMenuAttribute)))
            {
                var attribute = type.GetCustomAttribute<CreateAssetMenuAttribute>(false);

                path = attribute.menuName;
            }

            if (type.IsDefined(typeof(ScriptableObjectCollectionCreateAttribute)))
            {
                var attribute = type.GetCustomAttribute<ScriptableObjectCollectionCreateAttribute>(false);

                path = attribute.Path;
            }

            if (string.IsNullOrEmpty(path))
            {
                path = type.FullName?.Replace(".", "/") ?? type.Name;
            }

            return path;
        }

        private static FieldInfo GetFieldInfo(Type type, string name, BindingFlags flags)
        {
            var field = type.GetField(name, flags);

            if (field != null)
            {
                return field;
            }

            if (type.BaseType != null)
            {
                return GetFieldInfo(type.BaseType, name, flags);
            }

            return null;
        }
    }
}