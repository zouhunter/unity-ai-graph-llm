using UnityEngine;
using UnityEditor; 

namespace UAI{ 

    [System.Serializable]
    public class TextRefinerObject{
        public string input =  "Hi, what are you doing here?";
        public string instructions = "Make it sound like a pirate saying it."; 
        
        public string prevtextinput = "";
        public string prevtextinstructions = "";
    }

    public class TextRefinerWindow : EditorWindow
    { 
        // This is the prompt that will be sent to GPT for initialization, so the AI knows what is wanted
        private string SystemInitPrompt = "You are a true master of words. You can check the spelling of any text and correct it. You can also improve the text by adding more details or making it sound more interesting.";

        public TextRefinerObject textRefinerObject = new TextRefinerObject(); 

        private string apiResponse = "";  
        private Vector2 scrollPos;

        private GUIStyle style;
        private bool copied = false;
    

        [MenuItem("Tools/AI Assistant/Text Refiner", false, 100)]
        static void Init()
        {
            TextRefinerWindow window = (TextRefinerWindow)EditorWindow.GetWindow(typeof(TextRefinerWindow), false, "Text Refiner"); 
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 400);
            window.Show();  
        } 

        void OnGUI()
        {    
            HelperFunctions.drawSettings();

            if(style == null){
                style = new GUIStyle(EditorStyles.textArea);
                style.wordWrap = true;
            }
            GUILayout.Space(10);
            GUILayout.Label("Paste the content that you want to improve.", EditorStyles.boldLabel);         
            
            Undo.RecordObject(this, "Text Refiner Change");

            
            EditorGUI.BeginChangeCheck();
            textRefinerObject.input = EditorGUILayout.TextArea(textRefinerObject.input, style, GUILayout.Height(100)); 

            GUILayout.Space(10);  
    
            GUILayout.Label("textRefinerObject.instructions", EditorStyles.boldLabel); 
            textRefinerObject.instructions = EditorGUILayout.TextArea(textRefinerObject.instructions, style, GUILayout.Height(100));
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }

            if (GUILayout.Button("Improve the text")){  
                sendRequestToGPT(textRefinerObject.instructions + ". The text: \"" + textRefinerObject.input + "\"."); 
            } 
            
            if(GPTClient.status == GPTStatus.WaitingForResponse)
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Label("Generating response...");
                    if (GUILayout.Button("Stop Generation"))
                    {
                        GPTClient.StopGeneration();
                    }  
                GUILayout.EndHorizontal(); 
            }

            if(apiResponse != "")
            {
                GUILayout.Label("Response", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                EditorGUILayout.TextArea(apiResponse, style, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
                
                //HelperFunctions.drawCostLabel();
                
                if(copied){
                    GUILayout.Label("Copied to clipboard");
                }
                if(GUILayout.Button("Copy to clipboard")){
                    EditorGUIUtility.systemCopyBuffer = apiResponse;
                    copied = true;
                }
            } 
        }

        private void sendRequestToGPT(string prompt)
        { 
            GPTClient.Instance.SystemInitPrompt = SystemInitPrompt;
            apiResponse = "";
                
            GPTClient.Instance.OnResponseReceived = null;
                
            GPTClient.Instance.OnResponseReceived += (response, index) =>
            {
                apiResponse = response;
                copied = false;
                Repaint();
            };

            GPTClient.Instance.OnPartResponseReceived = null;
            GPTClient.Instance.OnPartResponseReceived += (response) =>
            {
                apiResponse += response; 
                copied = false;
                Repaint();
            };
            
            GPTClient.Instance.SendRequest(prompt); 
        }
    }
}