using System;
using System.Collections.Generic;
using PlateCheck.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlateCheck.UI
{
    /// <summary>The three responses the learner can pick in Act.</summary>
    public class CounselingPanel : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Button[] optionButtons = new Button[3];
        [SerializeField] TMP_Text[] optionLabels = new TMP_Text[3];

        public event Action<int> OptionChosen;

        void Awake()
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                optionButtons[i].onClick.AddListener(() => OptionChosen?.Invoke(index));
            }
        }

        public void Show(IReadOnlyList<CounselingOptionData> options)
        {
            root.SetActive(true);
            for (int i = 0; i < optionButtons.Length; i++)
            {
                bool used = i < options.Count;
                optionButtons[i].gameObject.SetActive(used);
                if (used) optionLabels[i].text = options[i].text;
            }
        }

        public void Hide() => root.SetActive(false);
    }
}
