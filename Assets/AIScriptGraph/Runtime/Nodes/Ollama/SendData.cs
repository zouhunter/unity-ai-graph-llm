using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIScripting.Ollama
{
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

}