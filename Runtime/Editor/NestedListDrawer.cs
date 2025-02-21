using UnityEditor;
using UnityEngine;
using WhiteArrow.Incremental;

namespace WhiteArrowEditor.Incremental
{
    [CustomPropertyDrawer(typeof(NestedList<>))]
    public class NestedListDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var listProperty = property.FindPropertyRelative("List");
            if (listProperty == null)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUI.GetPropertyHeight(listProperty, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var listProperty = property.FindPropertyRelative("List");
            if (listProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "Error: Could not find 'List'");
                return;
            }

            EditorGUI.PropertyField(position, listProperty, label, true);
        }


        [CustomPropertyDrawer(typeof(NestedList<>.SubList))]
        public class SubListDrawer : PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var listProperty = property.FindPropertyRelative("List");
                if (listProperty == null)
                    return EditorGUIUtility.singleLineHeight;

                return EditorGUI.GetPropertyHeight(listProperty, label, true);
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var listProperty = property.FindPropertyRelative("List");
                if (listProperty == null)
                {
                    EditorGUI.LabelField(position, label.text, "Error: Could not find 'List'");
                    return;
                }

                EditorGUI.PropertyField(position, listProperty, label, true);
            }
        }
    }
}
