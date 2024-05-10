using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(InspectorNameAttribute))]
public class LabelAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InspectorNameAttribute attr = attribute as InspectorNameAttribute;
        if (attr.displayName.Length > 0)
        {
            label.text = attr.displayName;
        }
        EditorGUI.PropertyField(position, property, label);
    }
}