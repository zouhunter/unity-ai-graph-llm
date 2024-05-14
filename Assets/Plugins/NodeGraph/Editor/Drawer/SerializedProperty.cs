using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Internal;
using System.Reflection;
using Debug = UnityEngine.Debug;
using UnityEditor;

namespace UFrame.NodeGraph.ObjDrawer
{
    [StructLayout(LayoutKind.Sequential)]
    public sealed class SerializedProperty
    {
        internal SerializedObject m_SerializedObject;
        public SerializedObject serializedObject
        {
            get
            {
                return this.m_SerializedObject;
            }
            set
            {
                m_SerializedObject = value;
            }
        }
        public SerializedProperty parentProp { get; private set; }

        public FieldInfo fieldInfo;

        private object content;

        private List<SerializedProperty> subProps;

        public object value { get { return fieldInfo.GetValue(content); }set { fieldInfo.SetValue(content,value); } }

        public bool isExpanded = true;

        public string displayName { get { return name; } }

        public string name { get { return fieldInfo.Name; } }

        public string type { get { return fieldInfo.FieldType.ToString(); } }

        public string tooltip { get; set; }

        public int depth { get; set; }

        public string propertyPath { get; private set; }

        internal int hashCodeForPropertyPathWithoutArrayIndex;

        public bool editable { get { return true; } }

        public bool isAnimated;


        public bool hasChildren { get { return subProps != null && subProps.Count > 0; } }

        public bool hasVisibleChildren { get { return hasChildren && subProps.Find(x => x.editable) != null; } }

        public PropertyType propertyType { get; private set; }

        public int intValue { get { return (propertyType == PropertyType.Integer) ? (int)value : 0; } }

        public long longValue { get { return (propertyType == PropertyType.Integer) ? (long)value : 0; } }

        public bool boolValue { get { return (propertyType == PropertyType.Boolean) ? (bool)value : false; } }

        public float floatValue { get { return (propertyType == PropertyType.Float) ? (float)value : 0; } }

        public double doubleValue { get { return (propertyType == PropertyType.Float) ? (double)value : 0; } }

        public string stringValue { get { return (propertyType == PropertyType.String) ? (string)value : null; } }

        public Color colorValue { get { return (propertyType == PropertyType.Color) ? (Color)value : Color.white; } } 

        public AnimationCurve animationCurveValue;

        internal Gradient gradientValue;

        public object objectReferenceValue;

        public int objectReferenceInstanceIDValue;

        internal string objectReferenceStringValue;

        internal string objectReferenceTypeString;

        internal string layerMaskStringValue;

        public int enumValueIndex;

        public string[] enumNames;
        public string[] enumDisplayNames;
        public Vector2 vector2Value;

        public Vector3 vector3Value;

        public Vector4 vector4Value { get { return new Vector4(); } }

        public Quaternion quaternionValue { get { return new Quaternion(); } }

        public Rect rectValue { get { return new Rect(); } }

        public Bounds boundsValue { get { return new Bounds(); } }

        public bool isArray { get { return fieldInfo.FieldType.IsArrayOrList(); } }

        public int arraySize;

        internal SerializedProperty(FieldInfo info, object holder)
        {
            this.fieldInfo = info;
            this.content = holder;
            this.propertyType = JudgePropertyType(info);
            subProps = GetSubPropopertys(fieldInfo, holder);
        }
        ~SerializedProperty()
        {
            this.Dispose();
        }
        public void SetParentProperty(SerializedProperty parent)
        {
            this.parentProp = parent;
            if (!string.IsNullOrEmpty(parent.propertyPath))
            {
                propertyPath = parent.propertyPath + "/" + fieldInfo.Name;
            }
            else
            {
                propertyPath = fieldInfo.Name;
            }
        }

