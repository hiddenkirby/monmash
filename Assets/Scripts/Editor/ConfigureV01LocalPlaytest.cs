using Tidepool.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Tidepool.Editor
{
    public static class ConfigureV01LocalPlaytest
    {
        private const string OverworldScenePath = "Assets/Scenes/Overworld.unity";
        private const string CatchEncounterScenePath = "Assets/Scenes/CatchEncounter.unity";
        private const string SpeciesDatabasePath = "Assets/Data/Databases/SpeciesDatabase.asset";

        [MenuItem("Tools/Tidepool/Configure v0.1 Local Playtest")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play mode before configuring the v0.1 local playtest.");
                return;
            }

            CreateTidepoolStarterAssets.CreateStarterSpeciesAssets();
            CreateV01CatchEncounterScene.CreateCatchEncounterScene();
            CreateV01OverworldScene.CreateOverworldScene();
            ConfigureBuildSettings();

            Scene scene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);
            WireOverworldPlaytestObjects();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Configured v0.1 local playtest. Press Play in Overworld and walk into seagrass to start a catch encounter.");
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(OverworldScenePath, true),
                new EditorBuildSettingsScene(CatchEncounterScenePath, true)
            };
        }

        private static void WireOverworldPlaytestObjects()
        {
            GameSaveService saveService = FindOrCreateComponent<GameSaveService>("GameSaveService");
            SerializedObject serializedSaveService = new SerializedObject(saveService);
            serializedSaveService.FindProperty("saveFileName").stringValue = "save.json";
            serializedSaveService.ApplyModifiedPropertiesWithoutUndo();

            EncounterDirector encounterDirector = FindOrCreateComponent<EncounterDirector>("EncounterDirector");
            PlayerGridMover player = GameObject.Find("Player")?.GetComponent<PlayerGridMover>();
            Tilemap seagrassTilemap = GameObject.Find("Seagrass")?.GetComponent<Tilemap>();
            SpeciesDatabase speciesDatabase = AssetDatabase.LoadAssetAtPath<SpeciesDatabase>(SpeciesDatabasePath);

            SerializedObject serializedDirector = new SerializedObject(encounterDirector);
            serializedDirector.FindProperty("player").objectReferenceValue = player;
            serializedDirector.FindProperty("seagrassTilemap").objectReferenceValue = seagrassTilemap;
            serializedDirector.FindProperty("speciesDatabase").objectReferenceValue = speciesDatabase;
            serializedDirector.FindProperty("catchSceneName").stringValue = "CatchEncounter";
            serializedDirector.FindProperty("encounterChance").floatValue = 1f;
            serializedDirector.FindProperty("graceStepsAfterEncounter").intValue = 0;
            serializedDirector.ApplyModifiedPropertiesWithoutUndo();

            if (player == null || seagrassTilemap == null || speciesDatabase == null)
            {
                Debug.LogWarning("v0.1 local playtest setup completed with missing references. Check Player, Grid/Seagrass, and SpeciesDatabase.");
            }
        }

        private static T FindOrCreateComponent<T>(string objectName) where T : Component
        {
            T existing = Object.FindAnyObjectByType<T>();
            if (existing != null)
            {
                existing.gameObject.name = objectName;
                return existing;
            }

            GameObject gameObject = new GameObject(objectName);
            return gameObject.AddComponent<T>();
        }
    }
}
