using UnityEngine;
using UnityEditor;
namespace UAI{ 
    public class QuestGeneratorWindow : EditorWindow
    { 
        // String variable that holds the initialization prompt for GPT
        private string SystemInitPrompt = "You are a creative writer and game designer, who can generate unique quests for your game.";

        private string apiResponse = "";  
        private Vector2 scrollPos;
        private string gameWorldDescription = "My game world is a fantasy world with magic, dragons, elves, dwarves, and orcs.";
        private string instructions = "Come up with an unique quest for my game.";  

        private GUIStyle style;
        private bool copied = false;
 

        /* Creates a new editor window for generating random names */
        [MenuItem("Tools/AI Assistant/Quest Generator", false, 20)]
        static void Init()
        {
            QuestGeneratorWindow window = GetWindow<QuestGeneratorWindow>("Quest Generator");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 300);
            window.Show();  

            window.gameWorldDescription = PlayerPrefs.GetString("gameWorldDescription_uAI", window.gameWorldDescription);
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
            GUILayout.Label("Tell me about your game world", EditorStyles.boldLabel);

            // Takes input from the user about the game world
            gameWorldDescription = EditorGUILayout.TextArea(gameWorldDescription, style, GUILayout.Height(100)); 

            GUILayout.Space(10);  

            if (GUILayout.Button("Generate Quest", GUILayout.Height(40)) ){ 
                SendRequestToGPT(gameWorldDescription + " - " + instructions + "- Generate the Quest: "); 
            } 

            // Shows waiting message while waiting for response from the OpenAI GPT-3 model
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
            //save game world description to player prefs
            PlayerPrefs.SetString("gameWorldDescription_uAI", gameWorldDescription);

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