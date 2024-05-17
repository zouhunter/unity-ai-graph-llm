using UnityEngine;
using UnityEditor;
namespace UAI{ 
    public class NameGeneratorWindow : EditorWindow
    { 
        // String variable that holds the initialization prompt for GPT
        private string SystemInitPrompt = "You are a creative writer, who can generate random names for characters, cities, and other things.";

        private string apiResponse = "";  
        private Vector2 scrollPos;
        private string gameWorldDescription = "My game world is a fantasy world with magic, dragons, elves, dwarves, and orcs.";
        private string extraCharacterInfo = "The character is an elf.";
        private string extraCityInfo = "It is a city where elves live.";
        private string extraCustomInfo = "I am searching for a name for my pet.";
        private int numberOfNames = 1;

        /* Creates a new editor window for generating random names */
        [MenuItem("Tools/AI Assistant/Name Generator", false, 20)]
        static void Init()
        {
            NameGeneratorWindow window = GetWindow<NameGeneratorWindow>("Name Generator");
            window.minSize = new Vector2(600, 400);
            window.Show();

            //load gameWorldDescription from PlayerPrefs
            window.gameWorldDescription = PlayerPrefs.GetString("gameWorldDescription_uAI", window.gameWorldDescription);  
        }  


    
        /* Draws the GUI for the editor window */
        void OnGUI()
        {    
            HelperFunctions.drawSettings();
            

            GUILayout.Space(10);
            GUILayout.Label("Tell me about your game world.", EditorStyles.boldLabel);

            // Takes input from the user about the game world
            gameWorldDescription = EditorGUILayout.TextArea(gameWorldDescription, GUILayout.Height(100)); 

            GUILayout.Space(10);

            // Takes input from the user about the number of names to generate
            numberOfNames = EditorGUILayout.IntSlider("Number of names:", numberOfNames, 1, 10);
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(); 

                    // Generates random names for characters
                    GUILayout.Label("Character Name Generator", EditorStyles.boldLabel);
                    GUILayout.Label("Extra info for the character:");
                    extraCharacterInfo = EditorGUILayout.TextArea(extraCharacterInfo, GUILayout.Height(100)); 

                    if (GUILayout.Button("Generate random name(s)")){ 
                        SendRequestToGPT(gameWorldDescription + "- Generate "+ numberOfNames +" random names. " + extraCharacterInfo + ""); 
                    }

                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                    
                    // Generates random names for cities
                    GUILayout.Label("City Name Generator", EditorStyles.boldLabel); 
                    GUILayout.Label("Extra info for the city:");
                    extraCityInfo = EditorGUILayout.TextArea(extraCityInfo, GUILayout.Height(100));
                    

                    if (GUILayout.Button("Generate random city name(s)")){ 
                        SendRequestToGPT(gameWorldDescription + "- Generate "+ numberOfNames +" random city names. " + extraCityInfo); 
                    } 
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                    
                    // Generates custom names
                    GUILayout.Label("Custom Name Generator", EditorStyles.boldLabel); 
                    GUILayout.Label("Extra info for your custom name:");
                    extraCustomInfo = EditorGUILayout.TextArea(extraCustomInfo, GUILayout.Height(100));
                    

                    if (GUILayout.Button("Generate random custom name(s)")){ 
                        SendRequestToGPT(gameWorldDescription + " - " + extraCustomInfo + "- Generate "+ numberOfNames +" random names. "); 
                    } 
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

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

            // Displays the generated names
            if (apiResponse != "")
            {
                GUILayout.Label("Response", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
                EditorGUILayout.TextArea(apiResponse, GUILayout.Height(200));
                EditorGUILayout.EndScrollView();

                //HelperFunctions.drawCostLabel();
            }
        }

        /* Sends a request to GPT to generate names */
        private void SendRequestToGPT(string prompt)
        { 
            apiResponse = ""; 

            // save gameWorldDescription to PlayerPrefs
            PlayerPrefs.SetString("gameWorldDescription_uAI", gameWorldDescription);

            GPTClient.Instance.SystemInitPrompt = SystemInitPrompt;

            GPTClient.Instance.OnResponseReceived = null;
            GPTClient.Instance.OnResponseReceived += OnAPIResponseReceived;

            
            GPTClient.Instance.OnPartResponseReceived = null;
            GPTClient.Instance.OnPartResponseReceived += (response) =>
            {
                apiResponse += response;  
                
                Repaint();
            };

            GPTClient.Instance.SendRequest(prompt); 
        }
        
        /* Called when the response from GPT is received */
        private void OnAPIResponseReceived(string response, int index)
        {
            apiResponse = response;
            Repaint();
        }
    }
}
