using System;
using UnityEngine;

namespace Tidepool.Domain
{
    [CreateAssetMenu(menuName = "Tidepool/Ambient Zone Profile", fileName = "NewAmbientZoneProfile")]
    public class AmbientZoneProfile : ScriptableObject
    {
        [SerializeField] private ZoneId zone = ZoneId.TidepoolShallows;
        [SerializeField] private Color dayTint = Color.white;
        [SerializeField] private Color nightTint = new Color(0.55f, 0.65f, 0.85f, 1f);
        [SerializeField, Range(0f, 1f)] private float reducedMotionDensityScale = 0.2f;
        [SerializeField] private AmbientMotionLayer[] layers = Array.Empty<AmbientMotionLayer>();

        public ZoneId Zone => zone;
        public Color DayTint => dayTint;
        public Color NightTint => nightTint;
        public float ReducedMotionDensityScale => Mathf.Clamp01(reducedMotionDensityScale);
        public AmbientMotionLayer[] Layers => layers ?? Array.Empty<AmbientMotionLayer>();

#if UNITY_EDITOR
        public void Configure(
            ZoneId profileZone,
            Color profileDayTint,
            Color profileNightTint,
            float profileReducedMotionDensityScale,
            AmbientMotionLayer[] profileLayers)
        {
            zone = profileZone;
            dayTint = profileDayTint;
            nightTint = profileNightTint;
            reducedMotionDensityScale = Mathf.Clamp01(profileReducedMotionDensityScale);
            layers = profileLayers ?? Array.Empty<AmbientMotionLayer>();
        }
#endif
    }
}
