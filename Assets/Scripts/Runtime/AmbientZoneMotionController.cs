using System.Collections.Generic;
using Tidepool.Domain;
using UnityEngine;

namespace Tidepool.Runtime
{
    public class AmbientZoneMotionController : MonoBehaviour
    {
        [SerializeField] private AmbientZoneProfile profile;
        [SerializeField] private Camera activationCamera;
        [SerializeField] private Transform parallaxReference;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField, Range(0f, 1f)] private float dayNightBlend;
        [SerializeField, Min(0f)] private float cameraPadding = 0.2f;

        private readonly List<LayerRuntime> layers = new List<LayerRuntime>();
        private bool isPlaying;
        private Vector3 initialParallaxPosition;

        public int PooledInstanceCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < layers.Count; i++)
                {
                    count += layers[i].Items.Count;
                }

                return count;
            }
        }

        private void Awake()
        {
            ResolveCamera();
            RebuildPools();
        }

        private void OnEnable()
        {
            isPlaying = playOnEnable;
            initialParallaxPosition = parallaxReference != null ? parallaxReference.position : Vector3.zero;
        }

        private void OnDisable()
        {
            SetAllInstancesVisible(false);
        }

        private void Update()
        {
            if (!isPlaying || profile == null)
            {
                SetAllInstancesVisible(false);
                return;
            }

            bool isCameraVisible = IsVisibleToCamera();
            if (!isCameraVisible)
            {
                SetAllInstancesVisible(false);
                return;
            }

            bool reducedMotion = TidepoolSettingsService.ReducedMotion;
            float deltaTime = Time.deltaTime;
            Vector3 parallaxOffset = GetParallaxOffset();

            for (int i = 0; i < layers.Count; i++)
            {
                UpdateLayer(layers[i], deltaTime, reducedMotion, parallaxOffset);
            }
        }

        public void SetProfile(AmbientZoneProfile ambientProfile)
        {
            if (profile == ambientProfile)
            {
                return;
            }

            profile = ambientProfile;
            RebuildPools();
        }

        public void SetDayNightBlend(float blend)
        {
            dayNightBlend = Mathf.Clamp01(blend);
            for (int i = 0; i < layers.Count; i++)
            {
                ApplyTint(layers[i]);
            }
        }

        public void SetPlaying(bool playing)
        {
            isPlaying = playing;
            if (!isPlaying)
            {
                SetAllInstancesVisible(false);
            }
        }

        public void RebuildPools()
        {
            ClearPools();
            if (profile == null)
            {
                return;
            }

            AmbientMotionLayer[] profileLayers = profile.Layers;
            for (int layerIndex = 0; layerIndex < profileLayers.Length; layerIndex++)
            {
                AmbientMotionLayer layer = profileLayers[layerIndex];
                if (layer == null || layer.Sprite == null || layer.PoolSize <= 0)
                {
                    continue;
                }

                LayerRuntime runtime = new LayerRuntime(layer, CreateLayerRoot(layer, layerIndex));
                for (int itemIndex = 0; itemIndex < layer.PoolSize; itemIndex++)
                {
                    runtime.Items.Add(CreateItem(runtime.Root, layer, itemIndex));
                }

                layers.Add(runtime);
                ApplyTint(runtime);
            }
        }

        private void UpdateLayer(LayerRuntime runtime, float deltaTime, bool reducedMotion, Vector3 parallaxOffset)
        {
            if (reducedMotion)
            {
                ApplyReducedMotion(runtime, parallaxOffset);
                return;
            }

            runtime.SpawnTimer -= deltaTime;
            if (runtime.SpawnTimer <= 0f)
            {
                TrySpawn(runtime);
                runtime.SpawnTimer = Random.Range(0.25f, 1.25f);
            }

            Color tint = GetTint(runtime.Layer);
            for (int i = 0; i < runtime.Items.Count; i++)
            {
                AmbientMotionItem item = runtime.Items[i];
                if (!item.Active)
                {
                    continue;
                }

                item.Age += deltaTime;
                if (item.Age >= item.Lifetime)
                {
                    SetItemActive(item, false);
                    continue;
                }

                float normalizedAge = Mathf.Clamp01(item.Age / item.Lifetime);
                Vector3 offset = item.Velocity * item.Age;
                float scale = item.Scale;
                float alphaMultiplier = ApplyMotion(runtime.Layer, item, normalizedAge, ref offset, ref scale);
                item.Transform.localPosition = item.Anchor + offset + parallaxOffset * runtime.Layer.ParallaxStrength;
                item.Transform.localScale = Vector3.one * scale;
                item.Renderer.color = FadeAlpha(tint, alphaMultiplier);
            }
        }

        private void ApplyReducedMotion(LayerRuntime runtime, Vector3 parallaxOffset)
        {
            int visibleCount = runtime.Layer.HideWhenReducedMotion
                ? 0
                : Mathf.CeilToInt(runtime.Layer.ReducedMotionVisibleCount * profile.ReducedMotionDensityScale);
            visibleCount = Mathf.Clamp(visibleCount, 0, runtime.Items.Count);
            Color tint = GetTint(runtime.Layer);

            for (int i = 0; i < runtime.Items.Count; i++)
            {
                AmbientMotionItem item = runtime.Items[i];
                bool shouldShow = i < visibleCount;
                if (shouldShow && !item.Active)
                {
                    ResetItem(runtime.Layer, item);
                    item.Anchor = ReducedMotionAnchor(runtime.Layer, i, visibleCount);
                }

                item.Transform.localPosition = item.Anchor + parallaxOffset * runtime.Layer.ParallaxStrength;
                item.Transform.localScale = Vector3.one * item.Scale;
                item.Renderer.color = tint;
                SetItemActive(item, shouldShow);
            }
        }

        private void TrySpawn(LayerRuntime runtime)
        {
            for (int i = 0; i < runtime.Items.Count; i++)
            {
                AmbientMotionItem item = runtime.Items[i];
                if (item.Active)
                {
                    continue;
                }

                ResetItem(runtime.Layer, item);
                SetItemActive(item, true);
                return;
            }
        }

        private void ResetItem(AmbientMotionLayer layer, AmbientMotionItem item)
        {
            Vector2 area = layer.SpawnArea;
            Vector2 speedRange = layer.SpeedRange;
            Vector2 sizeRange = layer.SizeRange;
            Vector2 lifetimeRange = layer.LifetimeRange;
            float direction = Random.value < 0.5f ? -1f : 1f;

            item.Anchor = new Vector3(
                Random.Range(-area.x * 0.5f, area.x * 0.5f),
                Random.Range(-area.y * 0.5f, area.y * 0.5f),
                0f);
            item.Velocity = new Vector3(Random.Range(speedRange.x, speedRange.y) * direction, 0f, 0f);
            item.Age = 0f;
            item.Lifetime = Random.Range(lifetimeRange.x, lifetimeRange.y);
            item.Phase = Random.Range(0f, Mathf.PI * 2f);
            item.Scale = Random.Range(sizeRange.x, sizeRange.y);
        }

        private float ApplyMotion(AmbientMotionLayer layer, AmbientMotionItem item, float normalizedAge, ref Vector3 offset, ref float scale)
        {
            float wave = Mathf.Sin(item.Phase + item.Age * layer.SwayFrequency * Mathf.PI * 2f);
            float alphaMultiplier = 1f;
            switch (layer.MotionKind)
            {
                case AmbientMotionKind.WaterShimmer:
                    offset.y += wave * layer.SwayAmplitude * 0.25f;
                    alphaMultiplier = Mathf.Lerp(0.35f, 0.85f, Mathf.Abs(wave));
                    break;
                case AmbientMotionKind.SwayingPlant:
                    offset.x += wave * layer.SwayAmplitude;
                    break;
                case AmbientMotionKind.SoftLightPulse:
                    scale *= 1f + Mathf.Abs(wave) * layer.PulseScale;
                    break;
                case AmbientMotionKind.DistantSilhouette:
                    offset.y += Mathf.Sin(normalizedAge * Mathf.PI) * layer.SwayAmplitude;
                    break;
            }

            return alphaMultiplier;
        }

        private static Color FadeAlpha(Color color, float alpha)
        {
            color.a *= Mathf.Clamp01(alpha);
            return color;
        }

        private AmbientMotionItem CreateItem(Transform parent, AmbientMotionLayer layer, int index)
        {
            GameObject itemObject = new GameObject($"{layer.Id}_{index:00}");
            itemObject.transform.SetParent(parent, false);
            SpriteRenderer spriteRenderer = itemObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = layer.Sprite;
            if (layer.Material != null)
            {
                spriteRenderer.sharedMaterial = layer.Material;
            }

            AmbientMotionItem item = new AmbientMotionItem(itemObject.transform, spriteRenderer);
            ResetItem(layer, item);
            SetItemActive(item, false);
            return item;
        }

        private Transform CreateLayerRoot(AmbientMotionLayer layer, int layerIndex)
        {
            GameObject root = new GameObject($"{layerIndex:00}_{layer.Id}");
            root.transform.SetParent(transform, false);
            return root.transform;
        }

        private void ApplyTint(LayerRuntime runtime)
        {
            Color tint = GetTint(runtime.Layer);
            for (int i = 0; i < runtime.Items.Count; i++)
            {
                runtime.Items[i].Renderer.color = tint;
            }
        }

        private Color GetTint(AmbientMotionLayer layer)
        {
            Color profileTint = Color.Lerp(profile.DayTint, profile.NightTint, dayNightBlend);
            Color layerTint = Color.Lerp(layer.DayTint, layer.NightTint, dayNightBlend);
            return new Color(
                profileTint.r * layerTint.r,
                profileTint.g * layerTint.g,
                profileTint.b * layerTint.b,
                profileTint.a * layerTint.a);
        }

        private Vector3 ReducedMotionAnchor(AmbientMotionLayer layer, int index, int visibleCount)
        {
            if (visibleCount <= 1)
            {
                return Vector3.zero;
            }

            Vector2 area = layer.SpawnArea;
            float t = (float)index / (visibleCount - 1);
            return new Vector3(Mathf.Lerp(-area.x * 0.35f, area.x * 0.35f, t), 0f, 0f);
        }

        private Vector3 GetParallaxOffset()
        {
            if (parallaxReference == null)
            {
                return Vector3.zero;
            }

            return parallaxReference.position - initialParallaxPosition;
        }

        private bool IsVisibleToCamera()
        {
            ResolveCamera();
            if (activationCamera == null)
            {
                return true;
            }

            Vector3 viewportPoint = activationCamera.WorldToViewportPoint(transform.position);
            return viewportPoint.z >= 0f
                && viewportPoint.x >= -cameraPadding
                && viewportPoint.x <= 1f + cameraPadding
                && viewportPoint.y >= -cameraPadding
                && viewportPoint.y <= 1f + cameraPadding;
        }

        private void ResolveCamera()
        {
            if (activationCamera == null)
            {
                activationCamera = Camera.main;
            }

            if (parallaxReference == null && activationCamera != null)
            {
                parallaxReference = activationCamera.transform;
            }
        }

        private void SetAllInstancesVisible(bool visible)
        {
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                LayerRuntime runtime = layers[layerIndex];
                for (int itemIndex = 0; itemIndex < runtime.Items.Count; itemIndex++)
                {
                    SetItemActive(runtime.Items[itemIndex], visible && runtime.Items[itemIndex].Active);
                }
            }
        }

        private static void SetItemActive(AmbientMotionItem item, bool active)
        {
            item.Active = active;
            item.Renderer.enabled = active;
        }

        private void ClearPools()
        {
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                if (layers[i].Root != null)
                {
                    DestroyChild(layers[i].Root.gameObject);
                }
            }

            layers.Clear();
        }

        private static void DestroyChild(GameObject child)
        {
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        private sealed class LayerRuntime
        {
            public readonly AmbientMotionLayer Layer;
            public readonly Transform Root;
            public readonly List<AmbientMotionItem> Items = new List<AmbientMotionItem>();
            public float SpawnTimer;

            public LayerRuntime(AmbientMotionLayer layer, Transform root)
            {
                Layer = layer;
                Root = root;
            }
        }

        private sealed class AmbientMotionItem
        {
            public readonly Transform Transform;
            public readonly SpriteRenderer Renderer;
            public Vector3 Anchor;
            public Vector3 Velocity;
            public float Age;
            public float Lifetime = 1f;
            public float Phase;
            public float Scale = 1f;
            public bool Active;

            public AmbientMotionItem(Transform transform, SpriteRenderer renderer)
            {
                Transform = transform;
                Renderer = renderer;
            }
        }
    }
}
