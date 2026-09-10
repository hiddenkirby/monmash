using System.Collections.Generic;
using Tidepool.Domain;
using UnityEngine;

namespace Tidepool.Runtime
{
    public class AmbientTidelingController : MonoBehaviour
    {
        private const float DefaultDaylightCycleSeconds = 480f;
        private const float DefaultLastHourSeconds = 60f;

        [SerializeField] private AmbientTidelingZoneProfile profile;
        [SerializeField] private Camera activationCamera;
        [SerializeField] private Transform playerReference;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField, Range(0f, 1f)] private float dayNightBlend;
        [SerializeField, Min(60f)] private float inGameDaylightCycleSeconds = DefaultDaylightCycleSeconds;
        [SerializeField, Range(10f, 240f)] private float lastHourOfDaylightSeconds = DefaultLastHourSeconds;
        [SerializeField, Min(0f)] private float cameraPadding = 0.2f;

        private readonly List<EntryRuntime> entries = new List<EntryRuntime>();
        private bool isPlaying;
        private static Sprite fallbackSprite;

        public int PooledActorCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < entries.Count; i++)
                {
                    count += entries[i].Actors.Count;
                }

                return count;
            }
        }

        public int ActiveActorCount
        {
            get
            {
                int count = 0;
                for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
                {
                    List<AmbientTidelingActor> actors = entries[entryIndex].Actors;
                    for (int actorIndex = 0; actorIndex < actors.Count; actorIndex++)
                    {
                        if (actors[actorIndex].Active)
                        {
                            count += 1;
                        }
                    }
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
        }

        private void OnDisable()
        {
            SetAllActorsVisible(false);
        }

        private void Update()
        {
            RefreshActors(Time.deltaTime);
        }

        public void SetProfile(AmbientTidelingZoneProfile ambientProfile)
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
            ApplyAllTints();
        }

        public void SetPlaying(bool playing)
        {
            isPlaying = playing;
            if (!isPlaying)
            {
                SetAllActorsVisible(false);
            }
        }

        public void RefreshActorsForEditMode()
        {
            RefreshActors(0f);
        }

        public bool IsEntryEligible(AmbientTidelingSpawnEntry entry)
        {
            return entry != null
                && EntryMatchesTimeWindow(entry)
                && EntryMatchesSaveState(entry);
        }

        public void RebuildPools()
        {
            ClearPools();
            if (profile == null)
            {
                return;
            }

            AmbientTidelingSpawnEntry[] profileSpawns = profile.Spawns;
            for (int entryIndex = 0; entryIndex < profileSpawns.Length; entryIndex++)
            {
                AmbientTidelingSpawnEntry entry = profileSpawns[entryIndex];
                if (entry == null || entry.PoolSize <= 0)
                {
                    continue;
                }

                EntryRuntime runtime = new EntryRuntime(entry, CreateEntryRoot(entry, entryIndex));
                for (int actorIndex = 0; actorIndex < entry.PoolSize; actorIndex++)
                {
                    runtime.Actors.Add(CreateActor(runtime.Root, entry, entryIndex, actorIndex));
                }

                entries.Add(runtime);
                ApplyTint(runtime);
            }
        }

        private void RefreshActors(float deltaTime)
        {
            if (!isPlaying || profile == null)
            {
                SetAllActorsVisible(false);
                return;
            }

            bool isCameraVisible = IsVisibleToCamera();
            if (!isCameraVisible)
            {
                SetAllActorsVisible(false);
                return;
            }

            bool reducedMotion = TidepoolSettingsService.ReducedMotion;
            int remainingVisible = profile.MaxVisibleActors;

            for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
            {
                EntryRuntime runtime = entries[entryIndex];
                int desiredCount = 0;
                if (IsEntryEligible(runtime.Entry) && remainingVisible > 0)
                {
                    desiredCount = reducedMotion
                        ? Mathf.CeilToInt(runtime.Entry.ReducedMotionVisibleCount * profile.ReducedMotionDensityScale)
                        : runtime.Entry.PoolSize;
                    desiredCount = Mathf.Clamp(desiredCount, 0, Mathf.Min(runtime.Actors.Count, remainingVisible));
                }

                remainingVisible -= desiredCount;
                UpdateEntry(runtime, desiredCount, reducedMotion, deltaTime);
            }
        }

        private void UpdateEntry(EntryRuntime runtime, int desiredCount, bool reducedMotion, float deltaTime)
        {
            Color tint = GetTint(runtime.Entry);
            for (int actorIndex = 0; actorIndex < runtime.Actors.Count; actorIndex++)
            {
                AmbientTidelingActor actor = runtime.Actors[actorIndex];
                bool shouldShow = actorIndex < desiredCount;
                if (!shouldShow)
                {
                    SetActorActive(actor, false);
                    continue;
                }

                if (!actor.Active)
                {
                    ResetActor(runtime.Entry, actor, actorIndex);
                }

                if (!reducedMotion)
                {
                    actor.Age += deltaTime;
                }

                ApplyBehavior(runtime.Entry, actor, tint, reducedMotion);
                SetActorActive(actor, true);
            }
        }

        private void ApplyBehavior(AmbientTidelingSpawnEntry entry, AmbientTidelingActor actor, Color tint, bool reducedMotion)
        {
            float frequency = entry.MotionFrequency;
            float wave = reducedMotion || frequency <= 0f
                ? 0f
                : Mathf.Sin(actor.Phase + actor.Age * frequency * Mathf.PI * 2f);
            float groupWave = reducedMotion || frequency <= 0f
                ? 0f
                : Mathf.Sin(actor.Age * frequency * Mathf.PI * 2f);
            Vector3 offset = Vector3.zero;
            float alphaMultiplier = 1f;
            float scale = actor.Scale;

            switch (entry.BehaviorKind)
            {
                case AmbientTidelingBehaviorKind.Drift:
                    offset.x += actor.Direction * actor.Speed * actor.Age;
                    offset.y += wave * entry.MotionAmplitude;
                    break;
                case AmbientTidelingBehaviorKind.School:
                    offset.x += actor.Direction * actor.Speed * actor.Age;
                    offset.y += groupWave * entry.MotionAmplitude * 0.5f + actor.SchoolOffset;
                    break;
                case AmbientTidelingBehaviorKind.Peek:
                    offset.y += Mathf.Abs(wave) * entry.MotionAmplitude;
                    alphaMultiplier = Mathf.Lerp(0.55f, 1f, Mathf.Abs(wave));
                    break;
                case AmbientTidelingBehaviorKind.Hide:
                    offset.y -= Mathf.Abs(wave) * entry.MotionAmplitude * 0.5f;
                    alphaMultiplier = Mathf.Lerp(0.35f, 0.95f, Mathf.Abs(wave));
                    break;
                case AmbientTidelingBehaviorKind.Surface:
                    offset.y += Mathf.Sin(actor.Age * frequency * Mathf.PI) * entry.MotionAmplitude;
                    break;
                case AmbientTidelingBehaviorKind.Glow:
                    scale *= 1f + Mathf.Abs(wave) * 0.12f;
                    alphaMultiplier = Mathf.Lerp(0.55f, 1f, Mathf.Abs(wave));
                    break;
                case AmbientTidelingBehaviorKind.ApproachAfterBefriending:
                    offset += GetApproachOffset(actor.Anchor, entry.ApproachStrength);
                    offset.y += wave * entry.MotionAmplitude * 0.35f;
                    break;
                case AmbientTidelingBehaviorKind.Rest:
                    scale *= 1f + Mathf.Abs(wave) * 0.04f;
                    break;
            }

            actor.Transform.localPosition = ClampToSpawnArea(entry, actor.Anchor + offset);
            actor.Transform.localScale = Vector3.one * scale;
            actor.Renderer.color = FadeAlpha(tint, alphaMultiplier);
        }

        private Vector3 GetApproachOffset(Vector3 anchor, float approachStrength)
        {
            if (playerReference == null || approachStrength <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 localPlayer = transform.InverseTransformPoint(playerReference.position);
            Vector3 toPlayer = localPlayer - anchor;
            toPlayer.z = 0f;
            return Vector3.ClampMagnitude(toPlayer, 1.5f) * approachStrength;
        }

        private static Vector3 ClampToSpawnArea(AmbientTidelingSpawnEntry entry, Vector3 position)
        {
            Vector2 area = entry.SpawnArea;
            return new Vector3(
                Mathf.Clamp(position.x, -area.x * 0.5f, area.x * 0.5f),
                Mathf.Clamp(position.y, -area.y * 0.5f, area.y * 0.5f),
                position.z);
        }

        private void ResetActor(AmbientTidelingSpawnEntry entry, AmbientTidelingActor actor, int actorIndex)
        {
            Vector2 area = entry.SpawnArea;
            Vector2 scaleRange = entry.ScaleRange;
            Vector2 speedRange = entry.SpeedRange;
            float x = Mathf.Lerp(-area.x * 0.45f, area.x * 0.45f, HashToUnit(entry.Id, actorIndex, 1));
            float y = Mathf.Lerp(-area.y * 0.45f, area.y * 0.45f, HashToUnit(entry.Id, actorIndex, 2));

            actor.Anchor = new Vector3(x, y, 0f);
            actor.Direction = HashToUnit(entry.Id, actorIndex, 3) < 0.5f ? -1f : 1f;
            actor.Speed = Mathf.Lerp(speedRange.x, speedRange.y, HashToUnit(entry.Id, actorIndex, 4));
            actor.Scale = Mathf.Lerp(scaleRange.x, scaleRange.y, HashToUnit(entry.Id, actorIndex, 5));
            actor.Phase = HashToUnit(entry.Id, actorIndex, 6) * Mathf.PI * 2f;
            actor.SchoolOffset = Mathf.Lerp(-entry.MotionAmplitude, entry.MotionAmplitude, HashToUnit(entry.Id, actorIndex, 7));
            actor.Age = HashToUnit(entry.Id, actorIndex, 8) * 3f;
        }

        private AmbientTidelingActor CreateActor(Transform parent, AmbientTidelingSpawnEntry entry, int entryIndex, int actorIndex)
        {
            GameObject actorObject = new GameObject($"{entry.Id}_{actorIndex:00}");
            actorObject.transform.SetParent(parent, false);

            SpriteRenderer renderer = actorObject.AddComponent<SpriteRenderer>();
            renderer.sprite = entry.Species != null && entry.Species.Sprite != null
                ? entry.Species.Sprite
                : GetFallbackSprite();
            renderer.sortingOrder = entry.SortingOrder;

            AmbientTidelingActor actor = new AmbientTidelingActor(actorObject.transform, renderer);
            ResetActor(entry, actor, actorIndex + entryIndex * 37);
            SetActorActive(actor, false);
            return actor;
        }

        private Transform CreateEntryRoot(AmbientTidelingSpawnEntry entry, int entryIndex)
        {
            GameObject root = new GameObject($"{entryIndex:00}_{entry.Id}");
            root.transform.SetParent(transform, false);
            return root.transform;
        }

        private bool EntryMatchesTimeWindow(AmbientTidelingSpawnEntry entry)
        {
            switch (entry.TimeWindow)
            {
                case AmbientTidelingTimeWindow.Day:
                    return dayNightBlend < 0.6f;
                case AmbientTidelingTimeWindow.Night:
                    return dayNightBlend >= 0.4f;
                case AmbientTidelingTimeWindow.LastHourOfDaylight:
                    return IsInLastHourOfDaylight();
                default:
                    return true;
            }
        }

        private bool EntryMatchesSaveState(AmbientTidelingSpawnEntry entry)
        {
            GameSaveService saveService = GameSaveService.Instance;
            if (entry.RequireSpeciesCaught)
            {
                string speciesId = entry.Species != null ? entry.Species.Id : null;
                if (!HasCaughtSpecies(saveService, speciesId))
                {
                    return false;
                }
            }

            string[] requiredIds = entry.RequiredStateIds;
            for (int i = 0; i < requiredIds.Length; i++)
            {
                if (!HasStateId(saveService, requiredIds[i]))
                {
                    return false;
                }
            }

            string[] blockedIds = entry.BlockedStateIds;
            for (int i = 0; i < blockedIds.Length; i++)
            {
                if (HasStateId(saveService, blockedIds[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasCaughtSpecies(GameSaveService saveService, string speciesId)
        {
            if (string.IsNullOrWhiteSpace(speciesId) || saveService?.Data?.caught == null)
            {
                return false;
            }

            for (int i = 0; i < saveService.Data.caught.Count; i++)
            {
                CaughtTideling caught = saveService.Data.caught[i];
                if (caught != null && string.Equals(caught.speciesId, speciesId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasStateId(GameSaveService saveService, string stateId)
        {
            if (string.IsNullOrWhiteSpace(stateId) || saveService == null)
            {
                return false;
            }

            string normalizedId = stateId.Trim();
            return saveService.HasTriggeredBeat(normalizedId)
                || saveService.HasCompletedQuest(normalizedId)
                || saveService.IsExpeditionChapterCompleted(normalizedId)
                || saveService.HasAuthoredDiscovery(normalizedId)
                || saveService.HasLandmarkState(normalizedId)
                || saveService.HasFieldStationUpgrade(normalizedId)
                || saveService.HasCompletedSetPiece(normalizedId)
                || string.Equals(saveService.Data?.activeExpeditionChapterId, normalizedId, System.StringComparison.Ordinal);
        }

        private bool IsInLastHourOfDaylight()
        {
            float cycleLength = Mathf.Max(60f, inGameDaylightCycleSeconds);
            float windowSeconds = Mathf.Clamp(lastHourOfDaylightSeconds, 10f, cycleLength);
            float cycleTime = Mathf.Repeat(Time.timeSinceLevelLoad, cycleLength);
            return cycleLength - cycleTime <= windowSeconds;
        }

        private void ApplyAllTints()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                ApplyTint(entries[i]);
            }
        }

        private void ApplyTint(EntryRuntime runtime)
        {
            Color tint = GetTint(runtime.Entry);
            for (int i = 0; i < runtime.Actors.Count; i++)
            {
                runtime.Actors[i].Renderer.color = tint;
            }
        }

        private Color GetTint(AmbientTidelingSpawnEntry entry)
        {
            return Color.Lerp(entry.DayTint, entry.NightTint, dayNightBlend);
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
        }

        private void SetAllActorsVisible(bool visible)
        {
            for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
            {
                List<AmbientTidelingActor> actors = entries[entryIndex].Actors;
                for (int actorIndex = 0; actorIndex < actors.Count; actorIndex++)
                {
                    SetActorActive(actors[actorIndex], visible && actors[actorIndex].Active);
                }
            }
        }

        private static void SetActorActive(AmbientTidelingActor actor, bool active)
        {
            actor.Active = active;
            actor.Renderer.enabled = active;
        }

        private void ClearPools()
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i].Root != null)
                {
                    DestroyChild(entries[i].Root.gameObject);
                }
            }

            entries.Clear();
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

        private static Color FadeAlpha(Color color, float alpha)
        {
            color.a *= Mathf.Clamp01(alpha);
            return color;
        }

        private static Sprite GetFallbackSprite()
        {
            if (fallbackSprite != null)
            {
                return fallbackSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "AmbientTidelingFallbackTexture";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 16f);
            fallbackSprite.name = "AmbientTidelingFallbackSprite";
            return fallbackSprite;
        }

        private static float HashToUnit(string id, int index, int salt)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < id.Length; i++)
                {
                    hash = hash * 31 + id[i];
                }

                hash = hash * 31 + index;
                hash = hash * 31 + salt;
                hash ^= hash >> 16;
                return (hash & 0x7fffffff) / (float)int.MaxValue;
            }
        }

        private sealed class EntryRuntime
        {
            public readonly AmbientTidelingSpawnEntry Entry;
            public readonly Transform Root;
            public readonly List<AmbientTidelingActor> Actors = new List<AmbientTidelingActor>();

            public EntryRuntime(AmbientTidelingSpawnEntry entry, Transform root)
            {
                Entry = entry;
                Root = root;
            }
        }

        private sealed class AmbientTidelingActor
        {
            public readonly Transform Transform;
            public readonly SpriteRenderer Renderer;
            public Vector3 Anchor;
            public float Age;
            public float Phase;
            public float Scale = 1f;
            public float Speed;
            public float Direction = 1f;
            public float SchoolOffset;
            public bool Active;

            public AmbientTidelingActor(Transform transform, SpriteRenderer renderer)
            {
                Transform = transform;
                Renderer = renderer;
            }
        }
    }
}
