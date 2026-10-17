using System.Collections;
using PlateCheck.Core;
using PlateCheck.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlateCheck.UI
{
    /// <summary>The client's reaction: a speech bubble, a color change and a small head movement.</summary>
    public class FeedbackView : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text reaction;
        public Button continueButton;
        [SerializeField] Renderer clientBody;
        [SerializeField] Transform clientHead;

        static readonly Color GoodColor = new Color(0.30f, 0.70f, 0.40f);
        static readonly Color NeutralColor = new Color(0.85f, 0.70f, 0.30f);
        static readonly Color JudgmentalColor = new Color(0.55f, 0.45f, 0.60f);

        Color restingColor;
        Vector3 headRest;
        Coroutine running;

        void Awake()
        {
            restingColor = clientBody.material.color;
            headRest = clientHead.localPosition;
        }

        public void Show(CounselingOptionData option)
        {
            root.SetActive(true);
            reaction.text = "<b>Client:</b> \"" + option.clientReaction + "\"";
            clientBody.material.color = option.quality == Quality.Good ? GoodColor
                : option.quality == Quality.Neutral ? NeutralColor : JudgmentalColor;
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(option.quality == Quality.Judgmental ? Shrink() : Nod(option.quality == Quality.Good ? 2 : 1));
        }

        public void Hide()
        {
            root.SetActive(false);
            if (running != null) StopCoroutine(running);
            running = null;
            clientBody.material.color = restingColor;
            clientHead.localPosition = headRest;
        }

        IEnumerator Nod(int times)
        {
            for (int n = 0; n < times; n++)
                for (float t = 0f; t < 1f; t += Time.deltaTime * 3f)
                {
                    clientHead.localPosition = headRest + Vector3.down * (0.06f * Mathf.Sin(t * Mathf.PI));
                    yield return null;
                }
            clientHead.localPosition = headRest;
        }

        IEnumerator Shrink()
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime * 2f)
            {
                clientHead.localPosition = headRest + new Vector3(0f, -0.08f * t, 0.08f * t);
                yield return null;
            }
        }
    }
}
