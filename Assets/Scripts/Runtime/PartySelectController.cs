using System.Collections.Generic;
using Tidepool.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class PartySelectController : MonoBehaviour
    {
        private const int ContestPartySize = 3;

        [SerializeField] private SpeciesDatabase speciesDatabase;
        [SerializeField] private Transform listRoot;
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private Text titleText;
        [SerializeField] private string contestSceneName = "Contest";
        [SerializeField] private PlayerGridMover playerMover;

        private TidelingSpecies selectedPlayerSpecies;

        private void Start()
        {
            if (playerMover == null)
            {
                playerMover = FindObjectOfType<PlayerGridMover>();
            }

            if (titleText != null)
            {
                titleText.text = "Pick your Tideling";
            }

            PopulateList();

            if (backButton != null)
            {
                backButton.onClick.AddListener(Close);
            }
        }

        public void Close()
        {
            playerMover?.SetInputEnabled(true);

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

                GameObject entryObj = entryPrefab != null
                    ? Instantiate(entryPrefab, listRoot)
                    : CreateDefaultEntry(listRoot);

                Button entryButton = entryObj.GetComponentInChildren<Button>();
                if (entryButton == null)
                {
                    entryButton = entryObj.AddComponent<Button>();
                }

                Text nameText = entryObj.GetComponentInChildren<Text>();
                if (nameText != null)
                {
                    string nickname = string.IsNullOrWhiteSpace(entry.nickname) ? species.DisplayName : entry.nickname;
                    nameText.text = $"{nickname} (Lv {entry.level})";
                }

                TidelingSpecies captured = species;
                entryButton.onClick.RemoveAllListeners();
                entryButton.onClick.AddListener(() => SelectPlayer(captured));
            }
        }

        private GameObject CreateDefaultEntry(Transform parent)
        {
            GameObject obj = new GameObject($"Entry_{parent.childCount}");
            obj.transform.SetParent(parent, false);

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.44f, 0.50f);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, 96f);

            Text text = new GameObject("Label").AddComponent<Text>();
            text.transform.SetParent(obj.transform, false);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return obj;
        }

        private void SelectPlayer(TidelingSpecies species)
        {
            selectedPlayerSpecies = species;

            TidelingSpecies opponent = PickOpponent(species);
            ContestContext.SetPlayerParty(BuildPlayerParty(species));
            ContestContext.SetVisitingParty(BuildVisitingParty(opponent, species));

            ContestContext.PlayerSpecies = species;
            ContestContext.VisitingSpecies = opponent;
            playerMover?.SetInputEnabled(false);
            SceneManager.LoadScene(contestSceneName, LoadSceneMode.Additive);
        }

        private TidelingSpecies PickOpponent(TidelingSpecies exclude)
        {
            if (speciesDatabase == null)
            {
                return null;
            }

            List<TidelingSpecies> candidates = new List<TidelingSpecies>();
            IReadOnlyList<TidelingSpecies> all = speciesDatabase.All;

            for (int i = 0; i < all.Count; i++)
            {
                TidelingSpecies candidate = all[i];
                if (candidate != null && candidate.Id != exclude?.Id)
                {
                    candidates.Add(candidate);
                }
            }

            if (candidates.Count == 0)
            {
                return exclude;
            }

            return candidates[Random.Range(0, candidates.Count)];
        }

        private List<TidelingSpecies> BuildPlayerParty(TidelingSpecies selected)
        {
            List<TidelingSpecies> party = new List<TidelingSpecies>();
            AddToParty(party, selected);

            if (speciesDatabase == null || GameSaveService.Instance == null)
            {
                return party;
            }

            List<CaughtTideling> caught = GameSaveService.Instance.Data.caught;
            for (int i = 0; i < caught.Count && party.Count < ContestPartySize; i++)
            {
                TidelingSpecies species = speciesDatabase.FindById(caught[i].speciesId);
                AddToParty(party, species);
            }

            return party;
        }

        private List<TidelingSpecies> BuildVisitingParty(TidelingSpecies selected, TidelingSpecies playerSpecies)
        {
            List<TidelingSpecies> party = new List<TidelingSpecies>();
            AddToParty(party, selected);

            if (speciesDatabase == null)
            {
                return party;
            }

            IReadOnlyList<TidelingSpecies> all = speciesDatabase.All;
            for (int i = 0; i < all.Count && party.Count < ContestPartySize; i++)
            {
                TidelingSpecies candidate = all[i];
                if (candidate != null && candidate.Id != playerSpecies?.Id)
                {
                    AddToParty(party, candidate);
                }
            }

            return party;
        }

        private static void AddToParty(List<TidelingSpecies> party, TidelingSpecies species)
        {
            if (species == null || party.Contains(species))
            {
                return;
            }

            party.Add(species);
        }
    }
}
