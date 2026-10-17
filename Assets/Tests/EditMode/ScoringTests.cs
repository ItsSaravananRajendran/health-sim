using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlateCheck.Core;

namespace PlateCheck.Tests
{
    /// <summary>The 12 baseline foods from docs/01-design.md, as plain items.</summary>
    static class Foods
    {
        public static readonly Dictionary<string, PlacedItem> All = new[]
        {
            new PlacedItem("Broccoli", FoodGroup.Vegetable, 2),
            new PlacedItem("Salad", FoodGroup.Vegetable, 2),
            new PlacedItem("Carrots", FoodGroup.Vegetable, 1),
            new PlacedItem("Apple", FoodGroup.Fruit, 1),
            new PlacedItem("Rice", FoodGroup.Grain, 2),
            new PlacedItem("Bread roll", FoodGroup.Grain, 1),
            new PlacedItem("Grilled chicken", FoodGroup.Protein, 2),
            new PlacedItem("Beans", FoodGroup.Protein, 1),
            new PlacedItem("Milk", FoodGroup.Dairy, 1),
            new PlacedItem("Cake slice", FoodGroup.Sweet, 2),
            new PlacedItem("Large soda", FoodGroup.Sweet, 2),
            new PlacedItem("Fries", FoodGroup.Fried, 2),
        }.ToDictionary(item => item.id);

        public static List<PlacedItem> Plate(params string[] names) => names.Select(n => All[n]).ToList();
    }

    public class ScoringTests
    {
        static readonly Ratios MyPlate = Ratios.MyPlate;

        static List<PlacedItem> Items(params (FoodGroup group, int units)[] items) =>
            items.Select((x, i) => new PlacedItem("item" + i, x.group, x.units)).ToList();

        [Test] // T1
        public void PerfectPlate_Scores60()
        {
            var plate = Items((FoodGroup.Vegetable, 2), (FoodGroup.Grain, 1), (FoodGroup.Protein, 1));
            Assert.AreEqual(60, Scoring.PlateBalance(plate, MyPlate));
        }

        [Test] // T2
        public void EmptyPlate_Scores0()
        {
            Assert.AreEqual(0, Scoring.PlateBalance(new List<PlacedItem>(), MyPlate));
        }

        [Test] // T3
        public void AllGrains_Scores15()
        {
            Assert.AreEqual(15, Scoring.PlateBalance(Items((FoodGroup.Grain, 3)), MyPlate));
        }

        [Test] // T4
        public void AllSweetAndFried_Scores0()
        {
            var plate = Items((FoodGroup.Sweet, 2), (FoodGroup.Fried, 2));
            Assert.AreEqual(0, Scoring.PlateBalance(plate, MyPlate));
        }

        [Test] // T5
        public void PerfectPlatePlusMilk_Scores60_DairyNotCounted()
        {
            var plate = Items((FoodGroup.Vegetable, 2), (FoodGroup.Grain, 1), (FoodGroup.Protein, 1), (FoodGroup.Dairy, 1));
            Assert.AreEqual(60, Scoring.PlateBalance(plate, MyPlate));
            Assert.AreEqual(4, Scoring.Breakdown(plate, MyPlate).TotalUnits);
        }

        [Test] // T6
        public void Counseling_GoodNeutralJudgmental()
        {
            Assert.AreEqual(30, Scoring.Counseling(Quality.Good));
            Assert.AreEqual(15, Scoring.Counseling(Quality.Neutral));
            Assert.AreEqual(0, Scoring.Counseling(Quality.Judgmental));
        }

        [Test] // T7
        public void Total_AllMaxed_Is100()
        {
            Assert.AreEqual(100, Scoring.Total(60, 30, true));
        }

        [Test] // T8
        public void Total_WithoutInspection_Is10Lower()
        {
            Assert.AreEqual(Scoring.Total(41, 15, true) - 10, Scoring.Total(41, 15, false));
        }

        [Test] // T9
        public void Total_IsClamped()
        {
            Assert.AreEqual(100, Scoring.Total(90, 30, true));
            Assert.AreEqual(0, Scoring.Total(-50, 0, false));
        }

        [Test] // T10
        public void Scoring_IsDeterministic()
        {
            var plate = Foods.Plate("Salad", "Apple", "Rice", "Cake slice");
            int first = Scoring.PlateBalance(plate, MyPlate);
            for (int i = 0; i < 100; i++)
                Assert.AreEqual(first, Scoring.PlateBalance(plate, MyPlate));
        }

        // T11: every row of the expected-score table in docs/01-design.md.
        static readonly object[] ExpectedTable =
        {
            new object[] { "A start", new[] { "Rice", "Bread roll", "Carrots", "Beans" }, 39 },
            new object[] { "A fixed", new[] { "Rice", "Carrots", "Apple", "Broccoli", "Grilled chicken" }, 60 },
            new object[] { "B start", new[] { "Salad", "Apple", "Rice", "Cake slice" }, 41 },
            new object[] { "B fixed", new[] { "Salad", "Apple", "Rice", "Carrots", "Grilled chicken" }, 60 },
            new object[] { "C start", new[] { "Broccoli", "Salad", "Rice", "Grilled chicken" }, 60 },
            new object[] { "C over-corrected", new[] { "Broccoli", "Salad", "Rice" }, 45 },
            new object[] { "D start", new[] { "Salad", "Broccoli", "Grilled chicken", "Fries", "Large soda" }, 36 },
            new object[] { "D fixed", new[] { "Salad", "Broccoli", "Grilled chicken", "Rice", "Milk" }, 60 },
        };

        [TestCaseSource(nameof(ExpectedTable))] // T11
        public void ExpectedScoreTable(string row, string[] foods, int expected)
        {
            Assert.AreEqual(expected, Scoring.PlateBalance(Foods.Plate(foods), MyPlate), row);
        }

        [Test]
        public void Breakdown_ReportsSharesAndLimitItems()
        {
            var shares = Scoring.Breakdown(Foods.Plate("Salad", "Broccoli", "Grilled chicken", "Fries", "Large soda"), MyPlate);
            Assert.AreEqual(10, shares.TotalUnits);
            Assert.AreEqual(0.4m, shares.Share(Bucket.VegFruit));
            Assert.AreEqual(0m, shares.Share(Bucket.Grain));
            Assert.AreEqual(0.4m, shares.Share(Bucket.Limit));
            CollectionAssert.AreEquivalent(new[] { "Fries", "Large soda" }, shares.LimitItemIds);
        }
    }
}
