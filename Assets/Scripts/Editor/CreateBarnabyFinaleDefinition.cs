using System.IO;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class CreateBarnabyFinaleDefinition
    {
        private const string Folder = "Assets/Data/Finale";
        private const string AssetPath = Folder + "/OldBarnabyFinale.asset";

        [MenuItem("Tools/Tidepool/Create Old Barnaby Finale Definition")]
        public static void CreateDefinition()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(Folder);

            BarnabyFinaleDefinition definition = AssetDatabase.LoadAssetAtPath<BarnabyFinaleDefinition>(AssetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<BarnabyFinaleDefinition>();
                AssetDatabase.CreateAsset(definition, AssetPath);
            }

            definition.Configure(FindSpecies(BarnabyFinaleProgress.SpeciesId));
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static TidelingSpecies FindSpecies(string speciesId)
        {
            string[] guids = AssetDatabase.FindAssets("t:TidelingSpecies");
            for (int i = 0; i < guids.Length; i++)
            {
                TidelingSpecies species = AssetDatabase.LoadAssetAtPath<TidelingSpecies>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (species != null && species.Id == speciesId)
                {
                    return species;
                }
            }

            Debug.LogWarning("Old Barnaby species asset was not found. Generate starter species assets before wiring the finale.");
            return null;
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
