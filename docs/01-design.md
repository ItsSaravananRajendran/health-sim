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
   Clicking a food item shows its name, food group and approximate portion. Dragging is off.
   A "Ready to help" button moves to Act.
3. **Act.** Inspecting still works, and two actions are available:
   - Rearrange: drag items between the plate area and a side tray next to it. The plate area
     includes a cup spot for drinks. The tray holds foods the learner can add, so plates that
     are missing a group can be fixed, not only trimmed.
   - Counsel: pick one of three responses (one good, one neutral, one judgmental).
   Picking a response locks the plate and ends Act. Rearrange first, then counsel.
4. **Feedback.** The client reacts (text bubble plus a simple animation or color change).
   A Continue button moves to Debrief.
5. **Debrief.** Score breakdown, what was caught, what was missed, and why (see Debrief
   below). Buttons: Try Again (next plate variant, wraps after D) and Quit.

## Plates (content)
Four variants, each defined as data (see architecture). The starting content below is a
baseline: an educator can change it, but then the expected-score table must be updated too.

### Foods (12)
| Food | Group | Units | | Food | Group | Units |
|------|-------|-------|-|------|-------|-------|
| Broccoli | Vegetable | 2 | | Grilled chicken | Protein | 2 |
| Salad | Vegetable | 2 | | Beans | Protein | 1 |
| Carrots | Vegetable | 1 | | Milk | Dairy | 1 |
| Apple | Fruit | 1 | | Cake slice | Sweet | 2 |
| Rice | Grain | 2 | | Large soda | Sweet | 2 |
| Bread roll | Grain | 1 | | Fries | Fried | 2 |

### Variants
Each food is used at most once per variant: on the plate or on the tray, never both.

| Variant | Problem | Starting plate | Tray |
|---------|---------|----------------|------|
| A | Too much grains, almost no vegetables | Rice, Bread roll, Carrots, Beans | Broccoli, Salad, Apple, Grilled chicken |
| B | No protein, oversized dessert | Salad, Apple, Rice, Cake slice | Grilled chicken, Beans, Carrots, Broccoli |
| C | Balanced plate (tests that the learner does not over-correct) | Broccoli, Salad, Rice, Grilled chicken | Bread roll, Cake slice, Apple |
| D | Large sugary drink plus fried item, vegetables present | Salad, Broccoli, Grilled chicken, Fries, Large soda | Rice, Milk, Apple |

## Scoring (max 100)
### Plate balance (0 to 60)
Counted over the items in the plate area (plate plus cup spot) at the moment the learner
picks a counseling response.

1. Sum portion units into four buckets:
   - **VegFruit** = Vegetable + Fruit, target 50%
   - **Grain**, target 25%
   - **Protein**, target 25%
   - **Limit** = Sweet + Fried, target 0%
   - Dairy is not counted. MyPlate shows dairy beside the plate, so milk is neutral.
2. Total units = the sum of the four buckets (Dairy excluded). If it is 0, the balance
   score is 0.
3. Each bucket's share = bucket units / total units.
4. Distance d = ½ × Σ |share − target| over the four buckets. d is between 0 and 1.
5. Balance score = round(60 × (1 − d)), rounding halves away from zero. Compute with
   `decimal`, not `float`, so values like 39.0 do not come out as 38.999 and round down.

Examples: perfect 50/25/25 plate = 60; all grains = 15; all sweets = 0.

### Other parts
- Counseling response: good 30, neutral 15, judgmental 0.
- Inspection: 10 if every item on the *starting* plate was inspected during Assess, else 0.
  Tray items do not count.
- Total = clamp(balance + counseling + inspection, 0, 100). Pass mark: 70.

### Expected balance scores
The EditMode test T11 recomputes every row of this table.

| Variant | Plate | Balance |
|---------|-------|---------|
| A start | Rice, Bread roll, Carrots, Beans | 39 |
| A fixed | Rice, Carrots, Apple, Broccoli, Grilled chicken (removed Bread roll, Beans) | 60 |
| B start | Salad, Apple, Rice, Cake slice | 41 |
| B fixed | Salad, Apple, Rice, Carrots, Grilled chicken (removed Cake slice) | 60 |
| C start | Broccoli, Salad, Rice, Grilled chicken | 60 |
| C over-corrected | Broccoli, Salad, Rice (removed Grilled chicken) | 45 |
| D start | Salad, Broccoli, Grilled chicken, Fries, Large soda | 36 |
| D fixed | Salad, Broccoli, Grilled chicken, Rice, Milk (removed Fries, Large soda) | 60 |

A perfect run on any variant (fixed plate, good response, full inspection) scores 100.

## Debrief
Generated from the score data, so nothing in it is hand-written per run:
- **Balance:** each bucket's final share next to its target (e.g. "Grain 60%, target 25%"),
  plus any Sweet or Fried items still in the plate area.
- **Counseling:** the chosen option's `explanation` (why it was good, neutral or judgmental).
- **Inspection:** the starting items that were never inspected, if any.
- **Teaching point:** the plate's `teachingPoint` text (one or two sentences).

## Out of scope for v1
Voice, multiple rooms, saving progress, accounts, analytics, VR, mobile layout.
