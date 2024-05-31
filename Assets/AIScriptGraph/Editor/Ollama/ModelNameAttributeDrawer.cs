using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AIScripting.Ollama
{
    [CustomPropertyDrawer(typeof(OllamaModelNameAttribute))]
    public class ModelNameAttributeDrawer : PropertyDrawer
    {
        private GUIContent[] allModels;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(allModels == null || AIScriptSettings.instance.models.Count != allModels.Length)
            {
                allModels = new GUIContent[AIScriptSettings.instance.models.Count];
                for (int i = 0; i < allModels.Length; i++)
                {
                    allModels[i] = new GUIContent(AIScriptSettings.instance.models[i]);
                }
            }
            var index = AIScriptSettings.instance.models.IndexOf(property.stringValue);
            if(index < 0)
            {
                index = 0;
            }
            index = EditorGUI.Popup(position,label, index, allModels,EditorStyles.textField);
            if(index >= 0)
            {
                property.stringValue = AIScriptSettings.instance.models[index];
            }
        }
    }
}
