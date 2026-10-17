using UnityEngine;
using UnityEngine.UI;

namespace PlateCheck.UI
{
    public class IntroScreen : MonoBehaviour
    {
        [SerializeField] GameObject root;
        public Button startButton;

        public void Show(bool visible) => root.SetActive(visible);
    }
}
