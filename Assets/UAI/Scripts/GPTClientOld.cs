// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using SimpleJSON;
// using UnityEngine.Networking;
// using System;

// namespace UAI{  
//     public class GPTClient 
//     { 
//         private static GPTClient _instance;
//         public static GPTClient Instance {
//             get {   
//                 if (null == _instance)
//                 {
//                     _instance = new GPTClient();
//                 } 
//                 #if UNITY_EDITOR 
//                     _instance.loadSavedConfiguration();
//                 #endif
//                 return _instance;
//             }
//         }
    
//         public GPTClient()
//         { 
//             _instance = this; 
//         }

//         public static string[] models = new string[] { "gpt-3.5-turbo", "gpt-3.5-turbo-16k", "gpt-4", "gpt-4-32k" };
//         //hashmap model and cost
//         public static Dictionary<string, float> modelCosts = new Dictionary<string, float>( );
//         public static int modelIndex = 0;
//         public static bool showSettings = false;

//         public string apiKey = "";  
//         private string apiUrl = "https://api.openai.com/v1/chat/completions";
        
//         public string model = "gpt-3.5-turbo";

//         [HideInInspector]
//         public GPTStatus status = GPTStatus.Idle;
//         [HideInInspector]
//         public string SystemInitPrompt = "You are a professional programmer.";

//         public static float temperature = 0.7f;
//         public static int maxTokens = 3000;

//         public static string defaultSavePath = "Assets/";
//         public static bool askForSavePath = true;
//         public static int n = 1;

//         public static float cost = 0f;


//         public float GPT_cost_gpt3_5_turbo_prompt = 0.0015f;
//         public float GPT_cost_gpt3_5_turbo_completion = 0.002f;

//         public float GPT_cost_gpt3_5_turbo_16k_prompt = 0.003f;
//         public float GPT_cost_gpt3_5_turbo_16k_completion = 0.004f;

//         public float GPT_cost_gpt4_8k_prompt = 0.03f;
//         public float GPT_cost_gpt4_8k_completion = 0.06f;

//         public float GPT_cost_gpt4_32k_prompt = 0.06f;
//         public float GPT_cost_gpt4_32k_completion = 0.12f;


//         public string response = "";

//         //string is the response, int is the index of the request. 
//         //Used for partial requests. Can be set to 0 if not used.
//         public Action<string, int> OnResponseReceived;


//         public void SendRequest(string promptSend, int index = 0)
//         { 
//             JSONNode requestBody = JSON.Parse("{}"); 
//             requestBody["model"] = model; 
//             requestBody["messages"] = new JSONArray();

//             JSONNode systemInitNode = JSON.Parse("{}");
//             systemInitNode["role"] = "system";
//             systemInitNode["content"] = SystemInitPrompt;

//             requestBody["messages"].Add(systemInitNode);

//             JSONNode promptNode = JSON.Parse("{}");
//             promptNode["role"] = "user";
//             promptNode["content"] = promptSend;

//             requestBody["messages"].Add(promptNode);

//             requestBody["temperature"] = temperature;
//             requestBody["max_tokens"] = maxTokens;
//             requestBody["n"] = n;
//             requestBody["stop"] = "";
    
//             string requestBodyString = requestBody.ToString();

//             CoroutineHelper.StartCor(SendRequestQ(requestBodyString , index));
//         }

//         public void SentRequestWithHistory(List<GPTChatMessage> chatMessages)
//         {
//             model = models[modelIndex];

//             JSONNode requestBody = JSON.Parse("{}"); 
//             requestBody["model"] = model; 
//             requestBody["messages"] = new JSONArray();  

//             foreach (GPTChatMessage chatMessage in chatMessages)
//             {
//                 JSONNode messageNode = JSON.Parse("{}");
//                 messageNode["role"] = chatMessage.role;
//                 messageNode["content"] = chatMessage.content;

//                 requestBody["messages"].Add(messageNode);
//             }

//             requestBody["max_tokens"] = maxTokens;

//             requestBody["n"] = n;

//             requestBody["stop"] = "";

//             requestBody["temperature"] = temperature;

//             string requestBodyString = requestBody.ToString();

//             CoroutineHelper.StartCor(SendRequestQ(requestBodyString));
//         }


//         public IEnumerator SendRequestQ(string requestBodyString, int index = 0)
//         {   
//             #if UNITY_EDITOR 
//                 // if(apiKey == "")
//                 loadSavedConfiguration(); 
//             #endif
    
//             // Create a new UnityWebRequest object for the API endpoint
//             UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");

