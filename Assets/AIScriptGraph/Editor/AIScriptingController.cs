using NUnit.Framework;

using System.Collections.Generic;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEditor;
using UnityEditor.MemoryProfiler;

using UnityEngine;

namespace AIScripting
{
    public class AIScriptingController : NodeGraphController
    {
        public override string Group => Define.GROUP;

        public override NodeGraphObj CreateNodeGraphObject()
        {
            var graph = UnityEngine.ScriptableObject.CreateInstance<AIScriptGraph>();
            graph.ControllerType = GetType().FullName;
            ProjectWindowUtil.CreateAsset(graph, Group + ".asset");
            return graph;
        }

        public override NodeView CreateDefaultNodeView()
        {
            return new ScriptingNodeView();
        }

        public override ConnectionGUI CreateConnection(string type, ConnectionPointData output, ConnectionPointData input)
        {
            var connection = ScriptableObject.CreateInstance<NodeConnection>();
            connection.type = type;
            connection.name = type;
            return new ConnectionGUI(
               new ConnectionData(type, connection, output, input),
               output,
               input
           );
        }

        public override void BuildFromGraph(NodeGraphObj graph)
        {
            base.BuildFromGraph(graph);
            var graphDirector = new GraphDirector(graph as AIScriptGraph);
            foreach (var env in AIScriptSettings.instance.envs)
            {
                switch (env.type)
                {
                    case EditorEnv.ValueType.String:
                        graphDirector.graph.SetVariable(new Variable<string>(env.key,env.value));
                        break;
                    case EditorEnv.ValueType.Int:
                        {
                            int.TryParse(env.value, out int value);
                            graphDirector.graph.SetVariable(new Variable<int>(env.key, value));
                        }
                        break;
                    case EditorEnv.ValueType.Float:
                        {
                            float.TryParse(env.value, out float value);
                            graphDirector.graph.SetVariable(new Variable<float>(env.key, value));
                        }
                        break;
                    case EditorEnv.ValueType.Bool:
                        {
                            bool.TryParse(env.value, out bool value);
                            graphDirector.graph.SetVariable(new Variable<bool>(env.key, value));
                        }
                        break;
                    case EditorEnv.ValueType.StrList:
                        {
                            graphDirector.graph.SetVariable(new Variable<List<string>>(env.key, new List<string>(env.value.Split('|'))));
                        }
                        break;
                    case EditorEnv.ValueType.StrArray:
                        {
                            graphDirector.graph.SetVariable(new Variable<string[]>(env.key, env.value.Split('|')));
                        }
                        break;
                    case EditorEnv.ValueType.IntList:
                        {
                            graphDirector.graph.SetVariable(new Variable<List<int>>(env.key, new List<int>(Str2IntArray(env.value))));
                        }
                        break;
                    case EditorEnv.ValueType.IntArray:
                        {
                            graphDirector.graph.SetVariable(new Variable<int[]>(env.key, Str2IntArray(env.value)));
                        }
                        break;
                    case EditorEnv.ValueType.Object:
                        {
                            var path = AssetDatabase.GUIDToAssetPath(env.value);
                            graphDirector.graph.SetVariable(new Variable<Object>(env.key, AssetDatabase.LoadAssetAtPath<Object>(path)));
                        }
                        break;
                    default:
                        break;
                }
            }
            var op = graphDirector.Run();
            op.RegistComplete((x) => { Debug.Log("graph finished!"); });
        }

        private int[] Str2IntArray(string str)
        {
            var strs = str.Split('|');
            var result = new int[strs.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                int.TryParse(strs[i], out result[i]);
            }
            return result;
        }
    }
}
