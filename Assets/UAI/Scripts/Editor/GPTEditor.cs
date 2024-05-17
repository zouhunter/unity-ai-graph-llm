using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq; 
using System;
using UnityEditor.UIElements;

namespace UAI{  
    public class GPTEditor : EditorWindow
    { 
        // CONSTANTS
        public static int MAX_CONVERSATIONS = 100;
        public static int MAX_MESSAGES = 20;

        // VISUAL ELEMENTS
        // set as default references in the inspector of the script
        public VisualTreeAsset GPTUIE;
        public VisualTreeAsset messageUser;
        public VisualTreeAsset messageAssistant; 
        public VisualTreeAsset lblChatMessageAssistant;
        public VisualTreeAsset veCodeBlock;
        public VisualTreeAsset veConversationButton;

        public Label lblWaitingForResponse;
        public VisualElement veConsiderDeletingOlderMessages;

        // UI ELEMENTS
        private TextField inpPrompt;
        private VisualElement veRightRow;
        private ScrollView swChatContent;
        private ScrollView scrollViewChats;
        private Button btnAbort;
        private Button btnSend;

        // DATA
        private Vector2 ScrollPos;
        private string apiResponse = "";  
        // private string input = "Hi GPT Chatbot! What are you doing?";    
        private string SystemInitPrompt = "The user is using the Unity Engine. You are a reliable Chatbot and a professional programmer. reply in Chinese Words";
        private GPTConversation currentConversation; 
        private List<GPTConversation> conversations = new List<GPTConversation>();
    

        [MenuItem("Tools/AI Assistant/AI Assistant", false, 0)] 
        public static void ShowWindow()
        { 
            GPTEditor window = (GPTEditor)EditorWindow.GetWindow(typeof(GPTEditor), false, "AI Assistant");
            window.minSize = new Vector2(700, 500);
        }

        public void OnEnable()
        {
            VisualElement root = rootVisualElement;
            root.style.flexGrow = 1;
    
            VisualTreeAsset visualTree = GPTUIE;
            VisualElement labelFromUXML = visualTree.CloneTree().Q<VisualElement>("veGPTChatbot");
            root.Add(labelFromUXML);
    
            VisualElement chatContent = root.Q<VisualElement>("veChatContent"); 
            swChatContent = chatContent.Q<ScrollView>("ScrollView");
            scrollViewChats = root.Q<ScrollView>("scrollViewChats");

            //clear scrollview
            // swChatContent.Clear();

    
            inpPrompt = root.Q<TextField>("inpPrompt");
    
            btnSend = root.Q<Button>("btnSendPrompt");
            btnSend.clickable.clicked += () => { 
                sendRequestToGPT(inpPrompt.value);
            };
            // add button behind btnSend
            btnAbort = new Button();
            btnAbort.text = "Stop";
            btnAbort.style.width = btnSend.style.width;
            btnAbort.style.height = btnSend.style.height;
            
            btnAbort.style.display = DisplayStyle.None;
            btnAbort.clickable.clicked += () => { 
                GPTClient.StopGeneration();
                btnAbort.style.display = DisplayStyle.None;
                btnSend.style.display = DisplayStyle.Flex;
            };
            btnSend.parent.Add(btnAbort);

            Button btnDeleteAllConversations = root.Q<Button>("btnDeleteAllConversations");
            btnDeleteAllConversations.clickable.clicked += () => { 
                deleteAllConversations();
            };
 

            Button btnCreateNewConversation = root.Q<Button>("btnCreateNewConversation");
            btnCreateNewConversation.clickable.clicked += () => { 
                createNewConversation();
            };

            veRightRow = root.Q<VisualElement>("veRightRow");
            lblWaitingForResponse = veRightRow.Q<Label>("lblWaitingForResponse");
            veConsiderDeletingOlderMessages = veRightRow.Q<VisualElement>("veConsiderDeletingOlderMessages");
            Button btnHideDeletingTipp = veConsiderDeletingOlderMessages.Q<Button>("btnHideDeletingTipp");
            btnHideDeletingTipp.clickable.clicked += () => { 
                veConsiderDeletingOlderMessages.style.display = DisplayStyle.None;
                currentConversation.showDeletingTipp = false;
            };

            loadAllConversationsFromPlayerPrefs(); 
            if(conversations.Count == 0){
                createNewConversation();
            }else{
                currentConversation = conversations[0];
                updateChat();
            }
 

            VisualElement veSetting = new VisualElement();
            veSetting.style.display = DisplayStyle.None;
            veSetting.style.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1);
            veSetting.style.paddingBottom = 10;
            veSetting.style.paddingTop = 10;
            veSetting.style.paddingLeft = 10;
            veSetting.style.paddingRight = 10;
            veRightRow.Insert(0, veSetting);
            Button btnOpenSettings = new Button();
            
