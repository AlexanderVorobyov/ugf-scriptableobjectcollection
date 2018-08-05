using System;
using JetBrains.Annotations;

namespace UGF.ScriptableObjectCollection.Runtime
{
    /// <summary>
    /// Mark a ScriptableObject-derived type to be listed in add menu for <see cref="ScriptableObjectCollection{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ScriptableObjectCollectionCreateAttribute : Attribute
    {
        /// <summary>
        /// The path used to show in add menu.
        /// </summary>
        [CanBeNull]
        public readonly string Path;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptableObjectCollectionCreateAttribute"/> class.
        /// </summary>
        /// <param name="path">The path used to show in add menu.</param>
        public ScriptableObjectCollectionCreateAttribute(string path = "")
        {
            Path = path;
        }
    }
}