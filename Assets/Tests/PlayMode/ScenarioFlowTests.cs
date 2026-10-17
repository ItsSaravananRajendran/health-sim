using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PlateCheck.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PlateCheck.Tests
{
    /// <summary>Drives the real Clinic scene through ScenarioController.</summary>
    public class ScenarioFlowTests
    {
        ScenarioController controller;

        [UnitySetUp]
        public IEnumerator LoadClinic()
        {
            SceneManager.LoadScene("Clinic");
            yield return null;
            yield return null;
            controller = UnityEngine.Object.FindAnyObjectByType<ScenarioController>();
            Assert.IsNotNull(controller, "ScenarioController in Clinic scene");
            Assert.AreEqual(Phase.Intro, controller.State.Phase);
        }

        // Moves that turn each starting plate into the "fixed" row of the expected-score table.
        static IEnumerable<TestCaseData> FixedPlates()
        {
            yield return Fixed(0, new[] { "Bread roll", "Beans" }, new[] { "Apple", "Broccoli", "Grilled chicken" });
            yield return Fixed(1, new[] { "Cake slice" }, new[] { "Carrots", "Grilled chicken" });
            yield return Fixed(2, new string[0], new string[0]);
            yield return Fixed(3, new[] { "Fries", "Large soda" }, new[] { "Rice", "Milk" });
        }

        static TestCaseData Fixed(int variant, string[] toTray, string[] toPlate) =>
            new TestCaseData(variant, toTray, toPlate).Returns(null).SetName($"FullRun_FixedPlate_Scores100(Plate{(char)('A' + variant)})");

        [UnityTest, TestCaseSource(nameof(FixedPlates))] // T15 (and M5, automated)
        public IEnumerator FullRun_FixedPlate_ReachesDebriefWith100(int variant, string[] toTray, string[] toPlate)
        {
            Assert.IsTrue(controller.SelectVariant(variant));
            Assert.IsTrue(controller.StartScenario());
            yield return Capture($"v{variant}-1-assess");

            foreach (var id in controller.State.StartingPlateIds.ToList()) Assert.IsTrue(controller.InspectItem(id), id);
            Assert.IsTrue(controller.ReadyToHelp());
            Assert.AreEqual(Phase.Act, controller.State.Phase);

            // Drop through the same path a mouse drag uses.
            foreach (var id in toTray) Assert.IsTrue(controller.DropItem(controller.View(id), controller.TrayArea.Center), id);
            foreach (var id in toPlate) Assert.IsTrue(controller.DropItem(controller.View(id), controller.PlateArea.Center), id);
            yield return null;
            yield return Capture($"v{variant}-2-act");

            Assert.IsTrue(controller.ChooseResponse(GoodOption()));
            Assert.AreEqual(Phase.Feedback, controller.State.Phase);
            yield return new WaitForSeconds(0.3f);
            yield return Capture($"v{variant}-3-feedback");

            Assert.IsTrue(controller.ContinueToDebrief());
            Assert.AreEqual(Phase.Debrief, controller.State.Phase);
            Assert.AreEqual(60, controller.State.Result.Balance);
            Assert.AreEqual(100, controller.State.Result.Total);
            StringAssert.Contains("Score 100 / 100", controller.DebriefScreen.BodyText);
            yield return Capture($"v{variant}-4-debrief");
        }

        [UnityTest] // M8, automated
        public IEnumerator PlateC_RemovingChicken_Scores45AndDebriefShowsProteinGap()
        {
            controller.SelectVariant(2);
            controller.StartScenario();
            foreach (var id in controller.State.StartingPlateIds.ToList()) controller.InspectItem(id);
            controller.ReadyToHelp();
            Assert.IsTrue(controller.DropItem(controller.View("Grilled chicken"), controller.TrayArea.Center));
            controller.ChooseResponse(GoodOption());
            controller.ContinueToDebrief();
            yield return null;
            Assert.AreEqual(45, controller.State.Result.Balance);
            StringAssert.Contains("Protein 0%, target 25%", controller.DebriefScreen.BodyText);
        }

        [UnityTest] // T16
        public IEnumerator TryAgain_MovesToNextVariant_AndWrapsAfterD()
        {
            controller.StartScenario();
            for (int expected = 1; expected <= controller.VariantCount; expected++)
            {
                controller.ReadyToHelp();
                controller.ChooseResponse(0);
                controller.ContinueToDebrief();
                Assert.IsTrue(controller.TryAgain());
                Assert.AreEqual(expected % controller.VariantCount, controller.VariantIndex);
                Assert.AreEqual(Phase.Assess, controller.State.Phase);
                Assert.AreEqual(controller.CurrentVariant.plateItems.Count, controller.State.Plate.Count);
                yield return null;
            }
            Assert.AreEqual(0, controller.VariantIndex, "wrapped after D");
        }

        [UnityTest] // T17
        public IEnumerator IllegalTransition_DebriefToAssess_Rejected()
        {
            controller.StartScenario();
            controller.ReadyToHelp();
            controller.ChooseResponse(0);
            controller.ContinueToDebrief();
            yield return null;
            var resultBefore = controller.State.Result;

            Assert.IsFalse(controller.StartScenario(), "Debrief to Assess");
            Assert.IsFalse(controller.ReadyToHelp());
            Assert.IsFalse(controller.ChooseResponse(1));
            Assert.IsFalse(controller.SelectVariant(1));
            Assert.AreEqual(Phase.Debrief, controller.State.Phase);
            Assert.AreSame(resultBefore, controller.State.Result);
        }

        [UnityTest] // T18
        public IEnumerator DropAfterResponse_Rejected_PlateUnchanged()
        {
            controller.StartScenario();
            controller.ReadyToHelp();
            controller.ChooseResponse(0);
            yield return null;
            var before = controller.State.Plate.Select(p => p.id).ToList();
            var rice = controller.View("Rice");
            var slot = rice.transform.position;

            Assert.IsFalse(controller.DropItem(rice, controller.TrayArea.Center));
            Assert.IsFalse(controller.TryMoveItem("Apple", true));
            CollectionAssert.AreEqual(before, controller.State.Plate.Select(p => p.id));
            Assert.AreEqual(slot, rice.transform.position, "item returned to its slot");
        }

        [UnityTest]
        public IEnumerator DropOnFloor_ReturnsItem()
        {
            controller.StartScenario();
            controller.ReadyToHelp();
            yield return null;
            var rice = controller.View("Rice");
            var slot = rice.transform.position;
            rice.transform.position = new Vector3(-1.5f, 0.82f, -0.5f); // mid-drag, off the table

            Assert.IsFalse(controller.DropItem(rice, rice.transform.position));
            Assert.IsTrue(controller.State.IsOnPlate("Rice"));
            Assert.AreEqual(slot, rice.transform.position);
        }

        [UnityTest]
        public IEnumerator Drinks_SnapToCupSpot()
        {
            controller.SelectVariant(3);
            controller.StartScenario();
            controller.ReadyToHelp();
            Assert.IsTrue(controller.DropItem(controller.View("Milk"), controller.PlateArea.Center));
            yield return null;
            var milk = controller.View("Milk").transform.position;
            var soda = controller.View("Large soda").transform.position;
            Assert.Less(Vector2.Distance(new Vector2(soda.x, soda.z), new Vector2(controller.PlateArea.CupSpot.x, controller.PlateArea.CupSpot.z)), 0.01f);
            Assert.Less(Vector2.Distance(new Vector2(milk.x, milk.z), new Vector2(controller.PlateArea.CupSpot.x, controller.PlateArea.CupSpot.z)), 0.25f);
            Assert.IsTrue(controller.PlateArea.Contains(milk), "second drink still counts as plate area");
        }

        [UnityTest]
        public IEnumerator Quit_ReturnsToIntro_Clean()
        {
            controller.StartScenario();
            controller.ReadyToHelp();
            controller.ChooseResponse(0);
            controller.ContinueToDebrief();
            Assert.IsTrue(controller.Quit());
            yield return null;
            Assert.AreEqual(Phase.Intro, controller.State.Phase);
            Assert.AreEqual(0, UnityEngine.Object.FindObjectsByType<Interaction.FoodItemView>().Length);
        }

        int GoodOption() => controller.CurrentVariant.counselingOptions.FindIndex(o => o.quality == Quality.Good);

        /// <summary>
        /// Saves a PNG of the camera plus UI when PLATECHECK_CAPTURE_DIR is set. For eyeballing
        /// layout from headless runs; does nothing otherwise.
        /// </summary>
        static IEnumerator Capture(string name)
        {
            string dir = Environment.GetEnvironmentVariable("PLATECHECK_CAPTURE_DIR");
            if (string.IsNullOrEmpty(dir)) yield break;
            yield return null;

            var camera = Camera.main;
            var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            var rt = new RenderTexture(1280, 720, 24);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.2f;
            camera.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            camera.Render();

            RenderTexture.active = rt;
            var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            camera.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, name + ".png"), texture.EncodeToPNG());
            UnityEngine.Object.Destroy(texture);
            rt.Release();
        }
    }
}
