using System;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;
using System.Reflection;
using System.Text;
using System.Diagnostics;

namespace AIScripting.Import
{
    [CustomNode("AssemblyAPI",orderPriority:2, group:Define.GROUP)]
    public class AssembleAPINode : ScriptNodeBase
    {
        [Tooltip("程序集列表")]
        public Ref<List<string>> assemblys;
        [Tooltip("类型api")]
        public Ref<Dictionary<Type, string>> typeApis;
        public BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        protected override void OnProcess()
        {
            if(typeApis.Value != null)
            {
                if (assemblys.Value != null)
                {
                    var sb = new StringBuilder();
                    foreach (var assemble in assemblys.Value)
                    {
                        var types = Assembly.Load(assemble)?.GetTypes();
                        foreach (var type in types)
                        {
                            sb.Clear();
                            TypesAPINode.WriteTypeClass(type, bindingFlags,sb);
                            typeApis.Value[type] = sb.ToString();
                        }
                    }
                    DoFinish(true);
                    return;
                }
            }
            DoFinish(false);
        }
    }
}
