# 03 Build plan

Do the steps in order. Each step lists a done check. Commit after each step.

## Step 0. Environment
- `unity editors -i` shows 6000.6.6f1 with platform Web.
- `unity auth status` shows signed in.
- Done when both succeed. (Already true on this machine.)

## Step 1. Create the project
- Create a 3D project with the **Built-in** render pipeline in this repo's root with the
  Unity CLI (`unity --help`, `unity templates`, `unity open --help` list the exact flags for
  your CLI version) or from Unity Hub.
- Confirm Player Settings, Active Input Handling is "Input System Package (New)" and the
  Input System package is installed (see the Input note in `02-architecture.md`).
- Add a `.gitignore` for Unity (Library, Temp, Obj, Logs, UserSettings, Builds).
- Done when `unity open .` opens the empty project and the first commit contains `Assets`,
  `Packages`, `ProjectSettings`.

## Step 2. Core logic first (no scene)
- Write `Scoring.cs`, `PlacedItem.cs`, `ScenarioState.cs` and the data classes.
- Write EditMode tests (test plan T1 to T12) before the visuals.
- Done when `unity test . --mode EditMode` passes.

## Step 3. Content
- Create the ScriptableObject assets listed in `01-design.md`: 12 foods, 4 plates (A to D),
  each with its plate items, tray items, teaching point and 3 counseling options.
- Done when `PlateVariantTests` (T13, T14) pass.

## Step 4. Clinic scene
- Floor, walls, table, a plate (cylinder) with a cup spot, a side tray, a client (capsule plus
  sphere head), directional light, fixed camera with a `PhysicsRaycaster`, an EventSystem.
- Done when pressing Play shows the room, plate and tray with no console errors.

## Step 5. Spawn and inspect food
- Instantiate `FoodItemView` objects from the active variant: plate items on the plate, tray
  items on the tray. Each has a world-space label.
- Click shows `InspectPanel` with name, group and portion, and records the inspection.
- Done when manual test M2 passes.

## Step 6. Drag to rearrange
- `DragController`: drag a food item with the EventSystem drag handlers, drop it in the plate
  area or on the tray. Drinks snap to the cup spot. Drops elsewhere return the item.
- `PlateArea` keeps the current `PlacedItem` list and shows live bucket totals.
- Dragging is off in Assess and after a response is chosen.
- Done when manual test M3 passes and the totals match `Scoring.Breakdown`.

## Step 7. Counseling and feedback
- `CounselingPanel` shows the plate's 3 options in Act. A choice locks the plate, stores the
  quality and shows the client reaction in `FeedbackView`.
- Done when manual test M4 passes.

## Step 8. State machine and UI screens
- Wire Intro, Assess, Act, Feedback, Debrief through `ScenarioController`, including the
  "Ready to help" and Continue buttons.
- Done when PlayMode tests T15, T17 and T18 pass and a full run reaches Debrief.

## Step 9. Debrief and Try Again
- Show the score breakdown and debrief text described in `01-design.md`. Try Again loads the
  next variant and wraps after D.
- Done when PlayMode test T16 passes and manual test M5 passes for all four variants.

## Step 10. WebGL build
- Build for WebGL (see `docs/05-release.md`). Set Compression Format to **Disabled** for local
  testing: `python3 -m http.server` does not send `Content-Encoding` headers, so a Gzip build
  fails to load there.
- Done when the build runs in Chrome and manual tests M1 to M8 pass.

## Step 11. Polish and publish
- Colors, a short on-screen help line, demo banner "Not clinical guidance".
- Record a 60 to 90 second screen capture. Host a live link. Finish the README.
