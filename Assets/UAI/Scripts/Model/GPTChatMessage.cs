
namespace UAI{ 
    [System.Serializable]
    public class GPTChatMessage
    {
        public string role;// user, system or assistant
        public string content;

        //record the time when the message is sent
        public string time;

        public float cost;

        public GPTChatMessage()
        {
            this.role = "";
            this.content = "";
            this.time = "";
            this.cost = 0.0f; 
        }

        public GPTChatMessage(string role, string content, float cost)
        {
            this.role = role;
            this.content = content;
            this.time = System.DateTime.Now.ToString();
            this.cost = cost;
        }

        public GPTChatMessage(string role, string content, string time, float cost)
        {
            this.role = role;
            this.content = content;
            this.time = time;
            this.cost = cost;
        }
    }
}