//             // Set the API key as an authorization header
//             request.SetRequestHeader("Authorization", "Bearer " + apiKey);

//             // Set the content type of the request to "application/json"
//             request.SetRequestHeader("Content-Type", "application/json");

//             // Set the request body with the JSON string
//             byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBodyString);
//             request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             request.downloadHandler = new DownloadHandlerBuffer();

//             status = GPTStatus.WaitingForResponse;

//             // Send the API request and wait for a response
//             yield return request.SendWebRequest();

//             // Check for any errors in the response
//             if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError){ 
//                 Debug.Log(request.error);
//                 Debug.Log("Keep in mind, that "+ HelperFunctions.getMaxTokenFromModel(model) +" tokens is the maximum allowed by OpenAI for the model " + model + " .");

//                 response = request.error;

//                 status = GPTStatus.Error;
//             } else {
//                 status = GPTStatus.Success;

//                 // Parse the response JSON and retrieve the generated text
//                 string responseJson = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                
//                 GPTResponse oaiResponse = JsonUtility.FromJson<GPTResponse>(responseJson);

//                 // Debug.Log(responseJson);
//                 string modelused = oaiResponse.model; 
//                 string generatedText = oaiResponse.choices[0].message.content;
//                 cost = 0;

//                 // Debug.Log("Model used: " + modelused);
//                 // Debug.Log("Max tokens: " + maxTokens);
//                 // Debug.Log("Tokens prompt: " + oaiResponse.usage.prompt_tokens);
//                 // Debug.Log("Tokens completion: " + oaiResponse.usage.completion_tokens);
//                 // Debug.Log("Tokens used: " + oaiResponse.usage.total_tokens);

//                 if(modelused == "gpt-3.5-turbo" || modelused == "gpt-3.5-turbo-0301"){
//                     cost = oaiResponse.usage.prompt_tokens * GPT_cost_gpt3_5_turbo_prompt + oaiResponse.usage.completion_tokens * GPT_cost_gpt3_5_turbo_completion;
//                 }else if(modelused == "gpt-3.5-turbo-16k"){
//                     cost = oaiResponse.usage.prompt_tokens * GPT_cost_gpt3_5_turbo_16k_prompt + oaiResponse.usage.completion_tokens * GPT_cost_gpt3_5_turbo_16k_completion;
//                 }else if(modelused == "gpt-4" || modelused == "gpt-4-0314"){
//                     cost = oaiResponse.usage.prompt_tokens * GPT_cost_gpt4_8k_prompt + oaiResponse.usage.completion_tokens * GPT_cost_gpt4_8k_completion;
//                 }else if(modelused == "gpt-4-32k" || modelused == "gpt-4-32k-0314"){
//                     cost = oaiResponse.usage.prompt_tokens * GPT_cost_gpt4_32k_prompt + oaiResponse.usage.completion_tokens * GPT_cost_gpt4_32k_completion;
//                 }
//                 cost /= 1000;

//                 // Debug.Log("Cost for this task: " + cost);

    
//                 if (generatedText.StartsWith("\""))
//                 {
//                     generatedText = generatedText.Substring(1);
//                 } 
//                 if (generatedText.EndsWith("\""))
//                 {
//                     generatedText = generatedText.Substring(0, generatedText.Length - 1);
//                 } 
    
//                 response = generatedText;

//                 OnResponseReceived?.Invoke(response, index); 
//             }
//         }

//     #if UNITY_EDITOR 
//         private void loadSavedConfiguration(){
//             apiKey = UnityEditor.EditorPrefs.GetString("UAISecretKey", "");
//             model = UnityEditor.EditorPrefs.GetString("GPTModel", "gpt-3.5-turbo");
//             modelIndex = UnityEditor.EditorPrefs.GetInt("GPTModelIndex", 0);
//             temperature = UnityEditor.EditorPrefs.GetFloat("GPTTemperature", 0.7f);
//             maxTokens = UnityEditor.EditorPrefs.GetInt("GPTMaxTokens", 3000);
//             n = 1;//UnityEditor.EditorPrefs.GetInt("GPTN", 1); 
//             defaultSavePath = UnityEditor.EditorPrefs.GetString("GPTDefaultSavePath", "Assets/");
//             askForSavePath = UnityEditor.EditorPrefs.GetBool("GPTAskForSavePath", false);
//         }
//     #endif

//     }


//     public enum GPTStatus {
//         Idle,
//         WaitingForResponse, 
//         Success,
//         Error
//     }
// }