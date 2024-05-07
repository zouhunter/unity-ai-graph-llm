using System;
using System.Collections;
using System.Collections.Generic;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEngine;
using UnityEngine.Networking;


namespace AIScripting
{
    [CustomNode("Ollama",0, "AIScripting")]
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
        /// api地址
        /// </summary>
        [SerializeField] protected string url;
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
        /// <summary>
        /// 缓存对话
        /// </summary>
        [SerializeField] public List<SendData> m_DataList = new List<SendData>();

        public Ref<string> input;
        public Ref<string> output;

        protected override int InCount => 1;

        protected override int OutCount => int.MaxValue;

        public override int Style => 1;

        protected override void OnProcess()
        {

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

            //StartCoroutine(Request(message, _callback));
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

        private void Start()
        {
            //运行时，添加AI设定
            m_DataList.Add(new SendData("system", m_SystemSetting));
        }


        /// <summary>
        /// 调用接口
        /// </summary>
        /// <param name="_postWord"></param>
        /// <param name="_callback"></param>
        /// <returns></returns>
        public IEnumerator Request(string _postWord, System.Action<string> _callback)
        {
            float startTime = System.DateTime.Now.Ticks;
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                PostData _postData = new PostData
                {
                    model = m_GptModel.ToString(),
                    messages = m_DataList
                };

                string _jsonText = JsonUtility.ToJson(_postData);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                //request.SetRequestHeader("Authorization", string.Format("Bearer {0}", api_key));

                yield return request.SendWebRequest();

                if (request.responseCode == 200)
                {
                    string _msgBack = request.downloadHandler.text;
                    MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msgBack);
                    if (_textback != null && _textback.message != null)
                    {

                        string _backMsg = _textback.message.content;
                        //添加记录
                        m_DataList.Add(new SendData("assistant", _backMsg));
                        _callback(_backMsg);
                    }
                }
                else
                {
                    string _msgBack = request.downloadHandler.text;
                    Debug.LogError(_msgBack);
                }

                Debug.Log("Ollama耗时：" + (System.DateTime.Now.Ticks - startTime));
            }
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
            public bool stream = false;//流式
        }
        [Serializable]
        public class MessageBack
        {
            public string created_at;
            public string model;
            public Message message;
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