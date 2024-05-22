using System;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;
using System.Reflection;
using System.Text;

namespace AIScripting.Describe
{
    [CustomNode("AssemblyFileAPI",orderPriority:2, group: Define.GROUP)]
    public class AssembleFileAPINode : DescribePrefixNode
    {
        [Tooltip("类型列表")]
        public Ref<List<string>> assemblyFiles;
        public List<string> supportAssemblePaths;
        public BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static|BindingFlags.DeclaredOnly;
        private Dictionary<string, Type> _typeDict = new Dictionary<string, Type>();

        protected override AsyncOp WriteContent(StringBuilder sb)
        {
            bool exists = false;
            if (assemblyFiles.Value != null)
            {
                foreach (var assembleFile in assemblyFiles.Value)
                {
                    var assemble = Assembly.LoadFrom(assembleFile);
                    LoadReferenceAssembles(assemble);
                    var types = assemble?.GetTypes();
                    foreach (var type in types)
                    {
                        exists = true;
                        TypesAPINode.WriteTypeClass(type, bindingFlags, sb);
                    }
                }
            }
            if (exists)
            {
                return AsyncOp.CompletedOp;
            }
            return null;
        }

        private void LoadReferenceAssembles(Assembly assembly)
        {
            var subAssembles = assembly.GetReferencedAssemblies();
            foreach (var item in subAssembles)
            {
                var name = item.Name;
                Assembly subAssembly = null;
                try
                {
                    subAssembly = Assembly.Load(name);
                }
                catch (Exception)
                {
                }
                if (subAssembly == null)
                {
                    foreach (var dir in supportAssemblePaths)
                    {
                        var subAssemblyPath = $"{dir}/{name}.dll";
                        if (System.IO.File.Exists(subAssemblyPath))
                        {
                            subAssembly = System.Reflection.Assembly.LoadFrom(subAssemblyPath);
                            if (subAssembly != null)
                            {
                                break;
                            }
                            else
                            {
                                Debug.LogError("faild load dll:" + subAssemblyPath);
                            }
                        }
                    }
                }
                if (subAssembly != null)
                {
                    //LoadReferenceAssembles(subAssembly);
                }
            }
        }
    }
}
