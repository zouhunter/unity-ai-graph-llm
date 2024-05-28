using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace AIScripting
{
    public class CodeResponceUtil
    {
        /// <summary>
        /// 获取代码内容
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetContentScript(string content)
        {
            return Regex.Replace(content, @"```(\w*)", "").TrimStart();
        }

        /// <summary>
        /// 拆解内容
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string[] SplitContents(string text)
        {
            var contents = new List<string>();
            var lastIndex = 0;
            var startIndex = text.IndexOf("```");
            int inCodeBlock = 0;
            while (startIndex >= 0)
            {
                if (startIndex >= 0)
                {
                    inCodeBlock++;
                    if (startIndex > lastIndex)
                    {
                        if (inCodeBlock % 2 == 0)
                            startIndex += 3;

                        contents.Add(text.Substring(lastIndex, startIndex - lastIndex));
                        lastIndex = startIndex;

                        if (inCodeBlock % 2 != 0)
                            startIndex += 3;
                    }
                    else
                    {
                        startIndex += 3;
                    }
                    startIndex = text.IndexOf("```", startIndex);
                }
                else
                {
                    break;
                }
            }
            if (text.Length > lastIndex)
            {
                contents.Add(text.Substring(lastIndex));
            }
            return contents.ToArray();
        }

        /// <summary>
        /// 分析代码类型
        /// </summary>
        /// <param name="content"></param>
        /// <param name="scriptName"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public static CodeType CheckCodeType(string content, out string scriptName, out string fileExt)
        {
            fileExt = null;
            scriptName = null;
            var match = Regex.Match(content, @"```(\w+)");
            if (match.Success)
            {
                var codeName = match.Groups[1].Value.ToLower();
                switch (codeName)
                {
                    case "csharp":
                    case "c#":
                        fileExt = "cs";
                        scriptName = GetCSharpScriptName(content);
                        return CodeType.CSharp;
                    case "shader":
                    case "glsl":
                    case "hlsl":
                        fileExt = "shader";
                        scriptName = GetShaderScriptName(content);
                        return CodeType.Shader;
                    case "py":
                        return CodeType.Python;
                    case "json":
                        return CodeType.Json;
                    case "shell":
                    case "sh":
                        return CodeType.Shell;
                    case "bat":
                    case "cmd":
                        return CodeType.Cmd;
                    default:
                        return CodeType.Other;
                }
            }
            return CodeType.None;
        }
        /// <summary>
        /// 获取csharp脚本名称
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetCSharpScriptName(string content)
        {
            var match = Regex.Match(content, "public class (\\w+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        /// <summary>
        /// 获取shader脚本名称    
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetShaderScriptName(string content)
        {
            var match = Regex.Match(content, "Shader \"(.*)\"");
            if (match.Success)
            {
                var fullname = match.Groups[1].Value;
                var index = fullname.LastIndexOf('/');
                if (index > 0)
                {
                    return fullname.Substring(index + 1);
                }
                return fullname;
            }
            return null;
        }

    }
}
