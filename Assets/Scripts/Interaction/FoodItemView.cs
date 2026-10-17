using PlateCheck.Core;
using PlateCheck.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlateCheck.Interaction
{
    /// <summary>
    /// A food on the table: a small model (see FoodModels) with a world-space label. Clicks and drags come
    /// through the EventSystem (PhysicsRaycaster on the camera), since OnMouseDown does not fire
    /// with the new Input System.
    /// </summary>
    public class FoodItemView : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        const float ModelScale = 1.35f; // stylized: bigger than life so foods read at a glance
        const float HoverScale = 1.12f;

        static readonly Color LabelColor = Color.white;
        static readonly Color InspectedLabelColor = new Color(0.65f, 1f, 0.65f);

        public FoodItemData Food { get; private set; }
        public string Id => Food.Id;

        ScenarioController controller;
        DragController drag;
        TextMeshPro label;
        Transform cameraTransform;
        Transform model;
        bool dragging;
        bool hovered;

        public static FoodItemView Create(FoodItemData food, ScenarioController controller, DragController drag,
            Transform parent, Material labelMaterial)
        {
            var root = new GameObject("Food_" + food.Id);
            root.transform.SetParent(parent, false);

            var model = new GameObject("Model").transform;
            model.SetParent(root.transform, false);
            FoodModels.Build(food, model);
            model.localScale = Vector3.one * ModelScale;

            // One box collider around the whole model, for clicks and drags.
            var bounds = new Bounds(root.transform.position, Vector3.zero);
            foreach (var r in model.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            var collider = root.AddComponent<BoxCollider>();
            collider.center = root.transform.InverseTransformPoint(bounds.center);
            collider.size = bounds.size + new Vector3(0.01f, 0.01f, 0.01f);
            float height = bounds.max.y - root.transform.position.y;

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(root.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, height + 0.035f, 0f);
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
            view.model = model;
            return view;
        }

        void Start()
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (cameraTransform != null) label.transform.rotation = cameraTransform.rotation;
            // Grow a little under the pointer and while held, so it is clear what can be picked up.
            bool lifted = dragging || (hovered && controller.CanInteract);
            float target = lifted ? ModelScale * HoverScale : ModelScale;
            float scale = Mathf.MoveTowards(model.localScale.x, target, Time.unscaledDeltaTime * 1.5f);
            model.localScale = new Vector3(scale, scale, scale);
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;

        public void OnPointerExit(PointerEventData eventData) => hovered = false;

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
