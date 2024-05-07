using AIScripting;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIScripting
{

    public class GraphDirecterBehaviour : MonoBehaviour
    {
        public string beginNode;
        public AIScriptGraph graph;
        public List<BindingInfo> bindings = new List<BindingInfo>();
        public GraphDirector graphDirector;

        private void OnEnable()
        {
            graphDirector = new GraphDirector(graph);
            graphDirector.Binding(bindings);
            graphDirector.Run(beginNode);
        }
    }
}