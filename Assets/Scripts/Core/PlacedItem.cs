namespace PlateCheck.Core
{
    public enum FoodGroup { Vegetable, Fruit, Grain, Protein, Dairy, Sweet, Fried }

    public enum Quality { Good, Neutral, Judgmental }

    /// <summary>
    /// A food as the scoring and state logic sees it. A plain value, not a reference to the
    /// ScriptableObject, so the logic stays free of Unity types.
    /// </summary>
    public readonly struct PlacedItem
    {
        public readonly string id;
        public readonly FoodGroup foodGroup;
        public readonly int portionUnits;

        public PlacedItem(string id, FoodGroup foodGroup, int portionUnits)
        {
            this.id = id;
            this.foodGroup = foodGroup;
            this.portionUnits = portionUnits;
        }

        public override string ToString() => $"{id} ({foodGroup}, {portionUnits})";
    }
}
