using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AIScripting
{
    public interface IScriptGraphNode 
    {
        string Title { get; }
        Status status { get; }
        float progress { get; }
        void ResetGraph(AIScriptGraph graph);
        AsyncOp Run();
        void Cancel();
    }
}
