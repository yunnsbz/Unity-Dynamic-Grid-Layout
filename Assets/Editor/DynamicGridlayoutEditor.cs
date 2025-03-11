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

        EditorGUILayout.PropertyField(serializedObject.FindProperty("preset"));

        EditorGUILayout.Space(10);

        if (gridLayout.preset == DynamicGridLayout.Presets.Custom)
        {
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
        }
        else
        {
            switch (gridLayout.preset)
            {
                case DynamicGridLayout.Presets.vertical_list:
                    gridLayout.fitType = DynamicGridLayout.FitType.FIXED_COLUMNS;
                    gridLayout.columns = 1;
                    gridLayout.childRatio = DynamicGridLayout.ChildRatio.Fixed;
                    gridLayout.fitY = true;
                    gridLayout.fitX = true;
                    
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedRatio"));
                    break;
                case DynamicGridLayout.Presets.horizontal_list:
                    break;
                case DynamicGridLayout.Presets.item_grid_v:
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

        //if(GUILayout.Button("print values"))
        //{
        //    string LogText = "";

        //    LogText += "fit type: " + gridLayout.fitType + "\n";
        //    LogText += "rows: " + gridLayout.rows + "\n";
        //    LogText += "fixedRatio: " + gridLayout.fixedRatio + "\n";
        //    LogText += "Child size: " + gridLayout.cellSize + "\n";

        //    Debug.Log(LogText);
        //}
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
