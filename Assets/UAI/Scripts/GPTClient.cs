using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using System.Text;

namespace UAI
{
    public class GPTClient
    {
        private static GPTClient _instance;
        public static GPTClient Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new GPTClient();
                }
#if UNITY_EDITOR
                _instance.loadSavedConfiguration();
#endif
                return _instance;
            }
        }

        public GPTClient()
        {
            _instance = this;
        }

        public static string[] models = new string[] { "llama3", "starcoder2:7b", "codellama", "gpt-3.5-turbo", "gpt-3.5-turbo-16k", "gpt-3.5-turbo-1106", "gpt-4", "gpt-4-1106-preview" };
        //hashmap model and cost
        public static Dictionary<string, float> modelCosts = new Dictionary<string, float>();
        public static int modelIndex = 0;
        public static bool showSettings = false;

        public string apiKey = "";
        public static string apiEndpoint = "";
        public static string defaultOpenAIURL = "https://api.openai.com/v1/chat/completions";


        public string model = "gpt-3.5-turbo";

        [HideInInspector]
        public static GPTStatus status = GPTStatus.Idle;
        [HideInInspector]
        public string SystemInitPrompt = "You are a professional programmer.";

        public static float temperature = 0.7f;
        public static int maxTokens = 3000;

        public static string defaultSavePath = "Assets/";
        public static bool askForSavePath = true;
        public static int n = 1;

        public static float cost = 0f;


        public string response = "";

        public Action<string, int> OnResponseReceived;
        public Action<string> OnPartResponseReceived;

        public static UnityWebRequest currentRequest;
        string reponseText = "";

        public void SendRequest(string promptSend, int index = 0)
        {
            JSONNode requestBody = JSON.Parse("{}");
            requestBody["model"] = model;
            requestBody["stream"] = true;
            requestBody["messages"] = new JSONArray();

            JSONNode systemInitNode = JSON.Parse("{}");
            systemInitNode["role"] = "system";
            systemInitNode["content"] = SystemInitPrompt;

            requestBody["messages"].Add(systemInitNode);

            JSONNode promptNode = JSON.Parse("{}");
            promptNode["role"] = "user";
            promptNode["content"] = promptSend;

            requestBody["messages"].Add(promptNode);

            requestBody["temperature"] = temperature;

            // if(maxTokens > 0)
            //     requestBody["max_tokens"] = maxTokens;

            requestBody["n"] = n;
            requestBody["stream"] = true;
            // requestBody["stop"] = "";

            string requestBodyString = requestBody.ToString();

            CoroutineHelper.StartCor(SendRequestQ(requestBodyString, index));
        }

        public void SentRequestWithHistory(List<GPTChatMessage> chatMessages)
        {
            model = models[modelIndex];

            JSONNode requestBody = JSON.Parse("{}");
            requestBody["model"] = model;
            requestBody["messages"] = new JSONArray();
            requestBody["options"] = new JSONObject();
            requestBody["options"]["max_tokens"] = 40960;

            foreach (GPTChatMessage chatMessage in chatMessages)
            {
                JSONNode messageNode = JSON.Parse("{}");
                messageNode["role"] = chatMessage.role;
                messageNode["content"] = chatMessage.content;

                if (chatMessage.content != "")
                    requestBody["messages"].Add(messageNode);
            }

            // requestBody["max_tokens"] = maxTokens;

            requestBody["n"] = n;
            requestBody["stream"] = true;

            // requestBody["stop"] = "";

            requestBody["temperature"] = temperature;

            string requestBodyString = requestBody.ToString();

            CoroutineHelper.StartCor(SendRequestQ(requestBodyString));
        }
        List<string> oldLines = new List<string>();
        string oldContent = "";
        private void CheckRequestProgress()
        {
            var handler = (currentRequest.downloadHandler as DownloadHandlerMessageQueue);
            if(handler != null)
            {
                if (!handler.Finished)
                    return;
            }
            else if (currentRequest.downloadHandler.text == null || currentRequest.downloadHandler.text == "")
                return;

            //string currentContent = currentRequest.downloadHandler.text;
            //Debug.Log("currentContent: " + currentContent);
            //List<string> alllines = new List<string>(currentContent.Split('\n'));

            //alllines = alllines.FindAll(s => !string.IsNullOrEmpty(s));

            //if (alllines.Count == 0) return;

            //string newText = "";
            //foreach (var line in alllines)
            //{
            //    string cleanedLine = line.Trim();
            //    cleanedLine = cleanedLine.Replace("\"object\"", "\"objectName\"");
            //    cleanedLine = cleanedLine.Replace("data:", "").Trim();


            //    // Debug.Log("c line: " + cleanedLine);

            //    if (!string.IsNullOrEmpty(cleanedLine) && cleanedLine != "[DONE]")
            //    {
            //        // Debug.Log("line: " + cleanedLine);
            //        JSONNode jsonNode = JSON.Parse(cleanedLine);

            //        GPTResponseStream oaiResponse = GetOAIObject(jsonNode);

            //        if (oaiResponse.choices != null && oaiResponse.choices.Length > 0)
            //        {
            //            newText += oaiResponse.choices[0].delta.content;
            //        }
            //        else
            //        {
            //            newText += "";
            //            // Debug.LogWarning("Something went wrong. Make sure you have entered a valid API key and your account has enough credits.");
            //            // break;
            //        }
            //    }
            //}
            var newText = handler.allText.ToString();
            string newContent = oldContent != "" ? newText.Replace(oldContent, "") : newText;

            reponseText += newContent;
            oldContent = newText;

            OnPartResponseReceived?.Invoke(newContent);

            if (currentRequest.isDone)
            {
                if (currentRequest.result == UnityWebRequest.Result.ConnectionError || (currentRequest.result == UnityWebRequest.Result.ProtocolError))
                {
                    Debug.Log(currentRequest.error);
                    status = GPTStatus.Error;
                }
                else
                {
                    status = GPTStatus.Success;
                }

                OnResponseReceived?.Invoke(reponseText, 0);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.update -= CheckRequestProgress;
#endif
            }
        }
        [Serializable]
        public class ReceiveData
        {
            public string model;
            public string created_at;
            public Message message;
            public bool done;
        }

        [Serializable]
        public class Message
        {
            public string role;
            public string content;
        }

        public class DownloadHandlerMessageQueue : DownloadHandlerScript
        {
            public StringBuilder allText = new StringBuilder();
            private Action<ReceiveData> _onReceive;
            private StringBuilder _textInProcess = new StringBuilder();
            private bool finished = false;
            public bool Finished => finished;

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                var text = Encoding.UTF8.GetString(data, 0, dataLength);
                if (text.Contains('\n'))
                {
                    var lines = text.Trim().Split('\n');
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            OnReceiveOne(line.Trim());
                        }
                    }
                }
                else
                {
                    OnReceiveOne(text.Trim());
                }
                return !finished;
            }

            private void OnReceiveOne(string text)
            {
                //Debug.Log("OnReceiveOne:" + text);
                var receiveData = JsonUtility.FromJson<ReceiveData>(text);
                allText.Append(receiveData.message.content);
                _onReceive?.Invoke(receiveData);
                if (receiveData.done)
                    finished = true;
            }

            internal void RegistReceive(Action<ReceiveData> onReceive)
            {
                this._onReceive = onReceive;
            }
        }

        public IEnumerator SendRequestQ(string requestBodyString, int index = 0)
        {
            // Debug.Log("Sending request to OpenAI API...");
            // Debug.Log("Request body: " + requestBodyString);
#if UNITY_EDITOR
            // if(apiKey == "")
            loadSavedConfiguration();
#endif
            currentRequest = new UnityWebRequest(apiEndpoint, "POST");

            currentRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
            currentRequest.SetRequestHeader("Content-Type", "application/json");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBodyString);
            currentRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            var downloadHandler = new DownloadHandlerMessageQueue();
            currentRequest.downloadHandler = downloadHandler;
            status = GPTStatus.WaitingForResponse;

            downloadHandler.RegistReceive(x =>
            {
                OnPartResponseReceived?.Invoke(x.message.content);
            });


            oldLines.Clear();
            oldContent = "";

            currentRequest.SendWebRequest();

            reponseText = "";

