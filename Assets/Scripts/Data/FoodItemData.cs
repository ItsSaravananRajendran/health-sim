using PlateCheck.Core;
using UnityEngine;

namespace PlateCheck.Data
{
    /// <summary>Which 3D model to show. Simple is a colored shape picked by food group.</summary>
    public enum FoodShape
    {
        Simple, Broccoli, Salad, Carrots, Apple, Rice, BreadRoll, GrilledChicken, Beans, Milk, CakeSlice, Soda, Fries
    }

    [CreateAssetMenu(menuName = "Plate Check/Food Item", fileName = "NewFood")]
    public class FoodItemData : ScriptableObject
    {
        public string displayName;
        public FoodGroup foodGroup;
        [Min(1)] public int portionUnits = 1;
        [Tooltip("3D model. Simple uses a colored shape based on the food group and the color below.")]
        public FoodShape shape = FoodShape.Simple;
        [Tooltip("Color of the Simple shape.")]
        public Color color = Color.white;
        [Tooltip("Drinks go to the cup spot instead of the plate.")]
        public bool isDrink;

        /// <summary>Each food appears at most once per variant, so its name is a usable id.</summary>
        public string Id => displayName;

        public PlacedItem ToPlacedItem() => new PlacedItem(Id, foodGroup, portionUnits);
    }
}
