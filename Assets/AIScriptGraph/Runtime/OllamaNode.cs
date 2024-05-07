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
        /// AI�趨
        /// </summary>
        public string m_SystemSetting = string.Empty;
        /// <summary>
        /// ����ģ��,ģ�������������
        /// </summary>
        public ModelType m_GptModel = ModelType.llama3;
        /// <summary>
        /// api��ַ
        /// </summary>
        [SerializeField] protected string url;
        /// <summary>
        /// ��ʾ�ʣ�����Ϣһ����
        /// </summary>
        [Header("���͵���ʾ���趨")]
        [SerializeField] protected string m_Prompt = string.Empty;
        /// <summary>
        /// ����
        /// </summary
        [Header("���ûظ�������")]
        [SerializeField] protected string lan = "����";
        /// <summary>
        /// �����ı�������
        /// </summary>
        [Header("�����ı�������")]
        [SerializeField] protected int m_HistoryKeepCount = 15;
        /// <summary>
        /// ����Ի�
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
        /// ������Ϣ
        /// </summary>
        public virtual void PostMsg(string _msg, Action<string> _callback)
        {
            //��������������
            CheckHistory();
            //��ʾ�ʴ���
            string message = "��ǰΪ��ɫ�������趨��" + m_Prompt +
                " �ش�����ԣ�" + lan +
                " ���������ҵ����ʣ�" + _msg;

            //���淢�͵���Ϣ�б�
            m_DataList.Add(new SendData("user", message));

            //StartCoroutine(Request(message, _callback));
        }
        /// <summary>
        /// ���ñ�������������������ֹ̫��
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
            //����ʱ�����AI�趨
            m_DataList.Add(new SendData("system", m_SystemSetting));
        }


        /// <summary>
        /// ���ýӿ�
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
                        //��Ӽ�¼
                        m_DataList.Add(new SendData("assistant", _backMsg));
                        _callback(_backMsg);
                    }
                }
                else
                {
                    string _msgBack = request.downloadHandler.text;
                    Debug.LogError(_msgBack);
                }

                Debug.Log("Ollama��ʱ��" + (System.DateTime.Now.Ticks - startTime));
            }
        }

        #region ���ݶ���

        public enum ModelType
        {
            llama3
        }

        [Serializable]
        public class PostData
        {
            public string model;
            public List<SendData> messages;
            public bool stream = false;//��ʽ
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