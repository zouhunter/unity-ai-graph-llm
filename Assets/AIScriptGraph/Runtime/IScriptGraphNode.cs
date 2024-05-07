using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AIScripting
{
    public interface IScriptGraphNode 
    {
        string Name { get; }
        Status status { get; }
        float progress { get; }
        AsyncOp Run();
        void Binding(AIScriptGraph graph);
        void Cancel();
    }
}
