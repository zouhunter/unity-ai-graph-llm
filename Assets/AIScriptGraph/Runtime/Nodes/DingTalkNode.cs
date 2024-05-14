using System.Collections;
using UFrame.NodeGraph;

using UnityEngine;
using UnityEngine.Networking;

namespace AIScripting
{
    [CustomNode("DingTalk", 0, "AIScripting")]
    public class DingTalkNode : ScriptNodeBase
    {
        public Ref<string> talk_url = new Ref<string>("talk_url", "https://oapi.dingtalk.com/robot/send?access_token=0d5881be6ebc0ce565481930584fd579ae7ccfa076ab6cddd05432f2bb615382");
        public Ref<string> talk_text;
        private LitCoroutine _litCoroutine;
        
        protected override void OnProcess()
        {
            _litCoroutine = Owner.StartCoroutine(SendRequest());
        }

        [System.Serializable]
        public class SendData
        {
            public Text text;
            public string msgtype;

            [System.Serializable]
            public class Text
            {
                public string content;
            }
        }

        private IEnumerator SendRequest()
        {
            var sendData = new SendData() { 
                text = new SendData.Text() { content = "����:" + talk_text.Value },
                msgtype = "text" 
           };
            var req = UnityWebRequest.Post(talk_url, JsonUtility.ToJson(sendData));
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                DoFinish(true);
            }
            else
            {
                DoFinish(false);
            }
        }
    }
}