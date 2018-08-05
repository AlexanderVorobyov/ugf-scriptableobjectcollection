using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGF.ScriptableObjectCollection.Runtime
{
    /// <summary>
    /// An abstarct class for implementing ScriptableObjectCollection.
    /// </summary>
    public abstract class ScriptableObjectCollection<T> : ScriptableObjectCollectionBase, IList<T> where T : ScriptableObject
    {
        [SerializeField, HideInInspector] private List<T> m_collection = new List<T>();

        public T this[int index] { get { return m_collection[index]; } set { m_collection[index] = value; } }
        public int Count { get { return m_collection.Count; } }
        public bool IsReadOnly { get { return false; } }

        public void Add(T item)
        {
            m_collection.Add(item);
        }

        public void Clear()
        {
            m_collection.Clear();
        }

        public bool Contains(T item)
        {
            return m_collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_collection.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return m_collection.Remove(item);
        }

        public int IndexOf(T item)
        {
            return m_collection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            m_collection.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_collection.RemoveAt(index);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return m_collection.GetEnumerator();
        }

        public override IEnumerator GetEnumerator()
        {
            return m_collection.GetEnumerator();
        }
    }
}