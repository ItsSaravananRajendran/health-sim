using System.Collections.Generic;
using System.Text;
using PlateCheck.Core;
using TMPro;
using UnityEngine;

namespace PlateCheck.Interaction
{
    /// <summary>
    /// The plate plus its cup spot. Decides whether a drop lands here, lays out the foods that
    /// are in it, and shows live bucket totals.
    /// </summary>
    public class PlateArea : MonoBehaviour
    {
        [SerializeField] Transform plateCenter;
        [SerializeField] float plateRadius = 0.36f;
        [SerializeField] Transform cupSpot;
        [SerializeField] float cupSpotRadius = 0.12f;
        [Tooltip("Radius of the ring the foods sit on.")]
        [SerializeField] float ringRadius = 0.23f;
        [SerializeField] TMP_Text totalsText;
        [SerializeField] GameObject totalsRoot;

        public Vector3 Center => plateCenter.position;
        public Vector3 CupSpot => cupSpot.position;

        public bool Contains(Vector3 worldPosition) =>
            FlatDistance(worldPosition, plateCenter.position) <= plateRadius ||
            FlatDistance(worldPosition, cupSpot.position) <= cupSpotRadius;

        public void Layout(IReadOnlyList<FoodItemView> items)
        {
            var foods = new List<FoodItemView>();
            var drinks = new List<FoodItemView>();
            foreach (var item in items) (item.Food.isDrink ? drinks : foods).Add(item);

            for (int i = 0; i < foods.Count; i++)
            {
                // One food sits in the middle once the ring would get crowded.
                bool middle = foods.Count >= 6 && i == foods.Count - 1;
                int ringCount = foods.Count >= 6 ? foods.Count - 1 : foods.Count;
                float angle = Mathf.PI * 2f * i / Mathf.Max(1, ringCount) + Mathf.PI / 2f;
                var offset = middle || foods.Count == 1 ? Vector3.zero
                    : new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ringRadius;
                foods[i].transform.position = plateCenter.position + offset;
            }

            for (int i = 0; i < drinks.Count; i++)
                drinks[i].transform.position = cupSpot.position + new Vector3(0f, 0f, -0.11f * i);
        }

        public void ShowTotals(bool visible, BucketShares shares = null)
        {
            totalsRoot.SetActive(visible);
            if (!visible || shares == null) return;
            var sb = new StringBuilder("<b>Plate now</b>\n");
            foreach (var bucket in BucketShares.AllBuckets)
            {
                sb.Append(DebriefReport.BucketName(bucket)).Append(": ")
                    .Append(shares.Units(bucket)).Append(" units (")
                    .Append(DebriefReport.Percent(shares.Share(bucket))).Append("%)\n");
            }
            sb.Append("Target 50 / 25 / 25 / 0");
            totalsText.text = sb.ToString();
        }

        static float FlatDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
