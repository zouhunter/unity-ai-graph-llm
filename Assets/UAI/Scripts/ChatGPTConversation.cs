using System.Collections.Generic;
using UnityEngine;

namespace UAI{ 
    [System.Serializable]
    public class GPTConversation
    {
        public string name;     
        public string conversation_id;
        public List<GPTChatMessage> chatMessages = new List<GPTChatMessage>();

        public bool showDeletingTipp = true;

        public GPTConversation()
        {
            conversation_id = System.Guid.NewGuid().ToString();
        }
        public GPTConversation(string name, string conversation_id)
        {
            this.name = name;
            this.conversation_id = conversation_id;
        }

        public void AddMessage(GPTChatMessage message)
        {
            chatMessages.Add(message);
        }

        public void AddMessage(string role, string content, float cost)
        {
            chatMessages.Add(new GPTChatMessage(role, content, cost));
        }

        public void saveToPlayerPrefs()
        {
            if(chatMessages.Count == 0) return;

            // string json = JsonUtility.ToJson(currentConversation);
            // PlayerPrefs.SetString(conversation_id, json);

            PlayerPrefs.SetInt(conversation_id + "_msgCount",  chatMessages.Count);
            PlayerPrefs.SetString(conversation_id + "_name", name);
            PlayerPrefs.SetInt(conversation_id + "_showDeletingTipp", showDeletingTipp ? 1 : 0);

            for(int i = 0; i < chatMessages.Count; i++){ 
                PlayerPrefs.SetString(conversation_id + "_role_" + i.ToString(), chatMessages[i].role);
                PlayerPrefs.SetString(conversation_id + "_msg_" + i.ToString(), chatMessages[i].content);
                PlayerPrefs.SetString(conversation_id + "_time_" + i.ToString(), chatMessages[i].time);
                PlayerPrefs.SetFloat(conversation_id + "_cost_" + i.ToString(), chatMessages[i].cost); 
            }
    

            PlayerPrefs.Save(); 
        }
    }

}