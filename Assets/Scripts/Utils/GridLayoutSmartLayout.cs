using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GridLayoutSmartLayout : MonoBehaviour
{
    [Range(1,10)]
    [SerializeField] private int columnCount = 1;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private bool matchVertical;

    [SerializeField] private Vector2 scaling = Vector2.one;
    public int ColumnCount => columnCount;

    private void LateUpdate()
    {
        if (gridLayout == null)
        {
            return;
        }
        if (!matchVertical)
        {
            var size = (gridLayout.transform as RectTransform).rect.size;
            var padding = gridLayout.padding.left + gridLayout.padding.right;
            var spacing = (columnCount - 1) * gridLayout.spacing;
            var cellSize = (size.x - padding - spacing.x) / columnCount;
            gridLayout.cellSize = new Vector2(cellSize * scaling.x, cellSize * scaling.y);
        }
        else
        {
            var size = (gridLayout.transform as RectTransform).rect.size;
            var padding = gridLayout.padding.bottom + gridLayout.padding.top;
            var spacing = (columnCount - 1) * gridLayout.spacing;
            var cellSize = (size.y - padding - spacing.y) / columnCount;
            gridLayout.cellSize = new Vector2(cellSize * scaling.x, cellSize * scaling.y);
        }

    }
}
