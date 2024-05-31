using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace AIScripting
{
    [FilePath("Library/AIScripting.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AIScriptSettings : ScriptableSingleton<AIScriptSettings>
    {
        [SettingsProvider]
        public static SettingsProvider CreateProjectMenu()
        {
            var provider = new SettingsProvider("Project/UFrame/AIScripting", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "AI-Scripting-Graph",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = instance.Draw,

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Node", "Graph" })
            };

            return provider;
        }

        public List<Prompt> prompts = new List<Prompt>();
        public List<string> models = new List<string>();
        public List<EditorEnv> envs = new List<EditorEnv>();
        private ReorderableList _promptsList;
        private ReorderableList _modelsList;
        private ReorderableList _envsList;
        private void Draw(string a)
        {
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Box("", GUILayout.Height(3), GUILayout.ExpandWidth(true));
            MakeSureModelList();
            MakeSureEditorEnv();
            MakeSurePromptsList();
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                using (var hor = new EditorGUILayout.HorizontalScope())
                {
                    using (var ver = new EditorGUILayout.VerticalScope(GUILayout.Width(200)))
                    {
                        _modelsList.DoLayoutList();
                    }
                    GUILayout.Box("", GUILayout.Width(3), GUILayout.MinHeight(200));
                    using (var ver = new EditorGUILayout.VerticalScope(GUILayout.MinHeight(200)))
                    {
                        _envsList.DoLayoutList();
                    }
                }
                GUILayout.Box("", GUILayout.Height(2), GUILayout.ExpandWidth(true));
                _promptsList.DoLayoutList();
                if (change.changed)
                {
                    Save();
                }
            }
        }

        private void MakeSureEditorEnv()
        {
            if (_envsList == null)
            {
                _envsList = new ReorderableList(envs, typeof(EditorEnv));
                _envsList.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "环境变量"); };
                _envsList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var env = envs[index];
                    var labelRect = new Rect(rect.x - 10, rect.y + 2, 20, rect.height - 4);
                    EditorGUI.LabelField(labelRect, (1 + index).ToString("00"));
                    rect = new Rect(rect.x + 10, rect.y + 2, rect.width - 80, rect.height - 4);
                    var keyRect = new Rect(rect.x, rect.y, rect.width * 0.3f, rect.height);
                    var valueRect = new Rect(keyRect.max.x + 5, rect.y, rect.width * 0.7f, rect.height);
                    var typeRect = new Rect(valueRect.max.x + 5, rect.y, 60, rect.height);
                    env.key = EditorGUI.TextField(keyRect, env.key);
                    env.value = EditorGUI.TextField(valueRect, env.value);
                    env.type = (EditorEnv.ValueType)EditorGUI.EnumPopup(typeRect, env.type);
                };
            }
        }

        private void MakeSureModelList()
        {
            if (_modelsList == null)
            {
                _modelsList = new ReorderableList(models, typeof(string));
                _modelsList.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "模型列表"); };
                _modelsList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var labelRect = new Rect(rect.x - 10, rect.y + 2, 20, rect.height - 4);
                    EditorGUI.LabelField(labelRect, (1 + index).ToString("00"));
                    rect = new Rect(rect.x + 20, rect.y + 2, rect.width - 20, rect.height - 4);
                    models[index] = EditorGUI.TextField(rect, models[index]);
                };
            }
        }

        private void MakeSurePromptsList()
        {
            if (_promptsList == null)
            {
                _promptsList = new ReorderableList(prompts, typeof(Prompt));
                _promptsList.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "提示词列表"); };
                _promptsList.drawElementCallback = OnDrawPromptItem;
            }
        }

        private void OnDrawPromptItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            var prompt = prompts[index];
            var labelRect = new Rect(rect.x - 10, rect.y + 2, 20, rect.height - 4);
            EditorGUI.LabelField(labelRect, (1 + index).ToString("00"));
            var innerRect = new Rect(rect.x + 20, rect.y + 2, rect.width - 50, rect.height - 4);

            using (var disableGroup = new EditorGUI.DisabledGroupScope(!prompt.active))
            {
                var keyRect = new Rect(innerRect.x, innerRect.y, innerRect.width * 0.15f, innerRect.height);
                var infoRect = new Rect(keyRect.max.x + 5, innerRect.y, innerRect.width * 0.7f, innerRect.height);
                var groupRect = new Rect(infoRect.max.x + 5, innerRect.y, innerRect.width * 0.1f, innerRect.height);
                prompt.key = EditorGUI.TextField(keyRect, prompt.key);
                prompt.info = EditorGUI.TextField(infoRect, prompt.info);
                prompt.group = EditorGUI.TextField(groupRect, prompt.group);
            }
            var activeRect = new Rect(rect.max.x - 20, innerRect.y, 20, innerRect.height);
            prompt.active = EditorGUI.Toggle(activeRect, prompt.active,EditorStyles.radioButton);
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            base.Save(true);
            AssetDatabase.SaveAssets();
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

    [Serializable]
    public class Prompt
    {
        public string key;
        public string group;
        public string info;
        public bool active = true;
    }

    [Serializable]
    public class EditorEnv
    {
        public string key;
        public string value;
        public ValueType type;
        public enum ValueType
        {
            String = 0,
            Int = 1,
            Float = 2,
            Bool = 3,
            StrList = 4,
            StrArray = 5,
            IntList = 6,
            IntArray = 7,
            Object = 8,
        }
    }
}
