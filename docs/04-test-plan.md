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
| T3 | Plate all grains | PlateBalance well below 30 |
| T4 | Counseling Good / Neutral / Judgmental | 30 / 15 / 0 |
| T5 | Total with all components maxed | 100 |
| T6 | Total with inspectedAll false | 10 lower than the same run with true |
| T7 | Total never below 0 or above 100 | clamped |
| T8 | Scoring is deterministic | same input gives same output |
| T9 | Every PlateVariantData has at least 3 items and a target ratio summing to 100 | passes |
| T10 | Every plate has exactly one Good counseling option | passes |

### PlayMode (scene-level)
| ID | Test | Expected |
|----|------|----------|
| T11 | Drive `ScenarioController` Intro to Debrief with scripted choices | ends in Debrief with a score |
| T12 | Try Again moves to the next variant and wraps after D | variant index correct |
| T13 | Illegal transition (Debrief to Assess) | rejected, state unchanged |

## Manual checklist (WebGL build in Chrome)
| ID | Steps | Pass when |
|----|-------|-----------|
| M1 | Load page, wait for build | No console errors, intro shows |
| M2 | Click each food item | Panel shows correct name and group |
| M3 | Drag an item off and onto the plate | Totals update, item snaps or returns |
| M4 | Pick each counseling option | Client reaction matches the option |
| M5 | Finish all four variants | Debrief score matches expectations |
| M6 | Reload mid run | Starts clean at intro |
| M7 | Resize window | UI stays usable |
| M8 | Plate C (already balanced): remove an item | Score drops, debrief says why |

## Acceptance for v1
All EditMode and PlayMode tests pass, M1 to M8 pass, and a first-time user (a friend)
completes one run without help.

## Build verification
```bash
unity build . --target WebGL
```
Serve the output folder (`python3 -m http.server` in the build directory) and open it in Chrome.
