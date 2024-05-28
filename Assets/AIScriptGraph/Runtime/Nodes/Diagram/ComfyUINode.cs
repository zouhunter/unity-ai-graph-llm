using AIScripting.Ollama;

using LitJson;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using UFrame.NodeGraph;

using UnityEngine;
using UnityEngine.Networking;
using System.Net.Http;
using System.IO;
using UnityEngine.Networking.Types;

namespace AIScripting.Diagram
{
    [CustomNode("ComfyUI", group: Define.GROUP)]
    public class ComfyUINode : ScriptNodeBase
    {
        public Ref<string> url = new Ref<string>("comfyui_url", "http://127.0.0.1:8188");
        [Tooltip("提示信息")]
        public Ref<JsonStrModifyNode> prompt = new ("", new JsonStrModifyNode() {
            path = "6.inputs.text",
            value= "beautiful scenery nature glass bottle landscape, , purple galaxy bottle," 
        });
        [Tooltip("batch数量")]
        public Ref<JsonIntModifyNode> batchCount = new("", new JsonIntModifyNode()
        {
            path = "5.inputs.batch_size",
            value = 2
        });
        [Tooltip("宽度")]
        public Ref<JsonIntModifyNode> picWidth = new("", new JsonIntModifyNode()
        {
            path = "5.inputs.width",
            value = 512
        });
        [Tooltip("高度")]
        public Ref<JsonIntModifyNode> picHeight = new("", new JsonIntModifyNode()
        {
            path = "5.inputs.height",
            value = 512
        });
        public Ref<TextAsset> textAsset = new Ref<TextAsset>();
        public Ref<string> exportDir = new Ref<string>("", "Build");
        public Ref<List<string>> exportFiles = new("comfyui_files", new List<string>());
        public string clientId;
        public string picNodeId = "9";
        private HttpClient _httpClient = new HttpClient();
        private string _wsUrl;

        protected override void OnProcess()
        {
            if (string.IsNullOrEmpty(clientId))
                clientId = System.Guid.NewGuid().ToString();
            if (url.Value.StartsWith("http:"))
                _wsUrl = "ws" + url.Value.Substring(4);
            if (url.Value.StartsWith("https:"))
                _wsUrl = "wss" + url.Value.Substring(5);
            Debug.Log(_wsUrl);
            DoRequest().ContinueWith(x =>
            {
                DoFinish(x.Result);
            });
        }

        private async Task<bool> DoRequest()
        {
            var mapData = JsonMapper.ToObject(textAsset.Value.text);
            mapData.Modify(prompt.Value);
            mapData.Modify(batchCount.Value);
            mapData.Modify(picWidth.Value);
            mapData.Modify(picHeight.Value);
            var promptId = await QueuePrompt(mapData);
            if (string.IsNullOrEmpty(promptId))
                return false;
            var imageMap = await GetImages(promptId);
            OnLoadImages(imageMap);
            return imageMap.Count > 0;
        }

        private void OnLoadImages(Dictionary<string, byte[]> images)
        {
            exportFiles.Value.Clear();
            System.IO.Directory.CreateDirectory($"{exportDir.Value}");
            foreach (var imageDic in images)
            {
                var path = $"{exportDir.Value}/{imageDic.Key}";
                exportFiles.Value.Add($"{imageDic.Key}");
                Debug.Log($"GIF_LOCATION:{path}");
                File.WriteAllBytes(path, imageDic.Value);
                Debug.Log($"{path} DONE!!!");
            }
        }

        // 获取历史记录
        private async Task<JsonData> GetHistory(string promptId)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{url.Value}/history/{promptId}");
                Debug.Log("history:" + response);
                return JsonMapper.ToObject(response);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return null;
        }

        // 向服务器队列发送提示信息
        private async Task<string> QueuePrompt(JsonData mapData)
        {
            var jsonData = new JsonData();
            jsonData["client_id"] = clientId;
            jsonData["prompt"] = mapData;
            var content = new StringContent(jsonData.ToJson(), Encoding.UTF8, "application/json");
            //Debug.LogError($"{url.Value}/prompt");
            var response = await _httpClient.PostAsync($"{url.Value}/prompt", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log("QueuePrompt result:" + responseBody);
            var json = JsonMapper.ToObject(responseBody);
            if (json.TryGetValue<string>("prompt_id", out var promptId))
            {
                return promptId;
            }
            return null;
        }

        // 获取图片
        private async Task<byte[]> GetImage(string filename, string subfolder, string folderType)
        {
            var data = new { filename = filename, subfolder = subfolder, type = folderType };
            var urlValues = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("filename", filename),
                new KeyValuePair<string, string>("subfolder", subfolder),
                new KeyValuePair<string, string>("type", folderType)
            });
            var urlBytes = $"{url.Value}/view?{await urlValues.ReadAsStringAsync()}";
            try
            {
                return await _httpClient.GetByteArrayAsync(urlBytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        // 获取图片，涉及到监听WebSocket消息
        private async Task<Dictionary<string, byte[]>> GetImages(string promptId)
        {
            Debug.Log($"prompt_id:{promptId}");
            var outputImages = new Dictionary<string, byte[]>();
            var buffer = new byte[1024 * 4];
            ClientWebSocket ws = new ClientWebSocket();

            try
            {
                await ws.ConnectAsync(new Uri($"{_wsUrl}/ws?client_id={clientId}"), CancellationToken.None);
            }
            catch (Exception ex)
            {
                // 错误处理
                Debug.LogError("WebSocket Connect Exception: " + ex.ToString());
                return outputImages;
            }
            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var jd = JsonMapper.ToObject(message);
                        Debug.Log(message);
                        var queueRemaining = (int)jd["data"]["status"]["exec_info"]["queue_remaining"];
                        //TODO判断队列
                        if (queueRemaining == 0)
                        {
                            break;
                        }
                        else
                        {
                            //wait
                            Debug.Log("wait queue:" + queueRemaining);
                        }
                    }
                    else
                    {
                        Debug.LogError(result.MessageType);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("WebSocket Recv Exception: " + ex.ToString());
                }

            }
            Debug.Log("socket execute finish");
            var history = await GetHistory(promptId);
            try
            {
                if (history[promptId]["status"]["status_str"].ToString() == "success")
                {
                    // 图片分支
                    var images = history[promptId]["outputs"][picNodeId]["images"];
                    foreach (JsonData image in images)
                    {
                        var imageName = image["filename"].ToString();
                        Debug.Log("start download:" + imageName);
                        var imageData = await GetImage(imageName, image["subfolder"].ToString(), image["type"].ToString());
                        if (imageData != null)
                        {
                            outputImages[image["filename"].ToString()] = imageData;
                            Debug.Log("download success:" + imageName);
                        }
                    }
                }
               
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            Debug.Log("download image finished!");
            return outputImages;
        }
    }
}

