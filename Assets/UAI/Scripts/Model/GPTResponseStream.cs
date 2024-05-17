
    
namespace UAI{
    [System.Serializable]
    public class GPTResponseStream
    {
        public string id; 
        public string objectName;
        public long created;
        public string model;
        public Choice[] choices;

        [System.Serializable]
        public class Choice
        {
            public int index;
            public Delta delta;
            public string finish_reason;
        }

        [System.Serializable]
        public class Delta
        {
            public string role;
            public string content;
        }
    }
}