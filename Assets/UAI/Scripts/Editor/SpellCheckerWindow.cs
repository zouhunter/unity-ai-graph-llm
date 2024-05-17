using UnityEngine;
using UnityEditor; 

namespace UAI{  
    public class SpellCheckerWindow : EditorWindow
    { 
        // String variable that holds the initialization prompt for GPT
        private string SystemInitPrompt = "You are a reliable spell checker. You can check the spelling of any text and correct it.";

        // String variable that will hold the API response
        private string apiResponse = "";  

        // Vector2 variable that holds the position of the scroll bar
        private Vector2 scrollPos;

        // String variable that holds the input text
        private string input = "CaN YoU pLeAsE cHeCk ThIs sEnTeNcEe fOr mE?"; 

        private GUIStyle style;
        private bool copied = false;

    
        // Define the "Spell Checker" window
        [MenuItem("Tools/AI Assistant/Spell Checker", false, 100)]
        static void Init()
        {
            // Create the window
            SpellCheckerWindow window = (SpellCheckerWindow)EditorWindow.GetWindow(typeof(SpellCheckerWindow), false, "Spell Checker"); 
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 300);
            // Display the window
            window.Show();  
        } 

        // Define the user interface of the window 
        void OnGUI()
        {   
            HelperFunctions.drawSettings();

            if(style == null){
                style = new GUIStyle(EditorStyles.textArea);
                style.wordWrap = true;
            }
            // Add some space
            GUILayout.Space(10);
            // Add a label that prompts the user to paste the content that needs to be spell-checked
            GUILayout.Label("Paste the content that should be spell-checked or improved.", EditorStyles.boldLabel);
            // Add a text area for the user to input the text
            input = EditorGUILayout.TextArea(input, style, GUILayout.Height(100)); 

            // Add some space
            GUILayout.Space(10);  

            // Add a label for the "Spell Checker" button
            GUILayout.Label("Spell Checker", EditorStyles.boldLabel); 
            // Add a button that triggers the spell-checking process when clicked
            if (GUILayout.Button("Check the spelling"))
            { 
                // Send the request to GPT
                sendRequestToGPT("Check the spelling of and correct: \"" + input + "\".");
            }
            
            // Add a label indicating that the system is waiting for a response
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

            // Display the API response when it is not empty
            if(apiResponse != "")
            {
                // Add a label indicating that the response is being displayed
                GUILayout.Label("Response", EditorStyles.boldLabel);
                // Add a scroll view for the response
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

        // Function that sends the request to GPT
        private void sendRequestToGPT(string prompt)
        { 
            // Set the initialization prompt for GPT
            GPTClient.Instance.SystemInitPrompt = SystemInitPrompt;
            // Clear the API response
            apiResponse = "";
                
            // Set the OnResponseReceived function to null
            GPTClient.Instance.OnResponseReceived = null;
                
            // Set the OnResponseReceived function to update the API response variable and repaint the window
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
            // Send the request to GPT
            GPTClient.Instance.SendRequest(prompt); 
        }
    }
    
}   