        public List<SerializedProperty> GetSubPropopertys(FieldInfo field, object holder)
        {
            List<SerializedProperty> list = new List<UFrame.NodeGraph.ObjDrawer.SerializedProperty>();

            var type = field.FieldType;

            if (field.GetValue(holder) == null && type.IsClass && type  != typeof(string))
            {
                field.SetValue( holder , Activator.CreateInstance(type));
            }

            if(field.GetValue(holder) == null && type == typeof(string))
            {
                field.SetValue(holder, "");
            }

            if(type.IsClass && type != typeof(string))
            {
                var value = field.GetValue(holder);

                if(value != null)
                {
                    FieldInfo[] fields = value.GetType().GetFields(BindingFlags.GetField | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public);
                    foreach (var item in fields)
                    {
                        if (!IsFieldNeed(item))
                        {
                            Debug.Log("ignore:" + item.Name);
                            continue;
                        }
                       
                        var prop = new SerializedProperty(item, value);
                        prop.SetParentProperty(this);
                        list.Add(prop);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 判断寡字段能否序列化
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static bool IsFieldNeed(FieldInfo fieldInfo)
        {
            var type = fieldInfo.FieldType;

            //排除字典
            if (type.IsGenericType && type.Name.Contains("Dictionary`"))
            {
                return false;
            }

            //排除非公有变量
            if (fieldInfo.Attributes != FieldAttributes.Public)
            {
                var attrs = fieldInfo.GetCustomAttributes(false);
                if (attrs.Length == 0 || (attrs.Length > 0 && Array.Find(attrs, x => x is SerializeField) == null))
                {
                    return false;
                }
            }

            //排出接口
            if (type.IsInterface)
            {
                return false;
            }

            //修正type
            if (type.IsArray || type.IsGenericType)
            {
                if (type.IsGenericType)
                {
                    type = type.GetGenericArguments()[0];
                }
                else
                {
                    type = type.GetElementType();
                }
            }

            //排出修正后的接口
            if (type.IsInterface)
            {
                return false;
            }

            //排除不能序列化的类
            if (type.IsClass)
            {
                if (!type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    var atts = type.GetCustomAttributes(false);
                    var seri = Array.Find(atts, x => x is System.SerializableAttribute);
                    if (seri == null)
                    {
                        return false;
                    }
                }
            }

            //排除内置变量
            if (fieldInfo.Name.Contains("k__BackingField"))
            {
                return false;
            }

            return true;
        }

      

        public void Dispose() { }

        public static bool EqualContents(SerializedProperty x, SerializedProperty y) { return false; }

        internal void SetBitAtIndexForAllTargetsImmediate(int index, bool value) { }

        public SerializedProperty Next(bool enterChildren)
        {
            if (enterChildren)
            {
                if (hasChildren)
                {
                    return subProps[0];
                }
                return null;
            }
            else
            {
                var currid = parentProp.subProps.IndexOf(this);
                if (currid < parentProp.subProps.Count - 1)
                {
                    return subProps[currid + 1];
                }
                return null;
            }
        }

        public SerializedProperty NextVisible(bool enterChildren)
        {
            if (enterChildren)
            {
                if (hasVisibleChildren)
                {
                    for (int i = 0; i < subProps.Count; i++)
                    {
                        if (subProps[i].editable)
                        {
                            return subProps[i];
                        }
                    }
                }
                return null;
            }
            else
            {
                var currid = parentProp.subProps.IndexOf(this);
                if (currid < parentProp.subProps.Count)
                {
                    for (int i = currid + 1; i < parentProp.subProps.Count; i++)
                    {
                        if (parentProp.subProps[i].editable)
                        {
                            return parentProp.subProps[i];
                        }
                    }
                }
                return null ;
            }
        }

        public bool DuplicateCommand() { return false; }

        public bool DeleteCommand() { return false; }
        
        internal SerializedProperty FindPropertyInternal(string propertyPath)
        {
            if (subProps != null)
            {
                var item = subProps.Find(x => x.propertyPath == propertyPath);
                if(item != null)
                {
                    return item;
                }
            }
            return null;
        }


        internal SerializedProperty FindPropertyRelativeInternal(string propertyPath)
        {
            var propertyPath_full = this.propertyPath + "/" + propertyPath;
            return FindPropertyInternal(propertyPath_full);
        }

        internal int[] GetLayerMaskSelectedIndex() { return null; }

        internal string[] GetLayerMaskNames() { return null; }

        internal void ToggleLayerMaskAtIndex(int index) { }

        private bool GetArrayElementAtIndexInternal(int index) { return false; }

        private static PropertyType JudgePropertyType(FieldInfo info)
        {
            var propertyType = PropertyType.None;
            var type = info.FieldType;
            if (Type.Equals(type,typeof(int))|| Type.Equals(type, typeof(long))|| Type.Equals(type, typeof(short)))
            {
                propertyType = PropertyType.Integer;
            }
            else if (Type.Equals(type, typeof(bool)))
            {
                propertyType = PropertyType.Boolean;
            }
            else if (Type.Equals(type, typeof(float))|| Type.Equals(type, typeof(double))|| Type.Equals(type, typeof(decimal)))
            {
                propertyType = PropertyType.Float;
            }
            else if (Type.Equals(type, typeof(string)))
            {
                propertyType = PropertyType.String;
            }
            else if (Type.Equals(type, typeof(Color)))
            {
                propertyType = PropertyType.Color;
            }
            else if (Type.Equals(type, typeof(Enum)))
            {
                propertyType = PropertyType.Enum;
            }
            else if (Type.Equals(type, typeof(LayerMask)))
            {
                propertyType = PropertyType.LayerMask;
            }
            else if (Type.Equals(type, typeof(Vector2)))
            {
                propertyType = PropertyType.Vector2;
            }
            else if (Type.Equals(type, typeof(Vector3)))
            {
                propertyType = PropertyType.Vector3;
            }
            else if (Type.Equals(type, typeof(Vector4)))
            {
                propertyType = PropertyType.Vector4;
            }
            else if (Type.Equals(type, typeof(Quaternion)))
            {
                propertyType = PropertyType.Quaternion;
            }
            else if (Type.Equals(type, typeof(Rect)))
            {
                propertyType = PropertyType.Rect;
            }
            else if (Type.Equals(type, typeof(Bounds)))
            {
                propertyType = PropertyType.Bounds;
            }
            else if (type.IsArrayOrList())
            {
                propertyType = PropertyType.ArraySize;
            }
            else if(type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                propertyType = PropertyType.ObjectReference;
            }
            else if (Type.Equals(type, typeof(AnimationCurve)))
            {
                propertyType = PropertyType.AnimationCurve;
            }
            return propertyType;
        }

        public void InsertArrayElementAtIndex(int index) { }

        public void DeleteArrayElementAtIndex(int index) { }


        public void ClearArray() { }


        public bool MoveArrayElement(int srcIndex, int dstIndex) { return false; }

    }
}
