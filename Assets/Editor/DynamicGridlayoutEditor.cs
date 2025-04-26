using UnityEditor;
using UnityEngine;
using UnityEngine.UI; // Required for LayoutRebuilder
[CustomEditor(typeof(DynamicGridLayout))]
[CanEditMultipleObjects]
public class DynamicGridLayoutEditor : Editor
{
    // Serialized Properties
    SerializedProperty presetProp;
    SerializedProperty fitTypeProp;
    SerializedProperty rowsProp;
    SerializedProperty columnsProp;
    SerializedProperty childRatioProp;
    SerializedProperty fixedRatioProp;
    SerializedProperty fitXProp;
    SerializedProperty fitYProp;
    SerializedProperty spacingProp;
    // Properties from LayoutGroup base class
    SerializedProperty paddingProp;
    SerializedProperty childAlignmentProp;
    protected virtual void OnEnable()
    {
        // Cache serialized properties
        presetProp = serializedObject.FindProperty("preset");
        fitTypeProp = serializedObject.FindProperty("fitType");
        rowsProp = serializedObject.FindProperty("rows");
        columnsProp = serializedObject.FindProperty("columns");
        childRatioProp = serializedObject.FindProperty("childRatio");
        fixedRatioProp = serializedObject.FindProperty("fixedRatio");
        fitXProp = serializedObject.FindProperty("fitX");
        fitYProp = serializedObject.FindProperty("fitY");
        spacingProp = serializedObject.FindProperty("spacing");
        // Cache LayoutGroup base property
        paddingProp = serializedObject.FindProperty("m_Padding");
        childAlignmentProp = serializedObject.FindProperty("m_ChildAlignment");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Always start with this

        DynamicGridLayout gridLayout = (DynamicGridLayout)target; // Get the target script instance

        EditorGUI.BeginChangeCheck(); // Start checking for changes

        // --- Preset Selection ---
        EditorGUILayout.PropertyField(presetProp, new GUIContent("Preset", "Select a predefined layout or Custom. Presets override some settings below."));
        DynamicGridLayout.Presets currentPreset = (DynamicGridLayout.Presets)presetProp.enumValueIndex;

        EditorGUILayout.Space(5);

        // --- Custom Configuration (Only if Preset is Custom) ---
        if (currentPreset == DynamicGridLayout.Presets.Custom)
        {
            EditorGUILayout.LabelField("Custom Configuration", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(fitTypeProp, new GUIContent("Fit Type", "How the grid determines rows/columns and sizing."));
            DynamicGridLayout.FitType currentFitType = (DynamicGridLayout.FitType)fitTypeProp.enumValueIndex;

            // Show Rows/Columns fields based on FitType
            EditorGUI.indentLevel++;
            switch (currentFitType)
            {
                case DynamicGridLayout.FitType.FIXED_ROWS:
                    EditorGUILayout.PropertyField(rowsProp, new GUIContent("Fixed Rows", "The fixed number of rows. Columns are calculated."));
                    break;
                case DynamicGridLayout.FitType.FIXED_COLUMNS:
                    EditorGUILayout.PropertyField(columnsProp, new GUIContent("Fixed Columns", "The fixed number of columns. Rows are calculated."));
                    break;
                case DynamicGridLayout.FitType.WIDTH:
                    EditorGUILayout.PropertyField(columnsProp, new GUIContent("Columns", "Number of columns influencing width fitting. Rows are calculated."));
                    EditorGUILayout.HelpBox("Container height adjusts based on content.", MessageType.Info);
                    break;
                case DynamicGridLayout.FitType.HEIGHT:
                    EditorGUILayout.PropertyField(rowsProp, new GUIContent("Rows", "Number of rows influencing height fitting. Columns are calculated."));
                    EditorGUILayout.HelpBox("Container width adjusts based on content.", MessageType.Info);
                    break;
                case DynamicGridLayout.FitType.UNIFORM:
                    EditorGUILayout.HelpBox("Grid attempts a balanced layout within the current container size. Rows/Columns are calculated.", MessageType.Info);
                    break;
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Child Sizing", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(childRatioProp, new GUIContent("Child Aspect Ratio", "How children maintain their aspect ratio."));
            DynamicGridLayout.ChildRatio currentChildRatio = (DynamicGridLayout.ChildRatio)childRatioProp.enumValueIndex;

            if (currentChildRatio == DynamicGridLayout.ChildRatio.Fixed)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(fixedRatioProp, new GUIContent("Fixed Ratio (W/H)", "Aspect ratio (Width/Height) for children."), true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(3);

            EditorGUILayout.PropertyField(fitXProp, new GUIContent("Fit Cell Width", "Force cell width to fill the available column width."));
            EditorGUILayout.PropertyField(fitYProp, new GUIContent("Fit Cell Height", "Force cell height to fill the available row height."));

            if (currentChildRatio == DynamicGridLayout.ChildRatio.Free)
            {
                EditorGUILayout.HelpBox("With 'Free' ratio, children stretch to fill cell. 'Fit Cell' options have less effect.", MessageType.Info);
            }
            else if (currentChildRatio == DynamicGridLayout.ChildRatio.Fixed && fitXProp.boolValue && fitYProp.boolValue)
            {
                EditorGUILayout.HelpBox("Warning: Fitting both Width and Height with 'Fixed' ratio overrides the aspect ratio.", MessageType.Warning);
            }


            EditorGUI.indentLevel--;

        }
        else // --- Preset Specific Configuration ---
        {
            EditorGUILayout.LabelField("Preset Configuration", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            switch (currentPreset)
            {
                case DynamicGridLayout.Presets.vertical_list:
                    EditorGUILayout.HelpBox("Layout: Single column, height adjusts. Configure item aspect ratio.", MessageType.Info);
                    EditorGUILayout.PropertyField(fixedRatioProp, new GUIContent("Item Ratio (W/H)", "Aspect ratio for list items."));
                    EditorGUILayout.PropertyField(fitYProp, new GUIContent("Fit Item Height", "Allow item height to stretch beyond aspect ratio if needed?"));
                    break;

                case DynamicGridLayout.Presets.horizontal_list:
                    EditorGUILayout.HelpBox("Layout: Single row, width adjusts. Configure item aspect ratio.", MessageType.Info);
                    EditorGUILayout.PropertyField(fixedRatioProp, new GUIContent("Item Ratio (W/H)", "Aspect ratio for list items."));
                    EditorGUILayout.PropertyField(fitXProp, new GUIContent("Fit Item Width", "Allow item width to stretch beyond aspect ratio if needed?"));
                    break;

                case DynamicGridLayout.Presets.item_grid_v:
                    EditorGUILayout.HelpBox("Layout: Fixed columns, square items by default, height adjusts.", MessageType.Info);
                    EditorGUILayout.PropertyField(columnsProp, new GUIContent("Columns", "Number of columns in the grid."));
                    break;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // --- General Layout Settings ---
        EditorGUILayout.LabelField("General Layout", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(spacingProp, new GUIContent("Spacing", "Space between cells."), true);

        // Ensure base class properties are found before drawing them
        if (paddingProp != null)
        {
            EditorGUILayout.PropertyField(paddingProp, new GUIContent("Padding", "Space inside the container boundaries."), true);
        }
        else { Debug.LogError("DynamicGridLayoutEditor: Could not find 'm_Padding'."); } // Should not happen

        if (childAlignmentProp != null)
        {
            EditorGUILayout.PropertyField(childAlignmentProp, new GUIContent("Child Alignment", "How children are aligned within their allocated cell space."), true);
        }
        else { Debug.LogError("DynamicGridLayoutEditor: Could not find 'm_ChildAlignment'."); } // Should not happen

        EditorGUI.indentLevel--;


        // Apply changes if any occurred
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties(); // Apply changes to the object

            // Explicitly mark the layout as dirty to ensure immediate update in Editor
            foreach (var t in targets)
            { // Handle multi-object editing
                if (t is DynamicGridLayout layout) // Ensure target is of correct type
                {
                    if (layout != null && layout.transform is RectTransform rectTransform)
                    {
                        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
                    }
                }
            }
        }
    }
}