using UnityEngine;

namespace Tidepool.Domain
{
    public enum ContestMoveCategory
    {
        Attack,
        Focus,
        Defend
    }

    [CreateAssetMenu(menuName = "Tidepool/Contest Move", fileName = "NewContestMove")]
    public class ContestMove : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private TidelingCurrent current;
        [SerializeField] private ContestMoveCategory category = ContestMoveCategory.Attack;
        [SerializeField, Min(0)] private int gentlePower = 1;
        [SerializeField, TextArea(2, 4)] private string description;

        public string Id => id;
        public string DisplayName => displayName;
        public TidelingCurrent Current => current;
        public ContestMoveCategory Category => category;
        public int GentlePower => gentlePower;
        public string Description => description;

        public static int ResolveCategoryAdvantage(ContestMoveCategory first, ContestMoveCategory second)
        {
            if (first == second)
            {
                return 0;
            }

            if ((first == ContestMoveCategory.Attack && second == ContestMoveCategory.Focus)
                || (first == ContestMoveCategory.Focus && second == ContestMoveCategory.Defend)
                || (first == ContestMoveCategory.Defend && second == ContestMoveCategory.Attack))
            {
                return 1;
            }

            return -1;
        }
    }
}
