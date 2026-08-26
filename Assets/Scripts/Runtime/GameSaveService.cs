using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Tidepool.Domain;
using UnityEngine;

namespace Tidepool.Runtime
{
    public class GameSaveService : MonoBehaviour
    {
        private const int CurrentSaveSchemaVersion = 2;

        public static GameSaveService Instance { get; private set; }

        [SerializeField] private string saveFileName = "save.json";

        public SaveData Data { get; private set; } = new SaveData();
        public event Action<TidelingSpecies, ZoneId> SpeciesCaught;
        public event Action<ZoneId> ZoneChanged;

        private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Load()
        {
            if (!File.Exists(SavePath))
            {
                Data = new SaveData();
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                Data = string.IsNullOrWhiteSpace(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
                NormalizeLoadedData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not load Tidepool save data. Starting a new save. {exception.Message}");
                Data = new SaveData();
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(SavePath, json);
        }

        public void SetPlayerTile(Vector2Int playerTile)
        {
            Data.playerTile = new SerializableVector2Int(playerTile);
            Save();
        }

        public void SetCurrentZone(ZoneId zone)
        {
            Data.currentZone = zone;
            Save();
            ZoneChanged?.Invoke(zone);
        }

        public bool HasTriggeredBeat(string beatId)
        {
            return ContainsId(Data?.triggeredStoryBeatIds, beatId);
        }

        public void MarkBeatTriggered(string beatId)
        {
            if (AddId(Data.triggeredStoryBeatIds, beatId))
            {
                Save();
            }
        }

        public bool IsZoneUnlocked(ZoneId zone)
        {
            return Data != null
                && Data.unlockedZoneIds != null
                && Data.unlockedZoneIds.Contains(zone);
        }

        public void UnlockZone(ZoneId zone)
        {
            if (Data == null)
            {
                return;
            }

            if (Data.unlockedZoneIds == null)
            {
                Data.unlockedZoneIds = new List<ZoneId>();
            }

            if (!Data.unlockedZoneIds.Contains(zone))
            {
                Data.unlockedZoneIds.Add(zone);
                Save();
            }
        }

        public bool HasCompletedQuest(string questId)
        {
            return ContainsId(Data?.completedQuestIds, questId);
        }

        public void MarkQuestCompleted(string questId)
        {
            if (AddId(Data.completedQuestIds, questId))
            {
                Save();
            }
        }

        public void MarkSeen(string speciesId)
        {
            MarkSeen(speciesId, true);
        }

        private void MarkSeen(string speciesId, bool saveWhenChanged)
        {
            if (string.IsNullOrWhiteSpace(speciesId))
            {
                return;
            }

            if (!Data.seenSpeciesIds.Contains(speciesId))
            {
                Data.seenSpeciesIds.Add(speciesId);
                if (saveWhenChanged)
                {
                    Save();
                }
            }
        }

        public void RecordCatch(TidelingSpecies species, ZoneId zone)
        {
            if (species == null)
            {
                return;
            }

            MarkSeen(species.Id, false);

            CaughtTideling existing = FindCaught(species.Id);
            if (existing != null)
            {
                existing.timesSeen += 1;
                Save();
                SpeciesCaught?.Invoke(species, zone);
                return;
            }

            Data.caught.Add(new CaughtTideling
            {
                speciesId = species.Id,
                nickname = species.DisplayName,
                caughtAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                caughtInZone = zone,
                timesSeen = 1,
                level = CaughtTideling.MinLevel,
                levelProgress = 0
            });

            Save();
            SpeciesCaught?.Invoke(species, zone);
        }

        public void RenameCaught(string speciesId, string nickname)
        {
            CaughtTideling caught = FindCaught(speciesId);
            if (caught == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            string trimmedNickname = nickname.Trim();
            caught.nickname = trimmedNickname.Substring(
                0,
                Mathf.Min(CaughtTideling.NicknameCharacterLimit, trimmedNickname.Length));
            Save();
        }

        public bool RecordGentleProgress(string speciesId, int progressPoints)
        {
            CaughtTideling caught = FindCaught(speciesId);
            bool changed = TidelingLevelProgression.AddProgress(caught, progressPoints);
            if (changed)
            {
                Save();
            }

            return changed;
        }

        public bool RememberGrowthForm(string speciesId, string formId)
        {
            CaughtTideling caught = FindCaught(speciesId);
            bool changed = TidelingGrowthForms.Remember(caught, formId);
            if (changed)
            {
                Save();
            }

            return changed;
        }

        public bool SelectGrowthForm(string speciesId, string formId)
        {
            CaughtTideling caught = FindCaught(speciesId);
            bool changed = TidelingGrowthForms.SelectRemembered(caught, formId);
            if (changed)
            {
                Save();
            }

            return changed;
        }

        public bool SelectOriginalGrowthForm(string speciesId)
        {
            return SelectGrowthForm(speciesId, TidelingGrowthForms.OriginalFormId);
        }

        public bool HasSeen(string speciesId)
        {
            if (string.IsNullOrWhiteSpace(speciesId) || Data == null || Data.seenSpeciesIds == null)
            {
                return false;
            }

            return Data.seenSpeciesIds.Contains(speciesId);
        }

        public int CountCaughtSpeciesExcluding(string excludedSpeciesId)
        {
            if (Data == null || Data.caught == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < Data.caught.Count; i++)
            {
                CaughtTideling caught = Data.caught[i];
                if (caught != null
                    && !string.IsNullOrWhiteSpace(caught.speciesId)
                    && !string.Equals(caught.speciesId, excludedSpeciesId, StringComparison.OrdinalIgnoreCase))
                {
                    count += 1;
                }
            }

            return count;
        }

        public int CountCaughtSpecies()
        {
            return CountCaughtSpeciesExcluding(null);
        }

        public int CountCaughtSpeciesInZone(ZoneId zone)
        {
            if (Data == null || Data.caught == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < Data.caught.Count; i++)
            {
                CaughtTideling caught = Data.caught[i];
                if (caught != null
                    && !string.IsNullOrWhiteSpace(caught.speciesId)
                    && caught.caughtInZone == zone)
                {
                    count += 1;
                }
            }

            return count;
        }

        public bool HasAnyProgress()
        {
            if (Data == null)
            {
                return false;
            }

            bool hasCaught = Data.caught != null && Data.caught.Count > 0;
            bool hasSeen = Data.seenSpeciesIds != null && Data.seenSpeciesIds.Count > 0;
            bool hasMoved = Data.playerTile.ToVector2Int() != Vector2Int.zero;
            return hasCaught || hasSeen || hasMoved || Data.currentZone != ZoneId.TidepoolShallows;
        }

        public CaughtTideling FindCaught(string speciesId)
        {
            for (int i = 0; i < Data.caught.Count; i++)
            {
                if (Data.caught[i].speciesId == speciesId)
                {
                    return Data.caught[i];
                }
            }

            return null;
        }

        private void NormalizeLoadedData()
        {
            if (Data == null || Data.schemaVersion <= 0)
            {
                Data = new SaveData();
                return;
            }

            bool unlockAllZonesForMigration = Data.schemaVersion < CurrentSaveSchemaVersion;

            if (Data.caught == null)
            {
                Data.caught = new List<CaughtTideling>();
            }

            if (Data.seenSpeciesIds == null)
            {
                Data.seenSpeciesIds = new List<string>();
            }

            if (Data.triggeredStoryBeatIds == null)
            {
                Data.triggeredStoryBeatIds = new List<string>();
            }

            if (Data.completedQuestIds == null)
            {
                Data.completedQuestIds = new List<string>();
            }

            if (Data.unlockedZoneIds == null)
            {
                Data.unlockedZoneIds = new List<ZoneId>();
            }

            if (unlockAllZonesForMigration)
            {
                UnlockAllZonesWithoutSaving();
            }
            else
            {
                UnlockZoneWithoutSaving(ZoneId.TidepoolShallows);
                UnlockZoneWithoutSaving(ZoneId.SeagrassMeadow);
            }

            Data.schemaVersion = CurrentSaveSchemaVersion;

            for (int i = 0; i < Data.caught.Count; i++)
            {
                TidelingLevelProgression.Normalize(Data.caught[i]);
                TidelingGrowthForms.Normalize(Data.caught[i]);
            }
        }

        private static bool ContainsId(List<string> ids, string id)
        {
            return !string.IsNullOrWhiteSpace(id)
                && ids != null
                && ids.Contains(id);
        }

        private static bool AddId(List<string> ids, string id)
        {
            if (string.IsNullOrWhiteSpace(id) || ids == null)
            {
                return false;
            }

            string trimmedId = id.Trim();
            if (ids.Contains(trimmedId))
            {
                return false;
            }

            ids.Add(trimmedId);
            return true;
        }

        private void UnlockAllZonesWithoutSaving()
        {
            foreach (ZoneId zone in Enum.GetValues(typeof(ZoneId)))
            {
                UnlockZoneWithoutSaving(zone);
            }
        }

        private void UnlockZoneWithoutSaving(ZoneId zone)
        {
            if (!Data.unlockedZoneIds.Contains(zone))
            {
                Data.unlockedZoneIds.Add(zone);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }
    }
}
