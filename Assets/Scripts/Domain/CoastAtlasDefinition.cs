using System;
using UnityEngine;

namespace Tidepool.Domain
{
    [CreateAssetMenu(menuName = "Tidepool/Coast Atlas Definition", fileName = "NewCoastAtlasDefinition")]
    public class CoastAtlasDefinition : ScriptableObject
    {
        [SerializeField] private Sprite mapSprite;
        [SerializeField] private string title = "The Great Low Tide";
        [SerializeField, TextArea(2, 4)] private string fallbackObjective = "The coast is waiting to be noticed.";
        [SerializeField] private CoastAtlasNode[] nodes = Array.Empty<CoastAtlasNode>();

        public Sprite MapSprite => mapSprite;
        public string Title => title;
        public string FallbackObjective => fallbackObjective;
        public CoastAtlasNode[] Nodes => nodes ?? Array.Empty<CoastAtlasNode>();

        public CoastAtlasNode FindNode(ZoneId zone)
        {
            CoastAtlasNode[] atlasNodes = Nodes;
            for (int i = 0; i < atlasNodes.Length; i++)
            {
                if (atlasNodes[i] != null && atlasNodes[i].Zone == zone)
                {
                    return atlasNodes[i];
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public void Configure(string atlasTitle, string atlasFallbackObjective, CoastAtlasNode[] atlasNodes)
        {
            title = atlasTitle;
            fallbackObjective = atlasFallbackObjective;
            nodes = atlasNodes ?? Array.Empty<CoastAtlasNode>();
        }
#endif
    }
}
