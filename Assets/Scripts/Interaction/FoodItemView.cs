using PlateCheck.Core;
using PlateCheck.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlateCheck.Interaction
{
    /// <summary>
    /// A food on the table: a colored primitive with a world-space label. Clicks and drags come
    /// through the EventSystem (PhysicsRaycaster on the camera), since OnMouseDown does not fire
    /// with the new Input System.
    /// </summary>
    public class FoodItemView : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        static readonly Color LabelColor = Color.white;
        static readonly Color InspectedLabelColor = new Color(0.65f, 1f, 0.65f);

        public FoodItemData Food { get; private set; }
        public string Id => Food.Id;

        ScenarioController controller;
        DragController drag;
        TextMeshPro label;
        Transform cameraTransform;
        bool dragging;

        public static FoodItemView Create(FoodItemData food, ScenarioController controller, DragController drag,
            Transform parent, Material labelMaterial)
        {
            var root = new GameObject("Food_" + food.Id);
            root.transform.SetParent(parent, false);

            var shape = food.isDrink ? PrimitiveType.Cylinder
                : food.foodGroup == FoodGroup.Vegetable || food.foodGroup == FoodGroup.Fruit ? PrimitiveType.Sphere
                : PrimitiveType.Cube;
            var visual = GameObject.CreatePrimitive(shape);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);

            float size = 0.09f + 0.03f * food.portionUnits;
            Vector3 scale = food.isDrink ? new Vector3(0.08f, 0.04f + 0.02f * food.portionUnits, 0.08f)
                : shape == PrimitiveType.Cube ? new Vector3(size, size * 0.5f, size)
                : Vector3.one * size;
            visual.transform.localScale = scale;
            // Cylinder primitives are 2 units tall, the others 1.
            float height = shape == PrimitiveType.Cylinder ? scale.y * 2f : scale.y;
            visual.transform.localPosition = new Vector3(0f, height / 2f, 0f);
            visual.GetComponent<Renderer>().material.color = food.color;

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(root.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, height + 0.04f, 0f);
            var label = labelObject.AddComponent<TextMeshPro>();
            label.text = food.displayName;
            label.fontSize = 0.36f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.rectTransform.sizeDelta = new Vector2(0.4f, 0.08f);
            label.color = LabelColor;
            // An overlay material keeps labels readable when a nearer food is in front.
            if (labelMaterial != null) label.fontSharedMaterial = labelMaterial;

            var view = root.AddComponent<FoodItemView>();
            view.Food = food;
            view.controller = controller;
            view.drag = drag;
            view.label = label;
            return view;
        }

        void Start()
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (cameraTransform != null) label.transform.rotation = cameraTransform.rotation;
        }

        public void SetInspected(bool inspected) => label.color = inspected ? InspectedLabelColor : LabelColor;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (dragging || eventData.dragging) return;
            controller.InspectItem(Id);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!controller.CanRearrange)
            {
                eventData.pointerDrag = null; // no OnDrag/OnEndDrag for this gesture
                return;
            }
            dragging = true;
            drag.Begin(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragging) drag.Move(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging) return;
            dragging = false;
            drag.End(this, eventData);
        }
    }
}
