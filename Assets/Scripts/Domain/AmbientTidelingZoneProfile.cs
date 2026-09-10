using System;
using UnityEngine;

namespace Tidepool.Domain
{
    [CreateAssetMenu(menuName = "Tidepool/Ambient Tideling Zone Profile", fileName = "NewAmbientTidelingZoneProfile")]
    public class AmbientTidelingZoneProfile : ScriptableObject
    {
        [SerializeField] private ZoneId zone = ZoneId.TidepoolShallows;
        [SerializeField, Range(0, 32)] private int maxVisibleActors = 8;
        [SerializeField, Range(0f, 1f)] private float reducedMotionDensityScale = 0.35f;
        [SerializeField] private AmbientTidelingSpawnEntry[] spawns = Array.Empty<AmbientTidelingSpawnEntry>();

        public ZoneId Zone => zone;
        public int MaxVisibleActors => Mathf.Max(0, maxVisibleActors);
        public float ReducedMotionDensityScale => Mathf.Clamp01(reducedMotionDensityScale);
        public AmbientTidelingSpawnEntry[] Spawns => spawns ?? Array.Empty<AmbientTidelingSpawnEntry>();

#if UNITY_EDITOR
        public void Configure(
            ZoneId profileZone,
            int profileMaxVisibleActors,
            float profileReducedMotionDensityScale,
            AmbientTidelingSpawnEntry[] profileSpawns)
        {
            zone = profileZone;
            maxVisibleActors = profileMaxVisibleActors;
            reducedMotionDensityScale = Mathf.Clamp01(profileReducedMotionDensityScale);
            spawns = profileSpawns ?? Array.Empty<AmbientTidelingSpawnEntry>();
        }
#endif
    }
}
