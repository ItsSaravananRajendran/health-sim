# 02 Architecture

## Project layout (Unity project at repo root)
```
Assets/
  Scenes/Clinic.unity
  Scripts/
    Core/        ScenarioController.cs  ScenarioState.cs  Scoring.cs
    Data/        FoodItemData.cs  PlateVariantData.cs  CounselingOptionData.cs
    Interaction/ FoodItemView.cs  DragController.cs  InspectPanel.cs
    UI/          IntroScreen.cs  CounselingPanel.cs  DebriefScreen.cs
  Content/       ScriptableObject assets (foods, plates, counseling options)
  Tests/
    EditMode/    ScoringTests.cs  PlateVariantTests.cs
    PlayMode/    ScenarioFlowTests.cs
Packages/
ProjectSettings/
docs/
```

## Key design decisions
- **Content as data.** Foods, plates and counseling options are ScriptableObjects, so an
  educator can add a plate without touching code. This is the main point to show in the demo.
- **Logic separate from visuals.** `Scoring` is a plain C# static class with no Unity
  dependencies, so it is tested fast in EditMode.
- **State machine.** `ScenarioController` moves Intro, Assess, Act, Feedback, Debrief. Each
  transition is a method with a guard, so illegal jumps are testable.
- **Primitives first.** Cubes, spheres and cylinders with colors stand in for food. Swap in
  free low-poly assets only after the loop works.

## Data model
- `FoodItemData`: `displayName`, `foodGroup` (Vegetable, Fruit, Grain, Protein, Dairy, Sweet,
  Fried), `portionUnits` (int), `color`.
- `PlateVariantData`: `title`, `items` (list of FoodItemData), `clientLine` (what the client
  says), `idealGroups` (ratios).
- `CounselingOptionData`: `text`, `quality` (Good, Neutral, Judgmental), `clientReaction`.

## Scoring contract
`Scoring.PlateBalance(IReadOnlyList<PlacedItem> items, Ratios target) -> int (0..60)`
`Scoring.Counseling(Quality q) -> int`
`Scoring.Total(int plate, int counsel, bool inspectedAll) -> int (0..100)`
