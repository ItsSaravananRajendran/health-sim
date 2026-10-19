# 04 Test plan

## Automated (Unity Test Framework, run with the CLI)

```bash
unity test . --mode EditMode --output results-edit.xml
unity test . --mode PlayMode --output results-play.xml
```

### EditMode (fast, pure logic)
| ID | Test | Expected |
|----|------|----------|
| T1 | Perfect plate (50/25/25) | PlateBalance = 60 |
| T2 | Empty plate | PlateBalance = 0 |
| T3 | Plate all grains | PlateBalance = 15 |
| T4 | Plate all Sweet and Fried | PlateBalance = 0 |
| T5 | Perfect plate plus Milk | PlateBalance = 60 (Dairy not counted) |
| T6 | Counseling Good / Neutral / Judgmental | 30 / 15 / 0 |
| T7 | Total with all components maxed | 100 |
| T8 | Total with inspectedAll false | 10 lower than the same run with true |
| T9 | Total never below 0 or above 100 | clamped |
| T10 | Scoring is deterministic | same input gives same output |
| T11 | Every row of the expected-score table in `01-design.md` | PlateBalance matches the table |
| T12 | `ScenarioState` inspection rule | bonus only if every starting plate item was inspected in Assess; inspecting in Act or inspecting tray items does not count |

### EditMode (content)
| ID | Test | Expected |
|----|------|----------|
| T13 | Every PlateVariantData | at least 3 plate items, at least 1 tray item, no food on both |
| T14 | Every plate's counseling options | exactly 3, exactly one Good, each has a reaction and an explanation |

### PlayMode (scene-level)
| ID | Test | Expected |
|----|------|----------|
| T15 | Drive `ScenarioController` Intro to Debrief with scripted choices | ends in Debrief with the score from the expected table |
| T16 | Try Again moves to the next variant and wraps after D | variant index correct |
| T17 | Illegal transition (Debrief to Assess) | rejected, state unchanged |
| T18 | Drop an item after a response is chosen | rejected, plate unchanged |

## Manual checklist (WebGL build in Chrome)
| ID | Steps | Pass when |
|----|-------|-----------|
| M1 | Load page, wait for build | No console errors, intro shows |
| M2 | Click each food item | Panel shows correct name, group and portion |
| M3 | Drag items from plate to tray and tray to plate, and drop one on the floor | Totals update, drinks snap to the cup spot, the floor drop returns the item |
| M4 | Pick each counseling option | Client reaction matches the option, plate can no longer be changed |
| M5 | Play each variant's "fixed" plate from the expected table with the good response and full inspection | Debrief shows 100 and the correct breakdown |
| M6 | Reload mid run | Starts clean at intro |
| M7 | Resize window | UI stays usable |
| M8 | Plate C (already balanced): remove Grilled chicken | Balance 45, debrief shows Protein 0%, target 25% |

## Acceptance for v1
All EditMode and PlayMode tests pass, M1 to M8 pass, and a first-time user (a friend)
completes one run without help.

## Build verification
```bash
unity build . --target WebGL
```
With Compression Format set to Disabled, serve the output folder (`python3 -m http.server` in
the build directory) and open it in Chrome.
