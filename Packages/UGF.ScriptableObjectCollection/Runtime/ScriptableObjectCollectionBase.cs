using System.Collections;
using UnityEngine;

namespace UGF.ScriptableObjectCollection.Runtime
{
    /// <summary>
    /// The base class for all ScriptableObjectCollections.
    /// </summary>
    public abstract class ScriptableObjectCollectionBase : ScriptableObject, IEnumerable
    {
        public abstract IEnumerator GetEnumerator();
    }
}