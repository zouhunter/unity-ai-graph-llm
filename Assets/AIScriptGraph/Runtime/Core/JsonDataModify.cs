using LitJson;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIScripting
{
    public static class JsonDataModify
    {
        private static JsonData SelectNode(string promptPath,JsonData mapData,out string finalPath)
        {
            if (!string.IsNullOrEmpty(promptPath))
            {
                var paths = promptPath.Split('.');
                var jd = mapData;
                for (int i = 0; i < paths.Length; i++)
                {
                    if (i == paths.Length - 1)
                    {
                        finalPath = paths[i];
                        return jd;
                    }
                    else
                    {
                        jd = jd[paths[i]];
                    }
                }
            }
            finalPath = null;
            return null;
        }

        public static void Modify(this JsonData data, JsonStrModifyNode modify)
        {
            var node = SelectNode(modify.path, data, out var finalPath);
            if(node != null && !string.IsNullOrEmpty(finalPath))
            {
                node[finalPath] = modify.value;
            }
        }
        public static void Modify(this JsonData data, JsonIntModifyNode modify)
        {
            var node = SelectNode(modify.path, data, out var finalPath);
            if (node != null && !string.IsNullOrEmpty(finalPath))
            {
                node[finalPath] = modify.value;
            }
        }
        public static void Modify(this JsonData data, JsonBoolModifyNode modify)
        {
            var node = SelectNode(modify.path, data, out var finalPath);
            if (node != null && !string.IsNullOrEmpty(finalPath))
            {
                node[finalPath] = modify.value;
            }
        }
    }


    [System.Serializable]
    public class JsonStrModifyNode
    {
        public string path;
        public string value;
    }
    [System.Serializable]
    public class JsonIntModifyNode
    {
        public string path;
        public int value;
    }
    [System.Serializable]
    public class JsonBoolModifyNode
    {
        public string path;
        public bool value;
    }
}
