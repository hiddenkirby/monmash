using System.Collections.Generic;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateContestMoveAssets
    {
        private const string MoveFolder = "Assets/Data/ContestMoves";
        private const string SpeciesFolder = "Assets/Data/Species";

        [MenuItem("Tools/Tidepool/Create Contest Move Assets")]
        public static void CreateMoveAssets()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(MoveFolder);

            foreach (MoveSeed seed in MoveSeeds)
            {
                string path = $"{MoveFolder}/{seed.Id}.asset";
                ContestMove move = AssetDatabase.LoadAssetAtPath<ContestMove>(path);
                if (move == null)
                {
                    move = ScriptableObject.CreateInstance<ContestMove>();
                    AssetDatabase.CreateAsset(move, path);
                }

                SerializedObject serializedMove = new SerializedObject(move);
                serializedMove.FindProperty("id").stringValue = seed.Id;
                serializedMove.FindProperty("displayName").stringValue = seed.DisplayName;
                serializedMove.FindProperty("current").enumValueIndex = (int)seed.Current;
                serializedMove.FindProperty("category").enumValueIndex = (int)seed.Category;
                serializedMove.FindProperty("gentlePower").intValue = seed.GentlePower;
                serializedMove.FindProperty("description").stringValue = seed.Description;
                serializedMove.ApplyModifiedProperties();

                EditorUtility.SetDirty(move);
            }

            AssignMovesToSpecies();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void AssignMovesToSpecies()
        {
            foreach (SpeciesMoveAssignment assignment in SpeciesAssignments)
            {
                string speciesPath = $"{SpeciesFolder}/{assignment.SpeciesId}.asset";
                TidelingSpecies species = AssetDatabase.LoadAssetAtPath<TidelingSpecies>(speciesPath);
                if (species == null)
                {
                    Debug.LogWarning($"Species not found at {speciesPath}. Run Create Starter Species Assets first.");
                    continue;
                }

                ContestMove firstMove = LoadMove(assignment.FirstMoveId);
                ContestMove secondMove = LoadMove(assignment.SecondMoveId);

                SerializedObject serializedSpecies = new SerializedObject(species);
                serializedSpecies.FindProperty("firstContestMove").objectReferenceValue = firstMove;
                serializedSpecies.FindProperty("secondContestMove").objectReferenceValue = secondMove;
                serializedSpecies.ApplyModifiedProperties();

                EditorUtility.SetDirty(species);
            }
        }

        private static ContestMove LoadMove(string moveId)
        {
            if (string.IsNullOrWhiteSpace(moveId))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<ContestMove>($"{MoveFolder}/{moveId}.asset");
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

        private static readonly MoveSeed[] MoveSeeds =
        {
            // Current moves
            new MoveSeed("current-splash", "Gentle Splash", TidelingCurrent.Current, ContestMoveCategory.Attack, 3, "A soft wash of cool water that ripples warmly."),
            new MoveSeed("current-drift", "Drift Current", TidelingCurrent.Current, ContestMoveCategory.Focus, 2, "A smooth glide that carries a kind wave along."),

            // Coral moves
            new MoveSeed("coral-bloom", "Coral Bloom", TidelingCurrent.Coral, ContestMoveCategory.Attack, 3, "A warm burst of coral color that shimmers."),
            new MoveSeed("coral-frill", "Frill Wave", TidelingCurrent.Coral, ContestMoveCategory.Defend, 2, "A ruffled display of soft coral fronds."),

            // Stone moves
            new MoveSeed("stone-pebble", "Steady Pebble", TidelingCurrent.Stone, ContestMoveCategory.Attack, 3, "A grounded little nudge, firm and friendly."),
            new MoveSeed("stone-clack", "Gentle Clack", TidelingCurrent.Stone, ContestMoveCategory.Defend, 2, "A soft click of stone on stone, reassuring."),

            // Glow moves
            new MoveSeed("glow-shimmer", "Warm Shimmer", TidelingCurrent.Glow, ContestMoveCategory.Attack, 3, "A gentle glow that lights up the water."),
            new MoveSeed("glow-spark", "Soft Spark", TidelingCurrent.Glow, ContestMoveCategory.Focus, 2, "A tiny blink of light that says hello."),

            // Tide moves
            new MoveSeed("tide-wash", "Tide Wash", TidelingCurrent.Tide, ContestMoveCategory.Attack, 3, "A slow, rolling wave that soothes."),
            new MoveSeed("tide-bubble", "Bubble Drift", TidelingCurrent.Tide, ContestMoveCategory.Defend, 2, "A round bubble that bobs up and pops kindly.")
        };

        private static readonly SpeciesMoveAssignment[] SpeciesAssignments =
        {
            new SpeciesMoveAssignment("blip", "current-splash", "current-drift"),
            new SpeciesMoveAssignment("nubbin", "stone-pebble", "stone-clack"),
            new SpeciesMoveAssignment("frillick", "coral-bloom", "coral-frill"),
            new SpeciesMoveAssignment("sputter", "glow-shimmer", "glow-spark"),
            new SpeciesMoveAssignment("wobbet", "tide-wash", "tide-bubble"),
            new SpeciesMoveAssignment("clackaw", "stone-clack", "stone-pebble"),
            new SpeciesMoveAssignment("sweepfin", "current-drift", "current-splash"),
            new SpeciesMoveAssignment("mossback", "coral-frill", "coral-bloom"),
            new SpeciesMoveAssignment("lumen", "glow-spark", "glow-shimmer"),
            new SpeciesMoveAssignment("thistlecoat", "coral-frill", "coral-bloom"),
            new SpeciesMoveAssignment("gullwing", "current-splash", "current-drift"),
            new SpeciesMoveAssignment("tanglemaw", "tide-bubble", "tide-wash"),
            new SpeciesMoveAssignment("old-barnaby", "stone-pebble", "stone-clack")
        };

        private readonly struct MoveSeed
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly TidelingCurrent Current;
            public readonly ContestMoveCategory Category;
            public readonly int GentlePower;
            public readonly string Description;

            public MoveSeed(
                string id,
                string displayName,
                TidelingCurrent current,
                ContestMoveCategory category,
                int gentlePower,
                string description)
            {
                Id = id;
                DisplayName = displayName;
                Current = current;
                Category = category;
                GentlePower = gentlePower;
                Description = description;
            }
        }

        private readonly struct SpeciesMoveAssignment
        {
            public readonly string SpeciesId;
            public readonly string FirstMoveId;
            public readonly string SecondMoveId;

            public SpeciesMoveAssignment(string speciesId, string firstMoveId, string secondMoveId)
            {
                SpeciesId = speciesId;
                FirstMoveId = firstMoveId;
                SecondMoveId = secondMoveId;
            }
        }
    }
}
