using System;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.UI
{
    public class JournalSlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image creatureImage;
        [SerializeField] private Text nameText;

        public void Bind(TidelingSpecies species, bool isCaught, Action onClick)
        {
            creatureImage.sprite = species.Sprite;
            creatureImage.enabled = species.Sprite != null;
            creatureImage.color = isCaught ? Color.white : Color.black;
            nameText.text = isCaught ? species.DisplayName : "?";

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}

