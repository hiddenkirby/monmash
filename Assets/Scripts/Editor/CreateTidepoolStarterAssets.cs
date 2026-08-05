using System.Collections.Generic;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateTidepoolStarterAssets
    {
        private const string SpeciesFolder = "Assets/Data/Species";
        private const string DatabasePath = "Assets/Data/Databases/SpeciesDatabase.asset";
        private const string CreatureSpriteFolder = "Assets/Art/Creatures";

        [MenuItem("Tools/Tidepool/Create Starter Species Assets")]
        public static void CreateStarterSpeciesAssets()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(SpeciesFolder);
            EnsureFolder("Assets/Data/Databases");

            List<TidelingSpecies> createdSpecies = new List<TidelingSpecies>();

            foreach (SpeciesSeed seed in Seeds)
            {
                string path = $"{SpeciesFolder}/{seed.Id}.asset";
                TidelingSpecies species = AssetDatabase.LoadAssetAtPath<TidelingSpecies>(path);
                if (species == null)
                {
                    species = ScriptableObject.CreateInstance<TidelingSpecies>();
                    AssetDatabase.CreateAsset(species, path);
                }

                species.Configure(
                    seed.Id,
                    seed.DisplayName,
                    seed.Current,
                    seed.Rarity,
                    seed.Habitats,
                    seed.Availability,
                    seed.AvailabilityHint,
                    seed.FieldNote,
                    seed.CatchZoneWidth,
                    seed.CatchMarkerSpeed);
                AssignSprite(species, seed.Id);
                EditorUtility.SetDirty(species);
                createdSpecies.Add(species);
            }

            SpeciesDatabase database = AssetDatabase.LoadAssetAtPath<SpeciesDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<SpeciesDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            SerializedObject serializedDatabase = new SerializedObject(database);
            SerializedProperty speciesList = serializedDatabase.FindProperty("species");
            speciesList.arraySize = createdSpecies.Count;
            for (int i = 0; i < createdSpecies.Count; i++)
            {
                speciesList.GetArrayElementAtIndex(i).objectReferenceValue = createdSpecies[i];
            }

            serializedDatabase.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void AssignSprite(TidelingSpecies species, string speciesId)
        {
            string spritePath = $"{CreatureSpriteFolder}/{speciesId}.png";
            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            SerializedObject serializedSpecies = new SerializedObject(species);
            serializedSpecies.FindProperty("sprite").objectReferenceValue = sprite;
            serializedSpecies.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string name = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static readonly SpeciesSeed[] Seeds =
        {
            new SpeciesSeed("blip", "Blip", TidelingCurrent.Current, TidelingRarity.Common, new[] { ZoneId.TidepoolShallows }, EncounterAvailability.Always, string.Empty, "A thumb-sized darting fish that always seems to be late for something.", 0.40f, 0.55f),
            new SpeciesSeed("nubbin", "Nubbin", TidelingCurrent.Stone, TidelingRarity.Common, new[] { ZoneId.TidepoolShallows }, EncounterAvailability.Always, string.Empty, "A hermit crab wearing a pebble that is much too big, but clearly treasured.", 0.40f, 0.55f),
            new SpeciesSeed("frillick", "Frillick", TidelingCurrent.Coral, TidelingRarity.Common, new[] { ZoneId.TidepoolShallows }, EncounterAvailability.Always, string.Empty, "A ruffled sea slug that moves like a ribbon settling through water.", 0.40f, 0.55f),
            new SpeciesSeed("sputter", "Sputter", TidelingCurrent.Glow, TidelingRarity.Common, new[] { ZoneId.TidepoolShallows }, EncounterAvailability.Always, string.Empty, "A blinking little plankton cluster that travels as one shy sparkle.", 0.40f, 0.55f),
            new SpeciesSeed("wobbet", "Wobbet", TidelingCurrent.Tide, TidelingRarity.Common, new[] { ZoneId.SeagrassMeadow }, EncounterAvailability.Always, string.Empty, "A round jelly that drifts calmly until it bumps into something interesting.", 0.38f, 0.58f),
            new SpeciesSeed("clackaw", "Clackaw", TidelingCurrent.Stone, TidelingRarity.Uncommon, new[] { ZoneId.SeagrassMeadow }, EncounterAvailability.Always, string.Empty, "A small shrimp with one enormous claw and a very serious snap.", 0.30f, 0.72f),
            new SpeciesSeed("sweepfin", "Sweepfin", TidelingCurrent.Current, TidelingRarity.Uncommon, new[] { ZoneId.SeagrassMeadow }, EncounterAvailability.Always, string.Empty, "A palm-sized ray that glides just below the surface like a leaf on wind.", 0.30f, 0.72f),
            new SpeciesSeed("mossback", "Mossback", TidelingCurrent.Coral, TidelingRarity.Uncommon, new[] { ZoneId.SeagrassMeadow }, EncounterAvailability.Always, string.Empty, "A tiny turtle carrying a soft green garden on its shell.", 0.30f, 0.72f),
            new SpeciesSeed("lumen", "Lumen", TidelingCurrent.Glow, TidelingRarity.Uncommon, new[] { ZoneId.SeagrassMeadow }, EncounterAvailability.Always, string.Empty, "A lanternfish whose light dims whenever it feels shy.", 0.30f, 0.72f),
            new SpeciesSeed("thistlecoat", "Thistlecoat", TidelingCurrent.Coral, TidelingRarity.Uncommon, new[] { ZoneId.SeagrassMeadow }, EncounterAvailability.Always, string.Empty, "A careful urchin whose spines lie flat when it decides to trust you.", 0.30f, 0.72f),
            new SpeciesSeed("gullwing", "Gullwing", TidelingCurrent.Current, TidelingRarity.Rare, new[] { ZoneId.SeagrassMeadow }, EncounterAvailability.LastHourOfDaylight, "Look near the end of the daylight cycle.", "A flying fish that flashes silver at the edge of daylight.", 0.24f, 0.88f),
            new SpeciesSeed("tanglemaw", "Tanglemaw", TidelingCurrent.Tide, TidelingRarity.Rare, new[] { ZoneId.SeagrassMeadow }, EncounterAvailability.Always, string.Empty, "A curious octopus that would rather inspect the jar than sit inside it.", 0.24f, 0.88f),
            new SpeciesSeed("old-barnaby", "Old Barnaby", TidelingCurrent.Stone, TidelingRarity.Secret, new[] { ZoneId.TidepoolShallows }, EncounterAvailability.Always, string.Empty, "An ancient barnacled shape that seems to know every quiet pool by name.", 0.22f, 0.92f)
        };

        private readonly struct SpeciesSeed
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly TidelingCurrent Current;
            public readonly TidelingRarity Rarity;
            public readonly ZoneId[] Habitats;
            public readonly EncounterAvailability Availability;
            public readonly string AvailabilityHint;
            public readonly string FieldNote;
            public readonly float CatchZoneWidth;
            public readonly float CatchMarkerSpeed;

            public SpeciesSeed(
                string id,
                string displayName,
                TidelingCurrent current,
                TidelingRarity rarity,
                ZoneId[] habitats,
                EncounterAvailability availability,
                string availabilityHint,
                string fieldNote,
                float catchZoneWidth,
                float catchMarkerSpeed)
            {
                Id = id;
                DisplayName = displayName;
                Current = current;
                Rarity = rarity;
                Habitats = habitats;
                Availability = availability;
                AvailabilityHint = availabilityHint;
                FieldNote = fieldNote;
                CatchZoneWidth = catchZoneWidth;
                CatchMarkerSpeed = catchMarkerSpeed;
            }
        }
    }
}
