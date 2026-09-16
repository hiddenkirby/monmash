namespace Tidepool.Domain
{
    public interface IRouteUnlockSaveState
    {
        bool IsZoneUnlocked(ZoneId zone);
        bool UnlockZone(ZoneId zone);
        bool IsExpeditionChapterCompleted(string chapterId);
        bool CompleteExpeditionChapter(string chapterId);
        bool HasLandmarkState(string landmarkStateId);
        bool RememberLandmarkState(string landmarkStateId);
        bool HasFieldStationUpgrade(string upgradeId);
        bool RememberFieldStationUpgrade(string upgradeId);
        bool HasCompletedSetPiece(string setPieceId);
        bool RememberCompletedSetPiece(string setPieceId);
        bool SetActiveExpeditionChapter(string chapterId);
    }
}
