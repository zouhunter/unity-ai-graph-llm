using UnityEngine;
using UnityEngine.UI;
using TMPro; 

namespace UAI{
    public enum NPC_Mood
    {
        Happy,
        Sad,
        Angry,
        Neutral
    }
    /* 
        This is a simple example of how to use the GPTClient to create a chatbot NPC.
        This script is attached to the NPC GameObject and it will send the prompt to the GPTClient
        and then it will display the response in the UI.

        You can use this script as a base for your own chatbot NPC.

        IMPORTANT

        But be aware that including your API key as a string in your code is not secure. 
        You should use a secure way to store your API key. 
    */
    public class GPT_NPC : MonoBehaviour
    {
        [Header("API Settings")]
        public string APIKey = "";

        [Header("NPC Settings")]
        [Tooltip("The prompt that will configure the chatbot. You can use [mood] which will be replaced with the mood of the NPC") ]
        [TextArea(3, 10)]
        public string SystemInitPrompt = "You are a [mood] NPC in a game. You are talking to a player and you have to help him."; 

        public string npcName = "NPC";
        public NPC_Mood mood = NPC_Mood.Neutral;

        //UI Elements
        [Header("UI Elements")] 
        public TMP_InputField inpPrompt;
        public Button btnSend;
        public GameObject chatMessagePrefab;
        public Transform chatMessageContainer; 

        GPTConversation conversation; 

        public void clearConversation()
        {
            if(conversation != null)
                conversation.chatMessages.Clear();

            updateChat();
        }

        private void Start() {
            if(APIKey == "") 
                Debug.LogError("Please enter your API key in the GPT_NPC script", this);

            GPTClient.Instance.apiKey = APIKey;

            conversation = new GPTConversation();
        }
        
        private void OnEnable() {
            SystemInitPrompt = SystemInitPrompt.Replace("[mood]", mood.ToString().ToLower());

            //remove all listeners
            btnSend.onClick.RemoveAllListeners();

            btnSend.onClick.AddListener(() => sendRequestToGPT(inpPrompt.text.Trim()));
        }

        public void updateChat()
        {
            foreach (Transform child in chatMessageContainer)
            {
                Destroy(child.gameObject);
            }

            if(conversation == null)
                return;

            foreach (GPTChatMessage message in conversation.chatMessages)
            {
                if(message.role == "system" && message.content == SystemInitPrompt)
                    continue;

                GameObject chatMessage = Instantiate(chatMessagePrefab, chatMessageContainer);
                UIMsgElement uim = chatMessage.GetComponent<UIMsgElement>();
                string nameToUse = message.role == "user" ? "you" : npcName;

                uim.updateUI(nameToUse, message.content, message.time); 
            }

            //scroll to bottom
            Canvas.ForceUpdateCanvases();
            chatMessageContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        } 

        private void sendRequestToGPT(string prompt)
        {   
            GPTClient.Instance.SystemInitPrompt = SystemInitPrompt;

            string currentTime = System.DateTime.Now.ToString("HH:mm:ss");

            //Add system prompt if there are no messages yet
            if(conversation.chatMessages.Count == 0){
                conversation.chatMessages.Add(new GPTChatMessage(){role = "system", content = SystemInitPrompt, time = currentTime}); 
            }

            conversation.chatMessages.Add(new GPTChatMessage(){role = "user", content = prompt, time = currentTime});

            GPTChatMessage answer = new GPTChatMessage(){role = "assistant", content = "", time = currentTime};
            conversation.chatMessages.Add(answer);
            //remove all listeners
            GPTClient.Instance.OnResponseReceived = null;  
            GPTClient.Instance.OnResponseReceived += (response, index) =>
            { 
                answer.content = response;
                updateChat();
            };
            

            GPTClient.Instance.OnPartResponseReceived = null;
            GPTClient.Instance.OnPartResponseReceived += (response) =>
            {  
                answer.content += response;
                updateChat();
            };

            GPTClient.Instance.SentRequestWithHistory(conversation.chatMessages); 

            inpPrompt.text = "";
            updateChat();
        }
    }
}
