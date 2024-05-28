using System;
using System.Collections.Generic;


namespace AIScripting
{
    [Serializable]
    public class PostData
    {
        public string model;
        public List<SendData> messages;
        public bool stream;//流式
    }
}
