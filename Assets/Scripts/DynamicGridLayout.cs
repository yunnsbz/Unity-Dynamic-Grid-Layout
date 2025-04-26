using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic; // Required for List
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))] // Ensure there's always a RectTransform
public class DynamicGridLayout : LayoutGroup
{
    #region Enums
    public enum Presets
    {
        Custom,
        vertical_list,
        horizontal_list,
        item_grid_v // Vertical Item Grid is common
    }

    public enum FitType
    {
        /// <summary>
        /// Cells are sized uniformly based on the smaller dimension of the available space.
        /// </summary>
        UNIFORM,
        /// <summary>
        /// Cell width is determined by available width and number of columns. Height adjusts based on ChildRatio. Container height adjusts.
        /// </summary>
        WIDTH,
        /// <summary>
        /// Cell height is determined by available height and number of rows. Width adjusts based on ChildRatio. Container width adjusts.
        /// </summary>
        HEIGHT,
        /// <summary>
        /// Fixed number of rows. Columns adjust based on child count. Container width adjusts. Cell size respects ChildRatio.
        /// </summary>
        FIXED_ROWS,
        /// <summary>
        /// Fixed number of columns. Rows adjust based on child count. Container height adjusts. Cell size respects ChildRatio.
        /// </summary>
        FIXED_COLUMNS
    }

    public enum ChildRatio
    {
        /// <summary>
        /// Children aim for a 1:1 aspect ratio, fitting within the calculated cell slot.
        /// </summary>
        Square,
        /// <summary>
        /// Children maintain a fixed aspect ratio defined by fixedRatio, fitting within the calculated cell slot.
        /// </summary>
        Fixed,
        /// <summary>
        /// Children stretch to fill the calculated cell slot dimensions (unless overridden by fitX/fitY).
        /// </summary>
        Free
    }

    #endregion

    #region Public Fields (Serialized)

    [Header("Grid Configuration")]
    [Tooltip("Select a predefined grid layout preset or choose Custom to define your own.")]
    public Presets preset = Presets.Custom;

    [Tooltip("Determine how the grid cells are arranged and sized.")]
    public FitType fitType = FitType.UNIFORM;

    [Tooltip("Number of rows (used if FitType is FIXED_ROWS or influences calculations in other modes).")]
    public int rows = 1;

    [Tooltip("Number of columns (used if FitType is FIXED_COLUMNS or influences calculations in other modes).")]
    public int columns = 1;

    [Header("Child Sizing")]
    [Tooltip("Defines how child elements will be resized in relation to each other.")]
    public ChildRatio childRatio = ChildRatio.Fixed;

    [Tooltip("The fixed aspect ratio (Width/Height) to maintain for children when ChildRatio is Fixed.")]
    public Vector2 fixedRatio = new Vector2(1, 1); // Default to square for safety

    [Tooltip("Forces cell width to fill the available horizontal space per column.")]
    public bool fitX = true;

    [Tooltip("Forces cell height to fill the available vertical space per row.")]
    public bool fitY = true;


    [Header("Layout Properties")]
    [Tooltip("Spacing between grid cells.")]
    public Vector2 spacing = Vector2.zero;

    // Note: Padding is inherited from LayoutGroup (m_Padding)

    // Note: cellSize is now calculated internally, not serialized directly unless needed for debugging
    [HideInInspector] public Vector2 calculatedCellSize;

    #endregion

    #region Layout Calculation

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (!isActiveAndEnabled) return;

        ApplyPresetSettings();
        SanitizeInputs();

        int childCount = rectChildren.Count;
        if (childCount == 0)
        {
            // No children, potentially reset size if needed, or just do nothing
            // Resetting size might be disruptive if the user set it manually.
            // Let's just ensure calculatedCellSize is zero.
            calculatedCellSize = Vector2.zero;
            // Optionally, reset container size based on padding:
            // SetLayoutSize(padding.horizontal, padding.vertical);
            return;
        }

        int currentRows = rows;
        int currentColumns = columns;

        // 1. Determine Grid Dimensions (Rows & Columns)
        CalculateGridDimensions(childCount, ref currentRows, ref currentColumns);

        // Ensure we have at least one row and column if there are children
        if (currentRows <= 0) currentRows = 1;
        if (currentColumns <= 0) currentColumns = 1;

        // 2. Calculate Available Space & Base Cell Size
        float availableWidth = rectTransform.rect.width - padding.horizontal;
        float availableHeight = rectTransform.rect.height - padding.vertical;

