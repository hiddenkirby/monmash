using System;
using System.Collections.Generic;

namespace Tidepool.Domain
{
    [Serializable]
    public class SaveData
    {
        public int schemaVersion = 3;
        public List<CaughtTideling> caught = new List<CaughtTideling>();
        public List<string> seenSpeciesIds = new List<string>();
        public List<string> triggeredStoryBeatIds = new List<string>();
        public List<string> completedQuestIds = new List<string>();
        public List<ZoneId> unlockedZoneIds = new List<ZoneId>
        {
            ZoneId.TidepoolShallows,
            ZoneId.SeagrassMeadow
        };
        public string activeExpeditionChapterId = ExpeditionStateIds.ChapterShallows;
        public List<string> completedExpeditionChapterIds = new List<string>();
        public List<string> authoredDiscoveryIds = new List<string>();
        public List<string> landmarkStateIds = new List<string>();
        public List<string> fieldStationUpgradeIds = new List<string>();
        public List<string> completedSetPieceIds = new List<string>();
        public SerializableVector2Int playerTile = new SerializableVector2Int(0, 0);
        public ZoneId currentZone = ZoneId.TidepoolShallows;
    }
}
