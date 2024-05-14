/***********************************************************************************//*
*  ����: �޺���                                                                       *
*  ʱ��: 2024-05-14                                                                   *
*  �汾: master_5122a2                                                                *
*  ����:                                                                              *
*   - ����ڵ�                                                                        *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using System.Xml.Linq;
using System;

namespace UFrame.NodeGraph.DataModel
{
    [System.Serializable]
    public class NodeBaseObject : ScriptableObject
    {
        [HideInInspector, SerializeField]
        public string _assembly;
        [HideInInspector, SerializeField]
        public string _type;

        internal NodeBaseObject Instantiate()
        {
            var json = ToJson();
            var type = GetType();
            var instance = CreateInstance(type) as NodeBaseObject;
            instance.DeSeraizlize(json);
            return instance;
        }

        public string ToJson()
        {
            _assembly = GetType().Assembly.FullName;
            _type = GetType().FullName;
            var json = JsonUtility.ToJson(this);
            return json;
        }

        public void DeSeraizlize(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}