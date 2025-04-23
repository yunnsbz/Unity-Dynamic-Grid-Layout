using UnityEngine.UI;
using UnityEngine;

[ExecuteAlways]
public class DynamicGridLayout : LayoutGroup
{
    /// <summary>
    /// Enum for the presets of the grid layout.
    /// </summary>
    public enum Presets {
        /// <summary>
        /// Custom preset allows the user to define their own layout.
        /// </summary>
        Custom, 

        vertical_list, 
        horizontal_list, 
        item_grid_v 
    }

    /// <summary>
    /// FitType enum defines how the layout will adjust the size of the cells in the grid.
    /// </summary>
    public enum FitType
    {
        /// <summary>
        /// Cells will be resized uniformly to fit within the available space. 
        /// </summary>
        UNIFORM,

        /// <summary>
        /// Cells will be resized to fit the width of the layout, maintaining their aspect ratio.
        /// </summary>
        WIDTH,

        /// <summary>
        /// Cells will be resized to fit the height of the layout, maintaining their aspect ratio.
        /// </summary>
        HEIGHT,

        /// <summary>
        /// The number of rows is fixed, and the layout will adjust the number of columns based on the available space.
        /// </summary>
        FIXED_ROWS,

        /// <summary>
        /// The number of columns is fixed, and the layout will adjust the number of rows based on the available space.
        /// </summary>
        FIXED_COLUMNS
    }

    /// <summary>
    /// defines how the child elements will be resized in relation to each other.
    /// </summary>
    public enum ChildRatio {

        /// <summary>
        /// All children will have the same size. and the same aspect ratio.
        /// </summary>
        Square,

        /// <summary>
        /// Children will have a fixed aspect ratio defined by fixedRatio.
        /// </summary>
        Fixed,

        /// <summary>
        /// Children will have their own size and aspect ratio.
        /// </summary>
        Free
    }


    // set the default values for the properties
    public Presets preset = Presets.Custom;
    public FitType fitType = FitType.UNIFORM;
    public ChildRatio childRatio = ChildRatio.Fixed;
    public Vector2 fixedRatio = new Vector2(4, 1);

    // fields for the layout
    public int rows;
    public int columns;
    public Vector2 cellSize;
    public Vector2 spacing;

    // for layout behaviors
    public bool fitX;
    public bool fitY;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (columns <= 0) columns = 1;
        if (rows <= 0) rows = 1;
        if (fixedRatio.x <= 0) fixedRatio.x = 1;
        if (fixedRatio.y <= 0) fixedRatio.y = 1;

        switch (preset)
        {
            case Presets.vertical_list:
                fitType = FitType.FIXED_COLUMNS;
                columns = 1;
                childRatio = ChildRatio.Fixed;
                fitY = true;
                fitX = true;
                break;
            case Presets.horizontal_list:
                fitType = FitType.FIXED_ROWS;
                rows = 1;
                columns = transform.childCount;
                fitY = true;
                fitX = true;
                break;
            case Presets.item_grid_v:
                fitType = FitType.FIXED_COLUMNS;
                rows = 4;
                childRatio = ChildRatio.Square;
                fitY = true;
                fitX = true;
                break;
        }

        if (fitType == FitType.WIDTH || fitType == FitType.HEIGHT || fitType == FitType.UNIFORM)
        {
            float squareRoot = Mathf.Sqrt(transform.childCount);
            rows = columns = Mathf.CeilToInt(squareRoot);
            switch (fitType)
            {
                case FitType.WIDTH:
                    fitX = true;
                    fitY = false;
                    break;
                case FitType.HEIGHT:
                    fitX = false;
                    fitY = true;
                    break;
                case FitType.UNIFORM:
                    fitX = true;
                    fitY = true;
                    rows = columns = Mathf.CeilToInt(squareRoot);

                    float _parentWidth = rectTransform.rect.width;
                    float _parentHeight = rectTransform.rect.height;

                    float _cellWidth = _parentWidth / (float)columns - ((spacing.x / (float)columns) * (columns - 1))
                        - (padding.left / (float)columns) - (padding.right / (float)columns);
                    float _cellHeight = _parentHeight / (float)rows - ((spacing.y / (float)rows) * (rows - 1))
                        - (padding.top / (float)rows) - (padding.bottom / (float)rows);

                    // Hücre boyutlarýný childRatio'ya göre ayarla
                    switch (childRatio)
                    {
                        case ChildRatio.Square:
                            cellSize.x = cellSize.y = Mathf.Min(_cellWidth, _cellHeight);
                            break;
                        case ChildRatio.Fixed:
                            // En küçük boyutu temel al ve oraný koru
                            float ratio = fixedRatio.x / fixedRatio.y;
                            if (_cellWidth / ratio <= _cellHeight)
                            {
                                cellSize.x = _cellWidth;
                                cellSize.y = _cellWidth / ratio;
                            }
                            else
                            {
                                cellSize.y = _cellHeight;
                                cellSize.x = _cellHeight * ratio;
                            }
                            break;
                        case ChildRatio.Free:
                            cellSize.x = _cellWidth;
                            cellSize.y = _cellHeight;
                            break;
                    }

                    // Layout boyutlarýný güncelle
                    float totalWidth = (cellSize.x * columns) + (spacing.x * (columns - 1)) + padding.left + padding.right;
                    float totalHeight = (cellSize.y * rows) + (spacing.y * (rows - 1)) + padding.top + padding.bottom;
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
                    break;
            }
        }

