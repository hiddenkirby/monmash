using System;
using UnityEngine;

namespace Tidepool.Domain
{
    public enum ContestAiPattern
    {
        Aggressive,
        Defensive,
        Tricky
    }

    [CreateAssetMenu(menuName = "Tidepool/Tideling Species", fileName = "NewTidelingSpecies")]
    public class TidelingSpecies : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private TidelingCurrent current;
        [SerializeField] private TidelingRarity rarity;
        [SerializeField] private Sprite sprite;
        [SerializeField, TextArea(2, 5)] private string fieldNote;
        [SerializeField] private ZoneId[] habitatZones = Array.Empty<ZoneId>();
        [SerializeField] private EncounterAvailability encounterAvailability = EncounterAvailability.Always;
        [SerializeField] private string availabilityHint;
        [SerializeField] private ContestMove firstContestMove;
        [SerializeField, Range(CaughtTideling.MinLevel, CaughtTideling.MaxLevel)]
        private int firstContestMoveUnlockLevel = CaughtTideling.MinLevel;
        [SerializeField] private ContestMove secondContestMove;
        [SerializeField, Range(CaughtTideling.MinLevel, CaughtTideling.MaxLevel)]
        private int secondContestMoveUnlockLevel = 3;
        [SerializeField] private ContestAiPattern visitingContestAiPattern = ContestAiPattern.Aggressive;
        [SerializeField, Range(0.1f, 0.75f)] private float catchZoneWidth = 0.35f;
        [SerializeField, Min(0.1f)] private float catchMarkerSpeed = 0.65f;

        public string Id => id;
        public string DisplayName => displayName;
        public TidelingCurrent Current => current;
        public TidelingRarity Rarity => rarity;
        public Sprite Sprite => sprite;
        public string FieldNote => fieldNote;
        public ZoneId[] HabitatZones => habitatZones;
        public EncounterAvailability EncounterAvailability => encounterAvailability;
        public string AvailabilityHint => availabilityHint;
        public ContestMove FirstContestMove => firstContestMove;
        public int FirstContestMoveUnlockLevel => firstContestMoveUnlockLevel;
        public ContestMove SecondContestMove => secondContestMove;
        public int SecondContestMoveUnlockLevel => secondContestMoveUnlockLevel;
        public ContestAiPattern VisitingContestAiPattern => visitingContestAiPattern;
        public float CatchZoneWidth => catchZoneWidth;
        public float CatchMarkerSpeed => catchMarkerSpeed;

        public ContestMove GetContestMove(int index)
        {
            if (index == 0)
            {
                return firstContestMove;
            }

            return index == 1 ? secondContestMove : null;
        }

        public int GetContestMoveUnlockLevel(int index)
        {
            if (index == 0)
            {
                return ClampUnlockLevel(firstContestMoveUnlockLevel);
            }

            return index == 1
                ? ClampUnlockLevel(secondContestMoveUnlockLevel)
                : CaughtTideling.MaxLevel;
        }

        public ContestMove GetUnlockedContestMove(int index, int level)
        {
            return IsContestMoveUnlocked(index, level) ? GetContestMove(index) : null;
        }

        public bool IsContestMoveUnlocked(int index, int level)
        {
            return GetContestMove(index) != null && level >= GetContestMoveUnlockLevel(index);
        }

        public int CountUnlockedContestMoves(int level)
        {
            int count = 0;
            if (IsContestMoveUnlocked(0, level))
            {
                count += 1;
            }

            if (IsContestMoveUnlocked(1, level))
            {
                count += 1;
            }

            return count;
        }

        public bool LivesIn(ZoneId zone)
        {
            for (int i = 0; i < habitatZones.Length; i++)
            {
                if (habitatZones[i] == zone)
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        public void Configure(
            string speciesId,
            string speciesName,
            TidelingCurrent speciesCurrent,
            TidelingRarity speciesRarity,
            ZoneId[] speciesHabitats,
            EncounterAvailability speciesEncounterAvailability,
            string speciesAvailabilityHint,
            string speciesFieldNote,
            ContestAiPattern speciesVisitingContestAiPattern,
            float speciesCatchZoneWidth,
            float speciesCatchMarkerSpeed)
        {
            id = speciesId;
            displayName = speciesName;
            current = speciesCurrent;
            rarity = speciesRarity;
            habitatZones = speciesHabitats;
            encounterAvailability = speciesEncounterAvailability;
            availabilityHint = speciesAvailabilityHint;
            firstContestMove = null;
            firstContestMoveUnlockLevel = CaughtTideling.MinLevel;
            secondContestMove = null;
            secondContestMoveUnlockLevel = 3;
            visitingContestAiPattern = speciesVisitingContestAiPattern;
            fieldNote = speciesFieldNote;
            catchZoneWidth = speciesCatchZoneWidth;
            catchMarkerSpeed = speciesCatchMarkerSpeed;
        }
#endif

        private static int ClampUnlockLevel(int unlockLevel)
        {
            if (unlockLevel < CaughtTideling.MinLevel)
            {
                return CaughtTideling.MinLevel;
            }

            return unlockLevel > CaughtTideling.MaxLevel ? CaughtTideling.MaxLevel : unlockLevel;
        }
    }
}
