using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace AIScripting
{
    [CustomEditor(typeof(NodeConnection))]
    public class NodeConnectionDrawer : Editor
    {
        private SerializedProperty titleProp;
        private SerializedProperty priorityProp;
        private SerializedProperty disableProp;
        private SerializedProperty conditionTypeProp;
        private SerializedProperty conditionsProp;
        private SerializedProperty scriptProp;
        private ReorderableList conditionsList;

        private void OnEnable()
        {
            if(target == null)
                return;

            titleProp = serializedObject.FindProperty("_title");
            priorityProp = serializedObject.FindProperty("priority");
            disableProp = serializedObject.FindProperty("disable");
            conditionTypeProp = serializedObject.FindProperty("conditionType");
            conditionsProp = serializedObject.FindProperty("conditions");
            scriptProp = serializedObject.FindProperty("m_Script");
            conditionsList = new ReorderableList(serializedObject, conditionsProp, true, true, true, true);
            conditionsList.drawHeaderCallback = (rect) =>
            {
                var disableRect = new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight);
                disableProp.boolValue = !EditorGUI.Toggle(disableRect, !disableProp.boolValue);
                EditorGUI.LabelField(new Rect(disableRect.max.x,rect.y,rect.width - 140,rect.height), "Conditions");
                var propRect = new Rect(rect.max.x - 120, rect.y, 120, EditorGUIUtility.singleLineHeight);
                conditionTypeProp.enumValueIndex = (int)(NodeConnection.ConditionType)EditorGUI.EnumPopup(propRect, (NodeConnection.ConditionType)conditionTypeProp.enumValueIndex);
            };
            conditionsList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
            conditionsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = conditionsProp.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            using(var disable = new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.PropertyField(scriptProp);
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(titleProp);
            EditorGUILayout.PropertyField(priorityProp);
            conditionsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            
        }

    }
}
