# 02 Architecture

## Project layout (Unity project at repo root)
```
Assets/
  Scenes/Clinic.unity
  Scripts/
    Core/        ScenarioController.cs  ScenarioState.cs  Scoring.cs  PlacedItem.cs
    Data/        FoodItemData.cs  PlateVariantData.cs  CounselingOptionData.cs
    Interaction/ FoodItemView.cs  DragController.cs  PlateArea.cs  InspectPanel.cs
    UI/          IntroScreen.cs  CounselingPanel.cs  FeedbackView.cs  DebriefScreen.cs
  Content/       ScriptableObject assets (foods, plates, counseling options)
  Tests/
    EditMode/    ScoringTests.cs  ScenarioStateTests.cs  PlateVariantTests.cs
    PlayMode/    ScenarioFlowTests.cs
Packages/
ProjectSettings/
docs/
```

## Key design decisions
- **Content as data.** Foods, plates and counseling options are ScriptableObjects, so an
  educator can add a plate without touching code. This is the main point to show in the demo.
- **Logic separate from visuals.** `Scoring`, `ScenarioState` and `PlacedItem` are plain C#
  with no Unity dependencies, so they are tested fast in EditMode. `PlacedItem` is a struct
  (`foodGroup`, `portionUnits`, `id`), not a reference to the ScriptableObject.
- **State machine.** `ScenarioState` holds the phase (Intro, Assess, Act, Feedback, Debrief),
  the inspected item ids, the plate contents and the chosen response. Each transition is a
  method with a guard that returns false and leaves the state unchanged on an illegal jump.
  `ScenarioController` is a thin MonoBehaviour that forwards scene events to it.
- **Plate locks on counseling.** `ChooseResponse` is only legal in Act. It snapshots the plate
  and moves to Feedback; after that, `PlateArea` rejects drops.
- **Input.** Unity 6 projects use the new Input System, where `OnMouseDown`/`OnMouseDrag` do
  not fire. Use the EventSystem instead: a `PhysicsRaycaster` on the camera and
  `IPointerClickHandler` / `IBeginDragHandler` / `IDragHandler` / `IEndDragHandler` on
  `FoodItemView`. The same path drives both clicks and drags.
- **Render pipeline.** Built-in, for a smaller and simpler WebGL build.
- **Primitives first.** Cubes, spheres and cylinders with colors stand in for food, each with a
  world-space TextMeshPro label, so foods are not told apart by color alone. Swap in free
  low-poly assets only after the loop works.

## Data model
- `FoodItemData`: `displayName`, `foodGroup` (Vegetable, Fruit, Grain, Protein, Dairy, Sweet,
  Fried), `portionUnits` (int), `color`, `isDrink` (goes to the cup spot).
- `PlateVariantData`: `title`, `plateItems` (starting plate), `trayItems` (foods that can be
  added), `clientLine` (what the client says), `counselingOptions` (exactly 3),
  `teachingPoint` (debrief text).
- `CounselingOptionData`: `text`, `quality` (Good, Neutral, Judgmental), `clientReaction`,
  `explanation` (debrief text).

The target ratios are one shared constant, `Ratios.MyPlate` (50/25/25/0), not per plate.

## Scoring contract
`Scoring.Breakdown(IReadOnlyList<PlacedItem> items, Ratios target) -> BucketShares`
`Scoring.PlateBalance(IReadOnlyList<PlacedItem> items, Ratios target) -> int (0..60)`
`Scoring.Counseling(Quality q) -> int`
`Scoring.Total(int plate, int counsel, bool inspectedAll) -> int (0..100)`

`Breakdown` returns each bucket's units and share. `PlateBalance` uses it, and the debrief
shows it. The formula is in `01-design.md`.
