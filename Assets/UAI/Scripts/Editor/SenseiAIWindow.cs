using UnityEngine;
using UnityEditor;
namespace UAI{ 
    public class SenseiAIWindow : EditorWindow
    { 
        // String variable that holds the initialization prompt for GPT
        private string SystemInitPrompt = "You are a Teacher who knows everything about Unity3D Engine.";

        private string apiResponse = "";  
        private Vector2 scrollPos;
        private Vector2 scrollPosCode;
        private string gameWorldDescription = "I have the following code, but don't know how to use it.";
        private string instructions = "Explain me Step by Step.";  

        bool showExtraFields = false; 

        private GUIStyle style;
        private bool copied = false;

        private string code = "";
        private MonoScript script; 

        /* Creates a new editor window for generating random names */
        [MenuItem("Tools/AI Assistant/Sensei AI", false, 9)]
        static void Init()
        {
            SenseiAIWindow window = GetWindow<SenseiAIWindow>("Sensei AI");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 500);
            window.Show();  
        } 

        /* Draws the GUI for the editor window */
        void OnGUI()
        {   
            HelperFunctions.drawSettings();

            if(style == null){
                style = new GUIStyle(EditorStyles.textArea);
                style.wordWrap = true;
            }

            GUILayout.Space(10);
            GUILayout.Label("What do you want to learn?", EditorStyles.boldLabel);

            // Takes input from the user about the game world
            gameWorldDescription = EditorGUILayout.TextArea(gameWorldDescription, style, GUILayout.Height(60)); 

            GUILayout.Space(10);  
            showExtraFields = EditorGUILayout.Foldout(showExtraFields, "Show more options");
            if (showExtraFields)
            {  
                if(script != null && script.text != code){
                    GUI.enabled = false;
                }
                EditorGUILayout.LabelField("Use this, if you have a question about a code snippet", EditorStyles.boldLabel);
                //scroll view

                scrollPosCode = EditorGUILayout.BeginScrollView(scrollPosCode, GUILayout.Height(100));

                    code = EditorGUILayout.TextArea(code, style, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                GUILayout.Space(10);  
                GUI.enabled = true;
                if(code != "" && script != null && script.text != code){
                    GUI.enabled = false;
                }
                EditorGUILayout.LabelField("Use this, if you have a question about a script", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("But be careful, since the length is limited. If the script is too long, copy a part of the script, and paste it in the text field above at the end.");
                script = (MonoScript)EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
                if(script != null){
                    code = script.text;
                }
                GUI.enabled = true;
            }

            GUILayout.Space(10);  

            if (GUILayout.Button("Teach me!", GUILayout.Height(40)) ){ 
                string prompt = gameWorldDescription + " - " + instructions;

                if(code != ""){
                    prompt += " - The code: " + code;
                }
                SendRequestToGPT(prompt); 
            } 

            // Shows waiting message while waiting for response from OpenAI   
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

            GUILayout.Space(10);  
            // Displays the generated names
            if (apiResponse != "")
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

        /* Sends a request to GPT to generate names */
        private void SendRequestToGPT(string prompt)
        { 
            apiResponse = ""; 
            GPTClient.Instance.SystemInitPrompt = SystemInitPrompt;

            GPTClient.Instance.OnResponseReceived = null;
            GPTClient.Instance.OnResponseReceived += OnAPIResponseReceived;

            GPTClient.Instance.OnPartResponseReceived = null;
            GPTClient.Instance.OnPartResponseReceived += (response) =>
            {
                apiResponse += response; 
                copied = false;
                Repaint();
            };

            GPTClient.Instance.SendRequest(prompt); 
        }
        
        /* Called when the response from GPT is received */
        private void OnAPIResponseReceived(string response, int index)
        {
            apiResponse = response;
            copied = false;
            Repaint();
        }
    }
}