///// <summary>
///// 加载图
///// </summary>
///// <param name="onLoad"></param>
///// <returns></returns>
//private IEnumerator DoRequest(Action<bool> onLoad)
//{
//    string promptId = null;
//    yield return QueuePrompt((pid) => { promptId = pid; });
//    if (promptId == null)
//    {
//        onLoad?.Invoke(false);
//        yield break;
//    }
//    GetImages(promptId).ContinueWith(x =>
//    {
//        var dic = x.Result;
//        OnLoadImages(dic);
//        onLoad?.Invoke(dic.Count > 0);
//    });
//}


//public IEnumerator QueuePrompt(System.Action<string> _callback)
//{
//    long startTime = System.DateTime.Now.Ticks;
//    Debug.Log(System.DateTime.Now.Ticks + ",request:" + url);
//    UnityWebRequest request = new UnityWebRequest(url + "/prompt", "POST");
//    {
//        var mapData = JsonMapper.ToObject(textAsset.Value.text);
//        if (!string.IsNullOrEmpty(promptPath))
//        {
//            var paths = promptPath.Split('.');
//            var jd = mapData;
//            for (int i = 0; i < paths.Length; i++)
//            {
//                if (i == paths.Length - 1)
//                {
//                    jd[paths[i]] = this.prompt.Value;
//                }
//                else
//                {
//                    jd = jd[paths[i]];
//                }
//            }
//        }
//        var jsonData = new JsonData();
//        jsonData["client_id"] = clientId;
//        jsonData["prompt"] = mapData;
//        string _jsonText = jsonData.ToJson();
//        byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
//        request.uploadHandler = new UploadHandlerRaw(data);
//        request.downloadHandler = new DownloadHandlerBuffer();
//        request.SetRequestHeader("Content-Type", "application/json");
//        var operation = request.SendWebRequest();
//        while (!operation.isDone)
//        {
//            yield return null;
//            var progress = (request.uploadProgress + request.downloadProgress) * 0.5f;
//            _asyncOp.SetProgress(progress);
//        }
//        if (string.IsNullOrEmpty(request.error))
//        {
//            var json = JsonMapper.ToObject(request.downloadHandler.text);
//            Debug.Log(request.downloadHandler.text);
//            if (json.TryGetValue<string>("prompt_id", out var promptId))
//            {
//                _callback?.Invoke(promptId);
//            }
//        }
//        else
//        {
//            _callback?.Invoke(null);
//            Debug.LogError(request.error);
//        }
//        request.Dispose();
//        Debug.Log(System.DateTime.Now.Ticks + ",Comfyui耗时：" + (System.DateTime.Now.Ticks - startTime) / 10000000);
//    }
//}

//public IEnumerator GetHistory(string promptId, Action<string> _callback)
//{
//    long startTime = System.DateTime.Now.Ticks;
//    Debug.Log(System.DateTime.Now.Ticks + ",request:" + url);
//    UnityWebRequest request = new UnityWebRequest(url + $"/history/{promptId}", "GET");
//    {
//        var operation = request.SendWebRequest();
//        while (!operation.isDone)
//        {
//            yield return null;
//            var progress = (request.uploadProgress + request.downloadProgress) * 0.5f;
//            _asyncOp.SetProgress(progress);
//        }
//        if (string.IsNullOrEmpty(request.error))
//        {
//            _callback?.Invoke(request.downloadHandler.text);
//        }
//        else
//        {
//            _callback?.Invoke(null);
//            Debug.LogError(request.error);
//        }
//        request.Dispose();
//        Debug.Log(System.DateTime.Now.Ticks + ",Comfyui耗时：" + (System.DateTime.Now.Ticks - startTime) / 10000000);
//    }
//}
