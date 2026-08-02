using UnityEngine;

namespace Tidepool.Domain
{
    [CreateAssetMenu(menuName = "Tidepool/Contest Move", fileName = "NewContestMove")]
    public class ContestMove : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private TidelingCurrent current;
        [SerializeField, Min(0)] private int gentlePower = 1;
        [SerializeField, TextArea(2, 4)] private string description;

        public string Id => id;
        public string DisplayName => displayName;
        public TidelingCurrent Current => current;
        public int GentlePower => gentlePower;
        public string Description => description;
    }
}
