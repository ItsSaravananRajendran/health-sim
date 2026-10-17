using System;
using System.Collections.Generic;
using System.Text;

namespace PlateCheck.Core
{
    /// <summary>
    /// Builds the debrief text from the score data, so nothing in it is hand-written per run.
    /// Output uses TextMeshPro rich text tags (&lt;b&gt;).
    /// </summary>
    public static class DebriefReport
    {
        public static string BucketName(Bucket bucket)
        {
            switch (bucket)
            {
                case Bucket.VegFruit: return "Vegetables + fruit";
                case Bucket.Grain: return "Grain";
                case Bucket.Protein: return "Protein";
                default: return "Sweet + fried";
            }
        }

        public static int Percent(decimal share) => (int)Math.Round(share * 100m, 0, MidpointRounding.AwayFromZero);

        /// <summary>One line per bucket, e.g. "Grain 60%, target 25%".</summary>
        public static List<string> BalanceLines(BucketShares shares, Ratios target)
        {
            var lines = new List<string>();
            if (shares.TotalUnits == 0)
            {
                lines.Add("The plate area was empty.");
                return lines;
            }
            foreach (var bucket in BucketShares.AllBuckets)
                lines.Add($"{BucketName(bucket)} {Percent(shares.Share(bucket))}%, target {Percent(target.For(bucket))}%");
            if (shares.LimitItemIds.Count > 0)
                lines.Add("Still in the plate area: " + string.Join(", ", shares.LimitItemIds));
            return lines;
        }

        public static string Build(ScoreResult result, Ratios target, string optionExplanation, string teachingPoint)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>Score {result.Total} / 100</b>  ({(result.Passed ? "pass" : "not yet")}, pass mark {Scoring.PassMark})");
            sb.AppendLine($"Plate balance {result.Balance}/{Scoring.MaxBalance}   Counseling {result.Counseling}/30   Inspection {result.Inspection}/{Scoring.InspectionBonus}");
            sb.AppendLine();

            sb.AppendLine("<b>Plate balance</b>");
            foreach (var line in BalanceLines(result.Breakdown, target)) sb.AppendLine(line);
            sb.AppendLine();

            sb.AppendLine($"<b>Counseling</b> ({result.Quality})");
            sb.AppendLine(optionExplanation);
            sb.AppendLine();

            sb.AppendLine("<b>Inspection</b>");
            sb.AppendLine(result.MissedInspections.Count == 0
                ? "You inspected every item on the starting plate."
                : "Not inspected during Assess: " + string.Join(", ", result.MissedInspections));
            sb.AppendLine();

            sb.AppendLine("<b>Teaching point</b>");
            sb.Append(teachingPoint);
            return sb.ToString();
        }
    }
}
