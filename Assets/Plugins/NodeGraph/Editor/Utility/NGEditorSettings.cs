/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 编辑器模式下设置                                                                *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;

namespace UFrame.NodeGraph
{
    public class NGEditorSettings
    {
        public class GUI
        {
            public const float NODE_BASE_WIDTH = 120f;
            public const float NODE_BASE_HEIGHT = 18f;
            public const float NODE_WIDTH_MARGIN = 48f;
            public const float NODE_TITLE_HEIGHT_MARGIN = 8f;

            public const float CONNECTION_ARROW_WIDTH = 12f;
            public const float CONNECTION_ARROW_HEIGHT = 15f;

            public const float INPUT_POINT_WIDTH = 21f;
            public const float INPUT_POINT_HEIGHT = 29f;

            public const float OUTPUT_POINT_WIDTH = 10f;
            public const float OUTPUT_POINT_HEIGHT = 23f;

            public const float FILTER_OUTPUT_SPAN = 32f;

            public const float CONNECTION_POINT_MARK_SIZE = 16f;

            public const float CONNECTION_CURVE_LENGTH = 20f;

            public const float TOOLBAR_HEIGHT = 20f;
            public const float TOOLBAR_GRAPHNAMEMENU_WIDTH = 150f;
            public const int TOOLBAR_GRAPHNAMEMENU_CHAR_LENGTH = 20;

            public static readonly Color COLOR_ENABLED = new Color(0.43f, 0.65f, 1.0f, 1.0f);
            public static readonly Color COLOR_CONNECTED = new Color(0.9f, 0.9f, 0.9f, 1.0f);
            public static readonly Color COLOR_NOT_CONNECTED = Color.grey;
            public static readonly Color COLOR_CAN_CONNECT = Color.white;//new Color(0.60f, 0.60f, 1.0f, 1.0f);
            public static readonly Color COLOR_CAN_NOT_CONNECT = new Color(0.33f, 0.33f, 0.33f, 1.0f);

            public static string Skin { get { return Path.GUIResourceBasePath + "NodeStyle.guiskin"; } }
            public static string ConnectionPoint { get { return Path.GUIResourceBasePath + "ConnectionPoint.png"; } }
            public static string InputBG { get { return Path.GUIResourceBasePath + "InputBG.png"; } }
            public static string OutputBG { get { return Path.GUIResourceBasePath + "OutputBG.png"; } }
        }
        public class Path
        {
            private static string guiResourcePath = "";
            public const string ASSETS_PATH = "Assets/";
            public static string GUIResourceBasePath
            {
                get
                {
                    if(string.IsNullOrEmpty(guiResourcePath))
                    {
                        guiResourcePath = AssetDatabase.GUIDToAssetPath("3706d7e9bff914014a8c0d30e8f854e6") + "/";
                    }
                    return guiResourcePath;
                }
            }
        }
    }
}
