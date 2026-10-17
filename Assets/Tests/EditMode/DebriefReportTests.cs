using NUnit.Framework;
using PlateCheck.Core;

namespace PlateCheck.Tests
{
    public class DebriefReportTests
    {
        [Test]
        public void BalanceLines_ShowShareNextToTarget()
        {
            var shares = Scoring.Breakdown(Foods.Plate("Broccoli", "Salad", "Rice"), Ratios.MyPlate);
            var lines = DebriefReport.BalanceLines(shares, Ratios.MyPlate);
            CollectionAssert.Contains(lines, "Vegetables + fruit 67%, target 50%");
            CollectionAssert.Contains(lines, "Grain 33%, target 25%");
            CollectionAssert.Contains(lines, "Protein 0%, target 25%");
        }

        [Test]
        public void BalanceLines_ListLimitItemsStillOnPlate()
        {
            var shares = Scoring.Breakdown(Foods.Plate("Salad", "Fries", "Large soda"), Ratios.MyPlate);
            CollectionAssert.Contains(DebriefReport.BalanceLines(shares, Ratios.MyPlate), "Still in the plate area: Fries, Large soda");
        }

        [Test]
        public void BalanceLines_EmptyPlate()
        {
            var shares = Scoring.Breakdown(Foods.Plate(), Ratios.MyPlate);
            CollectionAssert.AreEqual(new[] { "The plate area was empty." }, DebriefReport.BalanceLines(shares, Ratios.MyPlate));
        }

        [Test]
        public void Build_IncludesEverySection()
        {
            var result = new ScoreResult(Foods.Plate("Rice", "Bread roll", "Carrots", "Beans"), Quality.Neutral,
                new[] { "Beans" }, Ratios.MyPlate);
            string text = DebriefReport.Build(result, Ratios.MyPlate, "Why neutral.", "The teaching point.");
            StringAssert.Contains("Score 54 / 100", text); // 39 + 15 + 0
            StringAssert.Contains("not yet", text);
            StringAssert.Contains("Grain 60%, target 25%", text);
            StringAssert.Contains("Why neutral.", text);
            StringAssert.Contains("Not inspected during Assess: Beans", text);
            StringAssert.Contains("The teaching point.", text);
        }
    }
}
