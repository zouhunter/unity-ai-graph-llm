using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;

namespace UFrame.NodeGraph.ObjDrawer
{
    public static class EditorGUILayout
    {
        public static bool ObjectField(SerializedObject obj, GUIContent label = null)
        {
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;
            var changed = false;
            while ((iterator = iterator.NextVisible(enterChildren)) != null)
            {
                changed |= PropertyField(iterator, new GUILayoutOption[0]);
                enterChildren = false;
            }
            return changed;
        }
        public static bool PropertyField(SerializedProperty property, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();

            if (property.propertyType == PropertyType.Class)
            {
                DrawClassObject(property);
            }
            if(property.propertyType == PropertyType.ArraySize)
            {

            }
            else
            {
                DrawField(property);
            }
            return EditorGUI.EndChangeCheck();
        }
        public static void DrawClassObject(SerializedProperty property)
        {
            if (!property.hasChildren) return;

            EditorGUI.indentLevel++;

            if (GUILayout.Button(property.name, EditorStyles.boldLabel))
            {
                property.isExpanded = !property.isExpanded;
            }

            if (property.isExpanded)
            {
                var iterator = property;
                var enterChildren = true;
                while ((iterator = iterator.NextVisible(enterChildren)) != null)
                {
                    PropertyField(iterator, new GUILayoutOption[0]);
                    enterChildren = false;
                }
            }
        }
        public static void DrawField(SerializedProperty property)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField(property.name, GUILayout.Width(100));
            if (property.value is int)
            {
                property.value = UnityEditor.EditorGUILayout.IntField(Convert.ToInt32(property.value));
            }
            else if (property.value is bool)
            {
                property.value = UnityEditor.EditorGUILayout.Toggle(Convert.ToBoolean(property.value));
            }
            else if (property.value is float || property.value is double)
            {
                property.value = UnityEditor.EditorGUILayout.FloatField(float.Parse(property.value.ToString()));
            }
            else if (property.value is string)
            {
                property.value = UnityEditor.EditorGUILayout.TextField(property.value.ToString());
            }
            else if (property.value is Color)
            {
                property.value = UnityEditor.EditorGUILayout.ColorField((Color)property.value);
            }
            else if (property.value is Enum)
            {
                property.value = UnityEditor.EditorGUILayout.EnumPopup((Enum)property.value);
            }
            else if (property.value is Vector2)
            {
                property.value = UnityEditor.EditorGUILayout.Vector2Field("", (Vector2)property.value);
            }
            else if (property.value is Vector3)
            {
                property.value = UnityEditor.EditorGUILayout.Vector3Field("", (Vector3)property.value);
            }
            else if (property.value is Vector4)
            {
                property.value = UnityEditor.EditorGUILayout.Vector4Field("", (Vector4)property.value);
            }
            else if (property.value is Rect)
            {
                property.value = UnityEditor.EditorGUILayout.RectField("", (Rect)property.value);
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
        }
    }
}