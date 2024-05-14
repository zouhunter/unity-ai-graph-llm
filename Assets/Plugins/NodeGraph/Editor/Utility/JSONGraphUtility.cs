/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - json导出导入工具                                                                *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using UFrame.NodeGraph.DataModel;
using System.Reflection;
using System.CodeDom;

namespace UFrame.NodeGraph {

    public class JSONGraphUtility {

        public static void ExportGraphToJSONFromDialog(NodeGraphObj graph) {

            string path =
                EditorUtility.SaveFilePanel/*SaveFilePanelInProject*/(
                    string.Format("Export {0} to JSON file", graph.name), Application.dataPath,
                    graph.name, "json");
            if(string.IsNullOrEmpty(path)) {
                return;
            }
            string jsonString = SerializeGraph(graph);
            File.WriteAllText (path, jsonString, System.Text.Encoding.UTF8);
		}

        public static void ExportAllGraphsToJSONFromDialog() {

            var folderSelected = 
                EditorUtility.OpenFolderPanel("Select folder to export all graphs", Application.dataPath + "..", "");
            if(string.IsNullOrEmpty(folderSelected)) {
                return;
            }

            var guids = AssetDatabase.FindAssets(NGSettings.GRAPH_SEARCH_CONDITION);

            foreach(var guid in guids) {
                string graphPath = AssetDatabase.GUIDToAssetPath(guid);
                string graphName = Path.GetFileNameWithoutExtension(graphPath);

                string jsonFilePath = Path.Combine (folderSelected, string.Format("{0}.json", graphName));

                var graph = AssetDatabase.LoadAssetAtPath<NodeGraphObj>(graphPath);
                string jsonString = SerializeGraph (graph);

                File.WriteAllText (jsonFilePath, jsonString, System.Text.Encoding.UTF8);
            }
        }

        public static bool ImportJSONToGraphFromDialog(ref NodeGraphObj graph) {

            string fileSelected = EditorUtility.OpenFilePanelWithFilters("Select JSON files to import", Application.dataPath, new string[] {"JSON files", "json", "All files", "*"});
            if(string.IsNullOrEmpty(fileSelected)) {
                return false;
            }

            string name = Path.GetFileNameWithoutExtension(fileSelected);

            var jsonContent = File.ReadAllText (fileSelected, System.Text.Encoding.UTF8);

            if (graph != null) {
                Undo.RecordObject(graph, "Import");
                DeserializeGraph(jsonContent, ref graph);
            } else {
                var newAssetFolder = CreateFolderForImportedAssets ();
                var graphPath = FileUtility.PathCombine(newAssetFolder, string.Format("{0}.asset", name));
                graph = ScriptableObject.CreateInstance<NodeGraphObj>();
                AssetDatabase.CreateAsset (graph, graphPath);
                AssetDatabase.Refresh();
                graph = AssetDatabase.LoadAssetAtPath<NodeGraphObj>(graphPath);
                Debug.Log("create new:" + graph);
            }
            DeserializeGraph(jsonContent, ref graph);
            return true;
        }

        public static void ImportAllJSONInDirectoryToGraphFromDialog() {
            var folderSelected = 
                EditorUtility.OpenFolderPanel("Select folder contains JSON files to import", Application.dataPath + "..", "");
            if(string.IsNullOrEmpty(folderSelected)) {
                return;
            }

            var newAssetFolder = CreateFolderForImportedAssets ();

            var filePaths = FileUtility.GetAllFilePathsInFolder (folderSelected);
            foreach (var path in filePaths) {
                var ext = Path.GetExtension (path).ToLower ();
                if (ext != ".json") {
                    continue;
                }
                var jsonContent = File.ReadAllText (path, System.Text.Encoding.UTF8);
                var name = Path.GetFileNameWithoutExtension (path);

                var graph = ScriptableObject.CreateInstance<NodeGraphObj>();
                DeserializeGraph(jsonContent,ref graph);
                var graphPath = FileUtility.PathCombine(newAssetFolder, string.Format("{0}.asset", name));
                AssetDatabase.CreateAsset (graph, graphPath);
            }
        }

        private static string CreateFolderForImportedAssets() {
            var t = DateTime.Now;
            var folderName = String.Format ("ImportedGraphs_{0:D4}-{1:D2}_{2:D2}_{3:D2}{4:D2}{5:D2}", t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second);

            AssetDatabase.CreateFolder ("Assets", folderName);

            return String.Format("Assets/{0}", folderName);
        }

        public static void DeserializeGraph(string json, ref NodeGraphObj graph)
        {
            //var jsonNode = JSONClass.Parse(json);
            if (graph == null)
            {
                graph = ScriptableObject.CreateInstance<NodeGraphObj>();
            }
            //else
            //{
            //    DeleteSubAsset(graph);
            //}
            //List<NodeBaseObject> subAssets = new List<NodeBaseObject>();
#if UNITY_EDITOR
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, graph);
#else
            JsonUtility.FromJsonOverwrite(json, graph);
#endif
            graph.Validate();

            //for (int i = 0; i < jsonNode["m_allNodes"].AsArray.Count; i++)
            //{
            //    var item = jsonNode["m_allNodes"][i]["m_nodeJson"];
            //    var nodeType = Assembly.Load(item["_assembly"]).GetType(item["_type"]);
            //    var obj = System.Activator.CreateInstance(nodeType) as Node;
            //    obj.DeSeraizlize(item.ToString());
            //    graph.Nodes[i].Object = (obj);
            //    //subAssets.Add(obj);
            //}


            //for (int i = 0; i < jsonNode["m_allConnections"].AsArray.Count; i++)
            //{
            //    var item = jsonNode["m_allConnections"][i]["m_connectionJson"];
            //    var nodeType = Assembly.Load(item["_assembly"]).GetType(item["_type"]);
            //    var obj = System.Activator.CreateInstance(nodeType) as Connection;
            //    obj.DeSeraizlize(item.ToString());
            //    graph.Connections[i].Object = (obj);
            //    //subAssets.Add(obj);
            //}

            //ScriptableObjUtility.SetSubAssets(subAssets.ToArray(), graph, true);
            EditorUtility.SetDirty(graph);
            //foreach (var item in graph.Nodes)
            //{
            //    //Debug.Log(item.Object);
            //    EditorUtility.SetDirty(item.Object);
            //}
        }

        public static string SerializeGraph(NodeGraphObj graph)
        {
            var rootJson = JsonUtility.ToJson(graph);
#if UNITY_EDITOR
            rootJson = EditorJsonUtility.ToJson(graph);
#endif
            JSONClass jc = JSONClass.Parse(rootJson) as JSONClass;

            //var nodes = jc["m_allNodes"].AsArray;
            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    var node = graph.Nodes[i].Object;
            //    nodes[i]["m_node"] = JSONNode.Parse(node.Seraizlize());
            //}

            //var connections = jc["m_allConnections"].AsArray;
            //for (int i = 0; i < connections.Count; i++)
            //{
            //    var connection = graph.Connections[i].Object;
            //    nodes[i]["m_connection"] = JSONNode.Parse(connection.Seraizlize());
            //}
            return rootJson;
        }

        //private static void DeleteSubAsset(NodeGraphObj graph)
        //{
        //    var path = AssetDatabase.GetAssetPath(graph);
        //    if (!string.IsNullOrEmpty(path))
        //    {
        //        var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
        //        foreach (var item in subAssets)
        //        {
        //            if (item != graph)
        //            {
        //                //Debug.Log("delete:" + item);
        //                UnityEngine.Object.DestroyImmediate(item, true);
        //            }
        //        }
        //    }
        //}
    }
}
