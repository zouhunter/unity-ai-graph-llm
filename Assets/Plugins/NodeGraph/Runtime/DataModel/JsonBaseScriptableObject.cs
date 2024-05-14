/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2024-05-14                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 抽象节点                                                                        *
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
    public class NodeBaseObject
    {
        [HideInInspector, SerializeField]
        public string name;
        [HideInInspector, SerializeField]
        private string _assembly;
        [HideInInspector, SerializeField]
        private string _type;

        internal NodeBaseObject Instantiate()
        {
            var json = ToJson();
            var type = GetType();
            var instance = System.Activator.CreateInstance(type) as NodeBaseObject;
            instance.DeSeraizlize(json);
            return instance;
        }

        public string ToJson()
        {
            _assembly = GetType().Assembly.FullName;
            _type = GetType().FullName;
#if UNITY_EDITOR
            var json = EditorJsonUtility.ToJson(this);
#else
            varjson = JsonUtility.ToJson(this);
#endif
            return json;
        }

        public void DeSeraizlize(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

#if UNITY_EDITOR
            EditorJsonUtility.FromJsonOverwrite(json, this);
#else
            JsonUtility.FromJsonOverwrite(json, this);
#endif
        }

    }
}