using System;
using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;
using UnityEngine.Networking;

namespace AIScripting.Ollama
{
    [CustomNode("Ollama", 0, Define.GROUP)]
    public class OllamaNode : ScriptNodeBase
    {
        [OllamaModelName]
        public string model = "llama3";

        [Header("上下文保留条数")]
        [SerializeField] protected int m_HistoryKeepCount = 15;

        [Header("消息接受key")]
        [SerializeField] protected string eventReceiveKey = "ollama_receive_message";

        [Tooltip("modify data list after request")]
        public bool modifyData = false;
  
        [Tooltip("会话记录")]
        public Ref<List<SendData>> m_DataList;

        public Ref<string> input = new Ref<string>("input_text");
        public Ref<string> output = new Ref<string>("output_text");
        public Ref<string> url = new Ref<string>("ollama_url", "http://localhost:8081/api/chat");

        private LitCoroutine _litCoroutine;
        private List<SendData> historyData;

        protected override void OnProcess()
        {
            if(!modifyData)
            {
                historyData = new List<SendData>();
                historyData.AddRange(m_DataList.Value);
            }
            else
            {
                historyData = m_DataList.Value;
            }

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
            //上下文条数设置
            CheckHistory();

            //缓存发送的信息列表
            historyData.Add(new SendData("user", _msg));
            _litCoroutine = Owner.StartCoroutine(Request(_callback));
        }

        /// <summary>
        /// 设置保留的上下文条数，防止太长
        /// </summary>
        public virtual void CheckHistory()
        {
            if(m_HistoryKeepCount >= 0)
            {
                while (historyData.Count > m_HistoryKeepCount)
                {
                    historyData.RemoveAt(0);
                }
            }
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
        public IEnumerator Request(System.Action<string> _callback)
        {
            long startTime = System.DateTime.Now.Ticks;
            Debug.Log(System.DateTime.Now.Ticks + ",request:" + url);
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            {
                PostData _postData = new PostData
                {
                    model = model.ToString(),
                    messages = historyData,
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
                    var progress = (request.uploadProgress + request.downloadProgress) * 0.5f;
                    _asyncOp.SetProgress(progress);
                }

                if (string.IsNullOrEmpty(request.error))
                {
                    string _msgBack = downloadHandler.allText.ToString();
                    if (!string.IsNullOrEmpty(_msgBack))
                    {
                        //添加记录
                        historyData.Add(new SendData("assistant", _msgBack));
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
                    Debug.LogError(request.error);
                }
                request.Dispose();
                Debug.Log(System.DateTime.Now.Ticks + ",Ollama耗时：" + (System.DateTime.Now.Ticks - startTime) / 10000000);
            }
        }
    }
}
