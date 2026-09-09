using System;
using UnityEngine;

namespace Tidepool.Domain
{
    public enum AmbientMotionKind
    {
        WaterShimmer,
        SwayingPlant,
        DriftingParticle,
        DistantSilhouette,
        SoftLightPulse
    }

    [Serializable]
    public class AmbientMotionLayer
    {
        [SerializeField] private string id = "ambient-layer";
        [SerializeField] private AmbientMotionKind motionKind = AmbientMotionKind.DriftingParticle;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Material material;
        [SerializeField] private Color dayTint = Color.white;
        [SerializeField] private Color nightTint = new Color(0.55f, 0.72f, 0.9f, 0.75f);
        [SerializeField, Range(0, 64)] private int poolSize = 8;
        [SerializeField, Min(0f)] private int reducedMotionVisibleCount = 1;
        [SerializeField] private bool hideWhenReducedMotion;
        [SerializeField] private Vector2 spawnArea = new Vector2(18f, 8f);
        [SerializeField] private Vector2 speedRange = new Vector2(0.05f, 0.22f);
        [SerializeField] private Vector2 sizeRange = new Vector2(0.75f, 1.2f);
        [SerializeField] private Vector2 lifetimeRange = new Vector2(8f, 16f);
        [SerializeField, Min(0f)] private float swayAmplitude = 0.12f;
        [SerializeField, Min(0f)] private float swayFrequency = 0.65f;
        [SerializeField, Min(0f)] private float pulseScale = 0.08f;
        [SerializeField, Range(0f, 1f)] private float parallaxStrength = 0.15f;

        public string Id => id;
        public AmbientMotionKind MotionKind => motionKind;
        public Sprite Sprite => sprite;
        public Material Material => material;
        public Color DayTint => dayTint;
        public Color NightTint => nightTint;
        public int PoolSize => Mathf.Max(0, poolSize);
        public int ReducedMotionVisibleCount => Mathf.Clamp(reducedMotionVisibleCount, 0, PoolSize);
        public bool HideWhenReducedMotion => hideWhenReducedMotion;
        public Vector2 SpawnArea => ClampVector(spawnArea, 0f, float.MaxValue);
        public Vector2 SpeedRange => SortRange(speedRange, 0f);
        public Vector2 SizeRange => SortRange(sizeRange, 0.01f);
        public Vector2 LifetimeRange => SortRange(lifetimeRange, 0.1f);
        public float SwayAmplitude => Mathf.Max(0f, swayAmplitude);
        public float SwayFrequency => Mathf.Max(0f, swayFrequency);
        public float PulseScale => Mathf.Max(0f, pulseScale);
        public float ParallaxStrength => Mathf.Clamp01(parallaxStrength);

        public AmbientMotionLayer()
        {
        }

        public AmbientMotionLayer(
            string layerId,
            AmbientMotionKind layerMotionKind,
            Color layerDayTint,
            Color layerNightTint,
            int layerPoolSize,
            Vector2 layerSpawnArea,
            Vector2 layerSpeedRange,
            Vector2 layerSizeRange,
            Vector2 layerLifetimeRange,
            float layerSwayAmplitude,
            float layerSwayFrequency,
            float layerPulseScale,
            float layerParallaxStrength,
            bool layerHideWhenReducedMotion,
            int layerReducedMotionVisibleCount)
        {
            id = layerId;
            motionKind = layerMotionKind;
            dayTint = layerDayTint;
            nightTint = layerNightTint;
            poolSize = layerPoolSize;
            spawnArea = layerSpawnArea;
            speedRange = layerSpeedRange;
            sizeRange = layerSizeRange;
            lifetimeRange = layerLifetimeRange;
            swayAmplitude = layerSwayAmplitude;
            swayFrequency = layerSwayFrequency;
            pulseScale = layerPulseScale;
            parallaxStrength = layerParallaxStrength;
            hideWhenReducedMotion = layerHideWhenReducedMotion;
            reducedMotionVisibleCount = layerReducedMotionVisibleCount;
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
