using PlateCheck.Data;
using TMPro;
using UnityEngine;

namespace PlateCheck.Interaction
{
    /// <summary>Shows the name, food group and portion of the last food clicked.</summary>
    public class InspectPanel : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text details;

        public string ShownId { get; private set; }

        public void Show(FoodItemData food, bool onPlate)
        {
            ShownId = food.Id;
            root.SetActive(true);
            title.text = food.displayName;
            details.text =
                $"Food group: {food.foodGroup}\n" +
                $"Portion: {food.portionUnits} unit{(food.portionUnits == 1 ? "" : "s")}\n" +
                (food.isDrink ? "Drink (cup spot)\n" : "") +
                (onPlate ? "On the plate" : "On the tray");
        }

        public void Hide()
        {
            ShownId = null;
            root.SetActive(false);
        }
    }
}
