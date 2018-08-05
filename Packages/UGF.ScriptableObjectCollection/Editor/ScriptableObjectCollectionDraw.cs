using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UGF.ScriptableObjectCollection.Editor
{
    internal sealed class ScriptableObjectCollectionDraw : IDisposable
    {
        private readonly SerializedProperty m_serializedProperty;
        private readonly Type m_elementType;
        private readonly ScriptableObjectCollectionTracker m_tracker;
        private readonly List<ScriptableObjectCollectionElementDraw> m_draws = new List<ScriptableObjectCollectionElementDraw>();

        public ScriptableObjectCollectionDraw(SerializedProperty serializedProperty)
        {
            m_serializedProperty = serializedProperty;
            m_elementType = ScriptableObjectCollectionUtility.GetElementType(serializedProperty);
            m_tracker = new ScriptableObjectCollectionTracker(serializedProperty.serializedObject);

            Rebuild();
        }

        public void Dispose()
        {
            m_tracker.Dispose();

            foreach (var pair in m_draws)
            {
                pair.Dispose();
            }

            m_draws.Clear();
        }

        public void OnGUILayout()
        {
            m_serializedProperty.serializedObject.Update();

            DrawElements();
            DrawAddButton();

            m_serializedProperty.serializedObject.ApplyModifiedProperties();

            if (Event.current.type == EventType.Repaint && m_tracker.IsDirty())
            {
                Rebuild();
            }
        }

        public void DrawElements()
        {
            for (int i = 0; i < m_draws.Count; i++)
            {
                int index = i;

                m_draws[i].OnGUILayout(() => ShowMenu(index));
            }

            if (HasElementsWhichCannotBeMultiEdited())
            {
                ScriptableObjectCollectionGUIUtility.DrawLine();

                GUILayout.Space(4F);
                GUILayout.Label("ScriptableObjects that are only on some of the selected objects cannot be multi-edited.", EditorStyles.helpBox);
                GUILayout.Space(2F);
            }

            ScriptableObjectCollectionGUIUtility.DrawLine();
        }

        public void DrawAddButton()
        {
            using (new EditorGUI.DisabledScope(m_serializedProperty.hasMultipleDifferentValues))
            {
                if (ScriptableObjectCollectionGUIUtility.DrawAddButtonBig())
                {
                    ScriptableObjectCollectionUtility.CreateScriptableObjectMenu(MenuAdd, m_elementType).ShowAsContext();
                }
            }
        }

        private void Rebuild()
        {
            for (int i = 0; i < m_draws.Count; i++)
            {
                m_draws[i].Dispose();
            }

            m_draws.Clear();

            if (m_serializedProperty.hasMultipleDifferentValues)
            {
                var pairs = m_tracker.GetPairs();

                for (int i = 0; i < pairs.Count; i++)
                {
                    var targets = pairs[i];
                    var draw = new ScriptableObjectCollectionElementDraw(targets);

                    m_draws.Add(draw);
                }
            }
            else
            {
                for (int i = 0; i < m_serializedProperty.arraySize; i++)
                {
                    var propertyElement = m_serializedProperty.GetArrayElementAtIndex(i);
                    var draw = propertyElement.objectReferenceValue != null
                        ? new ScriptableObjectCollectionElementDraw(new[] { propertyElement.objectReferenceValue })
                        : new ScriptableObjectCollectionElementDraw();

                    m_draws.Add(draw);
                }
            }
        }

        private bool HasElementsWhichCannotBeMultiEdited()
        {
            return m_serializedProperty.hasMultipleDifferentValues && m_serializedProperty.arraySize != m_draws.Count;
        }

        private void ShowMenu(int index)
        {
            var menu = new GenericMenu();
            var contentReset = new GUIContent("Reset");
            var contentRemove = new GUIContent("Remove ScriptableObject");
            var contentMoveUp = new GUIContent("Move Up");
            var contentMoveDown = new GUIContent("Move Down");
            var contentCopy = new GUIContent("Copy ScriptableObject");
            var contentPasteAsNew = new GUIContent("Paste ScriptableObject As New");
            var contentPasteValues = new GUIContent("Paste ScriptableObject Values");
            var contentEditScript = new GUIContent("Edit Script");

            var draw = m_draws[index];
            bool hasMultipleValues = m_serializedProperty.hasMultipleDifferentValues;
            bool isEmpty = draw.TargetEditor == null;

            if (!isEmpty)
            {
                menu.AddItem(contentReset, false, MenuReset, index);
            }
            else
            {
                menu.AddDisabledItem(contentReset);
            }

            menu.AddSeparator(string.Empty);

            if (!hasMultipleValues)
            {
                menu.AddItem(contentRemove, false, MenuRemove, index);
            }
            else
            {
                menu.AddDisabledItem(contentRemove);
            }

            if (!hasMultipleValues && index > 0)
            {
                menu.AddItem(contentMoveUp, false, MenuMoveUp, index);
            }
            else
            {
                menu.AddDisabledItem(contentMoveUp);
            }

            if (!hasMultipleValues && index < m_serializedProperty.arraySize - 1)
            {
                menu.AddItem(contentMoveDown, false, MenuMoveDown, index);
            }
            else
            {
                menu.AddDisabledItem(contentMoveDown);
            }

            if (!hasMultipleValues && !isEmpty)
            {
                menu.AddItem(contentCopy, false, MenuCopy, index);
            }
            else
            {
                menu.AddDisabledItem(contentCopy);
            }

            if (!hasMultipleValues && ScriptableObjectCollectionUtility.CanPasteAsNew(m_elementType))
            {
                menu.AddItem(contentPasteAsNew, false, MenuPasteAsNew, index);
            }
            else
            {
                menu.AddDisabledItem(contentPasteAsNew);
            }

            if (!isEmpty && ScriptableObjectCollectionUtility.CanPasteValues(m_draws[index].TargetEditor))
            {
                menu.AddItem(contentPasteValues, false, MenuPasteValues, index);
            }
            else
            {
                menu.AddDisabledItem(contentPasteValues);
            }

            menu.AddSeparator(string.Empty);

            if (!isEmpty)
            {
                menu.AddItem(contentEditScript, false, MenuEditScript, index);
            }
            else
            {
                menu.AddDisabledItem(contentEditScript);
            }

            if (!isEmpty)
            {
                var editorWithCustomMenu = draw.TargetEditor as IHasCustomMenu;

                if (editorWithCustomMenu != null)
                {
                    menu.AddSeparator(string.Empty);

                    editorWithCustomMenu.AddItemsToMenu(menu);
                }
            }

            menu.ShowAsContext();
        }

        private void MenuReset(object userData)
        {
            int index = (int)userData;
            var draw = m_draws[index];

            ScriptableObjectCollectionUtility.Reset(draw.TargetEditor);

            EditorUtility.SetDirty(m_serializedProperty.serializedObject.targetObject);
        }

        private void MenuRemove(object userData)
        {
            int index = (int)userData;
            var propertyElement = m_serializedProperty.GetArrayElementAtIndex(index);

            if (propertyElement.objectReferenceValue != null)
            {
                ScriptableObjectCollectionUtility.RemoveElement(m_serializedProperty, index);
            }
            else
            {
                m_serializedProperty.DeleteArrayElementAtIndex(index);
                m_serializedProperty.serializedObject.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(m_serializedProperty.serializedObject.targetObject);
        }

        private void MenuMoveUp(object userData)
        {
            int index = (int)userData;

            m_serializedProperty.MoveArrayElement(index, index - 1);
            m_serializedProperty.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(m_serializedProperty.serializedObject.targetObject);
        }

        private void MenuMoveDown(object userData)
        {
            int index = (int)userData;

            m_serializedProperty.MoveArrayElement(index, index + 1);
            m_serializedProperty.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(m_serializedProperty.serializedObject.targetObject);
        }

        private void MenuCopy(object userData)
        {
            int index = (int)userData;
            var propertyElement = m_serializedProperty.GetArrayElementAtIndex(index);
            var scriptableObject = (ScriptableObject)propertyElement.objectReferenceValue;

            ScriptableObjectCollectionUtility.Copy(scriptableObject);
        }

        private void MenuPasteAsNew(object userData)
        {
            int index = (int)userData;

            ScriptableObjectCollectionUtility.PasteAsNew(m_serializedProperty, index);
        }

        private void MenuPasteValues(object userData)
        {
            int index = (int)userData;
            var draw = m_draws[index];

            ScriptableObjectCollectionUtility.PasteValues(draw.TargetEditor);

            EditorUtility.SetDirty(m_serializedProperty.serializedObject.targetObject);
        }

        private void MenuEditScript(object userData)
        {
            int index = (int)userData;
            var draw = m_draws[index];

            ScriptableObjectCollectionUtility.EditScript(draw.TargetEditor);
        }

        private void MenuAdd(object userdata)
        {
            var type = (Type)userdata;
            var scriptableObject = ScriptableObject.CreateInstance(type);

            scriptableObject.name = type.Name;

            ScriptableObjectCollectionUtility.AddElement(m_serializedProperty, scriptableObject);
        }
    }
}