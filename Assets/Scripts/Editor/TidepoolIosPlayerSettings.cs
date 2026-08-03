using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class TidepoolIosPlayerSettings
    {
        private const string CompanyName = "RKirby";
        private const string ProductName = "Tidepool";
        private const string BundleIdentifier = "com.rkirby.tidepool";
        private const string MinimumIosVersion = "15.0";
        private const string AppIconPath = "Assets/Art/UI/app_icon_glass_jar.png";
        private const int Arm64Architecture = 1;

        [MenuItem("Tools/Tidepool/Apply iPad iOS Player Settings")]
        public static void ApplyIpadIosPlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleIdentifier);
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPadOnly;
            PlayerSettings.iOS.targetOSVersionString = MinimumIosVersion;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, Arm64Architecture);
            ApplyAppIcon();
            AssetDatabase.SaveAssets();

            Debug.Log("Applied Tidepool iPad iOS Player Settings.");
            ValidateIpadIosPlayerSettings();
        }

        [MenuItem("Tools/Tidepool/Validate iPad iOS Player Settings")]
        public static void ValidateIpadIosPlayerSettingsMenu()
        {
            ValidateIpadIosPlayerSettings();
        }

        public static bool ValidateIpadIosPlayerSettings()
        {
            List<string> failures = new List<string>();

            AddFailureIf(failures, PlayerSettings.companyName != CompanyName, $"Company name must be {CompanyName}.");
            AddFailureIf(failures, PlayerSettings.productName != ProductName, $"Product name must be {ProductName}.");
            AddFailureIf(
                failures,
                PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS) != BundleIdentifier,
                $"Bundle ID must be {BundleIdentifier}.");
            AddFailureIf(failures, PlayerSettings.iOS.targetDevice != iOSTargetDevice.iPadOnly, "Target device must be iPad Only.");
            AddFailureIf(
                failures,
                PlayerSettings.iOS.targetOSVersionString != MinimumIosVersion,
                $"Minimum iOS version must be {MinimumIosVersion}.");
            AddFailureIf(
                failures,
                PlayerSettings.defaultInterfaceOrientation != UIOrientation.AutoRotation,
                "Default orientation must be Auto Rotation.");
            AddFailureIf(
                failures,
                !PlayerSettings.allowedAutorotateToLandscapeLeft || !PlayerSettings.allowedAutorotateToLandscapeRight,
                "Landscape Left and Landscape Right must both be enabled.");
            AddFailureIf(
                failures,
                PlayerSettings.allowedAutorotateToPortrait || PlayerSettings.allowedAutorotateToPortraitUpsideDown,
                "Portrait orientations must be disabled.");
            AddFailureIf(
                failures,
                PlayerSettings.GetScriptingBackend(NamedBuildTarget.iOS) != ScriptingImplementation.IL2CPP,
                "iOS scripting backend must be IL2CPP.");
            AddFailureIf(
                failures,
                PlayerSettings.GetArchitecture(NamedBuildTarget.iOS) != Arm64Architecture,
                "iOS architecture must be ARM64.");
            AddFailureIf(failures, !HasAppIconAssigned(), $"iOS app icon must be assigned from {AppIconPath}.");

            if (failures.Count == 0)
            {
                Debug.Log("Tidepool iPad iOS Player Settings are valid.");
                return true;
            }

            Debug.LogError("Tidepool iPad iOS Player Settings are invalid:\n- " + string.Join("\n- ", failures));
            return false;
        }

        private static void AddFailureIf(List<string> failures, bool condition, string message)
        {
            if (condition)
            {
                failures.Add(message);
            }
        }

        private static void ApplyAppIcon()
        {
            Texture2D appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
            if (appIcon == null)
            {
                Debug.LogWarning($"Tidepool app icon asset not found at {AppIconPath}.");
                return;
            }

            Texture2D[] icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.iOS, IconKind.Application);
            if (icons.Length == 0)
            {
                icons = new[] { appIcon };
            }

            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = appIcon;
            }

            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, icons, IconKind.Application);
        }

        private static bool HasAppIconAssigned()
        {
            Texture2D appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
            if (appIcon == null)
            {
                return false;
            }

            Texture2D[] icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.iOS, IconKind.Application);
            if (icons.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] != appIcon)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
