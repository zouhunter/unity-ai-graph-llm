using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace AIScripting
{
    [FilePath("ProjectSettings/AIScripting.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AIScriptSettings : ScriptableSingleton<AIScriptSettings>
    {
        public List<Prompt> prompts = new List<Prompt>();
        [SettingsProvider]
        public static SettingsProvider CreateProjectMenu()
        {
            var provider = new SettingsProvider("Project/UFrame/AIScripting", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "AIScripting",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = instance.Draw,

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Node", "Graph" })
            };

            return provider;
        }
        private ReorderableList _promptsList;

        private void Draw(string a)
        {
            MakeSurePromptsList();
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                _promptsList.DoLayoutList();
                if (change.changed)
                    Save();
            }
        }

        private void MakeSurePromptsList()
        {
            if (_promptsList == null)
            {
                _promptsList = new ReorderableList(prompts, typeof(Prompt));
                _promptsList.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect,"提示词列表"); };
                _promptsList.drawElementCallback = OnDrawPromptItem;
            }
        }

        private void OnDrawPromptItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            base.Save(true);
        }

        /// <summary>
        /// 提示词获取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dft"></param>
        /// <returns></returns>
        public string GetPrompt(string key, string dft = default)
        {
            var prompt = prompts.Find(x => x.key == key);
            if (prompt != null)
                return prompt.info;
            return dft;
        }
    }

    [SerializeField]
    public class Prompt
    {
        public string key;
        public string group;
        public string info;
    }
}