            // btnOpenSettings.clickable.clicked += () =>{ 
            //     veSetting.style.display = veSetting.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;

            //     if(veSetting.style.display == DisplayStyle.Flex){
            //         btnOpenSettings.text = "Close Settings";
            //     }else{
            //         btnOpenSettings.text = "Show Settings";
            //     }
            // };
            // if(veSetting.style.display == DisplayStyle.Flex){
            //     btnOpenSettings.text = "Close Settings";
            // }else{
            //     btnOpenSettings.text = "Show Settings";
            // }
            // veRightRow.Insert(0, btnOpenSettings);
            // updateSettings(veSetting);
    
        }

        private void updateSettings(VisualElement veSetting)
        {  
            veSetting.Clear();
            

            // Create a new field and assign it its value.
            var popupModelSelection = new PopupField<string>("Model", GPTClient.models.ToList(), 0);
            popupModelSelection.value = GPTClient.models[GPTClient.modelIndex];
            veSetting.Add(popupModelSelection); 

            // Mirror value of uxml field into the C# field.
            popupModelSelection.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                Debug.Log("Normal field value changed to: " + evt.newValue);
                GPTClient.modelIndex = GPTClient.models.ToList().IndexOf(evt.newValue);

                updateSettings(veSetting);
            });

            VisualElement veTemperature = new VisualElement();
            veTemperature.style.flexDirection = FlexDirection.Row;
             
            //slider for temperature
            Label lblTemperature = new Label(""+GPTClient.temperature);   
            lblTemperature.style.width = new StyleLength(120);



            Slider sliderTemperature = new Slider(0.0f, 1.0f);
            sliderTemperature.style.flexGrow = 1;
            sliderTemperature.label = "Temperature";
            sliderTemperature.value = GPTClient.temperature;
            sliderTemperature.RegisterValueChangedCallback((evt) => {
                GPTClient.temperature = evt.newValue;
                lblTemperature.text = ""+evt.newValue;
            });
            veTemperature.Add(sliderTemperature);
            veTemperature.Add(lblTemperature);

            //slider for max tokens

            VisualElement veMaxTokens = new VisualElement();
            veMaxTokens.style.flexDirection = FlexDirection.Row;

            Label lblMaxTokens = new Label(""+GPTClient.maxTokens + " (max: "+HelperFunctions.getMaxTokenFromModel(GPTClient.models[GPTClient.modelIndex])+")");
            lblMaxTokens.style.width = new StyleLength(120);

            SliderInt sliderMaxTokens = new SliderInt(1, HelperFunctions.getMaxTokenFromModel(GPTClient.models[GPTClient.modelIndex]));
            sliderMaxTokens.style.flexGrow = 1;
            sliderMaxTokens.label = "Max Tokens";
            sliderMaxTokens.value = GPTClient.maxTokens;
            sliderMaxTokens.RegisterValueChangedCallback((evt) => {
                GPTClient.maxTokens = (int)evt.newValue;
                lblMaxTokens.text = ""+evt.newValue + " (max: "+HelperFunctions.getMaxTokenFromModel(GPTClient.models[GPTClient.modelIndex])+")";
            });
            veMaxTokens.Add(sliderMaxTokens);
            veMaxTokens.Add(lblMaxTokens); 


            veSetting.Add(popupModelSelection); 
            veSetting.Add(veMaxTokens); 
            veSetting.Add(veTemperature); 
            

        }

        public void OnDisable()
        {
            saveConversationToPlayerPrefs(); 
        } 

        public void createNewConversation(){
            currentConversation = new GPTConversation("new conversation", "ChatbotGPTConv_"+conversations.Count.ToString()); 

            //update ids of all conversation chronological
            for(int i = 0; i < conversations.Count; i++){
                conversations[i].conversation_id = "ChatbotGPTConv_"+i.ToString();
            }
            conversations.Add(currentConversation);


            updateConversationsListView();
            updateChat();
        }

        public void saveConversationToPlayerPrefs(){
            if(currentConversation == null) return;

            currentConversation.saveToPlayerPrefs(); 
        }

        public GPTConversation loadConversationFromPlayerPrefs(string conversation_id){
            GPTConversation conv = new GPTConversation(PlayerPrefs.GetString(conversation_id + "_name"), conversation_id); 

            int msgCount = PlayerPrefs.GetInt(conversation_id + "_msgCount", 0);
            for(int i = 0; i < msgCount; i++){ 
                GPTChatMessage msg = new GPTChatMessage();
                msg.role = PlayerPrefs.GetString(conversation_id + "_role_" + i.ToString());
                msg.content = PlayerPrefs.GetString(conversation_id + "_msg_" + i.ToString());
                msg.time = PlayerPrefs.GetString(conversation_id + "_time_" + i.ToString());
                msg.cost = PlayerPrefs.GetFloat(conversation_id + "_cost_" + i.ToString());
                conv.chatMessages.Add(msg);
            }

            conv.showDeletingTipp = PlayerPrefs.GetInt(conversation_id + "_showDeletingTipp", 1) == 1 ? true : false;

            return conv;
        }
        public void loadAllConversationsFromPlayerPrefs(){
            conversations.Clear();
            
            for(int i = 0; i < MAX_CONVERSATIONS; i++){ 
                //check if player prefs has a conversation with this id
                if(!PlayerPrefs.HasKey("ChatbotGPTConv_"+i.ToString() + "_msgCount")) continue;

                GPTConversation conv = loadConversationFromPlayerPrefs("ChatbotGPTConv_"+i.ToString());
                if(conv.chatMessages.Count > 0)
                    conversations.Add(conv);
            }

            updateConversationsListView();
        }
        public void deleteAllConversations()
        {
            if(EditorUtility.DisplayDialog("Delete All Conversations", "Do you really want to delete all conversations?", "Yes", "No")){
                for(int i = 0; i < MAX_CONVERSATIONS; i++){ 
                    //check if player prefs has a conversation with this id
                    if(!PlayerPrefs.HasKey("ChatbotGPTConv_"+i.ToString() + "_msgCount")) continue;

                    PlayerPrefs.DeleteKey("ChatbotGPTConv_"+i.ToString() + "_msgCount");
                    PlayerPrefs.DeleteKey("ChatbotGPTConv_"+i.ToString() + "_name");
                    for(int j = 0; j < MAX_MESSAGES; j++){ 
                        PlayerPrefs.DeleteKey("ChatbotGPTConv_"+i.ToString() + "_role_" + j.ToString());
                        PlayerPrefs.DeleteKey("ChatbotGPTConv_"+i.ToString() + "_msg_" + j.ToString());
                        PlayerPrefs.DeleteKey("ChatbotGPTConv_"+i.ToString() + "_time_" + j.ToString());
                        PlayerPrefs.DeleteKey("ChatbotGPTConv_"+i.ToString() + "_cost_" + j.ToString());
                    }
                }
                PlayerPrefs.Save();

                conversations.Clear();
                updateConversationsListView();
                updateChat();
            }
        }

        public void updateConversationsListView(){
            scrollViewChats.Clear();

            for (int i = conversations.Count - 1; i >= 0; i--)
            {
                GPTConversation conv = conversations[i];
                VisualElement veConversationButton = this.veConversationButton.CloneTree();
                Button btnChat =  veConversationButton.Q<Button>("btnOpenConversation");
                btnChat.text = conv.name;

                btnChat.clickable.clicked += () => { 
                    currentConversation = conv;
                    updateChat();
                };
                Button btnDelete =  veConversationButton.Q<Button>("btnDeleteConversation");
                btnDelete.clickable.clicked += () => { 
                    //ask the user if he really wants to delete the conversation
                    //delete conversation from player prefs
                    if(EditorUtility.DisplayDialog("Delete Conversation", "Do you really want to delete this conversation?", "Yes", "No")){
                        PlayerPrefs.DeleteKey(conv.conversation_id + "_msgCount");
                        for(int i = 0; i < conv.chatMessages.Count; i++){ 
                            PlayerPrefs.DeleteKey(conv.conversation_id + "_role_" + i.ToString());
                            PlayerPrefs.DeleteKey(conv.conversation_id + "_msg_" + i.ToString());
                            PlayerPrefs.DeleteKey(conv.conversation_id + "_time_" + i.ToString());
                            PlayerPrefs.DeleteKey(conv.conversation_id + "_name" + i.ToString());
                            PlayerPrefs.DeleteKey(conv.conversation_id + "_cost" + i.ToString());
                        }
                        PlayerPrefs.Save();

                        conversations.Remove(conv);
                        if(currentConversation == conv)
                            currentConversation = null;

                        updateConversationsListView();
                    }
    
                };

                scrollViewChats.Add(veConversationButton);
            }

            if(conversations.Count == 0){
                //create a new conversation
                createNewConversation();

            }else{
                if(currentConversation == null){
                    currentConversation = conversations[0];
                    updateChat();
                }
            }
        }


        public void updateChat(){
            swChatContent.Clear();

            foreach(GPTChatMessage message in currentConversation.chatMessages){ 
                if(message.role == "system") continue;
                
                VisualElement veMessage;    
                if(message.role == "user"){
                    veMessage = messageUser.CloneTree().Q<VisualElement>("veMessageUser");
                }else{
                    veMessage = messageAssistant.CloneTree().Q<VisualElement>("veMessageAssistant");
                }
    
                veMessage.Q<Label>("lblName").text = message.role + " - " + message.time; 

                Button btnDeleteMsg = veMessage.Q<Button>("btnDeleteMsg");
                btnDeleteMsg.clickable.clicked += () => { 
                    
                    if(EditorUtility.DisplayDialog("Delete Message", "Do you really want to delete this message?", "Yes", "No")){
                        currentConversation.chatMessages.Remove(message);
                        updateChat();
                    }
                };

                //btnCopyMsg
                Button btnCopyMsg = veMessage.Q<Button>("btnCopyMsg");
                btnCopyMsg.clickable.clicked += () => { 
                    EditorGUIUtility.systemCopyBuffer = message.content;
                    btnCopyMsg.text = "Copied";
                }; 
 
    
                string[] content = message.content.Split(new string[] { "```" }, System.StringSplitOptions.None);
                
                for(int i = 0; i < content.Length; i++){
                    string text = content[i].Trim();
                    if(text != ""){
                        if(i % 2 == 0){
                            VisualElement veChatMessage = lblChatMessageAssistant.CloneTree();
                            veChatMessage.Q<Label>("lblChatMsg").text = text;
                            veMessage.Add(veChatMessage); 
                        }else if(i % 2 == 1){ 
                            string firstLine = "Code";  

                            VisualElement veChatMessage = this.veCodeBlock.CloneTree();
                            veChatMessage.Q<Label>("lblCodeLanguage").text = firstLine;

                            // text = text.Substring(text.IndexOf("\n") + 1);

                            veChatMessage.Q<Label>("lblCodeContent").text = text;
                            veMessage.Add(veChatMessage); 

                            Button btnCopy = veChatMessage.Q<Button>("btnCopy");
                            btnCopy.clickable.clicked += () => { 
                                TextEditor te = new TextEditor();
                                te.text = text;
                                te.SelectAll();
                                te.Copy();
 
                                Label lblCopied = new Label("Copied!");
                                lblCopied.style.color = Color.green;
                                btnCopy.parent.Add(lblCopied);

                                btnCopy.parent.schedule.Execute(() => {
                                    lblCopied.RemoveFromHierarchy();
                                }).StartingIn(2000);
                            };
                            
                            Button btnSaveToFile = veChatMessage.Q<Button>("btnSaveToFile");
                            btnSaveToFile.clickable.clicked += () => { 
                                createFile(text);
                            };

                        } 
                    }
                } 


                // Label lblCost = new Label("Cost: " + message.cost);
                // lblCost.style.color = Color.gray;
                // lblCost.tooltip = "Cost of the message. Keep in mind, that the whole conversation is send each time. So, make sure to delete unneeded messages.";
                // lblCost.style.fontSize = 10;
                // lblCost.style.unityTextAlign = TextAnchor.MiddleRight;
                // veMessage.Add(lblCost);
                swChatContent.Add(veMessage);  
            }
            
            if(currentConversation.chatMessages.Count > 5 && currentConversation.showDeletingTipp){
                veConsiderDeletingOlderMessages.style.display = DisplayStyle.Flex;
            }else{
                veConsiderDeletingOlderMessages.style.display = DisplayStyle.None;
            }

            swChatContent.schedule.Execute(() => {
                if(swChatContent.verticalScroller.visible)
                    swChatContent.verticalScroller.value = swChatContent.verticalScroller.highValue;
            }).StartingIn(10);
        }

        public void createFile(string codeText){ 
            //get the name of the script
            string scriptName = codeText.Substring(codeText.IndexOf("public class ") + 13);
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
            
            string scriptPath = EditorUtility.SaveFilePanel("Save script", "Assets", scriptName, "cs");
            string name = Path.GetFileNameWithoutExtension(scriptPath);

            if(name != scriptName){
                //change classname in codetext
                codeText = codeText.Replace(scriptName, name);
            }

            if (scriptPath.Length != 0)
            {
                File.WriteAllText(scriptPath, codeText);
                AssetDatabase.Refresh();
            } 
        }
        
        private void sendRequestToGPT(string prompt)
        { 
            if(currentConversation.chatMessages.Count == MAX_MESSAGES - 1){ // -1 because we will receive a new message from the GPT 
                if(EditorUtility.DisplayDialog("Delete oldest messages?", "The conversation has reached the maximum number of messages. Do you want to delete the oldest messages?", "Yes", "No")){
                    //delete oldest messages
                    currentConversation.chatMessages.RemoveAt(0);
                    updateConversationsListView();
                }else{
                    return;
                }
            }

            GPTClient.Instance.SystemInitPrompt = SystemInitPrompt;

            string currentTime = System.DateTime.Now.ToString("HH:mm:ss");
    
            if(currentConversation.chatMessages.Count == 0){
                currentConversation.chatMessages.Add(new GPTChatMessage(){role = "system", content = SystemInitPrompt, time = currentTime});

                currentConversation.name = prompt.Substring(0, Mathf.Min(prompt.Length, 10)) + "...";
                updateConversationsListView();
            }

            currentConversation.chatMessages.Add(new GPTChatMessage(){role = "user", content = prompt, time = currentTime});

            apiResponse = "";
            var chatMessage = new GPTChatMessage(){role = "assistant", content = "", time = currentTime, cost = 0};
            currentConversation.chatMessages.Add(chatMessage);

            GPTClient.Instance.OnResponseReceived = null;
    
            GPTClient.Instance.OnResponseReceived += (response, index) =>
            {
                // string currentTime = System.DateTime.Now.ToString("HH:mm:ss");
                // apiResponse = response;
                // apiResponse = HelperFunctions.RemoveScriptTagFromOpenAIResponse(apiResponse);
                // currentConversation.chatMessages.Add(new GPTChatMessage(){role = "assistant", content = apiResponse, time = currentTime, cost = GPTClient.cost});
                //refresh the window
                updateChat();
                lblWaitingForResponse.style.display = DisplayStyle.None;
                
                btnAbort.style.display = DisplayStyle.None;
                btnSend.style.display = DisplayStyle.Flex;
            };

            GPTClient.Instance.OnPartResponseReceived = null;
            GPTClient.Instance.OnPartResponseReceived += (response) =>
            {
                apiResponse += response; 
                chatMessage.content = apiResponse;
                updateChat();
            };
            btnAbort.style.display = DisplayStyle.Flex;
            btnSend.style.display = DisplayStyle.None;
            
            GPTClient.Instance.SentRequestWithHistory(currentConversation.chatMessages); 

            inpPrompt.value = "";
            updateChat();
            lblWaitingForResponse.style.display = DisplayStyle.Flex;
        }
    } 
}
