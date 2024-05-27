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

namespace AIScripting.Diagram
{
    [CustomNode("ComfyUI", group: Define.GROUP)]
    public class ComfyUINode : ScriptNodeBase
    {
        public Ref<string> url = new Ref<string>("comfyui_url", "http://127.0.0.1:8188");
        public Ref<string> prompt = new Ref<string>("comfyui_prompt", "beautiful scenery nature glass bottle landscape, , purple galaxy bottle,");
        public Ref<TextAsset> textAsset = new Ref<TextAsset>("comfyui_workflow");
        public Ref<string> exportDir = new Ref<string>("comfyui_dir", "Build");
        public string _clientId;
        public string promptPath = "6.widgets_values";
        private HttpClient _httpClient = new HttpClient();
        private string _wsUrl;
        protected override void OnProcess()
        {
            if(string.IsNullOrEmpty(_clientId))
                _clientId = System.Guid.NewGuid().ToString();
            if (url.Value.StartsWith("http:"))
                _wsUrl = "ws:" + url.Value.Substring(4);
            if (url.Value.StartsWith("https:"))
                _wsUrl = "wss:" + url.Value.Substring(5);
            Owner.StartCoroutine(DoRequest((texture) =>
            {
                Debug.LogError(texture);
                DoFinish(texture != null);
            }));
        }

        /// <summary>
        /// 加载图
        /// </summary>
        /// <param name="onLoad"></param>
        /// <returns></returns>
        private IEnumerator DoRequest(Action<Texture> onLoad)
        {
            string promptId = null;
            yield return QueuePrompt((pid) => { promptId = pid;});
            if(promptId == null)
            {
                onLoad?.Invoke(null);
                yield break;
            }
            GetImages(promptId).ContinueWith(x => {
                var dic = x.Result;
                WriteImage(dic);
                onLoad?.Invoke(null);
            });
        }
        private void WriteImage(Dictionary<string, Dictionary<string,byte[]>> images)
        {
            foreach (var nodeId in images.Keys)
            {
                foreach (var imageDic in images[nodeId])
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var path = $"{exportDir.Value}/{imageDic.Key}_{timestamp}.png";
                    Debug.Log($"GIF_LOCATION:{path}");
                    File.WriteAllBytes(path, imageDic.Value);
                    Debug.Log($"{path} DONE!!!");
                }
            }
        }

