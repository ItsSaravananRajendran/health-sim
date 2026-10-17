using System;
using System.Collections.Generic;

namespace PlateCheck.Core
{
    public enum Bucket { VegFruit, Grain, Protein, Limit }

    /// <summary>Target share per bucket. Shares are decimals that sum to 1.</summary>
    public readonly struct Ratios
    {
        public readonly decimal vegFruit;
        public readonly decimal grain;
        public readonly decimal protein;
        public readonly decimal limit;

        public Ratios(decimal vegFruit, decimal grain, decimal protein, decimal limit)
        {
            this.vegFruit = vegFruit;
            this.grain = grain;
            this.protein = protein;
            this.limit = limit;
        }

        /// <summary>Half vegetables and fruits, a quarter grains, a quarter protein, no sweets or fried.</summary>
        public static readonly Ratios MyPlate = new Ratios(0.50m, 0.25m, 0.25m, 0m);

        public decimal For(Bucket bucket)
        {
            switch (bucket)
            {
                case Bucket.VegFruit: return vegFruit;
                case Bucket.Grain: return grain;
                case Bucket.Protein: return protein;
                default: return limit;
            }
        }
    }

    /// <summary>Portion units and share per bucket for one plate.</summary>
    public sealed class BucketShares
    {
        public static readonly Bucket[] AllBuckets =
            { Bucket.VegFruit, Bucket.Grain, Bucket.Protein, Bucket.Limit };

        readonly int[] units = new int[4];

        /// <summary>Sum of the four buckets. Dairy is not included.</summary>
        public int TotalUnits { get; private set; }

        /// <summary>Sweet and Fried items in the plate, for the debrief.</summary>
        public IReadOnlyList<string> LimitItemIds => limitItemIds;
        readonly List<string> limitItemIds = new List<string>();

        internal void Add(Bucket bucket, int portionUnits)
        {
            units[(int)bucket] += portionUnits;
            TotalUnits += portionUnits;
        }

        internal void AddLimitItem(string id) => limitItemIds.Add(id);

        public int Units(Bucket bucket) => units[(int)bucket];

        public decimal Share(Bucket bucket) =>
            TotalUnits == 0 ? 0m : (decimal)units[(int)bucket] / TotalUnits;
    }

    /// <summary>Pure scoring rules from docs/01-design.md.</summary>
    public static class Scoring
    {
        public const int MaxBalance = 60;
        public const int InspectionBonus = 10;
        public const int PassMark = 70;

        public static bool TryGetBucket(FoodGroup group, out Bucket bucket)
        {
            switch (group)
            {
                case FoodGroup.Vegetable:
                case FoodGroup.Fruit: bucket = Bucket.VegFruit; return true;
                case FoodGroup.Grain: bucket = Bucket.Grain; return true;
                case FoodGroup.Protein: bucket = Bucket.Protein; return true;
                case FoodGroup.Sweet:
                case FoodGroup.Fried: bucket = Bucket.Limit; return true;
                default: bucket = default; return false; // Dairy sits beside the plate.
            }
        }

        public static BucketShares Breakdown(IReadOnlyList<PlacedItem> items, Ratios target)
        {
            var shares = new BucketShares();
            foreach (var item in items)
            {
                if (!TryGetBucket(item.foodGroup, out var bucket)) continue;
                shares.Add(bucket, item.portionUnits);
                if (bucket == Bucket.Limit) shares.AddLimitItem(item.id);
            }
            return shares;
        }

        public static int PlateBalance(IReadOnlyList<PlacedItem> items, Ratios target)
        {
            var shares = Breakdown(items, target);
            int total = shares.TotalUnits;
            if (total == 0) return 0;

            // d = 1/2 * sum |units/total - target| = sum |units - target*total| / (2*total).
            // Dividing once at the end keeps the result exact whenever it terminates, so a true
            // x.5 is never seen as x.4999 and rounded the wrong way.
            decimal deviation = 0m;
            foreach (var bucket in BucketShares.AllBuckets)
                deviation += Math.Abs(shares.Units(bucket) - target.For(bucket) * total);

            decimal balance = MaxBalance - MaxBalance * deviation / (2m * total);
            int rounded = (int)Math.Round(balance, 0, MidpointRounding.AwayFromZero);
            return Clamp(rounded, 0, MaxBalance);
        }

        public static int Counseling(Quality quality)
        {
            switch (quality)
            {
                case Quality.Good: return 30;
                case Quality.Neutral: return 15;
                default: return 0;
            }
        }

        public static int Total(int plate, int counsel, bool inspectedAll) =>
            Clamp(plate + counsel + (inspectedAll ? InspectionBonus : 0), 0, 100);

        static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    }

    /// <summary>Everything the debrief needs, captured when a response is chosen.</summary>
    public sealed class ScoreResult
    {
        public int Balance { get; }
        public int Counseling { get; }
        public int Inspection { get; }
        public int Total { get; }
        public bool Passed => Total >= Scoring.PassMark;
        public BucketShares Breakdown { get; }
        public Quality Quality { get; }
        public IReadOnlyList<string> MissedInspections { get; }

        public ScoreResult(IReadOnlyList<PlacedItem> finalPlate, Quality quality,
            IReadOnlyList<string> missedInspections, Ratios target)
        {
            Breakdown = Scoring.Breakdown(finalPlate, target);
            Balance = Scoring.PlateBalance(finalPlate, target);
            Counseling = Scoring.Counseling(quality);
            bool inspectedAll = missedInspections.Count == 0;
            Inspection = inspectedAll ? Scoring.InspectionBonus : 0;
            Total = Scoring.Total(Balance, Counseling, inspectedAll);
            Quality = quality;
            MissedInspections = missedInspections;
        }
    }
}
