using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AIToolMenus
{
    [MenuItem("Tools/AI-Assistant-Window", false, 100)]
    public static void OpenAIAssistWindow()
    {
        AIAssistWindow window = EditorWindow.GetWindow<AIAssistWindow>();
        window.titleContent = new GUIContent("AI-Assistant");
        window.Show();
    }

    [MenuItem("Tools/AI-Assistant/CodeAnalysis", false, 100)]
    public static void CodeAnalysis()
    {
        //执行代码分析流程
    }
}

public class AIAssistWindow : EditorWindow
{
    private string response = "";
    private Vector2 scrollPos;
    private string input = "locate art folder";
    private GUIStyle style;
    private bool copied = false;
    private bool waitting;

    // Define the user interface of the window 
    void OnGUI()
    {
        //HelperFunctions.drawSettings();

        if (style == null)
        {
            style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
        }
        // Add some space
        GUILayout.Space(10);
        // Add a label that prompts the user to paste the content that needs to be spell-checked
        GUILayout.Label("i can help you to operate unity engine.", EditorStyles.boldLabel);
        // Add a text area for the user to input the text
        input = EditorGUILayout.TextArea(input, style, GUILayout.Height(100));

        // Add some space
        GUILayout.Space(10);

        // Add a label for the "Spell Checker" button
        GUILayout.Label("Auto Analysis And Execute!", EditorStyles.boldLabel);
        // Add a button that triggers the spell-checking process when clicked
        if (GUILayout.Button("Process"))
        {
            // Send the request to GPT
            ProcessPrompt(input);
        }

        // Add a label indicating that the system is waiting for a response
        if (waitting)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Waiting process...");
            if (GUILayout.Button("Stop"))
            {
                waitting = false;
                CancelPrompt();
            }
            GUILayout.EndHorizontal();
        }

        // Display the API response when it is not empty
        if (response != "")
        {
            GUILayout.Label("Response", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
            EditorGUILayout.TextArea(response, style, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            if (copied)
            {
                GUILayout.Label("Copied to clipboard");
            }
            if (GUILayout.Button("Copy to clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = response;
                copied = true;
            }
        }
    }

    private void ProcessPrompt(string prompt)
    {
    }

    private void CancelPrompt()
    {

    }
}
