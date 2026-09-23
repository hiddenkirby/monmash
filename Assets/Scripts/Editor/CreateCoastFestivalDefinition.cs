using System.IO;
using Tidepool.Domain;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateCoastFestivalDefinition
    {
        private const string Folder = "Assets/Data/Festival";
        private const string AssetPath = Folder + "/GreatLowTideFestival.asset";

        [MenuItem("Tools/Tidepool/Create Coast Festival Definition")]
        public static void CreateDefinition()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(Folder);

            CoastFestivalDefinition definition = AssetDatabase.LoadAssetAtPath<CoastFestivalDefinition>(AssetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CoastFestivalDefinition>();
                AssetDatabase.CreateAsset(definition, AssetPath);
            }

            definition.Configure();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
