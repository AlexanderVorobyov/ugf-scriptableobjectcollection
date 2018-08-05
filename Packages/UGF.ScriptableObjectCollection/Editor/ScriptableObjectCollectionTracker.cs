using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace UGF.ScriptableObjectCollection.Editor
{
    internal sealed class ScriptableObjectCollectionTracker : IDisposable
    {
        private readonly List<SerializedProperty> m_propertyCollections = new List<SerializedProperty>();
        private readonly List<int[]> m_instanceIds = new List<int[]>();
        private readonly List<Object> m_objects = new List<Object>();
        private readonly HashSet<int> m_picked = new HashSet<int>();

        public ScriptableObjectCollectionTracker(SerializedObject serializedObject)
        {
            for (int i = 0; i < serializedObject.targetObjects.Length; i++)
            {
                var target = serializedObject.targetObjects[i];
                var targetSerializedObject = new SerializedObject(target);
                var propertyCollection = targetSerializedObject.FindProperty("m_collection");

                m_propertyCollections.Add(propertyCollection);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < m_propertyCollections.Count; i++)
            {
                m_propertyCollections[i].serializedObject.Dispose();
            }

            m_propertyCollections.Clear();
            m_instanceIds.Clear();
            m_objects.Clear();
        }

        public void Update()
        {
            for (int i = 0; i < m_propertyCollections.Count; i++)
            {
                m_propertyCollections[i].serializedObject.Update();
            }
        }

        public void Apply()
        {
            for (int i = 0; i < m_propertyCollections.Count; i++)
            {
                m_propertyCollections[i].serializedObject.ApplyModifiedProperties();
            }
        }

        public bool IsDirty()
        {
            for (int i = 0; i < m_propertyCollections.Count; i++)
            {
                var propertyCollection = m_propertyCollections[i];
                var ids = new int[propertyCollection.arraySize];

                for (int p = 0; p < propertyCollection.arraySize; p++)
                {
                    var propertyElement = propertyCollection.GetArrayElementAtIndex(p);

                    ids[p] = propertyElement.objectReferenceInstanceIDValue;
                }

                m_instanceIds.Add(ids);
            }

            Update();

            for (int i = 0; i < m_propertyCollections.Count; i++)
            {
                var propertyCollection = m_propertyCollections[i];
                var ids = m_instanceIds[i];

                if (propertyCollection.arraySize != ids.Length)
                {
                    m_instanceIds.Clear();
                    return true;
                }

                for (int p = 0; p < propertyCollection.arraySize; p++)
                {
                    var propertyElement = propertyCollection.GetArrayElementAtIndex(p);

                    if (propertyElement.objectReferenceInstanceIDValue != ids[p])
                    {
                        m_instanceIds.Clear();
                        return true;
                    }
                }
            }

            m_instanceIds.Clear();
            return false;
        }

        public List<Object[]> GetPairs()
        {
            m_picked.Clear();

            var pairs = new List<Object[]>();

            for (int i = 0; i < m_propertyCollections[0].arraySize; i++)
            {
                GetPairs(m_objects, i);
                
                if (m_objects.Count == m_propertyCollections.Count)
                {
                    pairs.Add(m_objects.ToArray());
                }

                m_objects.Clear();
            }
            
            return pairs;
        }
        
        private void GetPairs(List<Object> objects, int index)
        {
            var target = m_propertyCollections[0].GetArrayElementAtIndex(index).objectReferenceValue;

            if (target != null)
            {                
                objects.Add(target);

                var targetType = target.GetType();

                for (int i = 1; i < m_propertyCollections.Count; i++)
                {
                    var propertyCollection = m_propertyCollections[i];

                    for (int e = 0; e < propertyCollection.arraySize; e++)
                    {
                        var propertyElement = propertyCollection.GetArrayElementAtIndex(e);
                        var objectValue = propertyElement.objectReferenceValue;

                        if (objectValue != null)
                        {
                            int instanceId = objectValue.GetInstanceID();

                            if (objectValue.GetType() == targetType && m_picked.Add(instanceId))
                            {
                                objects.Add(objectValue);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}