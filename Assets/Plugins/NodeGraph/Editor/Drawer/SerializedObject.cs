using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Reflection;

namespace UFrame.NodeGraph.ObjDrawer
{
    public class SerializedObject<T> : SerializedObject where T : class
    {
        private SerializedInstance<T> _instence;
        public SerializedObject(SerializedInstance<T> instance):base(instance.Object)
        {
            this._instence = instance;
        }
        public void SetDirty()
        {
            _instence.Save();
        }
    }

    public class SerializedObject
    {
        protected SerializedProperty obj_Prop;

        protected object value;

        public SerializedObject(object value)
        {
            this.value = value;
            FieldInfo info = typeof(SerializedObject).GetField("value", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic);
            obj_Prop = new SerializedProperty(info, this);
            obj_Prop.serializedObject = this;
        }

        internal SerializedProperty GetIterator()
        {
            return obj_Prop;
        }

        public SerializedProperty FindProperty(string propertyPath)
        {
            return obj_Prop.FindPropertyInternal(propertyPath);
        }
    }
}