        public IEnumerator QueuePrompt(System.Action<string> _callback)
        {
            long startTime = System.DateTime.Now.Ticks;
            Debug.Log(System.DateTime.Now.Ticks + ",request:" + url);
            UnityWebRequest request = new UnityWebRequest(url + "/prompt", "POST");
            {
                var mapData = JsonMapper.ToObject(textAsset.Value.text);
                if(!string.IsNullOrEmpty(promptPath))
                {
                    var paths = promptPath.Split('.');
                    var jd = mapData;
                    for (int i = 0; i < paths.Length; i++)
                    {
                        if(i == paths.Length - 1)
                        {
                            jd[paths[i]] = this.prompt.Value;
                        }
                        else
                        {
                            jd = jd[paths[i]];
                        }
                    }
                }
                var jsonData = new JsonData();
                jsonData["client_id"] = _clientId;
                jsonData["prompt"] = mapData;
                string _jsonText = jsonData.ToJson();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
                request.uploadHandler = new UploadHandlerRaw(data);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    yield return null;
                    var progress = (request.uploadProgress + request.downloadProgress) * 0.5f;
                    _asyncOp.SetProgress(progress);
                }
                if (string.IsNullOrEmpty(request.error))
                {
                    var json = JsonMapper.ToObject(request.downloadHandler.text);
                    Debug.Log(request.downloadHandler.text);
                    if (json.TryGetValue<string>("prompt_id", out var promptId))
                    {
                        _callback?.Invoke(promptId);
                    }
                }
                else
                {
                    _callback?.Invoke(null);
                    Debug.LogError(request.error);
                }
                request.Dispose();
                Debug.Log(System.DateTime.Now.Ticks + ",Comfyui耗时：" + (System.DateTime.Now.Ticks - startTime) / 10000000);
            }
        }

        public IEnumerator GetHistory(string promptId,Action<string> _callback)
        {
            long startTime = System.DateTime.Now.Ticks;
            Debug.Log(System.DateTime.Now.Ticks + ",request:" + url);
            UnityWebRequest request = new UnityWebRequest(url + $"/history/{promptId}", "GET");
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    yield return null;
                    var progress = (request.uploadProgress + request.downloadProgress) * 0.5f;
                    _asyncOp.SetProgress(progress);
                }
                if (string.IsNullOrEmpty(request.error))
                {
                    _callback?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    _callback?.Invoke(null);
                    Debug.LogError(request.error);
                }
                request.Dispose();
                Debug.Log(System.DateTime.Now.Ticks + ",Comfyui耗时：" + (System.DateTime.Now.Ticks - startTime) / 10000000);
            }
        }

        // 获取历史记录
        private async Task<JsonData> GetHistory(string promptId)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{url.Value}/history/{promptId}");
                Debug.Log(response);
                return JsonMapper.ToObject(response);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return null;
        }

        // 向服务器队列发送提示信息
        private async Task<JsonData> QueuePrompt(string prompt)
        {
            var mapData = JsonMapper.ToObject(prompt);
            var jsonData = new JsonData();
            jsonData["client_id"] = _clientId;
            jsonData["prompt"] = mapData;
            var content = new StringContent(jsonData.ToJson(), Encoding.UTF8, "application/json");
            Debug.LogError($"{url.Value}/prompt");
            var response = await _httpClient.PostAsync(new Uri($"{url.Value}/prompt").AbsoluteUri, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonMapper.ToObject(responseBody);
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
            return await _httpClient.GetByteArrayAsync(urlBytes);
        }

        // 获取图片，涉及到监听WebSocket消息
        private async Task<Dictionary<string, Dictionary<string,byte[]>>> GetImages(string promptId)
        {
            Debug.Log($"prompt_id:{promptId}");
            var outputImages = new Dictionary<string, Dictionary<string, byte[]>>();
            var buffer = new byte[1024 * 4];
            ClientWebSocket ws = new ClientWebSocket();

            try
            {
                await ws.ConnectAsync(new Uri($"ws://127.0.0.1:8188/ws?client_id={_clientId}"), CancellationToken.None);
            }
            catch (Exception ex)
            {
                // 错误处理
                Debug.LogError("WebSocket Connect Exception: " + ex.ToString());
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
                        //TODO判断队列
                        if ((int)jd["data"]["status"]["exec_info"]["queue_remaining"] == 0)
                        {
                            break;
                        }
                        else
                        {
                            //wait
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
            foreach (var output in history[promptId]["outputs"])
            {
                foreach (string nodeId in history[promptId]["outputs"].Keys)
                {
                    var nodeOutput = history[promptId]["outputs"][nodeId];
                    // 图片分支
                    if (nodeOutput.ContainsKey("images"))
                    {
                        outputImages[nodeId] = new Dictionary<string, byte[]>();
                        foreach (JsonData image in nodeOutput["images"])
                        {
                            Debug.LogError(image["filename"].ToString());
                            var imageData = await GetImage(image["filename"].ToString(), image["subfolder"].ToString(), image["type"].ToString());
                            outputImages[nodeId][image["filename"].ToString()] = imageData;
                        }
                    }
                    //// 视频分支
                    //if (nodeOutput.ContainsKey("videos"))
                    //{
                    //    var videosOutput = new List<byte[]>();
                    //    foreach (var video in nodeOutput["videos"])
                    //    {
                    //        var videoData = await GetImage(video["filename"].ToString(), video["subfolder"].ToString(), video["type"].ToString());
                    //        videosOutput.Add(videoData);
                    //    }
                    //    outputImages[nodeId] = videosOutput;
                    //}
                }
            }
            return outputImages;
        }
    }

    [System.Serializable]
    public class WebSocketResult
    {
        public string type;
        public Data data;

        [System.Serializable]
        public class Data
        {
            public string sid;
            public Status status;
        }

        [System.Serializable]
        public class Status
        {
            public ExecInfo exec_info;
        }

        [System.Serializable]
        public class ExecInfo
        {
            public string queue_remaining;
        }
    }
}
