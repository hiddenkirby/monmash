using System;
using UnityEngine;

namespace Tidepool.Domain
{
    [Serializable]
    public class AmbientTidelingSpawnEntry
    {
        [SerializeField] private string id = "ambient-tideling";
        [SerializeField] private TidelingSpecies species;
        [SerializeField] private AmbientTidelingBehaviorKind behaviorKind = AmbientTidelingBehaviorKind.Drift;
        [SerializeField] private AmbientTidelingTimeWindow timeWindow = AmbientTidelingTimeWindow.Any;
        [SerializeField] private bool requireSpeciesCaught;
        [SerializeField] private string[] requiredStateIds = Array.Empty<string>();
        [SerializeField] private string[] blockedStateIds = Array.Empty<string>();
        [SerializeField, Range(0, 24)] private int poolSize = 3;
        [SerializeField, Range(0, 12)] private int reducedMotionVisibleCount = 1;
        [SerializeField] private Vector2 spawnArea = new Vector2(8f, 4f);
        [SerializeField] private Vector2 scaleRange = new Vector2(0.28f, 0.45f);
        [SerializeField] private Vector2 speedRange = new Vector2(0.04f, 0.12f);
        [SerializeField, Min(0f)] private float motionAmplitude = 0.24f;
        [SerializeField, Min(0f)] private float motionFrequency = 0.45f;
        [SerializeField, Range(0f, 1f)] private float approachStrength = 0.25f;
        [SerializeField] private Color dayTint = Color.white;
        [SerializeField] private Color nightTint = new Color(0.65f, 0.78f, 1f, 0.92f);
        [SerializeField] private int sortingOrder = 2;

        public string Id => string.IsNullOrWhiteSpace(id) ? "ambient-tideling" : id.Trim();
        public TidelingSpecies Species => species;
        public AmbientTidelingBehaviorKind BehaviorKind => behaviorKind;
        public AmbientTidelingTimeWindow TimeWindow => timeWindow;
        public bool RequireSpeciesCaught => requireSpeciesCaught;
        public string[] RequiredStateIds => requiredStateIds ?? Array.Empty<string>();
        public string[] BlockedStateIds => blockedStateIds ?? Array.Empty<string>();
        public int PoolSize => Mathf.Max(0, poolSize);
        public int ReducedMotionVisibleCount => Mathf.Clamp(reducedMotionVisibleCount, 0, PoolSize);
        public Vector2 SpawnArea => ClampVector(spawnArea, 0f, float.MaxValue);
        public Vector2 ScaleRange => SortRange(scaleRange, 0.01f);
        public Vector2 SpeedRange => SortRange(speedRange, 0f);
        public float MotionAmplitude => Mathf.Max(0f, motionAmplitude);
        public float MotionFrequency => Mathf.Max(0f, motionFrequency);
        public float ApproachStrength => Mathf.Clamp01(approachStrength);
        public Color DayTint => dayTint;
        public Color NightTint => nightTint;
        public int SortingOrder => sortingOrder;

        public AmbientTidelingSpawnEntry()
        {
        }

        public AmbientTidelingSpawnEntry(
            string entryId,
            TidelingSpecies entrySpecies,
            AmbientTidelingBehaviorKind entryBehaviorKind,
            AmbientTidelingTimeWindow entryTimeWindow,
            bool entryRequireSpeciesCaught,
            string[] entryRequiredStateIds,
            string[] entryBlockedStateIds,
            int entryPoolSize,
            int entryReducedMotionVisibleCount,
            Vector2 entrySpawnArea,
            Vector2 entryScaleRange,
            Vector2 entrySpeedRange,
            float entryMotionAmplitude,
            float entryMotionFrequency,
            float entryApproachStrength,
            Color entryDayTint,
            Color entryNightTint,
            int entrySortingOrder)
        {
            id = entryId;
            species = entrySpecies;
            behaviorKind = entryBehaviorKind;
            timeWindow = entryTimeWindow;
            requireSpeciesCaught = entryRequireSpeciesCaught;
            requiredStateIds = entryRequiredStateIds ?? Array.Empty<string>();
            blockedStateIds = entryBlockedStateIds ?? Array.Empty<string>();
            poolSize = entryPoolSize;
            reducedMotionVisibleCount = entryReducedMotionVisibleCount;
            spawnArea = entrySpawnArea;
            scaleRange = entryScaleRange;
            speedRange = entrySpeedRange;
            motionAmplitude = entryMotionAmplitude;
            motionFrequency = entryMotionFrequency;
            approachStrength = entryApproachStrength;
            dayTint = entryDayTint;
            nightTint = entryNightTint;
            sortingOrder = entrySortingOrder;
        }

        private static Vector2 SortRange(Vector2 range, float minimum)
        {
            float x = Mathf.Max(minimum, range.x);
            float y = Mathf.Max(minimum, range.y);
            return x <= y ? new Vector2(x, y) : new Vector2(y, x);
        }

        private static Vector2 ClampVector(Vector2 value, float minimum, float maximum)
        {
            return new Vector2(
                Mathf.Clamp(value.x, minimum, maximum),
                Mathf.Clamp(value.y, minimum, maximum));
        }
    }
}
