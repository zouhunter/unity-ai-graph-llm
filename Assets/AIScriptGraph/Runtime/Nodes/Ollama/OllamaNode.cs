using System;
using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEngine;
using UnityEngine.Networking;

namespace AIScripting.Ollama
{
    [CustomNode("Ollama", 0, "AIScripting")]
    public class OllamaNode : ScriptNodeBase
    {
        /// <summary>
        /// AI�趨
        /// </summary>
        [Multiline(3)]
        public string m_SystemSetting = string.Empty;

        /// <summary>
        /// ����ģ��,ģ�������������
        /// </summary>
        public string m_GptModel = "llama3";
        /// <summary>
        /// �����ı�������
        /// </summary>
        [Header("�����ı�������")]
        [SerializeField] protected int m_HistoryKeepCount = 15;

        [Header("��Ϣ����key")]
        [SerializeField] protected string eventReceiveKey = "ollama_receive_message";
        /// <summary>
        /// ����Ի�
        /// </summary>
        [NonSerialized] public List<SendData> m_DataList = new List<SendData>();

        public Ref<string> input = new Ref<string>("input_text");
        public Ref<string> output = new Ref<string>("output_text");
        public Ref<string> url = new Ref<string>("ollama_url", "http://localhost:8081/api/chat");

        public override int Style => 1;
        private LitCoroutine _litCoroutine;

        protected override void OnProcess()
        {
            if(m_DataList.Count == 0)
            {
                m_DataList.Add(new SendData("system", m_SystemSetting));
            }

            PostMsg(input, (result) =>
            {
                output.SetValue(result);
                DoFinish();
            });
        }

        public override void ResetGraph(AIScriptGraph graph)
        {
            base.ResetGraph(graph);
            m_DataList.Clear();
        }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        public virtual void PostMsg(string _msg, Action<string> _callback)
        {
            //��������������
            CheckHistory();

            //���淢�͵���Ϣ�б�
            m_DataList.Add(new SendData("user", _msg));
            _litCoroutine = Owner.StartCoroutine(Request(_callback));
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


        /// <summary>
        /// �յ��ظ�
        /// </summary>
        /// <param name="data"></param>
        private void OnReceive(ReceiveData data)
        {
            //Debug.Log(data.message.content);
            Owner.SendEvent(eventReceiveKey, data);
        }

        /// <summary>
        /// ���ýӿ�
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
                        //��Ӽ�¼
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
                    _callback?.Invoke(null);
                    Debug.LogError(request.downloadHandler.error);
                }
                request.Dispose();
                Debug.Log(System.DateTime.Now.Ticks + ",Ollama��ʱ��" + (System.DateTime.Now.Ticks - startTime) / 10000000);
            }
        }
    }
}