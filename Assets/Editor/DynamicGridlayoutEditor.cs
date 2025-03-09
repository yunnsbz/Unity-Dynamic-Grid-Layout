using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DynamicGridLayout))]
public class DynamicGridLayoutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DynamicGridLayout gridLayout = (DynamicGridLayout)target;
        
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("fitType"));

        // fields based on fit type
        switch (gridLayout.fitType)
        {
            case DynamicGridLayout.FitType.FIXED_ROWS:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rows"));
                childRatio(gridLayout);
                break;

            case DynamicGridLayout.FitType.FIXED_COLUMNS:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("columns"));
                childRatio(gridLayout);
                break;

            case DynamicGridLayout.FitType.UNIFORM:
                var fitX = serializedObject.FindProperty("fitX");
                var fitY = serializedObject.FindProperty("fitY");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Fit", GUILayout.Width(EditorGUIUtility.labelWidth - 1));
                EditorGUILayout.LabelField("X", GUILayout.MaxWidth(13));
                fitX.boolValue = EditorGUILayout.ToggleLeft("", fitX.boolValue, GUILayout.Width(20));
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(13));
                fitY.boolValue = EditorGUILayout.ToggleLeft("", fitY.boolValue, GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();
                break;
        }
        

        EditorGUILayout.PropertyField(serializedObject.FindProperty("cellSize"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("spacing"));

        SerializedProperty paddingProp = serializedObject.FindProperty("m_Padding");
        EditorGUILayout.PropertyField(paddingProp, true);


        serializedObject.ApplyModifiedProperties();
    }

    private void childRatio(DynamicGridLayout gridLayout)
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("childRatio"));

        // fixedRatio can be used only if child ratio is fixed
        if (gridLayout.childRatio == DynamicGridLayout.ChildRatio.Fixed)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedRatio"));
        }
        if (gridLayout.childRatio == DynamicGridLayout.ChildRatio.Free)
        {
            var resizeX = serializedObject.FindProperty("fitX");
            var resizeY = serializedObject.FindProperty("fitY");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resize", GUILayout.Width(EditorGUIUtility.labelWidth - 1));
            EditorGUILayout.LabelField("X", GUILayout.MaxWidth(13));
            resizeX.boolValue = EditorGUILayout.ToggleLeft("", resizeX.boolValue, GUILayout.Width(20));
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(13));
            resizeY.boolValue = EditorGUILayout.ToggleLeft("", resizeY.boolValue, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
        }
    }

}