        float totalSpacingWidth = Mathf.Max(0, spacing.x * (currentColumns - 1));
        float totalSpacingHeight = Mathf.Max(0, spacing.y * (currentRows - 1));

        float widthPerCellRaw = (availableWidth - totalSpacingWidth) / (float)currentColumns;
        float heightPerCellRaw = (availableHeight - totalSpacingHeight) / (float)currentRows;

        // Prevent negative sizes due to excessive spacing/padding
        widthPerCellRaw = Mathf.Max(0, widthPerCellRaw);
        heightPerCellRaw = Mathf.Max(0, heightPerCellRaw);


        // 3. Determine Actual Cell Size based on FitType, ChildRatio, fitX/Y
        calculatedCellSize = CalculateCellSize(widthPerCellRaw, heightPerCellRaw);

        // 4. Update Container Size if necessary (for fixed rows/columns or width/height fit types)
        UpdateContainerSize(currentRows, currentColumns, calculatedCellSize);

        // 5. Position Children
        PositionChildren(currentRows, currentColumns, calculatedCellSize);
    }

    /// <summary>
    /// Applies settings based on the selected preset, overriding other settings.
    /// </summary>
    private void ApplyPresetSettings()
    {
        // Store previous values to detect changes if needed (optional)
        // FitType prevFitType = fitType;
        // int prevRows = rows;
        // int prevColumns = columns;
        // ChildRatio prevChildRatio = childRatio;
        // bool prevFitX = fitX;
        // bool prevFitY = fitY;

        switch (preset)
        {
            case Presets.vertical_list:
                fitType = FitType.FIXED_COLUMNS;
                columns = 1;
                childRatio = ChildRatio.Fixed; // Often desired for lists, but could be Free
                fitX = true; // List items usually fill width
                             // fitY can be true or false depending on desired behavior (stretch or use ratio)
                             // Keep user's fitY unless specifically overridden by preset needs
                break;

            case Presets.horizontal_list:
                fitType = FitType.FIXED_ROWS;
                rows = 1;
                childRatio = ChildRatio.Fixed; // Often desired for lists
                                               // fitX can be true or false
                fitY = true; // List items usually fill height
                break;

            case Presets.item_grid_v:
                fitType = FitType.FIXED_COLUMNS; // Common grid type
                                                 // 'columns' should be set by user in inspector for this preset
                childRatio = ChildRatio.Square; // Common for item grids
                fitX = true; // Usually desired for grids
                fitY = true;
                break;

            case Presets.Custom:
            default:
                // No changes needed, use inspector values
                break;
        }

        // Optional: If preset changed settings, mark layout dirty
        // This happens automatically because modifying serialized properties marks dirty
    }

    /// <summary>
    /// Ensures key input values are valid before calculations.
    /// </summary>
    private void SanitizeInputs()
    {
        // Prevent division by zero or invalid states
        if (columns <= 0) columns = 1;
        if (rows <= 0) rows = 1;
        if (fixedRatio.x <= 0) fixedRatio.x = 1;
        if (fixedRatio.y <= 0) fixedRatio.y = 1;
    }

    /// <summary>
    /// Calculates the number of rows and columns based on FitType and child count.
    /// Modifies the passed-in row/col refs.
    /// </summary>
    private void CalculateGridDimensions(int childCount, ref int currentRows, ref int currentColumns)
    {
        switch (fitType)
        {
            case FitType.UNIFORM:
            case FitType.WIDTH:
            case FitType.HEIGHT:
                // For these types, calculate a balanced grid initially
                // Actual fitting happens during cell size calculation
                if (childCount > 0)
                {
                    float sqrtChildren = Mathf.Sqrt(childCount);
                    // Prioritize columns slightly for width/uniform, rows for height? Or just balanced? Let's go balanced.
                    currentRows = Mathf.CeilToInt(sqrtChildren);
                    currentColumns = Mathf.CeilToInt((float)childCount / currentRows);
                    // Re-evaluate if a more balanced grid is possible (e.g., 12 children -> 3x4 or 4x3)
                    if (currentRows > 1 && currentColumns > 1)
                    {
                        int potentialCols = Mathf.CeilToInt(sqrtChildren);
                        int potentialRows = Mathf.CeilToInt((float)childCount / potentialCols);
                        // Choose the configuration with fewer rows generally, or closer aspect ratio
                        if (potentialRows < currentRows)
                        {
                            currentRows = potentialRows;
                            currentColumns = potentialCols;
                        }
                        else if (potentialRows == currentRows && potentialCols < currentColumns)
                        {
                            currentColumns = potentialCols; // If rows are same, prefer fewer columns
                        }
                    }

                }
                else
                {
                    currentRows = 1;
                    currentColumns = 1;
                }
                break;

            case FitType.FIXED_ROWS:
                if (currentRows <= 0) currentRows = 1; // Already sanitized, but double-check
                currentColumns = (childCount > 0) ? Mathf.CeilToInt((float)childCount / currentRows) : 1;
                break;

            case FitType.FIXED_COLUMNS:
                if (currentColumns <= 0) currentColumns = 1; // Already sanitized, but double-check
                currentRows = (childCount > 0) ? Mathf.CeilToInt((float)childCount / currentColumns) : 1;
                break;

            default:
                // Should not happen
                currentRows = Mathf.Max(1, currentRows);
                currentColumns = Mathf.Max(1, currentColumns);
                break;
        }
        // Update the public fields for inspector visibility, careful not to cause loops if editor updates trigger layout
        // Only update if they were actually calculated (not fixed types)
        if (fitType != FitType.FIXED_ROWS && fitType != FitType.FIXED_COLUMNS)
        {
            rows = currentRows;
            columns = currentColumns;
        }
    }


    /// <summary>
    /// Calculates the final cell size based on available space per cell and constraints.
    /// </summary>
    private Vector2 CalculateCellSize(float widthPerCellRaw, float heightPerCellRaw)
    {
        Vector2 finalCellSize = new Vector2(widthPerCellRaw, heightPerCellRaw);

        float ratio = (fixedRatio.y == 0) ? 1.0f : fixedRatio.x / fixedRatio.y; // width/height ratio

        switch (childRatio)
        {
            case ChildRatio.Square:
                // Fit square within the available slot
                float squareSize = Mathf.Min(widthPerCellRaw, heightPerCellRaw);
                finalCellSize.x = fitX ? widthPerCellRaw : squareSize; // Allow stretching if fitX/Y is true
                finalCellSize.y = fitY ? heightPerCellRaw : squareSize;
                // If both fitX/Y, make it a square based on the smaller dim, unless one dim is much larger
                if (fitX && fitY)
                {
                    finalCellSize.x = finalCellSize.y = Mathf.Min(widthPerCellRaw, heightPerCellRaw);
                }
                else if (fitX)
                { // Only fit X, Y determined by square aspect
                    finalCellSize.y = widthPerCellRaw; // If fitting X, Y should match for square
                }
                else if (fitY)
                { // Only fit Y, X determined by square aspect
                    finalCellSize.x = heightPerCellRaw; // If fitting Y, X should match for square
                }
                else
                { // No fitting, use smallest square
                    finalCellSize.x = finalCellSize.y = Mathf.Min(widthPerCellRaw, heightPerCellRaw);
                }

                break;

            case ChildRatio.Fixed:
                // Fit fixed ratio within the available slot
                if (ratio > 0) // Valid ratio
                {
                    // Check if fitting based on width works within height constraint
                    if (widthPerCellRaw / ratio <= heightPerCellRaw)
                    {
                        // Width is the limiting factor
                        finalCellSize.x = widthPerCellRaw;
                        finalCellSize.y = widthPerCellRaw / ratio;
                    }
                    else // Height is the limiting factor
                    {
                        finalCellSize.y = heightPerCellRaw;
                        finalCellSize.x = heightPerCellRaw * ratio;
                    }
                }
                else // Invalid ratio, treat as Free? Or Square? Let's default to raw slot size.
                {
                    finalCellSize.x = widthPerCellRaw;
                    finalCellSize.y = heightPerCellRaw;
                }

                // Apply fitX/fitY overrides - stretch *if* allowed AND desired ratio doesn't already fill it
                if (fitX && finalCellSize.x < widthPerCellRaw - 0.01f) // Use a small epsilon
                    finalCellSize.x = widthPerCellRaw; // Stretch X
                if (fitY && finalCellSize.y < heightPerCellRaw - 0.01f)
                    finalCellSize.y = heightPerCellRaw; // Stretch Y

                // If stretching one dimension due to fitX/Y, should the other readjust for Fixed ratio?
                // Common use case: Fit width, height adjusts by ratio. Fit height, width adjusts by ratio.
                // Let's refine this:
                if (fitX && !fitY)
                { // Fit width, calculate height from ratio
                    finalCellSize.x = widthPerCellRaw;
                    finalCellSize.y = (ratio > 0) ? widthPerCellRaw / ratio : heightPerCellRaw;
                }
                else if (!fitX && fitY)
                { // Fit height, calculate width from ratio
                    finalCellSize.y = heightPerCellRaw;
                    finalCellSize.x = (ratio > 0) ? heightPerCellRaw * ratio : widthPerCellRaw;
                }
                else if (fitX && fitY)
                { // Fit both - this overrides fixed ratio, acts like Free
                    finalCellSize.x = widthPerCellRaw;
                    finalCellSize.y = heightPerCellRaw;
                }
                else
                { // !fitX && !fitY - Fit ratio within bounds (calculated above)
                  // Recalculate here for clarity, ensuring it fits within slot
                    if (ratio > 0)
                    {
                        if (widthPerCellRaw / ratio <= heightPerCellRaw)
                        {
                            finalCellSize.x = widthPerCellRaw; finalCellSize.y = widthPerCellRaw / ratio;
                        }
                        else
                        {
                            finalCellSize.y = heightPerCellRaw; finalCellSize.x = heightPerCellRaw * ratio;
                        }
                    }
                    else
                    {
                        // Fallback if ratio is invalid when not fitting
                        finalCellSize.x = widthPerCellRaw; finalCellSize.y = heightPerCellRaw;
                    }
                }

                break;

            case ChildRatio.Free:
            default:
                // Use the raw slot size, unless fitX/Y are false
                finalCellSize.x = fitX ? widthPerCellRaw : 0; // Start with 0 if not fitting
                finalCellSize.y = fitY ? heightPerCellRaw : 0;
                // If not fitting, we need *some* size. What should it be?
                // Option 1: Use preferred size of children (complicated with LayoutGroup)
                // Option 2: Fallback to some minimum or the raw size? Let's use raw size as the base.
                if (!fitX) finalCellSize.x = widthPerCellRaw; // Revert to calculated slot size if not fitting
                if (!fitY) finalCellSize.y = heightPerCellRaw;
                // This makes fitX/fitY less meaningful for 'Free'. Perhaps 'Free' should always fill?
                // Let's redefine: Free always tries to fill the slot.
                finalCellSize.x = widthPerCellRaw;
                finalCellSize.y = heightPerCellRaw;
                break;
        }

        // Ensure non-negative size as a final safety check
        finalCellSize.x = Mathf.Max(0, finalCellSize.x);
        finalCellSize.y = Mathf.Max(0, finalCellSize.y);

        return finalCellSize;
    }

    /// <summary>
    /// Adjusts the RectTransform's size based on calculated content size for specific FitTypes.
    /// </summary>
    private void UpdateContainerSize(int currentRows, int currentColumns, Vector2 finalCellSize)
    {
        if (currentRows <= 0 || currentColumns <= 0) return; // Nothing to size

        float requiredWidth = padding.horizontal + (finalCellSize.x * currentColumns) + Mathf.Max(0, spacing.x * (currentColumns - 1));
        float requiredHeight = padding.vertical + (finalCellSize.y * currentRows) + Mathf.Max(0, spacing.y * (currentRows - 1));

        // Resize the container only if the FitType implies the container size is determined by content
        bool resizeHorizontal = (fitType == FitType.HEIGHT || fitType == FitType.FIXED_ROWS);
        bool resizeVertical = (fitType == FitType.WIDTH || fitType == FitType.FIXED_COLUMNS);

        // Don't resize if UNIFORM, as it's meant to fit within the *existing* container.
        if (fitType == FitType.UNIFORM)
        {
            resizeHorizontal = false;
            resizeVertical = false;
        }

        // Only resize if the calculated size is different (within a tolerance)
        // to avoid unnecessary layout cycles.
        const float tolerance = 0.01f;

        if (resizeHorizontal && !Mathf.Approximately(rectTransform.rect.width, requiredWidth))
        {
            // Only set size if it's positive to avoid errors
            if (requiredWidth > tolerance)
            {
                SetLayoutInputForAxis(requiredWidth, requiredWidth, -1, 0); // Set preferred width
            }
            //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, requiredWidth);

        }
        if (resizeVertical && !Mathf.Approximately(rectTransform.rect.height, requiredHeight))
        {
            if (requiredHeight > tolerance)
            {
                SetLayoutInputForAxis(requiredHeight, requiredHeight, -1, 1); // Set preferred height
            }
            //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredHeight);
        }

        // Handle cases where fitting is disabled for Free ratio (less common)
        // If !fitX or !fitY maybe the container should shrink? This is complex.
        // Let's assume container size is either fixed or determined by FIXED_ROWS/COLS/WIDTH/HEIGHT FitTypes.
    }


    /// <summary>
    /// Sets the position and size of each child RectTransform.
    /// </summary>
    private void PositionChildren(int currentRows, int currentColumns, Vector2 finalCellSize)
    {
        if (finalCellSize.x <= 0 && finalCellSize.y <= 0)
        {
            // If cell size is zero (e.g., zero children or excessive padding),
            // still loop through children but set their size to zero to hide them cleanly.
            // This prevents potential errors in SetChildAlongAxis with negative sizes.
            // Alternatively, disable children? Setting size to zero is usually sufficient.
            finalCellSize = Vector2.zero; // Ensure it's exactly zero for the loop.
                                          // Log a warning if this happens unexpectedly
            if (rectChildren.Count > 0 && (m_Padding.horizontal + Mathf.Max(0, spacing.x * (currentColumns - 1)) > rectTransform.rect.width || m_Padding.vertical + Mathf.Max(0, spacing.y * (currentRows - 1)) > rectTransform.rect.height))
            {
                Debug.LogWarning($"DynamicGridLayout '{gameObject.name}': Calculated cell size is zero or negative. Check Padding and Spacing values relative to the container size.", this);
            }
        }


        int childIndex = 0;
        for (int r = 0; r < currentRows; r++)
        {
            for (int c = 0; c < currentColumns; c++)
            {
                if (childIndex >= rectChildren.Count) break; // Stop if we run out of children

                RectTransform child = rectChildren[childIndex];
                if (child == null)
                {
                    childIndex++;
                    continue; // Skip null children if any exist in the list
                }

                // Calculate position
                float xPos = padding.left + (finalCellSize.x + spacing.x) * c;
                // Unity UI Y-axis is typically top-down relative to anchor, but LayoutGroups work bottom-up origin (0,0 is bottom-left)
                float yPos = padding.top + (finalCellSize.y + spacing.y) * r;

                // Apply alignment within the cell (using LayoutGroup properties)
                // The base LayoutGroup handles alignment based on m_ChildAlignment
                // We just need to provide the position and size. The anchor points of children matter here.
                // Assuming children anchor to their centers (0.5, 0.5) or corners (0,0 / 1,1 etc.)
                // SetChildAlongAxis handles positioning relative to the container's bottom-left origin.

                // Use SetChildAlongAxis for positioning and sizing correctly within the LayoutGroup system
                SetChildAlongAxis(child, 0, xPos, finalCellSize.x); // Axis 0 is Horizontal
                SetChildAlongAxis(child, 1, yPos, finalCellSize.y); // Axis 1 is Vertical

                childIndex++;
            }
            if (childIndex >= rectChildren.Count) break;
        }

        // Disable remaining children if grid size is smaller than child count?
        // Generally layout groups just position the children they manage (rectChildren)
        // and don't disable others. Let's stick to that.
    }

    #endregion

    #region LayoutGroup Overrides (Required)
    
    public override void CalculateLayoutInputVertical() { }

    public override void SetLayoutHorizontal()
    {
        // calling positioning again for safety.
        if (!isActiveAndEnabled) return;
        // Recalculate necessary components or retrieve cached values
        int childCount = rectChildren.Count;
        if (childCount == 0) return;
        int currentRows = rows;
        int currentColumns = columns;
        CalculateGridDimensions(childCount, ref currentRows, ref currentColumns);
        if (currentRows <= 0) currentRows = 1;
        if (currentColumns <= 0) currentColumns = 1;
        // We need calculatedCellSize from the horizontal pass.
        // If CalculateLayoutInputHorizontal guarantees it's up-to-date, we can use it.
        PositionChildren(currentRows, currentColumns, calculatedCellSize);
    }

    public override void SetLayoutVertical()
    {
        // calling positioning again for safety.
        if (!isActiveAndEnabled) return;
        int childCount = rectChildren.Count;
        if (childCount == 0) return;
        int currentRows = rows;
        int currentColumns = columns;
        CalculateGridDimensions(childCount, ref currentRows, ref currentColumns);
        if (currentRows <= 0) currentRows = 1;
        if (currentColumns <= 0) currentColumns = 1;
        PositionChildren(currentRows, currentColumns, calculatedCellSize);
    }
    
    // Helper to potentially drive ContentSizeFitter if attached to parent
    // private void SetLayoutSize(float width, float height)
    // {
    //     SetLayoutInputForAxis(width, width, -1, 0);  // Set preferred width
    //     SetLayoutInputForAxis(height, height, -1, 1); // Set preferred height
    // }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }

    // On Validate ensures changes in inspector trigger updates
    protected override void OnValidate()
    {
        base.OnValidate();
        SetDirty();
    }

    #endregion

}