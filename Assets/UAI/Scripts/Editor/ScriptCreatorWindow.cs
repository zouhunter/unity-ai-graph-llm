using UnityEngine;
using UnityEditor;
using System.IO; 

namespace UAI{  
    public enum UAISCMode{ 
        EditExistingCode,
        CreateNewCode
    }

    public class ScriptCreatorWindow : EditorWindow
    { 
        // This is the prompt that will be sent to GPT for initialization, so the AI knows what is wanted
        private string SystemInitPrompt = "You are a professional unity programmer.reply in chinese words";
        private string promptScriptCreation = "I need a script that can make a cube move around in a circle."; 
        private string promptScriptEditing = "Edit this code to let the cubes rotate around themself. Give me the whole script.";
        private string apiResponse = ""; 
        private MonoScript scriptToCheck;
        private Vector2 ScrollPos; 
        private UAISCMode mode = UAISCMode.CreateNewCode;
        private bool useScriptFile = false;
        private string codeText = "";
        // private int maxTokens = 3000;

        private bool onlyCode = false;
        private bool addComments = false;
        private bool addUsingDirectives = true;
        private bool explainHowToUseId = false;

        [MenuItem("Tools/AI Assistant/Script Creator and Editor", false, 0)]
        static void Init()
        {
            ScriptCreatorWindow window = (ScriptCreatorWindow)EditorWindow.GetWindow(typeof(ScriptCreatorWindow), false, "Script Creator and Editor");
             
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 600, 400);
            window.Show();  
        }

        void OnGUI()
        {  
            HelperFunctions.drawSettings();

            GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create new code")){
                    mode = UAISCMode.CreateNewCode;
                } 
                if (GUILayout.Button("Edit existing code")){ 
                    mode = UAISCMode.EditExistingCode;
                }
            GUILayout.EndHorizontal();
                
            GUILayout.Space(10);

            if(mode == UAISCMode.CreateNewCode){
                GUILayout.Label("Create new code", EditorStyles.boldLabel);
                GUILayout.Space(10);

                GUILayout.Label("Please describe what you want the script to do:");
                promptScriptCreation = EditorGUILayout.TextArea(promptScriptCreation, GUILayout.MinHeight(50));

                GUILayout.Space(10);
                //toggle for onlyCode
                onlyCode = EditorGUILayout.Toggle("Only Code", onlyCode);
                if(onlyCode){
                    //label info 
                    GUILayout.Label("The AI will only generate the code. No explanation will be added.");
                }
                addComments = EditorGUILayout.Toggle("Add Comments", addComments);
                addUsingDirectives = EditorGUILayout.Toggle("Add Using Directives", addUsingDirectives);
                explainHowToUseId = EditorGUILayout.Toggle("Explain How To Use it", explainHowToUseId);

                GUILayout.Space(10);



                if (GUILayout.Button("Create Script")){
                    string prompt = promptScriptCreation;
                    if(onlyCode){
                        prompt += " - Only code."; 
                    }else{
                        prompt += " - Explain the code.";
                    }
                    if(addUsingDirectives){
                        prompt += " - Add using directives to the code.";
                    }else{
                        prompt += " - No using directives.";
                    }
                    if(explainHowToUseId){
                        prompt += " - Explain how to use the script.";
                    }
                    if(addComments){
                        prompt += " - Add comments.";
                    }else{
                        prompt += " - No comments in the code!";
                    }

                    sendRequestToGPT(prompt); 
                }

            }else if(mode == UAISCMode.EditExistingCode){
                GUILayout.Label("Edit existing code", EditorStyles.boldLabel);
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Use Script File")){
                        useScriptFile = true;
                    }
                    if (GUILayout.Button("Use Code snippet")){
                        useScriptFile = false;
                    }
                GUILayout.EndHorizontal();

                if(!useScriptFile){
                    GUILayout.Space(10); 
                    codeText = EditorGUILayout.TextArea(codeText, GUILayout.MinHeight(100));
                    GUILayout.Space(10);
                }else{
                    GUILayout.Space(10);
                    GUILayout.Label("Select a script:");
                    scriptToCheck = (MonoScript)EditorGUILayout.ObjectField(scriptToCheck, typeof(MonoScript), false);
        
                    if (scriptToCheck == null){  
                        GUILayout.Label("Please select a script first or drag and drop it into the field above.", EditorStyles.boldLabel);
                        apiResponse = "";
                    } 
                } 
    
                GUILayout.Label("Please describe what you want to change or add to the script:");
                promptScriptEditing = EditorGUILayout.TextArea(promptScriptEditing); 
                
