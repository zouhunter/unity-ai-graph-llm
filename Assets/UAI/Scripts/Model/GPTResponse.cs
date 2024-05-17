namespace UAI{ 
    [System.Serializable] 
    public class GPTResponse
    {
        [System.Serializable]
        public class Usage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
        [System.Serializable]
        public class Message
        {
            public string role;
            public string content;
        }

        [System.Serializable]
        public class Choice
        {
            public int index;
            public string finish_reason;
            public Message message;

        }

        public string id;
        public int created;
        public string model; 
        public Usage usage;
        public Choice[] choices;
    }
}