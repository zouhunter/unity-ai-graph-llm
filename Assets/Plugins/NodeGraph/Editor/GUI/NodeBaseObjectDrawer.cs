using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine;
using UFrame.NodeGraph.DataModel;

namespace UFrame.NodeGraph
{
   
    public class NodeBaseObjectDrawer
    {
        public object target { get; private set; }
        protected List<SerializedProperty> properties;
        private SerializedObject serializedObject;

        public NodeBaseObjectDrawer() 
        {
            target = new NodeBaseObject2();
        }

        public NodeBaseObjectDrawer(object target)
        {
            this.target = target;
        }

        private void InitProps(SerializedProperty property)
        {
            if (properties != null)
                return;

            properties = new List<SerializedProperty>();
            Debug.LogError(target.GetType());
            var props = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var prop in props)
            {
                Debug.LogError(prop.Name);
                var serializeProp = property.FindPropertyRelative(prop.Name);
                if (serializeProp != null)
                {
                    properties.Add(serializeProp);
                    Debug.LogError(prop.Name + ",ok!");
                }
            }
        }

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    InitProps(property);
        //    return base.GetPropertyHeight(property, label);
        //}

        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    //base.OnGUI(position, property, label);
        //    Debug.LogError("OnGUI");
        //}

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