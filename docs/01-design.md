# 01 Design

## Learning goal
After one session the learner can (1) spot portion and food group problems on a plate,
(2) fix the plate, and (3) choose a respectful counseling response instead of a judgmental one.

## Audience
Community health workers in remote or underserved areas, using a basic laptop browser with
mouse and keyboard. No VR, no install.

## Scenario flow (about 5 minutes)
1. **Intro screen.** One paragraph of context and a Start button.
2. **Assess.** Learner stands in a clinic room facing a client with a plate on a table.
   Clicking a food item shows its name, food group and approximate portion.
3. **Act.** Two actions are available:
   - Rearrange: drag items on or off the plate.
   - Counsel: pick one of three responses (one good, one neutral, one judgmental).
4. **Feedback.** The client reacts (text bubble plus a simple animation or color change).
5. **Debrief.** Score, what was caught, what was missed, and why. Buttons: Try Again (next
   plate variant) and Quit.

## Plates (content)
Four variants, each defined as data (see architecture):

| Variant | Problem |
|---------|---------|
| A | Too much grains, almost no vegetables |
| B | No protein, oversized dessert |
| C | Balanced plate (tests that the learner does not over-correct) |
| D | Large sugary drink plus fried item, vegetables present |

## Scoring (max 100)
- Plate balance: up to 60. Score is how close the final plate is to the target ratios
  (vegetable and fruit 50%, grains 25%, protein 25%), measured by portion units.
- Counseling response: good 30, neutral 15, judgmental 0.
- Inspection: 10 if the learner inspected every item before acting, else 0.
- Pass mark: 70.

## Out of scope for v1
Voice, multiple rooms, saving progress, accounts, analytics, VR, mobile layout.
