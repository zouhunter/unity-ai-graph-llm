/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - ScriptObject内存序列化                                                          *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UFrame.NodeGraph.DataModel;

namespace UFrame.NodeGraph
{
    public class ScriptObjectCatchUtil
    {
        private static Dictionary<string, System.Tuple<string,string>> typeCatch = new Dictionary<string, System.Tuple<string, string>>();
        private static Dictionary<string, string> jsonCatch = new Dictionary<string, string>();

        public static void Catch(string id, DataModel.NodeBaseObject data)
        {
            if (data != null)
            {
                Catch(id, data.GetType(), data.ToJson());
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(data);
#endif
            }
        }

        public static NodeBaseObject Revert(string id)
        {
            try
            {
                var json = GetJson(id);
                var type = GetType(id);
                if (!string.IsNullOrEmpty(json) && type != null)
                {
                    var m_node = NodeBaseObject.CreateInstance(type) as NodeBaseObject;
                    m_node.DeSeraizlize(json);
                    return m_node;
                }
                return null;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        private static System.Type GetType(string hash)
        {
            System.Tuple<string, string> tuple = null;
            if(typeCatch.TryGetValue(hash, out tuple))
            {
                return System.Reflection.Assembly.Load(tuple.Item1)?.GetType(tuple.Item2);
            }
            return null;
        }

        private static string GetJson(string hash)
        {
            string json = null;
            jsonCatch.TryGetValue(hash, out json);
            return json;
        }

        private static void Catch(string hash, System.Type type, string json)
        {
            typeCatch[hash] = new System.Tuple<string, string>(type.Assembly.FullName,type.FullName);
            jsonCatch[hash] = json;
        }
    }
}
