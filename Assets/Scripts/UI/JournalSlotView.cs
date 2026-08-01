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
            bool hasSpecies = species != null;
            Sprite sprite = hasSpecies ? species.Sprite : null;

            creatureImage.sprite = sprite;
            creatureImage.enabled = sprite != null;
            creatureImage.color = isCaught ? Color.white : Color.black;
            creatureImage.preserveAspect = true;
            nameText.text = isCaught && hasSpecies ? species.DisplayName : "?";
            button.interactable = hasSpecies;

            button.onClick.RemoveAllListeners();
            if (hasSpecies)
            {
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }
    }
}
