using System;
using UnityEngine;

namespace Tidepool.Domain
{
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
        [SerializeField, Range(0.1f, 0.75f)] private float catchZoneWidth = 0.35f;
        [SerializeField, Min(0.1f)] private float catchMarkerSpeed = 0.65f;

        public string Id => id;
        public string DisplayName => displayName;
        public TidelingCurrent Current => current;
        public TidelingRarity Rarity => rarity;
        public Sprite Sprite => sprite;
        public string FieldNote => fieldNote;
        public ZoneId[] HabitatZones => habitatZones;
        public float CatchZoneWidth => catchZoneWidth;
        public float CatchMarkerSpeed => catchMarkerSpeed;

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
            string speciesFieldNote,
            float speciesCatchZoneWidth,
            float speciesCatchMarkerSpeed)
        {
            id = speciesId;
            displayName = speciesName;
            current = speciesCurrent;
            rarity = speciesRarity;
            habitatZones = speciesHabitats;
            fieldNote = speciesFieldNote;
            catchZoneWidth = speciesCatchZoneWidth;
            catchMarkerSpeed = speciesCatchMarkerSpeed;
        }
#endif
    }
}

