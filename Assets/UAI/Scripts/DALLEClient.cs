using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using System.Net.Http;

namespace UAI
{
    public class DALLEClient
    {
        private static  DALLEClient _instance;

        public static  DALLEClient Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new  DALLEClient();
                }
                return _instance;
            }
        }

        public  DALLEClient()
        {
            _instance = this;
        }
 
        //private string apiUrl = "https://api.openai.com/v1/images/generations";
        private string apiUrl = "http://127.0.0.1/v1/images/generations";
        private string apiUrlImageEdit = "https://api.openai.com/v1/images/edits";

        [HideInInspector]
        public DALLEStatus status = DALLEStatus.Idle;
        [HideInInspector]
        public string SystemInitPrompt = "You are an AI trained to create images from text descriptions.";

        public Action<List<Texture2D>> OnResponseReceived; 

         
        public void SendRequestImageEdit2(Texture2D texture, Texture2D mask, string promptSend, int count, string size, string model)
        {  
            CoroutineHelper.StartCor(sendEditRequest(texture, mask, promptSend, count, size, model));
        }


        IEnumerator sendEditRequest(Texture2D texture, Texture2D mask, string promptSend, int count, string size, string model)
        {
            byte[] imageData = texture.EncodeToPNG();
            byte[] maskData = mask.EncodeToPNG();

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormFileSection("image", imageData, "image.png", "image/png"));
            formData.Add(new MultipartFormFileSection("mask", maskData, "mask.png", "image/png"));
            formData.Add(new MultipartFormDataSection("prompt", promptSend));
            formData.Add(new MultipartFormDataSection("model", model));
            formData.Add(new MultipartFormDataSection("n", count.ToString()));
            formData.Add(new MultipartFormDataSection("size", size.ToString()));

            UnityWebRequest www = UnityWebRequest.Post(apiUrlImageEdit, formData);
            www.SetRequestHeader("Authorization", "Bearer " + GPTClient.Instance.apiKey);

            status = DALLEStatus.WaitingForResponse;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError){
                Debug.Log(www.error); 
                status = DALLEStatus.Error;
            }else{
                Debug.Log("Form upload complete!");
                status = DALLEStatus.Success;

                string responseJson = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                DALLEResponse oaiResponse = JsonUtility.FromJson<DALLEResponse>(responseJson);
                // Debug.Log("DALLE Response:");
                // Debug.Log(responseJson);

                // Assuming DALLEResponse has a List<string> images property containing URLs of generated images
                List<Texture2D> images = new List<Texture2D>();
                foreach (var dat in oaiResponse.data)
                {
                    string imageUrl = dat.url;
                    UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageUrl);
                    yield return imageRequest.SendWebRequest();
                    Texture2D image = DownloadHandlerTexture.GetContent(imageRequest);
                    images.Add(image);
                }

                OnResponseReceived?.Invoke(images);
            }
        }

        public void SendRequest(string promptSend, int count, string size, string model)
        {
            JSONNode requestBody = JSON.Parse("{}"); 

            requestBody["prompt"] = promptSend;
            requestBody["model"] = model;

            requestBody["n"] = count;
            requestBody["size"] = size;
            requestBody["response_format"] = "url";  // Adjust as needed

            string requestBodyString = requestBody.ToString();

            CoroutineHelper.StartCor(SendRequestQ(apiUrl, requestBodyString));
        }

        public IEnumerator SendRequestQ(string url, string requestBodyString)
        { 

            UnityWebRequest request = new UnityWebRequest(url, "POST");

            request.SetRequestHeader("Authorization", "Bearer " + GPTClient.Instance.apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBodyString);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            status = DALLEStatus.WaitingForResponse;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.error);
                status = DALLEStatus.Error;
            }
            else
            {
                status = DALLEStatus.Success;

                string responseJson = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                DALLEResponse oaiResponse = JsonUtility.FromJson<DALLEResponse>(responseJson);
                // Debug.Log("DALLE Response:");
                // Debug.Log(responseJson);

                // Assuming DALLEResponse has a List<string> images property containing URLs of generated images
                List<Texture2D> images = new List<Texture2D>();
                foreach (var dat in oaiResponse.data)
                {
                    string imageUrl = dat.url;
                    UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageUrl);
                    yield return imageRequest.SendWebRequest();
                    Texture2D image = DownloadHandlerTexture.GetContent(imageRequest);
                    images.Add(image);
                }

                OnResponseReceived?.Invoke(images);
            }
        }
    }

    public enum DALLEStatus
    {
        Idle,
        WaitingForResponse,
        Success,
        Error
    }

    [System.Serializable]
    public class DALLEResponse
    {
        public int created;
        public List<ImageData> data;

        [System.Serializable]
        public class ImageData
        {
            public string url;
        }
    }

}
