using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UIElements;

namespace AIScripting
{
    public class GraphDirector
    {
        public AIScriptGraph graph { get; private set; }

        public GraphDirector(AIScriptGraph graph)
        {
            this.graph = graph;
        }

        public void Binding(List<BindingInfo> _bindings)
        {
            foreach (var binding in _bindings)
            {
                graph.SetVariable(binding.name, new Variable<UnityEngine.Object>() { Value = binding.target });
            }
        }
        public void Run(string beginNode)
        {
            graph.StartUp(beginNode);
        }

    }
}