using NUnit.Framework;
using PlateCheck.Core;

namespace PlateCheck.Tests
{
    public class ScenarioStateTests
    {
        static ScenarioState StartVariantA()
        {
            var state = new ScenarioState();
            Assert.IsTrue(state.Start(
                Foods.Plate("Rice", "Bread roll", "Carrots", "Beans"),
                Foods.Plate("Broccoli", "Salad", "Apple", "Grilled chicken")));
            return state;
        }

        static void InspectAll(ScenarioState state)
        {
            foreach (var id in state.StartingPlateIds) Assert.IsTrue(state.Inspect(id));
        }

        [Test] // T12
        public void InspectionBonus_OnlyWhenEveryStartingItemInspectedInAssess()
        {
            var state = StartVariantA();
            InspectAll(state);
            Assert.IsTrue(state.InspectedAllStartingItems);
            state.ReadyToHelp();
            state.ChooseResponse(Quality.Good);
            Assert.AreEqual(10, state.Result.Inspection);
        }

        [Test] // T12
        public void InspectionBonus_InspectingInActDoesNotCount()
        {
            var state = StartVariantA();
            state.Inspect("Rice");
            state.Inspect("Bread roll");
            state.Inspect("Carrots");
            state.ReadyToHelp();
            Assert.IsTrue(state.Inspect("Beans")); // allowed, but too late for the bonus
            Assert.IsFalse(state.InspectedAllStartingItems);
            state.ChooseResponse(Quality.Good);
            Assert.AreEqual(0, state.Result.Inspection);
            CollectionAssert.AreEqual(new[] { "Beans" }, state.Result.MissedInspections);
        }

        [Test] // T12
        public void InspectionBonus_TrayItemsDoNotCount()
        {
            var state = StartVariantA();
            state.Inspect("Rice");
            state.Inspect("Broccoli");
            state.Inspect("Salad");
            state.Inspect("Apple");
            state.Inspect("Grilled chicken");
            Assert.IsFalse(state.InspectedAllStartingItems);
            Assert.AreEqual(3, state.MissedInspections().Count);
        }

        [Test]
        public void InspectionBonus_MovingAStartingItemToTrayStillRequiresItsInspection()
        {
            var state = StartVariantA();
            state.Inspect("Rice");
            state.Inspect("Carrots");
            state.Inspect("Beans");
            state.ReadyToHelp();
            state.MoveToTray("Bread roll");
            state.ChooseResponse(Quality.Good);
            CollectionAssert.AreEqual(new[] { "Bread roll" }, state.Result.MissedInspections);
        }

        [Test]
        public void Inspect_UnknownId_Rejected()
        {
            var state = StartVariantA();
            Assert.IsFalse(state.Inspect("Pizza"));
        }

        [Test]
        public void Moves_OnlyLegalInAct()
        {
            var state = StartVariantA();
            Assert.IsFalse(state.MoveToTray("Rice"), "no dragging in Assess");
            state.ReadyToHelp();
            Assert.IsTrue(state.MoveToTray("Bread roll"));
            Assert.IsTrue(state.MoveToPlate("Broccoli"));
            Assert.IsFalse(state.MoveToPlate("Pizza"));
            Assert.IsFalse(state.MoveToPlate("Rice"), "already on the plate");
            Assert.IsTrue(state.IsOnPlate("Broccoli"));
            Assert.IsFalse(state.IsOnPlate("Bread roll"));
        }

        [Test]
        public void ChooseResponse_LocksPlate()
        {
            var state = StartVariantA();
            state.ReadyToHelp();
            Assert.IsTrue(state.ChooseResponse(Quality.Neutral));
            Assert.AreEqual(Phase.Feedback, state.Phase);
            Assert.IsTrue(state.PlateLocked);
            Assert.IsFalse(state.MoveToTray("Rice"));
            Assert.AreEqual(4, state.Plate.Count);
            Assert.IsFalse(state.ChooseResponse(Quality.Good), "only one response");
            Assert.AreEqual(Quality.Neutral, state.ChosenQuality);
        }

        [Test]
        public void FullRun_FixedPlateGoodResponseFullInspection_Scores100()
        {
            var state = StartVariantA();
            InspectAll(state);
            state.ReadyToHelp();
            state.MoveToTray("Bread roll");
            state.MoveToTray("Beans");
            state.MoveToPlate("Apple");
            state.MoveToPlate("Broccoli");
            state.MoveToPlate("Grilled chicken");
            state.ChooseResponse(Quality.Good);
            Assert.IsTrue(state.Continue());
            Assert.AreEqual(Phase.Debrief, state.Phase);
            Assert.AreEqual(60, state.Result.Balance);
            Assert.AreEqual(100, state.Result.Total);
            Assert.IsTrue(state.Result.Passed);
        }

        [Test]
        public void IllegalTransitions_LeaveStateUnchanged()
        {
            var state = new ScenarioState();
            Assert.IsFalse(state.ReadyToHelp());
            Assert.IsFalse(state.ChooseResponse(Quality.Good));
            Assert.IsFalse(state.Continue());
            Assert.IsFalse(state.Reset());
            Assert.AreEqual(Phase.Intro, state.Phase);

            state = StartVariantA();
            Assert.IsFalse(state.Start(Foods.Plate("Rice"), Foods.Plate("Apple")));
            Assert.AreEqual(4, state.Plate.Count);
            Assert.IsFalse(state.Continue());
            Assert.AreEqual(Phase.Assess, state.Phase);

            state.ReadyToHelp();
            state.ChooseResponse(Quality.Good);
            state.Continue();
            Assert.IsFalse(state.ReadyToHelp(), "Debrief to Act");
            Assert.IsFalse(state.Inspect("Rice"));
            Assert.AreEqual(Phase.Debrief, state.Phase);
        }

        [Test]
        public void Reset_ClearsEverything()
        {
            var state = StartVariantA();
            InspectAll(state);
            state.ReadyToHelp();
            state.ChooseResponse(Quality.Good);
            state.Continue();
            Assert.IsTrue(state.Reset());
            Assert.AreEqual(Phase.Intro, state.Phase);
            Assert.AreEqual(0, state.Plate.Count);
            Assert.AreEqual(0, state.Tray.Count);
            Assert.IsNull(state.Result);
            Assert.IsNull(state.ChosenQuality);
        }
    }
}
