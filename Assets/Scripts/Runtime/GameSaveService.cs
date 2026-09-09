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
        private const int CurrentSaveSchemaVersion = 3;

        public static GameSaveService Instance { get; private set; }

        [SerializeField] private string saveFileName = "save.json";

        public SaveData Data { get; private set; } = new SaveData();
        public event Action<TidelingSpecies, ZoneId> SpeciesCaught;
        public event Action<ZoneId> ZoneChanged;
        public event Action<ZoneId> ZoneUnlocked;
        public event Action ExpeditionStateChanged;

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
                ZoneUnlocked?.Invoke(zone);
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

        public bool IsExpeditionChapterCompleted(string chapterId)
        {
            return ContainsId(Data?.completedExpeditionChapterIds, chapterId);
        }

        public bool CompleteExpeditionChapter(string chapterId)
        {
            return RememberExpeditionId(Data?.completedExpeditionChapterIds, chapterId);
        }

        public bool HasAuthoredDiscovery(string discoveryId)
        {
            return ContainsId(Data?.authoredDiscoveryIds, discoveryId);
        }

        public bool RememberAuthoredDiscovery(string discoveryId)
        {
            return RememberExpeditionId(Data?.authoredDiscoveryIds, discoveryId);
        }

        public bool HasLandmarkState(string landmarkStateId)
        {
            return ContainsId(Data?.landmarkStateIds, landmarkStateId);
        }

        public bool RememberLandmarkState(string landmarkStateId)
        {
            return RememberExpeditionId(Data?.landmarkStateIds, landmarkStateId);
        }

        public bool HasFieldStationUpgrade(string upgradeId)
        {
            return ContainsId(Data?.fieldStationUpgradeIds, upgradeId);
        }

        public bool RememberFieldStationUpgrade(string upgradeId)
        {
            return RememberExpeditionId(Data?.fieldStationUpgradeIds, upgradeId);
        }

        public bool HasCompletedSetPiece(string setPieceId)
        {
            return ContainsId(Data?.completedSetPieceIds, setPieceId);
        }

        public bool RememberCompletedSetPiece(string setPieceId)
        {
            return RememberExpeditionId(Data?.completedSetPieceIds, setPieceId);
        }

        public bool SetActiveExpeditionChapter(string chapterId)
        {
            if (Data == null || string.IsNullOrWhiteSpace(chapterId))
            {
                return false;
            }

            string normalizedId = chapterId.Trim();
            if (string.Equals(Data.activeExpeditionChapterId, normalizedId, StringComparison.Ordinal))
            {
                return false;
            }

            Data.activeExpeditionChapterId = normalizedId;
            Save();
            ExpeditionStateChanged?.Invoke();
            return true;
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
            bool hasStory = Data.triggeredStoryBeatIds != null && Data.triggeredStoryBeatIds.Count > 0;
            bool hasGoals = Data.completedQuestIds != null && Data.completedQuestIds.Count > 0;
            bool hasWiderZone = IsZoneUnlocked(ZoneId.KelpCurtain) || IsZoneUnlocked(ZoneId.RockyShelf);
            bool hasExpeditionProgress = Data.completedExpeditionChapterIds != null && Data.completedExpeditionChapterIds.Count > 0;
            return hasCaught
                || hasSeen
                || hasMoved
                || hasStory
                || hasGoals
                || hasWiderZone
                || hasExpeditionProgress
                || Data.currentZone != ZoneId.TidepoolShallows;
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

            NormalizeExpeditionState(Data);

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

        private bool RememberExpeditionId(List<string> ids, string id)
        {
            if (!AddId(ids, id))
            {
                return false;
            }

            Save();
            ExpeditionStateChanged?.Invoke();
            return true;
        }

        private static void NormalizeExpeditionState(SaveData data)
        {
            data.completedExpeditionChapterIds = NormalizeIds(data.completedExpeditionChapterIds);
            data.authoredDiscoveryIds = NormalizeIds(data.authoredDiscoveryIds);
            data.landmarkStateIds = NormalizeIds(data.landmarkStateIds);
            data.fieldStationUpgradeIds = NormalizeIds(data.fieldStationUpgradeIds);
            data.completedSetPieceIds = NormalizeIds(data.completedSetPieceIds);

            data.activeExpeditionChapterId = string.IsNullOrWhiteSpace(data.activeExpeditionChapterId)
                ? InferActiveChapter(data)
                : data.activeExpeditionChapterId.Trim();
        }

        private static List<string> NormalizeIds(List<string> ids)
        {
            List<string> normalized = new List<string>();
            if (ids == null)
            {
                return normalized;
            }

            for (int i = 0; i < ids.Count; i++)
            {
                AddId(normalized, ids[i]);
            }

            return normalized;
        }

        private static string InferActiveChapter(SaveData data)
        {
            if (FindCaught(data, "old-barnaby") != null)
            {
                return ExpeditionStateIds.ChapterRocky;
            }

            switch (data.currentZone)
            {
                case ZoneId.RockyShelf:
                    return ExpeditionStateIds.ChapterRocky;
                case ZoneId.KelpCurtain:
                    return ExpeditionStateIds.ChapterKelp;
                case ZoneId.SeagrassMeadow:
                    return ExpeditionStateIds.ChapterMeadow;
                default:
                    return ExpeditionStateIds.ChapterShallows;
            }
        }

        private static CaughtTideling FindCaught(SaveData data, string speciesId)
        {
            if (data?.caught == null)
            {
                return null;
            }

            for (int i = 0; i < data.caught.Count; i++)
            {
                CaughtTideling caughtTideling = data.caught[i];
                if (caughtTideling != null
                    && string.Equals(caughtTideling.speciesId, speciesId, StringComparison.OrdinalIgnoreCase))
                {
                    return caughtTideling;
                }
            }

            return null;
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
