using PlateCheck.Core;
using UnityEngine;

namespace PlateCheck.Data
{
    [CreateAssetMenu(menuName = "Plate Check/Food Item", fileName = "NewFood")]
    public class FoodItemData : ScriptableObject
    {
        public string displayName;
        public FoodGroup foodGroup;
        [Min(1)] public int portionUnits = 1;
        public Color color = Color.white;
        [Tooltip("Drinks go to the cup spot instead of the plate.")]
        public bool isDrink;

        /// <summary>Each food appears at most once per variant, so its name is a usable id.</summary>
        public string Id => displayName;

        public PlacedItem ToPlacedItem() => new PlacedItem(Id, foodGroup, portionUnits);
    }
}
