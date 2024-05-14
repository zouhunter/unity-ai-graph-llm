/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 序列化                                                                          *
*//************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.NodeGraph.DataModel;
using UnityEditor.Compilation;

namespace UFrame.NodeGraph
{
    /// <summary>
    /// 从JsonGraphUtility简化，只能用于运行时
    /// </summary>
    public class SerializeUtil
    {
        //public static List<T> DeserializeGraphs<T>(string json) where T : NodeGraphObj
        //{
        //    List<T> graphs = new List<T>();
        //    var jsonnode = JSON.Parse(json);
        //    Debug.Log(jsonnode);

        //    if (jsonnode.AsArray != null)
        //    {
        //        foreach (var node in jsonnode.AsArray)
        //        {
        //            var graph = NodeGraph.SerializeUtil.DeserializeGraph<T>(node.ToString());
        //            if(graph != null){
        //                graphs.Add(graph);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var graph= NodeGraph.SerializeUtil.DeserializeGraph<T>(json);
        //        if (graph != null)
        //        {
        //            graphs.Add(graph);
        //        }
        //    }
        //    return graphs;
        //}

        public static T DeserializeGraph<T>(string json) where T: NodeGraphObj
        {
            var jsonNode = JSONClass.Parse(json);
            var graph = UnityEngine.ScriptableObject.CreateInstance<T>();

            json = json.Replace("bool", "Boolean");
            JsonUtility.FromJsonOverwrite(json, graph);
            graph.CheckValidate();
            //for (int i = 0; i < jsonNode["m_allNodes"].AsArray.Count; i++)
            //{
            //    var item = jsonNode["m_allNodes"][i]["m_nodeJson"];
            //    var type = System.Reflection.Assembly.Load(item["_assembly"].Value).GetType(item["_type"].Value);
            //    var obj = System.Activator.CreateInstance(type) as Node;
            //    obj.DeSeraizlize(item);
            //    obj.name = obj.GetType().FullName;
            //    graph.Nodes[i].Object = (obj);
            //}


            //for (int i = 0; i < jsonNode["m_allConnections"].AsArray.Count; i++)
            //{
            //    var item = jsonNode["m_allConnections"][i]["m_connection"]["json"];
            //    var type = System.Reflection.Assembly.Load(item["_assembly"].Value).GetType(item["_type"].Value);
            //    var obj = System.Activator.CreateInstance(type) as Connection;
            //    obj.DeSeraizlize(item);
            //    obj.name = obj.GetType().FullName;
            //    graph.Connections[i].Object = (obj);
            //}

            return graph;
        }
    }
}