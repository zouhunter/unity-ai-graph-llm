using System;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;
using System.Reflection;
using System.Text;

namespace AIScripting.Describe
{
    [CustomNode("AssemblyAPI", group:Define.GROUP)]
    public class AssembleAPINode : DescribeBaseNode
    {
        [Tooltip("类型列表")]
        public Ref<List<string>> assemblys;
        public BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        private Dictionary<string, Type> _typeDict = new Dictionary<string, Type>();

        protected override AsyncOp WriteContent(StringBuilder sb)
        {
            bool exists = false;
            if(assemblys.Value != null)
            {
                foreach (var assemble in assemblys.Value)
                {
                    var types = Assembly.Load(assemble)?.GetTypes();
                    foreach (var type in types)
                    {
                        exists = true;
                        TypesAPINode.WriteTypeClass(type,bindingFlags, sb);
                    }
                }
            }
            if (exists)
            {
                return AsyncOp.CompletedOp;
            }
            return null;
        }
    }
}