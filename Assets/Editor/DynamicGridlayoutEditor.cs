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

        // show presets enum with tooltip
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("preset"),
            new GUIContent("Preset", "Select a predefined grid layout preset or customize it.")
        );

        EditorGUILayout.Space(10);

        // if the preset is set to custom, show the rest of the properties
        if (gridLayout.preset == DynamicGridLayout.Presets.Custom)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("fitType"),
                new GUIContent("Fit Type", "Determine how the grid cells are arranged.")
            );

            // fields based on fit type
            switch (gridLayout.fitType)
            {
                case DynamicGridLayout.FitType.FIXED_ROWS:
                    EditorGUILayout.PropertyField(
                        serializedObject.FindProperty("rows"),
                        new GUIContent("Rows", "Number of fixed rows in the grid.")
                    );
                    ChildRatio(gridLayout);
                    break;

                case DynamicGridLayout.FitType.FIXED_COLUMNS:
                    EditorGUILayout.PropertyField(
                        serializedObject.FindProperty("columns"),
                        new GUIContent("Columns", "Number of fixed columns in the grid.")
                    );
                    ChildRatio(gridLayout);
                    break;

                case DynamicGridLayout.FitType.UNIFORM:
                    ChildRatio(gridLayout);
                    break;
            }

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("cellSize"),
                new GUIContent("Cell Size", "Size of each cell in the grid.")
            );
        }
        else
        {
            // if the preset is not custom, show the dedicated preset properties
            switch (gridLayout.preset)
            {
                case DynamicGridLayout.Presets.vertical_list:
                    EditorGUILayout.PropertyField(
                        serializedObject.FindProperty("fixedRatio"),
                        new GUIContent("Fixed Ratio", "Aspect ratio for vertical list items.")
                    );
                    break;
                case DynamicGridLayout.Presets.horizontal_list:
                    EditorGUILayout.PropertyField(
                        serializedObject.FindProperty("fixedRatio"),
                        new GUIContent("Fixed Ratio", "Aspect ratio for horizontal list items.")
                    );
                    break;
                case DynamicGridLayout.Presets.item_grid_v:
                    EditorGUILayout.PropertyField(
                        serializedObject.FindProperty("columns"),
                        new GUIContent("Columns", "Number of columns for the item grid.")
                    );
                    break;
            }
        }

        // general layout settings:
        EditorGUILayout.Space(10);

        // spacing with tooltip
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("spacing"),
            new GUIContent("Spacing", "Space between grid cells.")
        );

        // padding with tooltip
        SerializedProperty paddingProp = serializedObject.FindProperty("m_Padding");
        EditorGUILayout.PropertyField(
            paddingProp,
            new GUIContent("Padding", "Padding inside the grid layout."),
            true
        );

        serializedObject.ApplyModifiedProperties();
    }

    private void ChildRatio(DynamicGridLayout gridLayout)
    {
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("childRatio"),
            new GUIContent("Child Ratio", "Aspect ratio behavior for child elements.")
        );

        // fixedRatio can be used only if child ratio is fixed
        if (gridLayout.childRatio == DynamicGridLayout.ChildRatio.Fixed)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("fixedRatio"),
                new GUIContent("Fixed Ratio", "Fixed aspect ratio for child elements.")
            );
        }
        if (gridLayout.childRatio == DynamicGridLayout.ChildRatio.Free)
        {
            //var fitX = serializedObject.FindProperty("fitX");
            //var fitY = serializedObject.FindProperty("fitY");
            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField(new GUIContent("Fit", "Fit options for X and Y axes."), GUILayout.Width(EditorGUIUtility.labelWidth - 1));
            //EditorGUILayout.LabelField("X", GUILayout.MaxWidth(13));
            //fitX.boolValue = EditorGUILayout.ToggleLeft("", fitX.boolValue, GUILayout.Width(20));
            //GUILayout.Space(20);
            //EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(13));
            //fitY.boolValue = EditorGUILayout.ToggleLeft("", fitY.boolValue, GUILayout.Width(20));
            //EditorGUILayout.EndHorizontal();
        }
    }
}