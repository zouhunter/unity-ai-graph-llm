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
        [Tooltip("�����б�")]
        public Ref<List<string>> types;

        [Tooltip("���")]
        public Ref<string> output;

        protected override void OnProcess()
        {
            foreach (var type in types.Value)
            {
                
            }
        }
    }
}