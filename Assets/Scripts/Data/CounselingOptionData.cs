using PlateCheck.Core;
using UnityEngine;

namespace PlateCheck.Data
{
    [CreateAssetMenu(menuName = "Plate Check/Counseling Option", fileName = "NewOption")]
    public class CounselingOptionData : ScriptableObject
    {
        [TextArea(2, 4)] public string text;
        public Quality quality;
        [Tooltip("What the client says back.")]
        [TextArea(2, 4)] public string clientReaction;
        [Tooltip("Debrief text: why this response is good, neutral or judgmental.")]
        [TextArea(2, 4)] public string explanation;
    }
}