        if (fitType == FitType.FIXED_COLUMNS || fitType == FitType.WIDTH)
        {
            rows = Mathf.CeilToInt(transform.childCount / (float)columns);
        }
        if (fitType == FitType.FIXED_ROWS || fitType == FitType.HEIGHT)
        {
            columns = Mathf.CeilToInt(transform.childCount / (float)rows);
        }

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = parentWidth / (float)columns - ((spacing.x / (float)columns) * (columns - 1))
            - (padding.left / (float)columns) - (padding.right / (float)columns);
        float cellHeight = parentHeight / (float)rows - ((spacing.y / (float)rows) * (rows - 1))
            - (padding.top / (float)rows) - (padding.bottom / (float)rows);

        switch (childRatio)
        {
            case ChildRatio.Fixed:
                if (fitX) cellHeight = cellWidth * (fixedRatio.y / fixedRatio.x);
                if (fitY) cellWidth = cellHeight * (fixedRatio.x / fixedRatio.y);

                cellSize.y = cellWidth * (fixedRatio.y / (float)fixedRatio.x);
                cellSize.x = cellHeight * (fixedRatio.x / (float)fixedRatio.y);

                break;
            case ChildRatio.Square:
                if (fitX || fitY) cellHeight = cellWidth = Mathf.Min(cellWidth, cellHeight);
                break;
        }

        if (preset == Presets.Custom && !fitX && !fitY) { }
        else
        {
            if (fitX) cellSize.x = cellWidth;
            if (fitY) cellSize.y = cellHeight;
        }

        if (fitType == FitType.FIXED_COLUMNS || fitType == FitType.WIDTH)
        {
            float totalHeight = (cellHeight * rows) + (spacing.y * (rows - 1)) + padding.top + padding.bottom;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }
        if (fitType == FitType.FIXED_ROWS || fitType == FitType.HEIGHT)
        {
            float totalWidth = (cellWidth * columns) + (spacing.x * (columns - 1)) + padding.right + padding.left;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);

        }

        int columnCount = 0;
        int rowCount = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / columns;
            columnCount = i % columns;

            var item = rectChildren[i];
            var xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
            var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

            SetChildAlongAxis(item, 0, xPos, cellSize.x);
            SetChildAlongAxis(item, 1, yPos, cellSize.y);
        }

        // negative cellsize and grid size error detecting:
        if (cellSize.x <= 0 || cellSize.y <= 0)
        {
            var adjustmentX = 0 - cellSize.x;
            var adjustmentY = 0 - cellSize.y;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (adjustmentY * 2 * rows) + (spacing.y * (rows - 1)) + padding.top + padding.bottom);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (adjustmentX * 2 * columns) + (spacing.x * (columns - 1)) + padding.right + padding.left);

            if (cellSize.x <= 0 || cellSize.y <= 0)
            {
                cellSize.x = rectTransform.rect.width / (float)columns - ((spacing.x / (float)columns) * (columns - 1))
                    - (padding.left / (float)columns) - (padding.right / (float)columns);

                cellSize.y = rectTransform.rect.height / (float)rows - ((spacing.y / (float)rows) * (rows - 1))
                    - (padding.top / (float)rows) - (padding.bottom / (float)rows);

                for (int i = 0; i < rectChildren.Count; i++)
                {
                    rowCount = i / columns;
                    columnCount = i % columns;

                    var item = rectChildren[i];
                    var xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
                    var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

                    SetChildAlongAxis(item, 0, xPos, cellSize.x);
                    SetChildAlongAxis(item, 1, yPos, cellSize.y);
                }

            }
        }

    }

    // arbitrary methods to satisfy the LayoutGroup class
    public override void CalculateLayoutInputVertical() { }
    public override void SetLayoutHorizontal() { }
    public override void SetLayoutVertical() { }
}