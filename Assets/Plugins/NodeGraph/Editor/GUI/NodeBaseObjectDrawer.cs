using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;

using UnityEngine;
namespace UFrame.NodeGraph
{
    public class NodeBaseObjectDrawer
    {
        public object target { get; private set; }
        protected List<SerializedProperty> properties;
        private SerializedObject serializedObject;

        public NodeBaseObjectDrawer(object target)
        {
            this.target = target;
            properties = new List<SerializedProperty>();
            //serializedObject = new SerializedObject(target);
            //var props = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            //foreach (var prop in props)
            //{
            //    var serializeProp = serializedObject.FindProperty(prop.Name);
            //    if (serializeProp != null)
            //    {
            //        properties.Add(serializeProp);
            //    }
            //}
        }

        internal void DrawHeader()
        {
            //throw new NotImplementedException();
        }

        internal void OnInspectorGUI()
        {
            //throw new NotImplementedException();\
            Debug.Log("OnInspectorGUI");
        }
        /// <summary>
        /// 获取属性高度
        /// </summary>
        /// <returns></returns>
        public float GetHeight()
        {
            float height = 8;
            foreach (var item in properties)
            {
                height += EditorGUI.GetPropertyHeight(item, true);
            }
            return height;
        }

        /// <summary>
        /// 绘制属性
        /// </summary>
        /// <param name="rect"></param>
        public void OnGUI(Rect rect)
        {
            serializedObject.Update();
            GUI.Box(rect, GUIContent.none);
            rect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
            float y = rect.y;
            foreach (var item in properties)
            {
                var height = EditorGUI.GetPropertyHeight(item, true);
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, height), item, true);
                y += height;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}