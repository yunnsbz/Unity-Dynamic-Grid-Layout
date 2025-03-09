
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DynamicGridLayout : LayoutGroup
{
    public enum FitType
    {
        UNIFORM,
        WIDTH,
        HEIGHT,
        FIXED_ROWS,
        FIXED_COLUMNS
    }

    public enum ChildRatio
    {
        Square,
        Fixed,
        Free
    }

    public FitType fitType = FitType.UNIFORM;
    public ChildRatio childRatio = ChildRatio.Fixed;
    public Vector2 fixedRatio = new Vector2(4, 1);

    public int rows;
    public int columns;
    public Vector2 cellSize;
    public Vector2 spacing;

    public bool fitX;
    public bool fitY;
    public bool resizeX;
    public bool resizeY;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

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
                    fitX = fitY = true;
                    break;
            }
        }
        if (columns > 0 && rows > 0)
        {
            if (fitType == FitType.WIDTH || fitType == FitType.FIXED_COLUMNS)
            {
                rows = Mathf.CeilToInt(transform.childCount / (float)columns);
            }
            if (fitType == FitType.HEIGHT || fitType == FitType.FIXED_ROWS)
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
                    break;

                case ChildRatio.Square: 
                    if (fitX || fitY) cellHeight = cellWidth = Mathf.Min(cellWidth, cellHeight);
                    break;

                case ChildRatio.Free:
                    //fitX = fitY = false;
                    break;
            }

            if(fitX) cellSize.x = cellWidth;
            if(fitY) cellSize.y = cellHeight;

            if (fitType == FitType.WIDTH || fitType == FitType.FIXED_COLUMNS)
            {
                float totalHeight = (cellHeight * rows) + (spacing.y * (rows - 1)) + padding.top + padding.bottom;

                // RectTransform'un yüksekliðini ayarla
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
            }
            if (fitType == FitType.HEIGHT || fitType == FitType.FIXED_ROWS)
            {
                float totalWidth = (cellWidth * columns) + (spacing.x * (columns - 1)) + padding.right + padding.left;

                // RectTransform'un yüksekliðini ayarla
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
        }
        if(columns <= 0)
        {
            columns = 1;
        }
        if (rows <= 0)
        {
            rows = 1;
        }
    }

    public override void CalculateLayoutInputVertical()
    {
    }


    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
    }


}