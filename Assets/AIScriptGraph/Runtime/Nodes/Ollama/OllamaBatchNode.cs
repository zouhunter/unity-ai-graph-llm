using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UFrame.NodeGraph;
using UnityEngine;
using UnityEngine.Networking;

namespace AIScripting.Ollama
{
    [CustomNode("OllamaBatch", 0, Define.GROUP)]
    public class OllamaBatchNode : ScriptNodeBase
    {
        /// <summary>
        /// AI设定
        /// </summary>
        [Multiline(3)]
        public string m_SystemSetting = string.Empty;

        /// <summary>
        /// 设置模型,模型类型自行添加
        /// </summary>
        public string m_GptModel = "llama3";
        /// <summary>
        /// 上下文保留条数
        /// </summary>
        [Header("上下文保留条数")]
        [SerializeField] protected int m_HistoryKeepCount = 15;

        [Header("消息接受key")]
        [SerializeField] protected string eventReceiveKey = "ollama_receive_message";
        /// <summary>
        /// 缓存对话
        /// </summary>
        public Ref<List<SendData>> m_DataList;

        public Ref<string[]> input = new Ref<string[]>("input_text");
        public Ref<string> output = new Ref<string>("output_text");
        public Ref<string> url = new Ref<string>("ollama_url", "http://localhost:8081/api/chat");

        public override int Style => 1;
        private LitCoroutine _litCoroutine;
        private int _index = 0;
        private StringBuilder _resultSb;
        protected override void OnProcess()
        {
            if(input.Value != null && input.Value.Length > 0)
            {
                _resultSb = new StringBuilder();
                PostMsg(input.Value[_index], OnFinishOnce);
            }
            else
            {
                DoFinish(false);
            }
        }

        private void OnFinishOnce(string result)
        {
            _resultSb.AppendLine();
            System.IO.File.AppendAllText("ollama.txt", result);
            _index++;
            if (input.Value != null && input.Value.Length > _index)
            {
                PostMsg(input.Value[_index], OnFinishOnce);
            }
            else
            {
                output.SetValue(_resultSb.ToString());
                DoFinish();
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public virtual void PostMsg(string _msg, Action<string> _callback)
        {
            //上下文条数设置
            CheckHistory();

            //缓存发送的信息列表
            m_DataList.Value.Add(new SendData("user", _msg));
            _litCoroutine = Owner.StartCoroutine(Request(_callback));
        }

        /// <summary>
        /// 设置保留的上下文条数，防止太长
        /// </summary>
        public virtual void CheckHistory()
        {
            if (m_DataList.Value.Count > m_HistoryKeepCount)
            {
                m_DataList.Value.RemoveAt(0);
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
                var sendList = new List<SendData>(m_DataList.Value);
                sendList.Insert(0, new SendData("system", m_SystemSetting));
                PostData _postData = new PostData
                {
                    model = m_GptModel.ToString(),
                    messages = sendList,
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
                        //添加记录
                        m_DataList.Value.Add(new SendData("assistant", _msgBack));
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
