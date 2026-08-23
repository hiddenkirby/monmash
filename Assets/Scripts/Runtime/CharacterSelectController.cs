using System.Collections.Generic;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class CharacterSelectController : MonoBehaviour
    {
        [SerializeField] private SpeciesDatabase speciesDatabase;
        [SerializeField] private Transform listRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Button backButton;

        private void Start()
        {
            if (titleText != null)
            {
                titleText.text = "Pick who to walk as";
            }

            PopulateList();

            if (backButton != null)
            {
                backButton.onClick.AddListener(Close);
            }
        }

        public void Close()
        {
            PlayerGridMover playerMover = FindObjectOfType<PlayerGridMover>();
            if (playerMover != null)
            {
                playerMover.SetInputEnabled(true);
            }

            if (SceneManager.sceneCount > 1)
            {
                SceneManager.UnloadSceneAsync(gameObject.scene);
            }
        }

        private void PopulateList()
        {
            if (listRoot == null || speciesDatabase == null || GameSaveService.Instance == null)
            {
                return;
            }

            for (int i = listRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(listRoot.GetChild(i).gameObject);
            }

            List<CaughtTideling> caught = GameSaveService.Instance.Data.caught;

            if (caught.Count == 0)
            {
                if (titleText != null)
                {
                    titleText.text = "Catch some Tidelings first!";
                }

                return;
            }

            for (int i = 0; i < caught.Count; i++)
            {
                CaughtTideling entry = caught[i];
                TidelingSpecies species = speciesDatabase.FindById(entry.speciesId);
                if (species == null)
                {
                    continue;
                }

                GameObject entryObj = CreateEntry(listRoot, species, entry);
                Button entryButton = entryObj.GetComponentInChildren<Button>();

                TidelingSpecies captured = species;
                entryButton.onClick.RemoveAllListeners();
                entryButton.onClick.AddListener(() => SelectCharacter(captured));
            }
        }

        private GameObject CreateEntry(Transform parent, TidelingSpecies species, CaughtTideling caught)
        {
            GameObject obj = new GameObject($"Entry_{parent.childCount}");
            obj.transform.SetParent(parent, false);

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.44f, 0.50f);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, 96f);

            Image portrait = new GameObject("Portrait").AddComponent<Image>();
            portrait.transform.SetParent(obj.transform, false);
            portrait.sprite = species.Sprite;
            portrait.enabled = species.Sprite != null;
            portrait.preserveAspect = true;
            portrait.color = Color.white;
            RectTransform portraitRect = portrait.GetComponent<RectTransform>();
            portraitRect.sizeDelta = new Vector2(64f, 64f);
            portraitRect.anchoredPosition = new Vector2(-150f, 0f);

            Text text = new GameObject("Label").AddComponent<Text>();
            text.transform.SetParent(obj.transform, false);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            string nickname = string.IsNullOrWhiteSpace(caught.nickname) ? species.DisplayName : caught.nickname;
            text.text = $"{nickname} (Lv {caught.level})";
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(40f, 0f);
            textRect.offsetMax = new Vector2(0f, 0f);

            Button button = obj.AddComponent<Button>();
            button.targetGraphic = bg;

            return obj;
        }

        private void SelectCharacter(TidelingSpecies species)
        {
            PlayerGridMover playerMover = FindObjectOfType<PlayerGridMover>();
            if (playerMover != null)
            {
                SpriteRenderer renderer = playerMover.GetComponentInChildren<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = playerMover.GetComponent<SpriteRenderer>();
                }

                if (renderer != null && species.Sprite != null)
                {
                    renderer.sprite = species.Sprite;
                }
            }

            Close();
        }
    }
}
