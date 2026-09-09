using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Tidepool.Domain;
using Tidepool.Runtime;
using Tidepool.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Tidepool.Editor
{
    public static class OpeningFlowEditModeTests
    {
        private const string BootScenePath = "Assets/Scenes/Boot.unity";

        [MenuItem("Tools/Tidepool/Verify Great Low Tide Opening")]
        public static void RunAll()
        {
            VerifyPresentationRules();
            VerifyGeneratedScene();
            Debug.Log("Great Low Tide opening edit-mode tests passed: first run, interruption recovery, return copy, safe area, and 88pt controls.");
        }

        private static void VerifyPresentationRules()
        {
            WithSaveService(saveService =>
            {
                SetData(saveService, new SaveData());
                Require(
                    GreatLowTideOpeningController.DeterminePresentation(saveService) == BootPresentationMode.Opening,
                    "A new save must see the opening.");

                bool saved = saveService.RememberCompletedSetPiece(ExpeditionStateIds.SetPieceOpening);
                Require(saved, "Starting the opening must persist its acknowledgement immediately.");
                Require(
                    GreatLowTideOpeningController.DeterminePresentation(saveService) == BootPresentationMode.Returning,
                    "An interrupted opening must resume at the returning-player path.");

                SaveData existing = new SaveData
                {
                    caught = new List<CaughtTideling>
                    {
                        new CaughtTideling { speciesId = "blip", nickname = "Ripple", timesSeen = 1 }
                    },
                    activeExpeditionChapterId = ExpeditionStateIds.ChapterMeadow
                };
                SetData(saveService, existing);
                Require(
                    GreatLowTideOpeningController.DeterminePresentation(saveService) == BootPresentationMode.Returning,
                    "A pre-v0.8 save must use the returning-player path.");
                Require(
                    GreatLowTideOpeningController.GetChapterTitle(ExpeditionStateIds.ChapterMeadow) == "The Waving Path",
                    "The return card must resolve chapter copy.");
                Require(
                    !string.IsNullOrWhiteSpace(GreatLowTideOpeningController.GetReturnSummary("unknown.chapter")),
                    "Unknown chapter IDs need warm fallback copy.");
            });
        }

        private static void VerifyGeneratedScene()
        {
            EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            GreatLowTideOpeningController controller = UnityEngine.Object.FindAnyObjectByType<GreatLowTideOpeningController>(FindObjectsInactive.Include);
            Require(controller != null, "The generated Boot scene must contain the opening controller.");
            Require(UnityEngine.Object.FindAnyObjectByType<SafeAreaFitter>(FindObjectsInactive.Include) != null, "The generated Boot scene must contain a safe-area root.");

            CanvasScaler scaler = UnityEngine.Object.FindAnyObjectByType<CanvasScaler>(FindObjectsInactive.Include);
            Require(scaler != null, "The generated Boot scene must contain a Canvas Scaler.");
            Require(scaler.referenceResolution == new Vector2(1024f, 768f), "Boot must use the 1024 x 768 reference resolution.");
            Require(Mathf.Approximately(scaler.matchWidthOrHeight, 0.5f), "Boot Canvas Scaler match must be 0.5.");

            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
            Require(buttons.Length >= 3, "Boot must expose opening, skip, and returning Continue controls.");
            for (int i = 0; i < buttons.Length; i++)
            {
                RectTransform rect = buttons[i].GetComponent<RectTransform>();
                Require(rect != null && rect.rect.height >= 88f, $"Boot button {buttons[i].name} must be at least 88pt high.");
            }

            Toggle reducedMotion = UnityEngine.Object.FindAnyObjectByType<Toggle>(FindObjectsInactive.Include);
            Require(reducedMotion != null, "Boot must expose reduced-motion control.");
            RectTransform toggleRect = reducedMotion.GetComponent<RectTransform>();
            Require(toggleRect != null && toggleRect.rect.height >= 88f, "Reduced-motion control must be at least 88pt high.");
        }

        private static void WithSaveService(Action<GameSaveService> verification)
        {
            string saveFileName = $"opening-flow-edit-mode-{Guid.NewGuid():N}.json";
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            GameSaveService previousInstance = GameSaveService.Instance;
            GameObject saveObject = new GameObject("OpeningFlowEditModeTestService");

            try
            {
                GameSaveService saveService = saveObject.AddComponent<GameSaveService>();
                SetPrivateField(saveService, "saveFileName", saveFileName);
                SetStaticAutoProperty("Instance", saveService);
                verification(saveService);
            }
            finally
            {
                SetStaticAutoProperty("Instance", previousInstance);
                UnityEngine.Object.DestroyImmediate(saveObject);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }
        }

        private static void SetData(GameSaveService saveService, SaveData data)
        {
            SetPrivateField(saveService, "<Data>k__BackingField", data);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            field.SetValue(target, value);
        }

        private static void SetStaticAutoProperty(string propertyName, object value)
        {
            FieldInfo field = typeof(GameSaveService).GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(typeof(GameSaveService).Name, propertyName);
            }

            field.SetValue(null, value);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
