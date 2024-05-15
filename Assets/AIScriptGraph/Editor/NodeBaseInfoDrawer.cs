using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine;
using UFrame.NodeGraph.DataModel;

namespace AIScripting
{
    public class NodeBaseInfoDrawer
    {
        public bool expand;
        protected List<SerializedProperty> properties;
        protected SerializedObject serializedObject;
        private NodeBaseObject target;

        public NodeBaseInfoDrawer(NodeBaseObject target)
        {
            this.target = target;
            serializedObject = new SerializedObject(target);
            properties = new List<SerializedProperty>();
            var props = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<HideInInspector>() != null)
                    continue;

                if(prop.IsPrivate && prop.GetCustomAttribute<SerializeField>() == null)
                    continue;

                var serializeProp = serializedObject.FindProperty(prop.Name);
                if (serializeProp != null)
                {
                    properties.Add(serializeProp);
                }
            }
        }

        /// <summary>
        /// 获取属性高度
        /// </summary>
        /// <returns></returns>
        public float GetHeight()
        {
            float height = EditorGUIUtility.singleLineHeight + 4;
            if (expand)
            {
                foreach (var item in properties)
                {
                    height += EditorGUI.GetPropertyHeight(item, true);
                }
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
            rect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
            GUI.Box(rect, GUIContent.none);

            var expandRect = new Rect(rect.x - 10, rect.y, 30, EditorGUIUtility.singleLineHeight);
            expand = EditorGUI.Toggle(expandRect, expand, EditorStyles.foldout);

            var itemRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            if( EditorGUI.LinkButton(itemRect, target.Title))
            {
                expand = !expand;
            }

            var objectRect = new Rect(rect.width - 80, rect.y, 120, EditorGUIUtility.singleLineHeight);
            EditorGUI.ObjectField(objectRect, target, target.GetType(), false);
            GUI.Box(itemRect, GUIContent.none);

            if (expand)
            {
                float y = rect.y;
                y += EditorGUIUtility.singleLineHeight;
                foreach (var item in properties)
                {
                    var height = EditorGUI.GetPropertyHeight(item, true);
                    EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, height), item, true);
                    y += height;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}