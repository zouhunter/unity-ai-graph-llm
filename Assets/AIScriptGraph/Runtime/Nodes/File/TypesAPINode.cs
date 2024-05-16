using System;
using System.Collections;
using System.Collections.Generic;

using UFrame.NodeGraph;

using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;

namespace AIScripting
{
    [CustomNode("TypesAPI", 0, "AIScripting")]
    public class TypesAPINode : ScriptNodeBase
    {
        [Tooltip("类型列表")]
        public Ref<List<string>> types;

        [Tooltip("输出")]
        public Ref<string> output;

        protected override void OnProcess()
        {
            foreach (var type in types.Value)
            {
                
            }
        }
    }
}