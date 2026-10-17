using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlateCheck.Core;
using PlateCheck.Data;
using UnityEditor;

namespace PlateCheck.Tests
{
    /// <summary>Checks the authored content, so a bad edit in the Inspector is caught.</summary>
    public class PlateVariantTests
    {
        static IEnumerable<PlateVariantData> AllVariants() =>
            AssetDatabase.FindAssets("t:PlateVariantData", new[] { "Assets/Content" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<PlateVariantData>(AssetDatabase.GUIDToAssetPath(guid)))
                .OrderBy(v => v.name);

        static IEnumerable<TestCaseData> Variants() =>
            AllVariants().Select(v => new TestCaseData(v).SetName("{m}(" + v.name + ")"));

        [Test]
        public void ContentHasFourVariants()
        {
            CollectionAssert.AreEqual(new[] { "PlateA", "PlateB", "PlateC", "PlateD" }, AllVariants().Select(v => v.name));
        }

        [TestCaseSource(nameof(Variants))] // T13
        public void Variant_HasPlateAndTray_NoFoodOnBoth(PlateVariantData variant)
        {
            Assert.GreaterOrEqual(variant.plateItems.Count, 3, "plate items");
            Assert.GreaterOrEqual(variant.trayItems.Count, 1, "tray items");
            CollectionAssert.AllItemsAreNotNull(variant.plateItems);
            CollectionAssert.AllItemsAreNotNull(variant.trayItems);
            var all = variant.plateItems.Concat(variant.trayItems).Select(f => f.Id).ToList();
            CollectionAssert.AllItemsAreUnique(all, "a food is used more than once");
            Assert.IsFalse(string.IsNullOrWhiteSpace(variant.title), "title");
            Assert.IsFalse(string.IsNullOrWhiteSpace(variant.clientLine), "client line");
            Assert.IsFalse(string.IsNullOrWhiteSpace(variant.teachingPoint), "teaching point");
        }

        [TestCaseSource(nameof(Variants))] // T14
        public void Variant_HasThreeOptions_ExactlyOneGood(PlateVariantData variant)
        {
            var options = variant.counselingOptions;
            Assert.AreEqual(3, options.Count);
            CollectionAssert.AllItemsAreNotNull(options);
            Assert.AreEqual(1, options.Count(o => o.quality == Quality.Good));
            foreach (var option in options)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(option.text), option.name + " text");
                Assert.IsFalse(string.IsNullOrWhiteSpace(option.clientReaction), option.name + " reaction");
                Assert.IsFalse(string.IsNullOrWhiteSpace(option.explanation), option.name + " explanation");
            }
        }

        [Test]
        public void BaselineStartingPlates_MatchExpectedTable()
        {
            // "start" rows of the expected-score table in docs/01-design.md. If an educator
            // changes a plate, update the table (and this test) too.
            var expected = new Dictionary<string, int> { { "PlateA", 39 }, { "PlateB", 41 }, { "PlateC", 60 }, { "PlateD", 36 } };
            foreach (var variant in AllVariants())
                Assert.AreEqual(expected[variant.name], Scoring.PlateBalance(variant.PlatePlacedItems(), Ratios.MyPlate), variant.name);
        }
    }
}
