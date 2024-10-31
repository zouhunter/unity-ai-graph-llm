/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: 变量绘制
 *_*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;
using static UnityEditor.Search.SearchValue;
using System.Collections;

namespace MateAI.ScriptableBehaviourTree
{
    [CustomPropertyDrawer(typeof(Ref<>))]
    public class RefVarDrawer : PropertyDrawer
    {
        private SerializedProperty keyProp;
        private SerializedProperty autoCreateProp;
        private SerializedProperty defaultProp;
        private int _index = 0;
        private System.Type _valueType;
        private void FindProperties(SerializedProperty property)
        {
            keyProp = property.FindPropertyRelative("_key");
            autoCreateProp = property.FindPropertyRelative("_autoCreate");
            defaultProp = property.FindPropertyRelative("_default");
        }

        private void SelectIndex(SerializedProperty property)
        {
            var contentObj = GetContentObject(property.serializedObject.targetObject, property.propertyPath);
            //Debug.LogError(property.propertyPath);
            //Debug.LogError(contentObj);
            var subProps = contentObj.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy);
            var index = 0;
            foreach (var item in subProps)
            {
                if (!typeof(IRef).IsAssignableFrom(item.FieldType))
                    continue;

                _valueType = item.FieldType.GetGenericArguments()[0];

                index++;
                if (property.propertyPath.EndsWith(item.Name))
                {
                    this._index = index;
                    break;
                }
            }
        }

        private object GetContentObject(UnityEngine.Object target, string propertyPath)
        {
            Type type = target.GetType();
            string[] pathParts = propertyPath.Split('.');
            FieldInfo field = null;
            object content = target;

            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                if (pathParts[i] == "Array")
                {
                    if (i + 1 < pathParts.Length && pathParts[i + 1].StartsWith("data["))
                    {
                        i++;
                        if (field != null)
                        {
                            if(content != null)
                            {
                                var id = int.Parse(pathParts[i].Replace("data[", "").Replace("]", ""));
                                if(content is IList)
                                {
                                    MethodInfo getItemMethod = content.GetType().GetProperty("Item").GetGetMethod();
                                    content = getItemMethod.Invoke(content, new object[] { id });
                                    type = content.GetType();
                                }
                                else if(content is Array)
                                {
                                    content = (content as Array).GetValue(id);
                                    type = content.GetType();
                                }
                            }
                            else
                            {
                                type = field.FieldType.GetElementType();
                            }
                        }
                    }
                }
                else
                {
                    field = type?.GetField(pathParts[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
                    if (field != null)
                    {
                        content = field.GetValue(content);

                        type = field.FieldType;
                        if (type.IsArray)
                        {
                            type = type.GetElementType();
                        }
                        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            type = type.GetGenericArguments()[0];
                        }
                    }
                    else
                    {
                        return content;
                    }
                }
            }

            return content;
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
            else if (_valueType != null)
            {
                GUI.Label(textRect, new GUIContent($"{_index}.{label.text} ({_valueType.FullName})", "isExpanded unable"), EditorStyles.boldLabel);
            }
            else
            {
                GUI.Label(textRect, new GUIContent($"{_index}.{label.text}", "isExpanded unable"), EditorStyles.boldLabel);
            }

            var keyRect = new Rect(position.x + position.width - 160, position.y, 80, EditorGUIUtility.singleLineHeight);
            keyProp.stringValue = EditorGUI.TextField(keyRect, keyProp.stringValue);

            if (string.IsNullOrEmpty(keyProp.stringValue))
            {
                property.isExpanded = true;
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
                position.x += 10;
                position.width -= 20;
                position.height -= EditorGUIUtility.singleLineHeight;
                using (var disable = new EditorGUI.DisabledGroupScope(!string.IsNullOrEmpty(keyProp.stringValue)))
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

