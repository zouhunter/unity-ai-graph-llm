using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;
using AIScripting.MateAI;

namespace AIScripting.Diagram
{
    [CustomEditor(typeof(ComfyUINode))]
    public class ComfyUIDrawer : Editor
    {
        public ComfyUINode node;
        private List<Texture2D> _textures = new();
        private List<string> _textureLast = new();
        private Dictionary<Texture2D, string> _texturePath = new();
        private void OnEnable()
        {
            node = target as ComfyUINode;
            ReloadTextures();
        }

        private void ReloadTextures()
        {
            if (System.IO.Directory.Exists(node.exportDir.Value) && node.exportFiles.Value != null)
            {
                _textures.Clear();
                _textureLast.Clear();
                for (int i = 0; i < node.exportFiles.Value.Count; i++)
                {
                    _textureLast.Add(node.exportFiles.Value[i]);
                    var assetPath = System.IO.Path.Join(node.exportDir.Value, node.exportFiles.Value[i]);
                    var fullPath = System.IO.Path.GetFullPath(assetPath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        var texture = new Texture2D(1, 1);
                        texture.name = System.IO.Path.GetFileNameWithoutExtension(fullPath);
                        texture.LoadImage(System.IO.File.ReadAllBytes(fullPath));
                        _textures.Add(texture);
                        _texturePath[texture] = fullPath;
                    }
                }
            }
        }

        private void DrawTexture(int index, Texture2D image)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"{index}.{image.name}", EditorStyles.boldLabel);
            if (image != null)
            {
                var clicked = GUILayout.Button(image, GUILayout.Width(100), GUILayout.Height(100));
                if (clicked)
                {
                    Application.OpenURL(new System.Uri(_texturePath[image]).AbsoluteUri);
                }
            }
            else
            {
                GUILayout.Label("No Image", GUILayout.Width(100), GUILayout.Height(100));
            }
            EditorGUILayout.EndVertical();
        }


        private void CheckChanged()
        {
            if (_textureLast.Count != node.exportFiles.Value.Count)
            {
                ReloadTextures();
            }
            else
            {
                for (int i = 0; i < node.exportFiles.Value.Count; i++)
                {
                    if (_textureLast[i] != node.exportFiles.Value[i])
                    {
                        ReloadTextures();
                        break;
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CheckChanged();

            var index = 0;
            using (var vert = new EditorGUILayout.VerticalScope())
            {
                int lineBtnCount = 2;
                int counter = 0;
                foreach (var texture in _textures)
                {
                    if (counter % lineBtnCount == 0)
                    {
                        if (counter != 0)
                            GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                    }
                    DrawTexture(++index, texture);
                    counter++;
                }
                if (counter != 0)
                    GUILayout.EndHorizontal();
            }
        }
    }
}
