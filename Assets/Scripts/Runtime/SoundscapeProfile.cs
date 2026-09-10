using System;
using Tidepool.Domain;
using UnityEngine;

namespace Tidepool.Runtime
{
    public enum SoundscapeProfileRole
    {
        Zone,
        FieldStation,
        Discovery,
        RouteUnlock,
        Festival,
        Finale
    }

    [CreateAssetMenu(menuName = "Tidepool/Soundscape Profile", fileName = "SoundscapeProfile")]
    public class SoundscapeProfile : ScriptableObject
    {
        [SerializeField] private string id = "soundscape.shallows";
        [SerializeField] private SoundscapeProfileRole role = SoundscapeProfileRole.Zone;
        [SerializeField] private ZoneId zone = ZoneId.TidepoolShallows;
        [SerializeField, Min(0f)] private float fadeSeconds = 1.5f;
        [SerializeField, Range(0f, 1f)] private float duckExistingTo = 1f;
        [SerializeField, Min(0f)] private float duckSeconds;
        [SerializeField] private SoundscapeLayer[] layers = new SoundscapeLayer[0];

        public string Id => id;
        public SoundscapeProfileRole Role => role;
        public ZoneId Zone => zone;
        public float FadeSeconds => fadeSeconds;
        public float DuckExistingTo => duckExistingTo;
        public float DuckSeconds => duckSeconds;
        public SoundscapeLayer[] Layers => layers;

        [Serializable]
        public class SoundscapeLayer
        {
            [SerializeField] private string id = "layer";
            [SerializeField] private AudioClip clip;
            [SerializeField] private bool loop = true;
            [SerializeField] private bool playOnEnter = true;
            [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
            [SerializeField, Range(0.1f, 3f)] private float pitch = 1f;
            [SerializeField, Range(0f, 1f)] private float spatialBlend;

            public string Id => id;
            public AudioClip Clip => clip;
            public bool Loop => loop;
            public bool PlayOnEnter => playOnEnter;
            public float Volume => volume;
            public float Pitch => pitch;
            public float SpatialBlend => spatialBlend;
        }
    }
}
