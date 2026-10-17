using System.Collections.Generic;
using PlateCheck.Core;
using UnityEngine;

namespace PlateCheck.Data
{
    [CreateAssetMenu(menuName = "Plate Check/Plate Variant", fileName = "NewPlate")]
    public class PlateVariantData : ScriptableObject
    {
        public string title;
        [Tooltip("The starting plate.")]
        public List<FoodItemData> plateItems = new List<FoodItemData>();
        [Tooltip("Foods the learner can add. A food must not be on both lists.")]
        public List<FoodItemData> trayItems = new List<FoodItemData>();
        [Tooltip("What the client says about the meal.")]
        [TextArea(2, 4)] public string clientLine;
        [Tooltip("Exactly 3: one Good, one Neutral, one Judgmental.")]
        public List<CounselingOptionData> counselingOptions = new List<CounselingOptionData>();
        [Tooltip("Debrief text, one or two sentences.")]
        [TextArea(2, 4)] public string teachingPoint;

        public List<PlacedItem> PlatePlacedItems() => ToPlaced(plateItems);
        public List<PlacedItem> TrayPlacedItems() => ToPlaced(trayItems);

        static List<PlacedItem> ToPlaced(List<FoodItemData> foods)
        {
            var items = new List<PlacedItem>(foods.Count);
            foreach (var food in foods) items.Add(food.ToPlacedItem());
            return items;
        }
    }
}
