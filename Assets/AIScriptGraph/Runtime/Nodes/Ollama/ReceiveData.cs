using System;
using UnityEngine;

namespace AIScripting.Ollama
{
    //{"model":"llama3","created_at":"2024-05-08T09:27:16.4077711Z","message":{"role":"assistant","content":" harmony"},"done":false}
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

}