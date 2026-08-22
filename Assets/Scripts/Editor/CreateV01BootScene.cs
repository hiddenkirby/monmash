using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Editor
{
    public static class CreateV01BootScene
    {
        private const string ScenePath = "Assets/Scenes/Boot.unity";

        [MenuItem("Tools/Tidepool/Create v0.1 Boot Scene")]
        public static void CreateBootScene()
        {
            EnsureFolder("Assets/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject saveServiceObj = new GameObject("GameSaveService");
            saveServiceObj.AddComponent<Tidepool.Runtime.GameSaveService>();

            GameObject routerObj = new GameObject("BootRouter");
            Tidepool.Runtime.BootRouter router = routerObj.AddComponent<Tidepool.Runtime.BootRouter>();

            SerializedObject serializedRouter = new SerializedObject(router);
            serializedRouter.FindProperty("overworldSceneName").stringValue = "Overworld";
            serializedRouter.FindProperty("loadOverworldOnStart").boolValue = true;
            serializedRouter.ApplyModifiedProperties();

            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
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
