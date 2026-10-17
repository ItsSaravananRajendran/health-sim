using UnityEngine;
using UnityEngine.EventSystems;

namespace PlateCheck.Interaction
{
    /// <summary>Moves a dragged food across the table top and hands the drop to the controller.</summary>
    public class DragController : MonoBehaviour
    {
        [SerializeField] Camera viewCamera;
        [SerializeField] ScenarioController controller;
        [Tooltip("Height of the table surface the food slides on.")]
        [SerializeField] float tableHeight = 0.81f;
        [SerializeField] float liftWhileDragging = 0.06f;

        Vector3 grabOffset;

        public void Begin(FoodItemView item, PointerEventData eventData)
        {
            grabOffset = Vector3.zero;
            if (TryProject(eventData.position, out var point))
                grabOffset = new Vector3(item.transform.position.x - point.x, 0f, item.transform.position.z - point.z);
        }

        public void Move(FoodItemView item, PointerEventData eventData)
        {
            if (!TryProject(eventData.position, out var point)) return;
            var position = point + grabOffset;
            position.y = tableHeight + liftWhileDragging;
            item.transform.position = position;
        }

        public void End(FoodItemView item, PointerEventData eventData)
        {
            var drop = item.transform.position;
            drop.y = tableHeight;
            controller.DropItem(item, drop);
        }

        bool TryProject(Vector2 screenPosition, out Vector3 point)
        {
            var ray = viewCamera.ScreenPointToRay(screenPosition);
            var table = new Plane(Vector3.up, new Vector3(0f, tableHeight, 0f));
            if (table.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = default;
            return false;
        }
    }
}
