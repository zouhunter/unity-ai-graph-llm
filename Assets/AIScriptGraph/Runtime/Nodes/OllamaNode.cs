using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEditor.PackageManager.Requests;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;


namespace AIScripting
{
    [CustomNode("Ollama", 0, "AIScripting")]
    public class OllamaNode : ScriptNodeBase
    {
        /// <summary>
        /// AI设定
        /// </summary>
        public string m_SystemSetting = string.Empty;
        /// <summary>
        /// 设置模型,模型类型自行添加
        /// </summary>
        public ModelType m_GptModel = ModelType.llama3;
        /// <summary>
        /// 提示词，与消息一起发送
        /// </summary>
        [Header("发送的提示词设定")]
        [SerializeField] protected string m_Prompt = string.Empty;
        /// <summary>
        /// 语言
        /// </summary
        [Header("设置回复的语言")]
        [SerializeField] protected string lan = "中文";
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
        [NonSerialized] public List<SendData> m_DataList = new List<SendData>();

        public Ref<string> input = new Ref<string>("input_text");
        public Ref<string> output = new Ref<string>("output_text");
        public Ref<string> url = new Ref<string>("ollama_url", "http://localhost:8081/api/chat");

        protected override int InCount => int.MaxValue;

        protected override int OutCount => int.MaxValue;

        public override int Style => 1;
        private LitCoroutine _litCoroutine;

        protected override void OnProcess()
        {
            PostMsg(input, (result) =>
            {
                output.Value = result;
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
            //提示词处理
            string message = "当前为角色的人物设定：" + m_Prompt +
                " 回答的语言：" + lan +
                " 接下来是我的提问：" + _msg;

            //缓存发送的信息列表
            m_DataList.Add(new SendData("user", message));
            _litCoroutine = Owner.StartCoroutine(Request(_callback));
        }

        /// <summary>
        /// 设置保留的上下文条数，防止太长
        /// </summary>
        public virtual void CheckHistory()
        {
            if (m_DataList.Count > m_HistoryKeepCount)
            {
                m_DataList.RemoveAt(0);
            }
        }


        [Serializable]
        public class SendData
        {
            [SerializeField] public string role;
            [SerializeField] public string content;
            public SendData() { }
            public SendData(string _role, string _content)
            {
                role = _role;
                content = _content;
            }
        }

        //{"model":"llama3","created_at":"2024-05-08T09:27:16.4077711Z","message":{"role":"assistant","content":" harmony"},"done":false}
        [Serializable]
        public class ReceiveData
        {
            public string model;
            public string created_at;
            public Message message;
            public bool done;
        }

        public class DownloadHandlerMessageQueue : DownloadHandlerScript
        {
            public StringBuilder allText = new StringBuilder();
            private Action<ReceiveData> _onReceive;
            private StringBuilder _textInProcess = new StringBuilder();

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                _textInProcess.Append(Encoding.UTF8.GetString(data, 0, dataLength));
                //Debug.Log("ReceiveData:" + _textInProcess.ToString());
                int index = -1;
                var startIndex = -1;
                var endIndex = 0;
                var paired = 0;
                while (++index < _textInProcess.Length)
                {
                    var charItem = _textInProcess[index];
                    if (charItem == '{')
                    {
                        if(startIndex < 0)
                            startIndex = index;
                        paired++;
                    }
                    if (charItem == '}')
                    {
                        paired--;
                    }
                    if (paired == 0 && startIndex >= 0)
                    {
                        endIndex = index;
                        var oneMessage = _textInProcess.ToString(startIndex, endIndex - startIndex + 1);
                        OnReceiveOne(oneMessage);
                        startIndex = -1;
                    }
                }
                if (endIndex < _textInProcess.Length - 1)
                {
                    _textInProcess.Remove(0, endIndex+1);
                }
                return base.ReceiveData(data, dataLength);
            }

            private void OnReceiveOne(string text)
            {
                var receiveData = JsonUtility.FromJson<ReceiveData>(text);
                allText.Append(receiveData.message.content);
                _onReceive?.Invoke(receiveData);
            }

            internal void RegistReceive(Action<ReceiveData> onReceive)
            {
                this._onReceive = onReceive;
            }
        }

        private void Start()
        {
            //运行时，添加AI设定
            m_DataList.Add(new SendData("system", m_SystemSetting));
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
                    model = m_GptModel.ToString(),
                    messages = m_DataList,
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
                        m_DataList.Add(new SendData("assistant", _msgBack));
                        _callback(_msgBack);
                    }
                    else
                    {
                        _callback(null);
                    }
                }
                else
                {
                    string _msgBack = request.downloadHandler.text;
                    _callback?.Invoke(null);
                    Debug.LogError(_msgBack);
                }
                request.Dispose();
                Debug.Log(System.DateTime.Now.Ticks + ",Ollama耗时：" + (System.DateTime.Now.Ticks - startTime) / 10000000);
            }
        }

        private void OnRecevieRequest()
        {

        }

        #region 数据定义

        public enum ModelType
        {
            llama3
        }

        [Serializable]
        public class PostData
        {
            public string model;
            public List<SendData> messages;
            public bool stream;//流式
        }

        [Serializable]
        public class Message
        {
            public string role;
            public string content;
        }

        #endregion

    }
}