                if (GUILayout.Button("Edit the Script")){ 
                    string scriptText = "";

                    if(useScriptFile){  
                        string scriptFilePath = AssetDatabase.GetAssetPath(scriptToCheck);
                        if (Path.GetExtension(scriptFilePath) != ".cs")
                        {
                            Debug.LogError("Selected asset is not a C# script.");
                            return;
                        } 
                        scriptText = File.ReadAllText(scriptFilePath); 
                    }else{
                        scriptText = codeText;
                    }
    
                    string prompt =  promptScriptEditing + " - To the following script (´SCRIPT_TEXT´).";
                    
                    prompt = prompt.Replace("SCRIPT_TEXT", scriptText);

                    sendRequestToGPT(prompt); 
                } 
            } 
    
            GUILayout.Space(10); 
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

            if (!string.IsNullOrEmpty(apiResponse))
            {
                GUILayout.Label("API Response:");

                bool val = EditorStyles.textField.wordWrap;
                EditorStyles.textField.wordWrap = true;

                ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, GUILayout.ExpandHeight(true));
                    

                    string[] content = apiResponse.Split(new string[] { "```" }, System.StringSplitOptions.None);
                    
                    for(int i = 0; i < content.Length; i++){
                        if(content[i].Trim() == ""){
                            continue;
                        }

                        EditorGUILayout.BeginHorizontal(); 
                            EditorGUILayout.TextArea(content[i].Trim(), GUILayout.ExpandWidth(true));
                            
                            EditorGUILayout.BeginVertical(); 
                                if (GUILayout.Button("Copy", GUILayout.Width(50))) {
                                    TextEditor te = new TextEditor();
                                    te.text = content[i].Trim();
                                    te.SelectAll();
                                    te.Copy();
                                }
                                
                                if(content[i].Contains("public class ")){
                                    if (GUILayout.Button("Create", GUILayout.Width(50))) { 
                                        string scriptName = content[i].Substring(content[i].IndexOf("public class ") + 13);
                                        scriptName = scriptName.Substring(0, scriptName.IndexOf(" ")); 
                                        if(scriptName[scriptName.Length - 1] == ':' || scriptName[scriptName.Length - 1] == '{'){
                                            scriptName = scriptName.Substring(0, scriptName.Length - 1);
                                        } 
                                        if(scriptName.Contains(":")){
                                            scriptName = scriptName.Substring(0, scriptName.IndexOf(":"));
                                        } 
                                        if(scriptName.Contains("{")){
                                            scriptName = scriptName.Substring(0, scriptName.IndexOf("{"));
                                        } 
                                        if(AssetDatabase.FindAssets(scriptName).Length > 0){  
                                            if(EditorUtility.DisplayDialog("Script already exists", "A script with the name " + scriptName + " already exists. Do you want to overwrite it? (Note: Be careful!)", "Yes", "No")){
                                                string scriptPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(scriptName)[0]);
                                                File.WriteAllText(scriptPath, content[i].Trim());
                                                AssetDatabase.Refresh();

                                                EditorUtility.DisplayDialog("Script overwritten", "The script " + scriptName + " was overwritten.", "Ok");
                                                Selection.activeObject = AssetDatabase.LoadAssetAtPath(scriptPath, typeof(MonoScript));
                                            } 
                                        } else{
                                            string scriptPath = GPTClient.defaultSavePath + "/" + scriptName + ".cs";
                                            if(GPTClient.askForSavePath){
                                                scriptPath = EditorUtility.SaveFilePanel("Save script", GPTClient.defaultSavePath, scriptName, "cs");
                                            }

                                            if (scriptPath.Length != 0)
                                            {
                                                File.WriteAllText(scriptPath, content[i].Trim());
                                                AssetDatabase.Refresh();

                                                EditorUtility.DisplayDialog("Script created", "The script " + scriptName + " was created.", "Ok");
                                                Selection.activeObject = AssetDatabase.LoadAssetAtPath(scriptPath, typeof(MonoScript));
                                            }
                                        }

                                    }
                                }
                            EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal(); 
                        GUILayout.Space(5); 
                    } 
                EditorGUILayout.EndScrollView();  
                
                // HelperFunctions.drawCostLabel() ;
            } 
        }

        private void sendRequestToGPT(string prompt)
        {  
            apiResponse = ""; 
            GPTClient.Instance.SystemInitPrompt = SystemInitPrompt;
    
            // GPTClient.maxTokens = maxTokens; 
            GPTClient.Instance.OnResponseReceived = null; 
            
            GPTClient.Instance.OnResponseReceived += (response, index) =>
            {
                apiResponse = response;
                apiResponse = HelperFunctions.RemoveScriptTagFromOpenAIResponse(apiResponse);
                Repaint();
            };

            GPTClient.Instance.OnPartResponseReceived = null;
            GPTClient.Instance.OnPartResponseReceived += (response) =>
            {
                apiResponse += response; 
                apiResponse = HelperFunctions.RemoveScriptTagFromOpenAIResponse(apiResponse);
                
                Repaint();
            };
            GPTClient.Instance.SendRequest(prompt); 
        }
    }
}
