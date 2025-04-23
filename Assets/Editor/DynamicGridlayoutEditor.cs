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

        // show presets enum
        EditorGUILayout.PropertyField(serializedObject.FindProperty("preset"));

        EditorGUILayout.Space(10);

        // if the preset is set to custom, show the rest of the properties
        if (gridLayout.preset == DynamicGridLayout.Presets.Custom)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fitType"));

            // fields based on fit type
            switch (gridLayout.fitType)
            {
                case DynamicGridLayout.FitType.FIXED_ROWS:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rows"));
                    ChildRatio(gridLayout);
                    break;

                case DynamicGridLayout.FitType.FIXED_COLUMNS:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("columns"));
                    ChildRatio(gridLayout);
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
        }
        else
        {
            // if the preset is not custom, show the dedicated preset properties
            switch (gridLayout.preset)
            {
                case DynamicGridLayout.Presets.vertical_list:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedRatio"));
                    break;
                case DynamicGridLayout.Presets.horizontal_list:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedRatio"));
                    break;
                case DynamicGridLayout.Presets.item_grid_v:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("columns"));
                    break;
            }
        }

        // general layout settings:

        EditorGUILayout.Space(10);

        // spacing
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spacing"));

        // padding
        SerializedProperty paddingProp = serializedObject.FindProperty("m_Padding");
        EditorGUILayout.PropertyField(paddingProp, true);


        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// display and manage the properties related to the child ratio
    /// </summary>
    private void ChildRatio(DynamicGridLayout gridLayout)
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
