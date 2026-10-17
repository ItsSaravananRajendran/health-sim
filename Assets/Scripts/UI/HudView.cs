using PlateCheck.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlateCheck.UI
{
    /// <summary>Phase instructions, the client's opening line and the "Ready to help" button.</summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text instruction;
        [SerializeField] GameObject speechRoot;
        [SerializeField] TMP_Text speech;
        public Button readyButton;

        public void Show(Phase phase, string clientLine, int inspected, int toInspect)
        {
            bool inRoom = phase == Phase.Assess || phase == Phase.Act || phase == Phase.Feedback;
            root.SetActive(inRoom);
            if (!inRoom) return;

            switch (phase)
            {
                case Phase.Assess:
                    instruction.text = "<b>Assess.</b> Click each food on the plate to inspect it " +
                                       $"({inspected} of {toInspect} inspected). Then press Ready to help.";
                    break;
                case Phase.Act:
                    instruction.text = "<b>Act.</b> Drag foods between the plate and the tray to fix the meal, " +
                                       "then choose what to say. Choosing a response locks the plate.";
                    break;
                default:
                    instruction.text = "<b>Feedback.</b> See how the client responds.";
                    break;
            }

            speechRoot.SetActive(phase != Phase.Feedback);
            speech.text = "<b>Client:</b> \"" + clientLine + "\"";
            readyButton.gameObject.SetActive(phase == Phase.Assess);
        }
    }
}
