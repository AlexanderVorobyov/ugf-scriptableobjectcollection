using UGF.ScriptableObjectCollection.Runtime;
using UnityEditor;

namespace UGF.ScriptableObjectCollection.Editor
{
    /// <summary>
    /// Custom editor for drawing ScriptableObjectCollectionBase.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObjectCollectionBase), true)]
    public class ScriptableObjectCollectionEditor : UnityEditor.Editor
    {
        private ScriptableObjectCollectionDraw m_draw;

        private void OnEnable()
        {
            var propertyCollection = serializedObject.FindProperty("m_collection");

            if (propertyCollection != null)
            {
                m_draw = new ScriptableObjectCollectionDraw(propertyCollection);
            }

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            
            m_draw?.Dispose();
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawCollectionLayout();
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        /// <summary>
        /// Draws the collection layout.
        /// </summary>
        public void DrawCollectionLayout()
        {
            m_draw?.OnGUILayout();
        }

        private void OnUndoRedoPerformed()
        {
            Repaint();
        }
    }
}