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
using UFrame.NodeGraph.DataModel;

namespace UFrame.NodeGraph
{

    public class JSONGraphUtility
    {

        public static void ExportGraphToJSONFromDialog(NodeGraphObj graph)
        {
            string path =
                EditorUtility.SaveFilePanel/*SaveFilePanelInProject*/(
                    string.Format("Export {0} to JSON file", graph.name), Application.dataPath,
                    graph.name, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string jsonString = SerializeGraph(graph);
            File.WriteAllText(path, jsonString, System.Text.Encoding.UTF8);
        }

        public static void ExportAllGraphsToJSONFromDialog()
        {
            var folderSelected =
                EditorUtility.OpenFolderPanel("Select folder to export all graphs", Application.dataPath + "..", "");
            if (string.IsNullOrEmpty(folderSelected))
            {
                return;
            }

            var guids = AssetDatabase.FindAssets(NGSettings.GRAPH_SEARCH_CONDITION);

            foreach (var guid in guids)
            {
                string graphPath = AssetDatabase.GUIDToAssetPath(guid);
                string graphName = Path.GetFileNameWithoutExtension(graphPath);
                string jsonFilePath = Path.Combine(folderSelected, string.Format("{0}.json", graphName));
                var graph = AssetDatabase.LoadAssetAtPath<NodeGraphObj>(graphPath);
                string jsonString = SerializeGraph(graph);
                File.WriteAllText(jsonFilePath, jsonString, System.Text.Encoding.UTF8);
            }
        }

        public static bool ImportJSONToGraphFromDialog(ref NodeGraphObj graph)
        {
            string fileSelected = EditorUtility.OpenFilePanelWithFilters("Select JSON files to import", Application.dataPath, new string[] { "JSON files", "json", "All files", "*" });
            if (string.IsNullOrEmpty(fileSelected))
            {
                return false;
            }

            string name = Path.GetFileNameWithoutExtension(fileSelected);

            var jsonContent = File.ReadAllText(fileSelected, System.Text.Encoding.UTF8);

            if (graph != null)
            {
                Undo.RecordObject(graph, "Import");
                DeserializeGraph(jsonContent, ref graph);
            }
            else
            {
                var newAssetFolder = CreateFolderForImportedAssets();
                var graphPath = FileUtility.PathCombine(newAssetFolder, string.Format("{0}.asset", name));
                graph = ScriptableObject.CreateInstance<NodeGraphObj>();
                AssetDatabase.CreateAsset(graph, graphPath);
                AssetDatabase.Refresh();
                graph = AssetDatabase.LoadAssetAtPath<NodeGraphObj>(graphPath);
                Debug.Log("create new:" + graph);
            }
            DeserializeGraph(jsonContent, ref graph);
            return true;
        }

        public static void ImportAllJSONInDirectoryToGraphFromDialog()
        {
            var folderSelected =
                EditorUtility.OpenFolderPanel("Select folder contains JSON files to import", Application.dataPath + "..", "");
            if (string.IsNullOrEmpty(folderSelected))
            {
                return;
            }

            var newAssetFolder = CreateFolderForImportedAssets();

            var filePaths = FileUtility.GetAllFilePathsInFolder(folderSelected);
            foreach (var path in filePaths)
            {
                var ext = Path.GetExtension(path).ToLower();
                if (ext != ".json")
                {
                    continue;
                }
                var jsonContent = File.ReadAllText(path, System.Text.Encoding.UTF8);
                var name = Path.GetFileNameWithoutExtension(path);

                var graph = ScriptableObject.CreateInstance<NodeGraphObj>();
                DeserializeGraph(jsonContent, ref graph);
                var graphPath = FileUtility.PathCombine(newAssetFolder, string.Format("{0}.asset", name));
                AssetDatabase.CreateAsset(graph, graphPath);
            }
        }

        private static string CreateFolderForImportedAssets()
        {
            var t = DateTime.Now;
            var folderName = String.Format("ImportedGraphs_{0:D4}-{1:D2}_{2:D2}_{3:D2}{4:D2}{5:D2}", t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second);

            AssetDatabase.CreateFolder("Assets", folderName);

            return String.Format("Assets/{0}", folderName);
        }

        public static void DeserializeGraph(string json, ref NodeGraphObj graph)
        {
            if (graph == null)
                graph = ScriptableObject.CreateInstance<NodeGraphObj>();
            EditorJsonUtility.FromJsonOverwrite(json, graph);
            graph.CheckValidate();
        }

        public static string SerializeGraph(NodeGraphObj graph)
        {
            var rootJson = EditorJsonUtility.ToJson(graph);
            return rootJson;
        }
    }
}
