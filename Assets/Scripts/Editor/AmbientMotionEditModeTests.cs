using System;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class AmbientMotionEditModeTests
    {
        [MenuItem("Tools/Tidepool/Verify Ambient Motion Layer")]
        public static void RunAll()
        {
            AmbientZoneProfile profile = ScriptableObject.CreateInstance<AmbientZoneProfile>();
            profile.Configure(
                ZoneId.SeagrassMeadow,
                Color.white,
                new Color(0.4f, 0.55f, 0.75f, 1f),
                0.35f,
                new[]
                {
                    new AmbientMotionLayer(
                        "test-bubbles",
                        AmbientMotionKind.DriftingParticle,
                        Color.white,
                        Color.cyan,
                        3,
                        new Vector2(6f, 4f),
                        new Vector2(0.1f, 0.2f),
                        new Vector2(0.5f, 1f),
                        new Vector2(4f, 8f),
                        0.1f,
                        0.5f,
                        0.05f,
                        0.1f,
                        false,
                        1)
                });

            GameObject controllerObject = new GameObject("AmbientMotionEditModeTest");
            try
            {
                AmbientZoneMotionController controller = controllerObject.AddComponent<AmbientZoneMotionController>();
                controller.SetProfile(profile);
                Require(controller.PooledInstanceCount == 0, "A layer with no sprite must fail softly without creating pooled renderers.");
                controller.SetDayNightBlend(1f);
                controller.SetPlaying(true);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(profile);
            }

            Debug.Log("Ambient motion edit-mode tests passed: missing sprites fail softly and profile assignment is stable.");
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