#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += CheckRequestProgress;
#else
            yield return new WaitForSeconds(0.1f);
            while (!currentRequest.isDone)
            {
                CheckRequestProgress();
                yield return new WaitForSeconds(0.1f);
            }  
#endif
            yield return new WaitForSeconds(0.2f);
            CheckRequestProgress();

            yield return null;

        }

        public static void StopGeneration()
        {
            if (currentRequest != null)
            {
                currentRequest.Abort();
                status = GPTStatus.Idle;
            }
        }

        private static GPTResponseStream GetOAIObject(JSONNode jsonNode)
        {
            GPTResponseStream oaiResponse = new GPTResponseStream();
            oaiResponse.id = jsonNode["id"];
            oaiResponse.objectName = jsonNode["object"];
            oaiResponse.created = jsonNode["created"];
            oaiResponse.model = jsonNode["model"];

            if (jsonNode["choices"] == null)
            {
                // Debug.LogError("response: " + jsonNode.ToString());
                return oaiResponse;
            }
            JSONArray jsonChoices = jsonNode["choices"].AsArray;
            oaiResponse.choices = new GPTResponseStream.Choice[jsonChoices.Count];

            for (int i = 0; i < jsonChoices.Count; i++)
            {
                oaiResponse.choices[i] = new GPTResponseStream.Choice();
                oaiResponse.choices[i].index = jsonChoices[i]["index"];

                JSONNode deltaNode = jsonChoices[i]["delta"];
                oaiResponse.choices[i].delta = new GPTResponseStream.Delta();
                oaiResponse.choices[i].delta.role = deltaNode["role"];
                oaiResponse.choices[i].delta.content = deltaNode["content"];
            }

            return oaiResponse;
        }
#if UNITY_EDITOR
        private void loadSavedConfiguration()
        {
            apiKey = UnityEditor.EditorPrefs.GetString("UAISecretKey", "");
            apiEndpoint = UnityEditor.EditorPrefs.GetString("UAIEndpoint", defaultOpenAIURL);
            model = UnityEditor.EditorPrefs.GetString("GPTModel", "gpt-3.5-turbo");
            modelIndex = UnityEditor.EditorPrefs.GetInt("GPTModelIndex", 0);
            temperature = UnityEditor.EditorPrefs.GetFloat("GPTTemperature", 0.7f);
            maxTokens = UnityEditor.EditorPrefs.GetInt("GPTMaxTokens", 3000);
            n = 1;//UnityEditor.EditorPrefs.GetInt("GPTN", 1); 
            defaultSavePath = UnityEditor.EditorPrefs.GetString("GPTDefaultSavePath", "Assets/");
            askForSavePath = UnityEditor.EditorPrefs.GetBool("GPTAskForSavePath", false);
        }
#endif

    }


    public enum GPTStatus
    {
        Idle,
        WaitingForResponse,
        Success,
        Error
    }
}
