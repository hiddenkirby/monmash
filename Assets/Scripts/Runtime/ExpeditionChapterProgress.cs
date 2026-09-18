using System;
using System.Collections.Generic;
using Tidepool.Domain;

namespace Tidepool.Runtime
{
    public static class ExpeditionChapterProgress
    {
        public static ExpeditionChapter FindActiveChapter(ExpeditionChapter[] chapters, SaveData data)
        {
            if (chapters == null || chapters.Length == 0)
            {
                return null;
            }

            if (data != null && !string.IsNullOrWhiteSpace(data.activeExpeditionChapterId))
            {
                ExpeditionChapter active = FindById(chapters, data.activeExpeditionChapterId);
                if (active != null && !IsCompleted(active, data))
                {
                    return active;
                }
            }

            for (int i = 0; i < chapters.Length; i++)
            {
                ExpeditionChapter chapter = chapters[i];
                if (chapter != null && !IsCompleted(chapter, data) && HasPrerequisite(chapter, data))
                {
                    return chapter;
                }
            }

            return null;
        }

        public static bool IsCompletionReady(ExpeditionChapter chapter, SaveData data)
        {
            if (chapter == null || data == null || !HasPrerequisite(chapter, data))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(chapter.RequiredLandmarkStateId)
                && !ContainsId(data.landmarkStateIds, chapter.RequiredLandmarkStateId))
            {
                return false;
            }

            bool hasRule = false;
            bool ready = false;

            if (chapter.MinimumAuthoredDiscoveries > 0)
            {
                hasRule = true;
                ready |= CountMatchingIds(data.authoredDiscoveryIds, chapter.QualifyingDiscoveryIds)
                    >= chapter.MinimumAuthoredDiscoveries;
            }

            if (chapter.MinimumCaughtSpecies > 0)
            {
                hasRule = true;
                ready |= CountCaughtSpeciesInZone(data, chapter.CatchZone) >= chapter.MinimumCaughtSpecies;
            }

            if (!string.IsNullOrWhiteSpace(chapter.CompletionSpeciesId))
            {
                hasRule = true;
                ready |= HasCaughtSpecies(data, chapter.CompletionSpeciesId);
            }

            return hasRule && ready;
        }

        public static bool ShouldBackfillCompletion(ExpeditionChapter chapter, SaveData data)
        {
            if (chapter == null || data == null || string.IsNullOrWhiteSpace(chapter.Id))
            {
                return false;
            }

            if (IsCompleted(chapter, data))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(chapter.CompletionSpeciesId)
                && HasCaughtSpecies(data, chapter.CompletionSpeciesId))
            {
                return true;
            }

            switch (chapter.Zone)
            {
                case ZoneId.TidepoolShallows:
                    return data.currentZone == ZoneId.KelpCurtain
                        || data.currentZone == ZoneId.RockyShelf
                        || IsZoneUnlocked(data, ZoneId.KelpCurtain)
                        || IsZoneUnlocked(data, ZoneId.RockyShelf);
                case ZoneId.SeagrassMeadow:
                    return data.currentZone == ZoneId.KelpCurtain
                        || data.currentZone == ZoneId.RockyShelf
                        || IsZoneUnlocked(data, ZoneId.KelpCurtain)
                        || IsZoneUnlocked(data, ZoneId.RockyShelf);
                case ZoneId.KelpCurtain:
                    return data.currentZone == ZoneId.RockyShelf
                        || IsZoneUnlocked(data, ZoneId.RockyShelf);
                default:
                    return false;
            }
        }

        public static string GetNextStep(ExpeditionChapter chapter, SaveData data)
        {
            if (chapter == null)
            {
                return "The coast remembers your Great Low Tide.";
            }

            if (IsCompletionReady(chapter, data))
            {
                return string.IsNullOrWhiteSpace(chapter.CompletionSummary)
                    ? "The next path is ready."
                    : chapter.CompletionSummary;
            }

            return string.IsNullOrWhiteSpace(chapter.ActiveObjective)
                ? "Look around. The coast has something to notice."
                : chapter.ActiveObjective;
        }

        public static ExpeditionChapter FindById(ExpeditionChapter[] chapters, string chapterId)
        {
            if (chapters == null || string.IsNullOrWhiteSpace(chapterId))
            {
                return null;
            }

            for (int i = 0; i < chapters.Length; i++)
            {
                ExpeditionChapter chapter = chapters[i];
                if (chapter != null && IdEquals(chapter.Id, chapterId))
                {
                    return chapter;
                }
            }

            return null;
        }

        private static bool HasPrerequisite(ExpeditionChapter chapter, SaveData data)
        {
            return string.IsNullOrWhiteSpace(chapter.RequiredCompletedChapterId)
                || ContainsId(data?.completedExpeditionChapterIds, chapter.RequiredCompletedChapterId);
        }

        private static bool IsCompleted(ExpeditionChapter chapter, SaveData data)
        {
            return chapter != null && ContainsId(data?.completedExpeditionChapterIds, chapter.Id);
        }

        private static int CountMatchingIds(List<string> savedIds, string[] qualifyingIds)
        {
            if (savedIds == null || qualifyingIds == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < qualifyingIds.Length; i++)
            {
                if (ContainsId(savedIds, qualifyingIds[i]))
                {
                    count += 1;
                }
            }

            return count;
        }

        private static int CountCaughtSpeciesInZone(SaveData data, ZoneId zone)
        {
            if (data?.caught == null)
            {
                return 0;
            }

            HashSet<string> speciesIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < data.caught.Count; i++)
            {
                CaughtTideling caught = data.caught[i];
                if (caught != null && caught.caughtInZone == zone && !string.IsNullOrWhiteSpace(caught.speciesId))
                {
                    speciesIds.Add(caught.speciesId.Trim());
                }
            }

            return speciesIds.Count;
        }

        private static bool HasCaughtSpecies(SaveData data, string speciesId)
        {
            if (data?.caught == null || string.IsNullOrWhiteSpace(speciesId))
            {
                return false;
            }

            for (int i = 0; i < data.caught.Count; i++)
            {
                if (data.caught[i] != null && IdEquals(data.caught[i].speciesId, speciesId))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsZoneUnlocked(SaveData data, ZoneId zone)
        {
            return data.unlockedZoneIds != null && data.unlockedZoneIds.Contains(zone);
        }

        private static bool ContainsId(List<string> ids, string id)
        {
            if (ids == null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            for (int i = 0; i < ids.Count; i++)
            {
                if (IdEquals(ids[i], id))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IdEquals(string left, string right)
        {
            return string.Equals(left?.Trim(), right?.Trim(), StringComparison.Ordinal);
        }
    }
}
