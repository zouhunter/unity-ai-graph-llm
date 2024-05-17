using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic; 

namespace UAI{ 
    
    public class ScriptDocWindow: EditorWindow
    {  
        private string SystemInitPrompt = "You are a professional programmer,reply in chinese words,and as detailed as possible.";
        private string apiResponse = ""; 
        private MonoScript scriptToCheck;
        private Vector2 ScrollPos;
        // private int maxTokens = 3000;
        private GUIStyle style;

        private bool useFile = true;
        private string codeToCheck = "";

        private string errorMessageToFix = "";
        Vector2 scrollPos = Vector2.zero;
        Vector2 scrollPos2 = Vector2.zero;
        Vector2 scrollPosBug = Vector2.zero;

        public bool forceUpdate = false;

        
        [MenuItem("Tools/AI Assistant/Script Doctor", false, 4)]
        static void Init()
        {
            ScriptDocWindow window = (ScriptDocWindow)EditorWindow.GetWindow(typeof(ScriptDocWindow), false, "Script Doctor");
            //set the window size to 1100x600
            window.minSize = new Vector2(1100, 600);
            window.Show();  
        }  

        void OnGUI()
        {    
            if(style == null){
                style = new GUIStyle(EditorStyles.textArea);
                style.wordWrap = true;
            }
            HelperFunctions.drawSettings();


            GUILayout.BeginHorizontal();
                GUI.enabled = !useFile;
                if (GUILayout.Button("Check whole script")){
                    useFile = true;
                } 
                GUI.enabled = useFile;
                if (GUILayout.Button("Check code")){ 
                    useFile = false;
                }
                GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
 
            float windowWidth = (position.width - 200)/2;
            
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true)); 
                GUILayout.BeginVertical(GUILayout.Width(200), GUILayout.MaxWidth(200));
                    if(scriptToCheck == null && useFile ){  
                        GUI.enabled = false;
                    }else if(!useFile && codeToCheck.Trim() == ""){ 
                        GUILayout.Label("Please enter the code you want to check first");
                        GUI.enabled = false;
                    }
                    drawCheckButtons();  
                    drawImprovementButtons();
                    drawCommentButtons();
                    drawFixButtons();
                    GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.MaxWidth(windowWidth));
                    drawSettingsAndInput();
                GUILayout.EndHorizontal(); 
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.MaxWidth(windowWidth));
                    drawAPIResponse();
                GUILayout.EndHorizontal();
            GUILayout.EndHorizontal(); 

        }

        private void drawSettingsAndInput(){

            if(useFile){
                GUILayout.Label("Select a script:");
                scriptToCheck = (MonoScript)EditorGUILayout.ObjectField(scriptToCheck, typeof(MonoScript), false);
        
                if (scriptToCheck == null){  
                    GUILayout.Label("Please select a script first or drag and drop it into the field above.", EditorStyles.boldLabel);
                    apiResponse = "";
                } else{
                    GUILayout.Space(10);
                    GUILayout.Label("Script to check:");
                    scrollPos2 = GUILayout.BeginScrollView(scrollPos2,  GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        GUILayout.TextArea(scriptToCheck.text, style, GUILayout.ExpandHeight(true));
                    GUILayout.EndScrollView();

                }
            }else{ 
                GUILayout.Label("Enter the code:"); 
                
                scrollPos = GUILayout.BeginScrollView(scrollPos,  GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    codeToCheck = EditorGUILayout.TextArea(codeToCheck, style, GUILayout.ExpandHeight(true));
                GUILayout.EndScrollView();

                // codeToCheck = EditorGUILayout.TextArea(codeToCheck, style);
            }
        }

        private void drawAPIResponse(){
            
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
                GUILayout.Label("Response:");
                if(GUILayout.Button("Reset")){
                    apiResponse = "";
                }

                apiResponse = apiResponse.Replace("\\n", "\n"); 

                bool val = EditorStyles.textField.wordWrap;
                EditorStyles.textField.wordWrap = true;

                ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, GUILayout.ExpandHeight(true));
                    GUILayout.Space(20); 
                        
                    string[] content = apiResponse.Split(new string[] { "```" }, System.StringSplitOptions.None); 
                    for(int i = 0; i < content.Length; i++){
                        if(content[i].Trim() == "") continue;

                        EditorGUILayout.BeginHorizontal(); 

                            EditorGUILayout.TextArea(content[i].Trim(),style, GUILayout.ExpandWidth(true));
                            
                            EditorGUILayout.BeginVertical(); 
                                if (GUILayout.Button("Copy", GUILayout.Width(60))) {
                                    TextEditor te = new TextEditor();
                                    te.text = content[i].Trim();
                                    te.SelectAll();
                                    te.Copy();
                                }
                                if (GUILayout.Button("Save file", GUILayout.Width(60)))
                                { 
                                    if(useFile){
                                        if (EditorUtility.DisplayDialog("Save to file", "Do you really want to overwrite the selected script(name: " + scriptToCheck.name + ")?", "Yes", "No"))
                                        { 
                                            string scriptFilePath = AssetDatabase.GetAssetPath(scriptToCheck);
                                            if (Path.GetExtension(scriptFilePath) != ".cs")
                                            {
                                                Debug.LogError("Selected asset is not a C# script.");
                                                return;
                                            }
                                            File.WriteAllText(scriptFilePath, content[i].Trim());
                                            AssetDatabase.Refresh();

                                            EditorUtility.DisplayDialog("Success", "The script was successfully saved.", "Ok");
                                        // ask if create new script file
                                        }else if( EditorUtility.DisplayDialog("Save to new file", "Do you want to save the script to a new file (\"" +scriptToCheck.name + "_new.cs\") ?", "Yes", "No")){
                                        

                                            string scriptName = scriptToCheck.name + "_new";
                                            string scriptPath = GPTClient.defaultSavePath + "/" + scriptName + ".cs";
                                            if(GPTClient.askForSavePath){
                                                scriptPath = EditorUtility.SaveFilePanel("Save script", GPTClient.defaultSavePath, scriptName, "cs");
                                            }

                                            if (scriptPath.Length != 0)
                                            { 
                                                content[i] = content[i].Replace("class " + scriptToCheck.name, "class " + scriptName);
                                                File.WriteAllText(scriptPath, content[i].Trim());
                                                AssetDatabase.Refresh();

                                                EditorUtility.DisplayDialog("Script created", "The script " + scriptName + " was created.", "Ok");
                                                Selection.activeObject = AssetDatabase.LoadAssetAtPath(scriptPath, typeof(MonoScript));
                                            }
                                        }
                                    }else{
                                        // string path = EditorUtility.SaveFilePanel("Save file", GPTClient.defaultSavePath, "script", "cs");
                                        // if (path.Length != 0)
                                        // {
                                        //     File.WriteAllText(path, content[i].Trim());
                                        //     AssetDatabase.Refresh();
                                        // }

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
                            EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                GUILayout.Space(20);
                EditorGUILayout.EndScrollView();
                
                //HelperFunctions.drawCostLabel();

                EditorStyles.textField.wordWrap = val; 
            } 
        }


        private void drawCheckButtons(){
            GUILayout.Label("Check", EditorStyles.boldLabel);
            if (GUILayout.Button("Check the script for bugs")){
                sendRequestToGPT("Check the following script for bugs:´SCRIPT_TEXT´"); 
            } 
            if (GUILayout.Button("Check the script for vulnerabilities")){ 
                sendRequestToGPT("Check the following script for vulnerabilities:´SCRIPT_TEXT´"); 
            }
            if (GUILayout.Button("Check the script for performance issues")){ 
                sendRequestToGPT("Check the following script for performance issues:´SCRIPT_TEXT´"); 
            }
            if (GUILayout.Button("Check the script for readability issues")){ 
                sendRequestToGPT("Check the following script for readability issues:´SCRIPT_TEXT´"); 
            }
            if (GUILayout.Button("Check the script for structure issues")){ 
                sendRequestToGPT("Check the following script for structure issues:´SCRIPT_TEXT´"); 
            }
        }
        private void drawImprovementButtons()
        {
            GUILayout.Label("Improvements", EditorStyles.boldLabel);
            if (GUILayout.Button("Improve script performance")){ 
                sendRequestToGPT("Improve the following script for performance and add comments with the improvements:´SCRIPT_TEXT´"); 
            }
            if (GUILayout.Button("Improve script readability")){ 
                sendRequestToGPT("Improve the following script for readability:´SCRIPT_TEXT´"); 
            }
            if (GUILayout.Button("Improve script structure")){ 
                sendRequestToGPT("Improve the following script for structure:´SCRIPT_TEXT´"); 
            }

            if (GUILayout.Button("Give me a better solution for the script")){ 
                sendRequestToGPT("Give me a better solution for the following script:´SCRIPT_TEXT´"); 
            }
            if(GUILayout.Button("Give me a suggestion for the script")){ 
                sendRequestToGPT("Give me a suggestion for the following script:´SCRIPT_TEXT´"); 
            }  
        }
        private void drawFixButtons(){
            GUILayout.Label("Fix", EditorStyles.boldLabel);

            GUILayout.Label("Error Message", EditorStyles.boldLabel); 
            scrollPosBug = GUILayout.BeginScrollView(scrollPosBug, GUILayout.MaxHeight(50), GUILayout.MaxWidth(230));
                errorMessageToFix = GUILayout.TextArea(errorMessageToFix,  GUILayout.MaxWidth(230));
            GUILayout.EndScrollView();
            if (GUILayout.Button("Fix this error in the code")){ 

                sendRequestToGPT("Fix this error:`"+errorMessageToFix+"` in the following code:´SCRIPT_TEXT´"); 
            } 
            if(GUILayout.Button("Explain the error message")){ 
                sendRequestToGPT("Explain the following error: ´"+errorMessageToFix+"´ in this code:´SCRIPT_TEXT´");
            }
            if(GUILayout.Button("Explain why this error happens in the code")){ 
                sendRequestToGPT("Explain why this error happens: ´"+errorMessageToFix+"´ in this code:´SCRIPT_TEXT´");
            }
        }
        private void drawCommentButtons()
        {
            GUILayout.Label("Comments", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();  
                if (GUILayout.Button("Add comments to the script")){  
                    sendRequestToGPT("Add a comment to each line (after it) of the following script:´SCRIPT_TEXT´"); 
                } 
                
                // if (GUILayout.Button(new GUIContent("Partial", "Partial for longer scripts."))){  
                //     sendPartialRequestToGPT("Add comments to this piece of script and don't add code!:´SCRIPT_TEXT´"); 
                // }
            GUILayout.EndHorizontal();
            //remove
            if (GUILayout.Button("Remove comments from the script")){ 
                sendRequestToGPT("Remove comments from the following script:´SCRIPT_TEXT´"); 
            }
            if (GUILayout.Button("Add a comment block before each function")){ 
                sendRequestToGPT("Add a comment block before each function that explains what they do. Give me the whole script:´SCRIPT_TEXT´"); 
            }
            if (GUILayout.Button("Add a comment block before each class")){ 
                sendRequestToGPT("Add a comment block before each class that explains what they do. Give me the whole script:´SCRIPT_TEXT´"); 
            }
            if (GUILayout.Button("Add a comment block before each variable")){ 
                sendRequestToGPT("Add a comment block before each variable that explains what they do. Give me the whole script:´SCRIPT_TEXT´"); 
            }
        }

        private void sendRequestToGPT(string prompt)
        {
            if (useFile && scriptToCheck == null){  
                Debug.Log("Please select a script first or drag and drop it into the field above.");
                return;
            }
            apiResponse = "";

            GPTClient.Instance.SystemInitPrompt = SystemInitPrompt;

            string scriptText = "";

            if(useFile){
                string scriptFilePath = AssetDatabase.GetAssetPath(scriptToCheck);
                if (Path.GetExtension(scriptFilePath) != ".cs")
                {
                    Debug.LogError("Selected asset is not a C# script.");
                    return;
                } 
                scriptText = File.ReadAllText(scriptFilePath);
            }else{
                scriptText = codeToCheck;
            }
    
    
            prompt = prompt.Replace("SCRIPT_TEXT", scriptText);
            prompt += "\n-Use ``` to separate the code from non-code.";

            // GPTClient.maxTokens = maxTokens;
    
            GPTClient.Instance.OnResponseReceived = null;
            GPTClient.Instance.OnPartResponseReceived = null;

            GPTClient.Instance.OnResponseReceived += (response, index) =>
            {
                apiResponse = response; 
                apiResponse = HelperFunctions.RemoveScriptTagFromOpenAIResponse(apiResponse);
                
                Repaint();
            };
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
