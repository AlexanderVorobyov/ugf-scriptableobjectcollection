using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UGF.ScriptableObjectCollection.Editor
{
    internal sealed class ScriptableObjectCollectionElementDraw : IDisposable
    {
        [CanBeNull]
        public UnityEditor.Editor TargetEditor { get; }
        public GUIContent Content { get; }
        public bool Foldout { get; set; } = true;

        public ScriptableObjectCollectionElementDraw(Object[] targets = null)
        {
            if (targets != null)
            {
                TargetEditor = UnityEditor.Editor.CreateEditor(targets);
                Content = ScriptableObjectCollectionGUIUtility.GetTitlebarContent(targets[0]);
            }
            else
            {
                Content = ScriptableObjectCollectionGUIUtility.GetTitlebarContent();
            }
        }
        
        public void Dispose()
        {
            Object.DestroyImmediate(TargetEditor, true);
        }

        public void OnGUILayout(Action onSettingsOpen)
        {
            var targets = TargetEditor != null ? TargetEditor.targets : null;

            Foldout = ScriptableObjectCollectionGUIUtility.DrawTitlebar(Content, Foldout, onSettingsOpen, targets);

            if (Foldout)
            {
                if (TargetEditor != null)
                {
                    TargetEditor.OnInspectorGUI();
                }
                else
                {
                    EditorGUILayout.HelpBox("Missing reference, remove this element.", MessageType.Warning);
                }
            }
        }
    }
}