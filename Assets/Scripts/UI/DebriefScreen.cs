using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlateCheck.UI
{
    public class DebriefScreen : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text body;
        public Button tryAgainButton;
        public Button quitButton;

        public string BodyText => body.text;

        public void Show(string heading, string text)
        {
            root.SetActive(true);
            title.text = heading;
            body.text = text;
        }

        public void Hide() => root.SetActive(false);
    }
}
