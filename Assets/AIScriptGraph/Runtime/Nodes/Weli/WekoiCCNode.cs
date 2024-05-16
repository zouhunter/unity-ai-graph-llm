using AIScripting.Ollama;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UFrame.NodeGraph;

using UnityEngine;
using UnityEngine.Networking;

using static System.Net.WebRequestMethods;

namespace AIScripting
{
    [CustomNode("WekoiCC", 0, "AIScripting")]
    public class WekoiCCNode : ScriptNodeBase
    {
        public string conversation_id = "295948";
        public string model = "gpt-4-1106-preview";
        public string Authorization = "cb-dabd1913552249b3b14670b3ada4227c";
        public Ref<string> input = new Ref<string>("input_text");
        public Ref<string> output = new Ref<string>("output_text");
        private LitCoroutine _litCoroutine;

        [Header("消息接受key")]
        [SerializeField] protected string eventReceiveKey = "wekoi_receive_message";

        protected override void OnProcess()
        {
            PostMsg(input, (result) =>
            {
                output.SetValue(result);
                DoFinish();
            });
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        public virtual void PostMsg(string _msg, Action<string> _callback)
        {
            _litCoroutine = Owner.StartCoroutine(Request(_msg,  _callback));
        }

        public class DownloadHandlerMessageQueue : DownloadHandlerScript
        {
            public StringBuilder allText = new StringBuilder();
            public Action<string> onReceive { get; set; }
            public bool Finished { get; internal set; }

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                if (Finished)
                    return false;

                var text = Encoding.UTF8.GetString(data, 0, dataLength);
                var lines = text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if(line.StartsWith("data:"))
                    {
                        if(line.Length > 6)
                        {
                            var textData = line.Substring(6,line.Length-7);
                            if(textData != "连接成功")
                            {
                                allText.AppendLine(textData);
                                onReceive?.Invoke(textData);
                            }
                            Finished = textData == "[DONE]";
                            if(Finished)
                            {
                                break;
                            }
                        }
                    }
                }
                return base.ReceiveData(data, dataLength);
            }
        }
        /// <summary>
        /// 调用接口
        /// </summary>
        /// <param name="_postWord"></param>
        /// <param name="_callback"></param>
        /// <returns></returns>
        public IEnumerator Request(string msg,System.Action<string> _callback)
        {
            var url = $"https://api-chatbot.wekoi.co/chatbot/api/v1/chat/stream?content={msg}&conversation_id={conversation_id}&" +
                $"from_user=zouhangte%40wekoi.cn&model={model}&max_tokens=1024&" +
                "temperature=1&presence_penalty=0.6&add_context=true&use_context=true";
            url = new System.Uri(url).AbsoluteUri;
            Debug.Log(System.DateTime.Now.Ticks + ",request:" + url);
            long startTime = System.DateTime.Now.Ticks;
            UnityWebRequest request = new UnityWebRequest(url, "OPTIONS");
            {
                request.SetRequestHeader("Content-Type", "text/event-stream;charset=UTF-8");
                request.SetRequestHeader("Accept", "*/*");
                request.SetRequestHeader("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
                //request.SetRequestHeader("Access-Control-Request-Headers", "authorization,content-type");
                //request.SetRequestHeader("Access-Control-Request-Method", "GET");
                //request.SetRequestHeader("Origin", "https://wekit.wekoi.cc");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    yield return null;
                    _asyncOp.SetProgress(operation.progress);
                }
                if (request.responseCode == 200)
                {
                    yield return Request2(url,msg, _callback);
                }
                else
                {
                    _callback?.Invoke(null);
                    Debug.LogError(request.error);
                }
                request.Dispose();
                Debug.Log(System.DateTime.Now.Ticks + ",Ollama耗时：" + (System.DateTime.Now.Ticks - startTime) / 10000000);
            }
        }
        /// <summary>
        /// 收到回复
        /// </summary>
        /// <param name="data"></param>
        private void OnReceive(string data)
        {
            Owner.SendEvent(eventReceiveKey, data);
        }

        public IEnumerator Request2(string url,string msg, System.Action<string> _callback)
        {
            Debug.Log(System.DateTime.Now.Ticks + ",request:" + url);
            long startTime = System.DateTime.Now.Ticks;
            UnityWebRequest request = new UnityWebRequest(url, "GET");
            {
                var downloadHandler = new DownloadHandlerMessageQueue();
                downloadHandler.onReceive = OnReceive;
                request.downloadHandler = downloadHandler;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "*/*");
                request.SetRequestHeader("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
                //request.SetRequestHeader("Access-Control-Request-Headers", "authorization,content-type");
                //request.SetRequestHeader("Access-Control-Request-Method", "GET");
                request.SetRequestHeader("Accept-Encoding", "gzip, deflate, br, zstd");
                request.SetRequestHeader("Authorization", Authorization);
                //request.SetRequestHeader("Origin", "https://wekit.wekoi.cc");
                //request.SetRequestHeader("Referer", "https://wekit.wekoi.cc");
                request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Mobile Safari/537.36 Edg/124.0.0.0");
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    yield return null;
                    _asyncOp.SetProgress(operation.progress);
                    if(downloadHandler.Finished)
                    {
                        break;
                    }
                }
                if (request.responseCode == 200)
                {
                    string _msgBack = downloadHandler.allText.ToString();
                    if (!string.IsNullOrEmpty(_msgBack))
                    {
                        _callback(_msgBack);
                    }
                    else
                    {
                        _callback(null);
                    }
                }
                else
                {
                    _callback?.Invoke(null);
                    Debug.LogError(request.downloadHandler.error);
                }
                request.Dispose();
                Debug.Log(System.DateTime.Now.Ticks + ",Ollama耗时：" + (System.DateTime.Now.Ticks - startTime) / 10000000);
            }
        }
    }
}