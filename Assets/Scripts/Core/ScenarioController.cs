using System.Collections.Generic;
using PlateCheck.Core;
using PlateCheck.Data;
using PlateCheck.Interaction;
using PlateCheck.UI;
using UnityEngine;

namespace PlateCheck
{
    /// <summary>
    /// Thin scene glue: forwards scene events to <see cref="ScenarioState"/>, which owns the rules,
    /// and refreshes the views after every change.
    /// </summary>
    public class ScenarioController : MonoBehaviour
    {
        [Tooltip("Played in order. Try Again moves to the next one and wraps.")]
        [SerializeField] PlateVariantData[] variants;

        [Header("Scene")]
        [SerializeField] Transform foodParent;
        [SerializeField] PlateArea plateArea;
        [SerializeField] TrayArea trayArea;
        [SerializeField] DragController dragController;
        [SerializeField] InspectPanel inspectPanel;
        [Tooltip("Material for the world-space food labels (TMP overlay shader with outline).")]
        [SerializeField] Material labelMaterial;

        [Header("UI")]
        [SerializeField] IntroScreen introScreen;
        [SerializeField] HudView hud;
        [SerializeField] CounselingPanel counselingPanel;
        [SerializeField] FeedbackView feedbackView;
        [SerializeField] DebriefScreen debriefScreen;

        readonly Dictionary<string, FoodItemView> views = new Dictionary<string, FoodItemView>();

        public ScenarioState State { get; private set; } = new ScenarioState();
        public int VariantIndex { get; private set; }
        public int VariantCount => variants.Length;
        public PlateVariantData CurrentVariant => variants[VariantIndex];
        public CounselingOptionData ChosenOption { get; private set; }
        public bool CanRearrange => !State.PlateLocked;
        public bool CanInteract => State.Phase == Phase.Assess || State.Phase == Phase.Act;
        public PlateArea PlateArea => plateArea;
        public TrayArea TrayArea => trayArea;
        public DebriefScreen DebriefScreen => debriefScreen;

        void Awake()
        {
            introScreen.startButton.onClick.AddListener(() => StartScenario());
            hud.readyButton.onClick.AddListener(() => ReadyToHelp());
            counselingPanel.OptionChosen += index => ChooseResponse(index);
            feedbackView.continueButton.onClick.AddListener(() => ContinueToDebrief());
            debriefScreen.tryAgainButton.onClick.AddListener(() => TryAgain());
            debriefScreen.quitButton.onClick.AddListener(() => Quit());
        }

        void Start() => Refresh();

        public FoodItemView View(string id) => views.TryGetValue(id, out var view) ? view : null;

        /// <summary>Choose which plate the next Start uses. Only on the intro screen.</summary>
        public bool SelectVariant(int index)
        {
            if (State.Phase != Phase.Intro || index < 0 || index >= variants.Length) return false;
            VariantIndex = index;
            return true;
        }

        /// <summary>Intro to Assess with the current variant.</summary>
        public bool StartScenario()
        {
            var variant = CurrentVariant;
            if (!State.Start(variant.PlatePlacedItems(), variant.TrayPlacedItems())) return false;
            SpawnFoods(variant);
            Refresh();
            return true;
        }

        public bool InspectItem(string id)
        {
            if (!State.Inspect(id)) return false;
            var view = views[id];
            inspectPanel.Show(view.Food, State.IsOnPlate(id));
            view.SetInspected(State.WasInspectedInAssess(id));
            Refresh();
            return true;
        }

        public bool ReadyToHelp()
        {
            if (!State.ReadyToHelp()) return false;
            Refresh();
            return true;
        }

        /// <summary>Move a food between tray and plate area. Rejected outside Act.</summary>
        public bool TryMoveItem(string id, bool toPlate)
        {
            bool moved = toPlate ? State.MoveToPlate(id) : State.MoveToTray(id);
            if (moved) Refresh();
            return moved;
        }

        /// <summary>
        /// Called when a drag ends. A drop in the plate area or on the tray moves the food there;
        /// anything else (or any drop once the plate is locked) puts it back where it was.
        /// </summary>
        public bool DropItem(FoodItemView view, Vector3 worldPosition)
        {
            bool moved = false;
            bool onPlate = State.IsOnPlate(view.Id);
            if (plateArea.Contains(worldPosition) && !onPlate) moved = TryMoveItem(view.Id, true);
            else if (trayArea.Contains(worldPosition) && onPlate) moved = TryMoveItem(view.Id, false);
            LayoutFoods(); // snaps the food into its slot, or back to where it came from
            return moved;
        }

        public bool ChooseResponse(int optionIndex)
        {
            var options = CurrentVariant.counselingOptions;
            if (optionIndex < 0 || optionIndex >= options.Count) return false;
            if (!State.ChooseResponse(options[optionIndex].quality)) return false;
            ChosenOption = options[optionIndex];
            Refresh();
            return true;
        }

        public bool ContinueToDebrief()
        {
            if (!State.Continue()) return false;
            Refresh();
            return true;
        }

        /// <summary>Debrief to Assess with the next variant, wrapping after the last one.</summary>
        public bool TryAgain()
        {
            if (!State.Reset()) return false;
            ClearRun();
            VariantIndex = (VariantIndex + 1) % variants.Length;
            return StartScenario();
        }

        /// <summary>Debrief back to the intro screen.</summary>
        public bool Quit()
        {
            if (!State.Reset()) return false;
            ClearRun();
            Refresh();
            return true;
        }

        void SpawnFoods(PlateVariantData variant)
        {
            foreach (var food in variant.plateItems) Spawn(food);
            foreach (var food in variant.trayItems) Spawn(food);
        }

        void Spawn(FoodItemData food) =>
            views[food.Id] = FoodItemView.Create(food, this, dragController, foodParent, labelMaterial);

        void ClearRun()
        {
            foreach (var view in views.Values) Destroy(view.gameObject);
            views.Clear();
            ChosenOption = null;
        }

        void LayoutFoods()
        {
            plateArea.Layout(ViewsFor(State.Plate));
            trayArea.Layout(ViewsFor(State.Tray));
        }

        List<FoodItemView> ViewsFor(IReadOnlyList<PlacedItem> items)
        {
            var list = new List<FoodItemView>(items.Count);
            foreach (var item in items) list.Add(views[item.id]);
            return list;
        }

        void Refresh()
        {
            var phase = State.Phase;
            LayoutFoods();

            introScreen.Show(phase == Phase.Intro);
            int inspected = State.StartingPlateIds.Count - State.MissedInspections().Count;
            hud.Show(phase, phase == Phase.Intro ? "" : CurrentVariant.clientLine, inspected, State.StartingPlateIds.Count);
            plateArea.ShowTotals(phase == Phase.Act, State.CurrentBreakdown());
            if (phase != Phase.Assess && phase != Phase.Act) inspectPanel.Hide();
            else if (inspectPanel.ShownId != null && views.TryGetValue(inspectPanel.ShownId, out var shown))
                inspectPanel.Show(shown.Food, State.IsOnPlate(shown.Id));

            if (phase == Phase.Act) counselingPanel.Show(CurrentVariant.counselingOptions);
            else counselingPanel.Hide();

            if (phase == Phase.Feedback) feedbackView.Show(ChosenOption);
            else feedbackView.Hide();

            if (phase == Phase.Debrief)
            {
                debriefScreen.Show($"Debrief: {CurrentVariant.title}",
                    DebriefReport.Build(State.Result, Ratios.MyPlate, ChosenOption.explanation, CurrentVariant.teachingPoint));
            }
            else debriefScreen.Hide();
        }
    }
}
