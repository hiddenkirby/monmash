using System.Collections.Generic;
using Tidepool.Domain;
using UnityEngine;

namespace Tidepool.Runtime
{
    public class AdaptiveSoundscapeController : MonoBehaviour
    {
        [SerializeField] private SoundscapeProfile[] profiles = new SoundscapeProfile[0];
        [SerializeField] private ZoneId startingZone = ZoneId.TidepoolShallows;
        [SerializeField] private bool followSaveZone = true;
        [SerializeField, Min(0f)] private float defaultFadeSeconds = 1.5f;
        [SerializeField] private Transform sourceRoot;

        private readonly List<LayerPlayback> activeLayers = new List<LayerPlayback>();
        private GameSaveService subscribedSaveService;
        private SoundscapeProfile currentPersistentProfile;
        private ZoneId currentZone;
        private float duckUntilTime;
        private float duckMultiplier = 1f;

        private void Awake()
        {
            currentZone = startingZone;
            sourceRoot ??= transform;
        }

        private void OnEnable()
        {
            SubscribeToSaveService();
            if (followSaveZone && subscribedSaveService != null && subscribedSaveService.Data != null)
            {
                currentZone = subscribedSaveService.Data.currentZone;
            }

            PlayZone(currentZone);
        }

        private void OnDisable()
        {
            if (subscribedSaveService != null)
            {
                subscribedSaveService.ZoneChanged -= HandleZoneChanged;
                subscribedSaveService = null;
            }
        }

        private void Update()
        {
            SubscribeToSaveService();
            TidepoolSettingsService.ApplyGlobalAudio();
            float duck = Time.unscaledTime < duckUntilTime ? duckMultiplier : 1f;

            for (int index = activeLayers.Count - 1; index >= 0; index--)
            {
                LayerPlayback playback = activeLayers[index];
                playback.Elapsed += Time.unscaledDeltaTime;
                float fadeSeconds = Mathf.Max(0.01f, playback.FadeSeconds);
                float progress = Mathf.Clamp01(playback.Elapsed / fadeSeconds);
                float targetVolume = Mathf.Lerp(playback.StartVolume, playback.TargetVolume, progress);
                if (playback.AffectedByDuck)
                {
                    targetVolume *= duck;
                }

                playback.Source.volume = TidepoolSettingsService.Muted ? 0f : targetVolume;

                if (playback.StopWhenSilent && progress >= 1f)
                {
                    Destroy(playback.Source.gameObject);
                    activeLayers.RemoveAt(index);
                    continue;
                }

                if (!playback.Source.loop && !playback.Source.isPlaying)
                {
                    Destroy(playback.Source.gameObject);
                    activeLayers.RemoveAt(index);
                }
            }
        }

        public void PlayZone(ZoneId zone)
        {
            currentZone = zone;
            SoundscapeProfile profile = FindZoneProfile(zone);
            if (profile == null)
            {
                FadeOutCurrent(defaultFadeSeconds);
                return;
            }

            PlayProfile(profile, true);
        }

        public void PlayFieldStation()
        {
            PlayRole(SoundscapeProfileRole.FieldStation, true);
        }

        public void PlayDiscoveryCue()
        {
            PlayRole(SoundscapeProfileRole.Discovery, false);
        }

        public void PlayRouteUnlockCue()
        {
            PlayRole(SoundscapeProfileRole.RouteUnlock, false);
        }

        public void PlayFestival()
        {
            PlayRole(SoundscapeProfileRole.Festival, true);
        }

        public void PlayFinale()
        {
            PlayRole(SoundscapeProfileRole.Finale, true);
        }

        public void DuckExistingAudio(float seconds)
        {
            DuckExistingAudio(seconds, 0.45f);
        }

        public void DuckExistingAudio(float seconds, float targetMultiplier)
        {
            duckUntilTime = Time.unscaledTime + Mathf.Max(0f, seconds);
            duckMultiplier = Mathf.Clamp01(targetMultiplier);
        }

        private void PlayRole(SoundscapeProfileRole role, bool replaceCurrent)
        {
            SoundscapeProfile profile = FindRoleProfile(role);
            if (profile == null)
            {
                return;
            }

            PlayProfile(profile, replaceCurrent);
        }

