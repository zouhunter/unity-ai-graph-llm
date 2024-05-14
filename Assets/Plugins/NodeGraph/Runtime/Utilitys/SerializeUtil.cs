/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 序列化                                                                          *
*//************************************************************************************/

using UnityEngine;
using UFrame.NodeGraph.DataModel;

namespace UFrame.NodeGraph
{
    /// <summary>
    /// 从JsonGraphUtility简化，只能用于运行时
    /// </summary>
    public class SerializeUtil
    {
        public static T DeserializeGraph<T>(string json) where T: NodeGraphObj
        {
            var graph = ScriptableObject.CreateInstance<T>();
            json = json.Replace("bool", "Boolean");
            JsonUtility.FromJsonOverwrite(json, graph);
            graph.CheckValidate();
            return graph;
        }
    }
}