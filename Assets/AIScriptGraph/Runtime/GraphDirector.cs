using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace AIScripting
{
    public class GraphDirector
    {
        public AIScriptGraph graph { get; private set; }

        public Status status;

        public GraphDirector(AIScriptGraph graph)
        {
            this.graph = graph;
            graph.Clean();
        }

        public void Binding(List<BindingInfo> _bindings)
        {
            foreach (var binding in _bindings)
            {
                graph.SetVariable(binding.name, new Variable<UnityEngine.Object>() { Value = binding.target });
            }
        }

        public AsyncOp Run()
        {
            return graph.Run();
        }

        public void Update()
        {
            graph?.Update();
        }
    }
}