        private void PlayProfile(SoundscapeProfile profile, bool replaceCurrent)
        {
            if (replaceCurrent)
            {
                if (currentPersistentProfile == profile && HasActivePersistentAudio())
                {
                    return;
                }

                currentPersistentProfile = profile;
                FadeOutCurrent(profile.FadeSeconds);
            }

            if (profile.DuckSeconds > 0f && profile.DuckExistingTo < 1f)
            {
                DuckExistingAudio(profile.DuckSeconds, profile.DuckExistingTo);
            }

            SoundscapeProfile.SoundscapeLayer[] layers = profile.Layers;
            if (layers == null)
            {
                return;
            }

            foreach (SoundscapeProfile.SoundscapeLayer layer in layers)
            {
                if (layer == null || !layer.PlayOnEnter || layer.Clip == null)
                {
                    continue;
                }

                AudioSource source = CreateSource(profile, layer);
                LayerPlayback playback = new LayerPlayback(source, 0f, layer.Volume, profile.FadeSeconds, !layer.Loop, replaceCurrent);
                activeLayers.Add(playback);
                source.Play();
            }
        }

        private AudioSource CreateSource(SoundscapeProfile profile, SoundscapeProfile.SoundscapeLayer layer)
        {
            GameObject sourceObject = new GameObject($"{profile.Id}.{layer.Id}");
            sourceObject.transform.SetParent(sourceRoot == null ? transform : sourceRoot, false);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.clip = layer.Clip;
            source.loop = layer.Loop;
            source.playOnAwake = false;
            source.spatialBlend = layer.SpatialBlend;
            source.pitch = layer.Pitch;
            source.volume = 0f;
            return source;
        }

        private void FadeOutCurrent(float fadeSeconds)
        {
            foreach (LayerPlayback playback in activeLayers)
            {
                playback.StartVolume = playback.Source.volume;
                playback.TargetVolume = 0f;
                playback.Elapsed = 0f;
                playback.FadeSeconds = fadeSeconds;
                playback.StopWhenSilent = true;
            }
        }

        private bool HasActivePersistentAudio()
        {
            foreach (LayerPlayback playback in activeLayers)
            {
                if (playback.AffectedByDuck && !playback.StopWhenSilent)
                {
                    return true;
                }
            }

            return false;
        }

        private SoundscapeProfile FindZoneProfile(ZoneId zone)
        {
            foreach (SoundscapeProfile profile in profiles)
            {
                if (profile != null && profile.Role == SoundscapeProfileRole.Zone && profile.Zone == zone)
                {
                    return profile;
                }
            }

            return null;
        }

        private SoundscapeProfile FindRoleProfile(SoundscapeProfileRole role)
        {
            foreach (SoundscapeProfile profile in profiles)
            {
                if (profile != null && profile.Role == role)
                {
                    return profile;
                }
            }

            return null;
        }

        private void SubscribeToSaveService()
        {
            if (!followSaveZone)
            {
                return;
            }

            GameSaveService saveService = GameSaveService.Instance;
            if (saveService == null || saveService == subscribedSaveService)
            {
                return;
            }

            if (subscribedSaveService != null)
            {
                subscribedSaveService.ZoneChanged -= HandleZoneChanged;
            }

            subscribedSaveService = saveService;
            subscribedSaveService.ZoneChanged += HandleZoneChanged;
        }

        private void HandleZoneChanged(ZoneId zone)
        {
            PlayZone(zone);
        }

        private class LayerPlayback
        {
            public LayerPlayback(
                AudioSource source,
                float startVolume,
                float targetVolume,
                float fadeSeconds,
                bool stopWhenSilent,
                bool affectedByDuck)
            {
                Source = source;
                StartVolume = startVolume;
                TargetVolume = targetVolume;
                FadeSeconds = fadeSeconds;
                StopWhenSilent = stopWhenSilent;
                AffectedByDuck = affectedByDuck;
            }

            public AudioSource Source { get; }
            public float StartVolume { get; set; }
            public float TargetVolume { get; set; }
            public float FadeSeconds { get; set; }
            public float Elapsed { get; set; }
            public bool StopWhenSilent { get; set; }
            public bool AffectedByDuck { get; }
        }
    }
}
