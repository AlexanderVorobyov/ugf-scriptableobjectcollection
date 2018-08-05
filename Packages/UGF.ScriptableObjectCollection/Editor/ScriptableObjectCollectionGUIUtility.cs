using System;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UGF.ScriptableObjectCollection.Editor
{
    internal static class ScriptableObjectCollectionGUIUtility
    {
        private static readonly int m_controlHash = "scriptable_object_collection_element_foldout".GetHashCode();
        private static Styles m_styles;

        public class Styles
        {
            public GUIStyle Line { get; } = new GUIStyle("IN Title");
            public GUIStyle Foldout { get; } = EditorStyles.foldout;
            public GUIStyle Text { get; } = new GUIStyle("IN TitleText");
            public GUIContent HelpIcon { get; } = EditorGUIUtility.IconContent("_Help");
            public GUIContent PresetIcon { get; } = EditorGUIUtility.IconContent("Preset.Context");
            public GUIContent SettingsIcon { get; } = EditorGUIUtility.IconContent("_Popup");
            public GUIContent AddIcon { get; } = EditorGUIUtility.IconContent("Toolbar Plus More");
            public GUIStyle IconButton { get; } = new GUIStyle("IconButton");
            public GUIStyle AddButton { get; } = new GUIStyle("AC Button");
            public GUIStyle InsertionMarker { get; } = new GUIStyle("InsertionMarker");
        }

        public static void DrawInsertionMarker(Rect position)
        {
            var styles = GetStyles();
            var rectLine = new Rect(0F, position.y + position.height + EditorGUIUtility.standardVerticalSpacing, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);

            styles.InsertionMarker.Draw(rectLine, false, false, false, false);
        }

        public static void DrawLine()
        {
            var rectLast = GUILayoutUtility.GetLastRect();
            var rectLine = new Rect(0F, rectLast.y + rectLast.height + EditorGUIUtility.standardVerticalSpacing, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);

            DrawTitlebarBackground(rectLine);
        }

        public static bool DrawTitlebar(GUIContent content, bool foldout, Action onSettingsOpen = null, Object[] targets = null)
        {
            var position = GetTitlebarRect();

            return DrawTitlebar(position, content, foldout, onSettingsOpen, targets);
        }

        public static bool DrawTitlebar(Rect position, GUIContent content, bool foldout, Action onSettingsOpen = null, Object[] targets = null)
        {
            var styles = GetStyles();
            var sizeHelp = styles.IconButton.CalcSize(styles.HelpIcon);
            var sizePreset = styles.IconButton.CalcSize(styles.PresetIcon);
            var sizeSettings = styles.IconButton.CalcSize(styles.SettingsIcon);

            var rectFoldout = new Rect(position.x, position.y + 2F, position.width - sizeSettings.x, position.height);
            var rectHelp = new Rect(position.x + position.width - sizeSettings.x - sizePreset.x - sizeHelp.x, position.y + 2F, sizeHelp.x, sizeHelp.y);
            var rectPreset = new Rect(position.x + position.width - sizeSettings.x - sizePreset.x, position.y + 2F, sizePreset.x, sizePreset.y);
            var rectSettings = new Rect(position.x + position.width - sizeSettings.x, position.y + 2F, sizeSettings.x, sizeSettings.y);

            DrawTitlebarBackground(position);

            if (targets != null)
            {
                rectFoldout.width -= sizePreset.x + sizeHelp.x;

                if (GUI.Button(rectHelp, styles.HelpIcon, styles.IconButton))
                {
                    Help.ShowHelpForObject(targets[0]);
                }

                PresetSelector.DrawPresetButton(rectPreset, targets);
            }

            if (GUI.Button(rectSettings, styles.SettingsIcon, styles.IconButton))
            {
                onSettingsOpen?.Invoke();
            }

            return DrawTitlebarFoldout(rectFoldout, content, foldout, onSettingsOpen);
        }

        public static void DrawTitlebarBackground(Rect position)
        {
            var current = Event.current;

            if (current.type == EventType.Repaint)
            {
                GetStyles().Line.Draw(position, false, false, false, false);
            }
        }

        public static bool DrawTitlebarFoldout(Rect position, GUIContent content, bool foldout, Action onLeftClick)
        {
            var current = Event.current;
            int id = GUIUtility.GetControlID(m_controlHash, FocusType.Keyboard, position);

            var rectFoldout = new Rect(position.x + 2F, position.y, position.width - 2F, position.height);
            var rectText = new Rect(position.x + 16F, position.y, position.width - 16F, position.height);

            switch (current.type)
            {
                case EventType.MouseDown:
                {
                    if (position.Contains(current.mousePosition))
                    {
                        if (current.button == 0)
                        {
                            GUIUtility.hotControl = id;
                            current.Use();
                        }

                        if (current.button == 1)
                        {
                            onLeftClick?.Invoke();
                            current.Use();
                        }
                    }
                    break;
                }
                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;

                        current.Use();

                        if (position.Contains(current.mousePosition))
                        {
                            GUI.changed = true;
                            return !foldout;
                        }
                    }
                    break;
                }
                case EventType.MouseDrag:
                {
                    if (GUIUtility.hotControl == id)
                    {
                        current.Use();
                    }
                    break;
                }
                case EventType.Repaint:
                {
                    m_styles.Foldout.Draw(rectFoldout, GUIContent.none, id, foldout);
                    m_styles.Text.Draw(rectText, content, false, false, false, false);
                    break;
                }
            }

            return foldout;
        }

        public static bool DrawAddButtonBig()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var styles = GetStyles();
            var content = new GUIContent("Add ScriptableObject");
            var rect = GUILayoutUtility.GetRect(content, styles.AddButton);
            bool result = EditorGUI.DropdownButton(rect, content, FocusType.Passive, styles.AddButton);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4F);

            return result;
        }

        public static bool DrawAddButton()
        {
            var styles = GetStyles();
            var sizeIcon = styles.IconButton.CalcSize(styles.AddIcon);
            var position = EditorGUILayout.GetControlRect();
            var rectIcon = new Rect(position.x + position.width - sizeIcon.x, position.y, sizeIcon.x, sizeIcon.y);

            return DrawAddButton(rectIcon);
        }

        public static bool DrawAddButton(Rect positon)
        {
            var styles = GetStyles();

            return GUI.Button(positon, styles.AddIcon, styles.IconButton);
        }

        public static Rect GetTitlebarRect()
        {
            var position = EditorGUILayout.GetControlRect();

            position.x -= 13F;
            position.width += 17F;

            return position;
        }

        public static GUIContent GetTitlebarContent(Object target = null)
        {
            var content = new GUIContent("Missing ScriptableObject");

            if (target != null)
            {
                var type = target.GetType();
                string name = ObjectNames.NicifyVariableName(type.Name);
                var icon = AssetPreview.GetMiniThumbnail(target);

                content.text = $"{name} (Script)";
                content.image = icon;
            }
            else
            {
                content.image = AssetPreview.GetMiniTypeThumbnail(typeof(ScriptableObject));
            }

            return content;
        }

        public static Styles GetStyles()
        {
            return m_styles ?? (m_styles = new Styles());
        }
    }
}