using UnityEngine;
using UnityEditor; 

namespace UAI{  
    public class TranslatorWindow : EditorWindow
    { 
        // This is the prompt that will be sent to GPT for initialization, so the AI knows what is wanted
        private string SystemInitPrompt = "You are a reliable translator.";

        private string apiResponse = "";  
        private Vector2 ScrollPos;
        private string input = "This is just an example text. You can write anything you want. It will be translated into the language you choose. Try it out";
        private string language = "german"; 

        public string extraCharacteristics = "Informal";

        bool copied = false;
    

        [MenuItem("Tools/AI Assistant/Translator", false, 100)]
        static void Init()
        {
            TranslatorWindow window = (TranslatorWindow)EditorWindow.GetWindow(typeof(TranslatorWindow), false, "Translator"); 
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 300);
            window.Show();  
        } 

        private GUIStyle style;
        void OnGUI()
        {   
            HelperFunctions.drawSettings();

            if(style == null){
                style = new GUIStyle(EditorStyles.textArea);
                style.wordWrap = true;
            }
            GUILayout.Space(10);
            GUILayout.Label("Paste the content that should be translated.", EditorStyles.boldLabel);
            input = EditorGUILayout.TextArea(input, style, GUILayout.Height(100)); 
    
            language = EditorGUILayout.TextField("Language", language);

            extraCharacteristics = EditorGUILayout.TextField("Extra characteristics", extraCharacteristics);

            GUILayout.Space(10);  

            if (GUILayout.Button("Translate")){ 
                if(extraCharacteristics != ""){
                    sendRequestToGPT("Translate the text: ´" + input + "´ into " + language + " with the extra characteristics: " + extraCharacteristics);
                }else{
                    sendRequestToGPT("Translate the text: ´" + input + "´ into " + language);
                } 
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
                ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, style, GUILayout.ExpandHeight(true));
                EditorGUILayout.TextArea(apiResponse,  style, GUILayout.ExpandHeight(true));
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