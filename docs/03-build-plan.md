# 03 Build plan

Do the steps in order. Each step lists a done check. Commit after each step.

## Step 0. Environment
- `unity editors -i` shows 6000.6.6f1 with platform Web.
- `unity auth status` shows signed in.
- Done when both succeed. (Already true on this machine.)

## Step 1. Create the project
- Create a 3D (Built-in or URP) project in this repo's root with the Unity CLI
  (`unity --help`, `unity templates`, `unity open --help` list the exact flags for your CLI
  version) or from Unity Hub.
- Add a `.gitignore` for Unity (Library, Temp, Obj, Logs, UserSettings, Builds).
- Done when `unity open .` opens the empty project and the first commit contains `Assets`,
  `Packages`, `ProjectSettings`.

## Step 2. Core logic first (no scene)
- Write `Scoring.cs` and the data classes.
- Write EditMode tests (see test plan T1 to T8) before the visuals.
- Done when `unity test . --mode EditMode` passes.

## Step 3. Content
- Create ScriptableObject assets for about 12 foods, 4 plates (A to D), 3 counseling options
  per plate.
- Done when `PlateVariantTests` (T9, T10) passes.

## Step 4. Clinic scene
- Floor, walls, table, a plate (cylinder), a client (capsule plus sphere head), directional
  light, first-person or fixed camera.
- Done when pressing Play shows the room and the plate with no console errors.

## Step 5. Spawn and inspect food
- Instantiate `FoodItemView` objects from the active plate variant.
- Click shows `InspectPanel` with name, group and portion.
- Done when manual test M2 passes.

## Step 6. Drag to rearrange
- `DragController`: pick up a food item with the mouse, drop on or off the plate area.
- Done when manual test M3 passes and the plate area reports correct portion totals.

## Step 7. Counseling and feedback
- `CounselingPanel` shows 3 options. Choice shows the client reaction and stores quality.
- Done when manual test M4 passes.

## Step 8. State machine and UI screens
- Wire Intro, Assess, Act, Feedback, Debrief with `ScenarioController`.
- Done when PlayMode test T11 passes and a full run reaches Debrief.

## Step 9. Debrief and Try Again
- Show score breakdown and the missed items. Try Again loads the next variant.
- Done when manual test M5 passes for all four variants.

## Step 10. WebGL build
- Build for WebGL (see `docs/05-release.md`). Compression: Gzip or disabled for easy local
  hosting.
- Done when the build runs in Chrome and manual tests M1 to M7 pass.

## Step 11. Polish and publish
- Colors, labels, a short on-screen help line, demo banner "Not clinical guidance".
- Record a 60 to 90 second screen capture. Host a live link. Finish the README.
