# Plate Check: a nutrition counseling simulation

A small browser-based 3D training simulation built in Unity (WebGL). A community health
worker assesses a virtual client's meal against basic portion guidance, adjusts the plate,
chooses counseling responses, and gets feedback.

Built as a portfolio piece for the UIUC Carle College of Medicine job "Computer Assistant -
Programmer / Web Developer" (health workforce training simulation, Unity and Web3D).

> Demo content only. Not clinical guidance. Portion rules follow the public USDA MyPlate idea
> (half vegetables and fruits, a quarter grains, a quarter protein).

## Documents

| File | What it covers |
|------|----------------|
| `docs/01-design.md` | Learning goal, scenario, scoring rules, content format |
| `docs/02-architecture.md` | Project layout, scripts, data model, design decisions |
| `docs/03-build-plan.md` | Step by step implementation plan with a done check per step |
| `docs/04-test-plan.md` | Automated and manual tests, and the exact commands |
| `docs/05-release.md` | WebGL build, hosting, README polish, resume wording |

## Environment

- Unity Editor 6000.6.6f1 with the Web (WebGL) module, installed through the Unity CLI
- Unity CLI 1.0.0-beta.13 (`unity`), signed in, Personal license
- Linux, Chrome for manual testing

## Status

Steps 1 to 10 of `docs/03-build-plan.md` are done: the full loop (intro, assess, act, feedback,
debrief, try again) plays in the editor and in a local WebGL build. Step 11 (polish, screen
recording, hosting) is next.

## Working on it

| Task | Command or menu |
|------|-----------------|
| EditMode tests (scoring, state, content, debrief) | `unity test . --mode EditMode --output results-edit.xml` |
| PlayMode tests (drives the Clinic scene) | `unity test . --mode PlayMode --output results-play.xml` |
| Rebuild content, scene and project settings | menu **Plate Check** or `unity run . -- -executeMethod PlateCheck.EditorTools.SceneBuilder.BuildAll` |
| WebGL build | `unity build . --target WebGL --execute-method PlateCheck.EditorTools.BuildScript.BuildWebGL -o Builds/WebGL` |
| Play the build locally | `cd Builds/WebGL && python3 -m http.server 8000`, open http://localhost:8000 |

- **Content** (foods, plates, counseling options) lives in `Assets/Content` as ScriptableObjects.
  Edit them in the Inspector. `ContentBuilder` only creates assets that are missing, so it never
  overwrites edits. If you change a plate, update the expected-score table in `docs/01-design.md`
  and `PlateVariantTests`.
- **The Clinic scene is generated** by `SceneBuilder`. Rebuilding it overwrites manual changes
  to the scene. Change the builder instead.
- Set `PLATECHECK_CAPTURE_DIR=/some/dir` when running PlayMode tests to save screenshots of each
  phase.
