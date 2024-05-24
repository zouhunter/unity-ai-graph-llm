using System;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;
using System.Reflection;
using System.Text;

namespace AIScripting.Import
{
    [CustomNode("AssemblyFileAPI",orderPriority:2, group: Define.GROUP)]
    public class AssembleFileAPINode : ScriptNodeBase
    {
        [Tooltip("类型列表")]
        public Ref<List<string>> assemblyFiles;
        public List<string> supportAssemblePaths;
        [Tooltip("类型api")]
        public Ref<Dictionary<Type, string>> typeApis;
        public BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static|BindingFlags.DeclaredOnly;

        protected override void OnProcess()
        {
            if (typeApis.Value != null)
            {
                if (assemblyFiles.Value != null)
                {
                    var sb = new StringBuilder();
                    foreach (var assembleFile in assemblyFiles.Value)
                    {
                        var assemble = Assembly.LoadFrom(assembleFile);
                        LoadReferenceAssembles(assemble);
                        var types = assemble?.GetTypes();
                        foreach (var type in types)
                        {
                            sb.Clear();
                            TypesAPINode.WriteTypeClass(type, bindingFlags, sb);
                            typeApis.Value[type] = sb.ToString();
                        }
                    }
                    DoFinish(true);
                    return;
                }
            }
            DoFinish(false);
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
