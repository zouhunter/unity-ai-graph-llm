/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: 变量绘制
 *_*/

using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AIScripting
{
    [CustomPropertyDrawer(typeof(Ref<>))]
    public class RefVarDrawer : PropertyDrawer
    {
        private SerializedProperty keyProp;
        private SerializedProperty autoCreateProp;
        private SerializedProperty defaultProp;
        private int _index = 0;
        private System.Type _valueType;
        private HashSet<int> _autoHideProp = new();
        private int _currentHash;

        private void FindProperties(SerializedProperty property)
        {
            keyProp = property.FindPropertyRelative("_key");
            autoCreateProp = property.FindPropertyRelative("_autoCreate");
            defaultProp = property.FindPropertyRelative("_default");
        }

        private void SelectIndex(SerializedProperty property)
        {
            var subProps = property.serializedObject.targetObject.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy);
            Array.Sort(subProps, (a, b) => a.ReflectedType == b.ReflectedType ? 0: a.ReflectedType.IsAssignableFrom(b.ReflectedType) ? -1:1);
            var sortedProps = subProps.Reverse();
            var index = 0;
            foreach (var item in sortedProps)
            {
                if (!typeof(IRef).IsAssignableFrom(item.FieldType))
                    continue;
                index++;
                if (item.Name == property.propertyPath)
                {
                    _currentHash = item.GetValue(property.serializedObject.targetObject).GetHashCode();
                    _valueType = item.FieldType.GetGenericArguments()[0];
                    this._index = index;
                    break;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FindProperties(property);
            var height = EditorGUIUtility.singleLineHeight + 12;
            if (property.isExpanded && defaultProp != null)
            {
                height += EditorGUI.GetPropertyHeight(defaultProp);
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = new Rect(position.x + 2, position.y + 2, position.width - 4, position.height - 4);
            GUI.Box(position, GUIContent.none);
            FindProperties(property);
            SelectIndex(property);

            var textRect = new Rect(position.x, position.y, position.width - 180, EditorGUIUtility.singleLineHeight);
            if (defaultProp != null)
            {
                if (GUI.Button(textRect, new GUIContent($"{_index}.{label.text} ({defaultProp.type})", "isExpanded able"), property.isExpanded ? EditorStyles.label : EditorStyles.boldLabel))
                {
                    property.isExpanded = !property.isExpanded;
                }
            }
            else
            {
                GUI.Label(textRect, new GUIContent($"{_index}.{label.text} ({_valueType.Name})", "isExpanded unable"), EditorStyles.boldLabel);
            }

            var keyRect = new Rect(position.x + position.width - 160, position.y, 80, EditorGUIUtility.singleLineHeight);
            keyProp.stringValue = EditorGUI.TextField(keyRect, keyProp.stringValue);

            if (string.IsNullOrEmpty(keyProp.stringValue))
            {
                property.isExpanded = true;
            }
            else if(!_autoHideProp.Contains(_currentHash) && _valueType.IsValueType)
            {
                if(_valueType == typeof(Ref<string>) && string.IsNullOrEmpty(defaultProp.stringValue))
                {
                    _autoHideProp.Add(_currentHash);
                    property.isExpanded = false;
                }
                else if (_valueType == typeof(Ref<int>) && defaultProp.intValue == 0)
                {
                    _autoHideProp.Add(_currentHash);
                    property.isExpanded = false;
                }
                else if (_valueType == typeof(Ref<bool>) && defaultProp.boolValue == false)
                {
                    _autoHideProp.Add(_currentHash);
                    property.isExpanded = false;
                }
            }

            var idRect = new Rect(position.x + position.width - 180, position.y, 20, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(idRect, "ID:");

            using (var disable = new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(keyProp.stringValue)))
            {
                var autoCreateRect = new Rect(position.x + position.width - 20, position.y, 20, EditorGUIUtility.singleLineHeight);
                autoCreateProp.boolValue = EditorGUI.Toggle(autoCreateRect, autoCreateProp.boolValue);
            }

            var autoRect = new Rect(position.x + position.width - 70, position.y, 50, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(autoRect, new GUIContent("Ensure:", "create if not exists!"));

            if (property.isExpanded && defaultProp != null)
            {
                position.y += EditorGUIUtility.singleLineHeight + 4;
                position.x += 30;
                position.width -= 40;
                position.height -= EditorGUIUtility.singleLineHeight;
                using (var disable = new EditorGUI.DisabledGroupScope(!autoCreateProp.boolValue))
                {
                    float originalLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 60;
                    EditorGUI.PropertyField(position, defaultProp, true);
                    EditorGUIUtility.labelWidth = originalLabelWidth;
                }
            }
        }
    }
}

