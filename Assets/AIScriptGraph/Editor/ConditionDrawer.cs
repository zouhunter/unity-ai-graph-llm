using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AIScripting
{
    [CustomPropertyDrawer(typeof(Condition))]
    public class ConditionDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var paramProp = property.FindPropertyRelative("param");
            var valueTypeProp = property.FindPropertyRelative("type");
            var compareTypeProp = property.FindPropertyRelative("compareType");
            var valueProp = property.FindPropertyRelative("compareValue");

            var width = position.width * 0.9f;
            var offset = width * 0.1f * 0.33f;
            var rect = new Rect(position.x, position.y, width / 4, position.height);
            EditorGUI.PropertyField(rect, paramProp, GUIContent.none);
            rect.x += rect.width + offset;
            EditorGUI.PropertyField(rect, valueTypeProp, GUIContent.none);
            rect.x += rect.width + offset;
            EditorGUI.PropertyField(rect, compareTypeProp, GUIContent.none);
            rect.x += rect.width + offset;
            EditorGUI.PropertyField(rect, valueProp, GUIContent.none);
        }
    }
}
