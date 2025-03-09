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
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fitX"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fitY"));
                break;
        }
        

        EditorGUILayout.PropertyField(serializedObject.FindProperty("cellSize"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("spacing"));

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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resizeX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resizeY"));
        }
    }

}
