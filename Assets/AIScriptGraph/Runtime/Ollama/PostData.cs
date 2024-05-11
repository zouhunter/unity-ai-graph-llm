using System;
using System.Collections.Generic;


namespace AIScripting.Ollama
{
    [Serializable]
    public class PostData
    {
        public string model;
        public List<SendData> messages;
        public bool stream;//Á÷Ê½
    }
}