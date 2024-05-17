using UnityEditor;
using UnityEngine;

namespace UAI{ 
    public class HelpWindow : EditorWindow
    { 
        private static string _secretKey;

        [MenuItem("Tools/AI Assistant/Help", false, 999)]
        public static void ShowWindow()
        {
            HelpWindow window = GetWindow<HelpWindow>("Help"); 
            window.minSize = new Vector2(500, 150);
        }
    
        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("If you need help, you can read the documentation or join the Discord Server.", EditorStyles.boldLabel); 
            GUILayout.Space(10);
            GUILayout.Label("Differences between the models:", EditorStyles.boldLabel);
            GUILayout.Label("GPT-3.5 Turbo:", EditorStyles.boldLabel);
            GUILayout.Label("\tThe fastest and cheapest model. 4.000 Tokens possible."); 

            GUILayout.Label("GPT-3.5 Turbo 16k:", EditorStyles.boldLabel);
            GUILayout.Label("\tThe 2nd fastest and 2nd cheapest model, but also more capable because \n\tof the increased context length. 16.000 Tokens possible.");
            
            GUILayout.Label("GPT-4:", EditorStyles.boldLabel);
            GUILayout.Label("\tMore expensive model, but also more capable. 8.000 Tokens possible.");

            GUILayout.Label("GPT-4 32k:", EditorStyles.boldLabel);
            GUILayout.Label("\tThe most expensive model, but also the most capable. 32.000 Tokens possible.");

            GUILayout.Space(5);

            GUILayout.Label("Cost per 1000 Tokens:", EditorStyles.boldLabel);
            GUILayout.Label("Model \t\t Prompt Tokens \t Completion Tokens");
            GUILayout.Label("GPT-3.5 Turbo \t 0.0015$ \t 0.002$");
            GUILayout.Label("GPT-3.5 T 16k \t 0.002$ \t 0.004$");
            GUILayout.Label("GPT-4 \t\t 0.03$ \t\t 0.06$");
            GUILayout.Label("GPT-4 32k \t 0.06$ \t\t 0.12$");

            if (GUILayout.Button("See official price list"))
            {
                Application.OpenURL("https://openai.com/pricing");
            } 

            GUILayout.Space(10);
            GUILayout.Label("Useful links", EditorStyles.boldLabel); 

            if (GUILayout.Button("Tokenizer to check the token count")) Application.OpenURL("https://platform.openai.com/tokenizer"); 
            if (GUILayout.Button("Check your current usage")) Application.OpenURL("https://platform.openai.com/account/usage"); 
            if (GUILayout.Button("API Keys")) Application.OpenURL("https://platform.openai.com/account/api-keys"); 
            if (GUILayout.Button("GPT 4 Waitinglist")) Application.OpenURL("https://openai.com/waitlist/gpt-4-api"); 
            

            GUILayout.Space(10);
            GUILayout.Label("Help", EditorStyles.boldLabel); 

            if (GUILayout.Button("Open Documentation")) Application.OpenURL("https://asset-realm.com/uAI-Manual-Documentation.pdf");   


            if (GUILayout.Button("Open Asset Store Page"))
            {
                Application.OpenURL("https://u3d.as/31V7");
            }


            if (GUILayout.Button("Open Discord"))
            {
                Application.OpenURL("https://discord.gg/znYMdMJR5w");
            } 
            // if (GUILayout.Button("Open YouTube"))
            // {
            //     Application.OpenURL("");
            // }
        }
    }
}