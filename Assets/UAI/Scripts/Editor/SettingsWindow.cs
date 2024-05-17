using UnityEditor;
using UnityEngine;

namespace UAI{ 
    public class SettingsWindow : EditorWindow
    {
        //models

        private static string _secretKey = ""; 
        private static int _selectedModelIndex = 0; 

        
        public static float _temperature = 0.7f;
        public static int _maxTokens = 0;
        public static int _n = 1;

        public static string _apiEndpoint = "";

        public static string _defaultSavePath = "Assets/";
        public static bool _askForSavePath = true;

        [MenuItem("Tools/AI Assistant/Settings", false, 1000)]
        public static void ShowWindow()
        {
            SettingsWindow window = GetWindow<SettingsWindow>("AI Assistant Settings"); 
            window.minSize = new Vector2(400, 350);
        }

        private void OnEnable() {
            LoadAPIKey();

            _temperature = EditorPrefs.GetFloat("GPTTemperature", 0.7f);
            _apiEndpoint = EditorPrefs.GetString("UAIEndpoint", GPTClient.defaultOpenAIURL);
            _maxTokens = EditorPrefs.GetInt("GPTMaxTokens", 0);
            _n = EditorPrefs.GetInt("GPTN", 1);
            _defaultSavePath = EditorPrefs.GetString("GPTDefaultSavePath", "Assets/");
            _askForSavePath = EditorPrefs.GetBool("GPTAskForSavePath", false);


            GPTClient.temperature = _temperature;
            GPTClient.maxTokens = _maxTokens;
            GPTClient.n = _n;
            GPTClient.defaultSavePath = _defaultSavePath;
            GPTClient.askForSavePath = _askForSavePath; 
            GPTClient.apiEndpoint = _apiEndpoint;
        }

        public static string LoadAPIKey()  
        {
            _secretKey = EditorPrefs.GetString("UAISecretKey", "");
            GPTClient.Instance.apiKey = _secretKey;

            GPTClient.Instance.model = EditorPrefs.GetString("GPTModel", "gpt-3.5-turbo");
            GPTClient.modelIndex = -1;

            for (int i = 0; i < GPTClient.models.Length; i++)
            {
                if (GPTClient.models[i] == GPTClient.Instance.model)
                {
                    GPTClient.modelIndex = i;
                    break;
                }
            }

            if(GPTClient.modelIndex == -1)
            {
                GPTClient.modelIndex = 0;
                GPTClient.Instance.model = GPTClient.models[0];

                // Debug.LogWarning("Model not found, using default model: " + GPTClient.models[0]);
            }

            _selectedModelIndex = GPTClient.modelIndex;

            // Debug.Log("Model: " + GPTClient.Instance.model);

            return _secretKey;
        } 

        public void SaveSettings()
        {
            GPTClient.modelIndex = _selectedModelIndex;
            GPTClient.Instance.model = GPTClient.models[_selectedModelIndex];  
            EditorPrefs.SetString("UAISecretKey", _secretKey);
            EditorPrefs.SetString("GPTModel", GPTClient.models[_selectedModelIndex]);
            EditorPrefs.SetInt("GPTModelIndex", _selectedModelIndex);
            EditorPrefs.SetFloat("GPTTemperature", _temperature);
            EditorPrefs.SetInt("GPTMaxTokens", _maxTokens);
            EditorPrefs.SetInt("GPTN", 1);//_n);
            EditorPrefs.SetString("GPTDefaultSavePath", _defaultSavePath);
            EditorPrefs.SetBool("GPTAskForSavePath", _askForSavePath);
            EditorPrefs.SetString("UAIEndpoint", _apiEndpoint);

            GPTClient.Instance.apiKey = _secretKey;
            GPTClient.temperature = _temperature;
            GPTClient.maxTokens = _maxTokens;
            GPTClient.n = _n;
            GPTClient.defaultSavePath = _defaultSavePath;
            GPTClient.askForSavePath = _askForSavePath;
            GPTClient.apiEndpoint = _apiEndpoint;
        }

        private void OnGUI()
        {
            GUILayout.Label("AI Assistant Settings", EditorStyles.boldLabel);
            _secretKey = EditorGUILayout.PasswordField("OpenAI API Key:", _secretKey);
 
            GUILayout.Space(10);

            _apiEndpoint = EditorGUILayout.TextField("API Endpoint:", _apiEndpoint);
            //button to set to default https://api.openai.com/v1/chat/completions
            if(GUILayout.Button("Set to default"))
            {
                _apiEndpoint = GPTClient.defaultOpenAIURL;
            }
 
            GUILayout.Space(10);
 
            _selectedModelIndex = EditorGUILayout.Popup("Model:", _selectedModelIndex, GPTClient.models);
 
            GUILayout.Space(10);

            _temperature = EditorGUILayout.FloatField("Temperature:", _temperature);
            if(_temperature < 0)
                _temperature = 0;
            if(_temperature > 1)
                _temperature = 1;
 
            // _maxTokens = EditorGUILayout.IntSlider("Max Tokens:", _maxTokens, 0, HelperFunctions.getMaxTokenFromModel (GPTClient.models[_selectedModelIndex]));
             
            // _n = EditorGUILayout.IntField("N:", _n);
            GUILayout.Space(10);

            _defaultSavePath = EditorGUILayout.TextField("Default Save Path:", _defaultSavePath);
            //label
            GUILayout.Label("Ask for Save Path on every creation?");
            _askForSavePath = EditorGUILayout.Toggle("Ask for Save Path?", _askForSavePath); 


    
            GUILayout.Space(10);

            GUI.enabled = GPTClient.Instance.apiKey != _secretKey || GPTClient.Instance.model != GPTClient.models[_selectedModelIndex] || GPTClient.temperature != _temperature || GPTClient.maxTokens != _maxTokens || GPTClient.defaultSavePath != _defaultSavePath || GPTClient.askForSavePath != _askForSavePath || GPTClient.apiEndpoint != _apiEndpoint;
            if (GUILayout.Button("Save Settings"))
            {
                SaveSettings();
            }
            GUI.enabled = true;

            GUILayout.Space(10);
            if(GUILayout.Button("Get API Key"))
            {
                Application.OpenURL("https://platform.openai.com/account/api-keys");
            }
    
        }
    }
}
