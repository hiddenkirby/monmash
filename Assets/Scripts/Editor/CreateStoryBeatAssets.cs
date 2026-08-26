using System.IO;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateStoryBeatAssets
    {
        private const string StoryBeatFolder = "Assets/Data/StoryBeats";

        [MenuItem("Tools/Tidepool/Create Story Beat Assets")]
        public static void CreateAssets()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(StoryBeatFolder);

            UpsertStoryBeat(
                "first_catch_intro",
                StoryBeatTriggerCondition.OnFirstCatch,
                1,
                ZoneId.TidepoolShallows,
                "What did you find? Show me!",
                false,
                ZoneId.KelpCurtain);

            UpsertStoryBeat(
                "meadow_pointer",
                StoryBeatTriggerCondition.OnSpeciesCount,
                3,
                ZoneId.SeagrassMeadow,
                "The meadow is just through here. Look for the grass waving in the water.",
                false,
                ZoneId.KelpCurtain);

            UpsertStoryBeat(
                "kelp_clue",
                StoryBeatTriggerCondition.OnSpeciesCount,
                5,
                ZoneId.KelpCurtain,
                "Something moves in the kelp, but it is still too thick to push through.",
                false,
                ZoneId.KelpCurtain);

            UpsertStoryBeat(
                "kelp_unlock",
                StoryBeatTriggerCondition.OnSpeciesCount,
                8,
                ZoneId.KelpCurtain,
                "The kelp has thinned. Care to look further?",
                true,
                ZoneId.KelpCurtain);

            UpsertStoryBeat(
                "old_barnaby_omen",
                StoryBeatTriggerCondition.OnSpeciesCount,
                10,
                ZoneId.TidepoolShallows,
                "The oldest shells are waking up. Keep your eyes on the shallows.",
                false,
                ZoneId.KelpCurtain);

            UpsertStoryBeat(
                "all_found_celebration",
                StoryBeatTriggerCondition.OnSpeciesCount,
                13,
                ZoneId.TidepoolShallows,
                "You found them all! The tidepools are full.",
                false,
                ZoneId.KelpCurtain);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void UpsertStoryBeat(
            string id,
            StoryBeatTriggerCondition triggerCondition,
            int triggerThreshold,
            ZoneId triggerZone,
            string dialogueText,
            bool unlockZoneOnTrigger,
            ZoneId unlockZoneId)
        {
            string path = $"{StoryBeatFolder}/{id}.asset";
            StoryBeat beat = AssetDatabase.LoadAssetAtPath<StoryBeat>(path);
            if (beat == null)
            {
                beat = ScriptableObject.CreateInstance<StoryBeat>();
                AssetDatabase.CreateAsset(beat, path);
            }

            SerializedObject serializedBeat = new SerializedObject(beat);
            serializedBeat.FindProperty("id").stringValue = id;
            serializedBeat.FindProperty("triggerCondition").enumValueIndex = (int)triggerCondition;
            serializedBeat.FindProperty("triggerThreshold").intValue = triggerThreshold;
            serializedBeat.FindProperty("triggerZone").enumValueIndex = (int)triggerZone;
            serializedBeat.FindProperty("dialogueText").stringValue = dialogueText;
            serializedBeat.FindProperty("unlockZoneOnTrigger").boolValue = unlockZoneOnTrigger;
            serializedBeat.FindProperty("unlockZoneId").enumValueIndex = (int)unlockZoneId;
            serializedBeat.ApplyModifiedProperties();
            EditorUtility.SetDirty(beat);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
