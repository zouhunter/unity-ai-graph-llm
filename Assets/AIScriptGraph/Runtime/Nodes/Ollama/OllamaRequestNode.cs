using System;
using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;
using UnityEngine.Networking;

namespace AIScripting.Ollama
{
    [CustomNode("OllamaRequest", 0, Define.GROUP)]
    public class OllamaRequestNode : ScriptNodeBase
    {
        [SerializeField,Header("消息接受key")]
        protected string eventReceiveKey = "ollama_receive_message";
        public string ollama_model = "llama3";
   
        public Ref<string> url = new ("ollama_url", "http://localhost:8081/api/chat");
        public Ref<List<SendData>> input = new ("send_data_list");
        public Ref<string> output = new ("output_text");

        public override int Style => 1;

        private LitCoroutine _litCoroutine;

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
        public virtual void PostMsg(List<SendData> _msg, Action<string> _callback)
        {
            _litCoroutine = Owner.StartCoroutine(Request(input, _callback));
        }
        /// <summary>
        /// 收到回复
        /// </summary>
        /// <param name="data"></param>
        private void OnReceive(ReceiveData data)
        {
            //Debug.Log(data.message.content);
            Owner.SendEvent(eventReceiveKey, data);
        }

        /// <summary>
        /// 调用接口
        /// </summary>
        /// <param name="_postWord"></param>
        /// <param name="_callback"></param>
        /// <returns></returns>
        public IEnumerator Request(List<SendData> _msg,System.Action<string> _callback)
        {
            long startTime = System.DateTime.Now.Ticks;
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            {
                PostData _postData = new PostData
                {
                    model = ollama_model,
                    messages = _msg,
                    stream = true
                };

                string _jsonText = JsonUtility.ToJson(_postData);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
                request.uploadHandler = new UploadHandlerRaw(data);
                var downloadHandler = new DownloadHandlerMessageQueue();
                downloadHandler.RegistReceive(OnReceive);
                request.downloadHandler = downloadHandler;
                request.SetRequestHeader("Content-Type", "application/json");
                //request.SetRequestHeader("Authorization", string.Format("Bearer {0}", api_key));

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    yield return null;
                    _asyncOp.SetProgress(operation.progress);
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
                Debug.Log("Ollama耗时(s)：" + (System.DateTime.Now.Ticks - startTime) / 10000000);
            }
        }
    }
}
