using System.Collections.Generic;
using UnityEngine;

namespace PlateCheck.Interaction
{
    /// <summary>The side tray: foods the learner can add, and where removed foods go.</summary>
    public class TrayArea : MonoBehaviour
    {
        [Tooltip("The tray surface. Its scale on X and Z is the drop area.")]
        [SerializeField] Transform surface;
        [SerializeField] int columns = 3;
        [SerializeField] float margin = 0.05f;

        public Vector3 Center => surface.position;

        public bool Contains(Vector3 worldPosition)
        {
            var local = worldPosition - surface.position;
            var half = surface.lossyScale / 2f;
            return Mathf.Abs(local.x) <= half.x + margin && Mathf.Abs(local.z) <= half.z + margin;
        }

        public void Layout(IReadOnlyList<FoodItemView> items)
        {
            var size = surface.lossyScale;
            int rows = Mathf.Max(3, Mathf.CeilToInt(items.Count / (float)columns));
            float cellX = size.x / columns;
            float cellZ = size.z / rows;
            for (int i = 0; i < items.Count; i++)
            {
                int column = i % columns;
                int row = i / columns;
                var offset = new Vector3(
                    -size.x / 2f + cellX * (column + 0.5f), 0f,
                    -size.z / 2f + cellZ * (row + 0.5f)); // front row first, clear of the side panels
                items[i].transform.position = new Vector3(surface.position.x, surface.position.y + size.y / 2f, surface.position.z) + offset;
            }
        }
    }
}
