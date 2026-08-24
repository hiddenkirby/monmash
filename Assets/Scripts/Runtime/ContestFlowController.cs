using Tidepool.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class ContestFlowController : MonoBehaviour
    {
        private const float CategoryAdvantageScoreBonus = 1000f;

        [Header("Prototype fallback")]
        [SerializeField] private TidelingSpecies fallbackPlayerSpecies;
        [SerializeField] private TidelingSpecies fallbackVisitingSpecies;

        [Header("Creature views")]
        [SerializeField] private Image playerImage;
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text playerStatusText;
        [SerializeField] private Image visitingImage;
        [SerializeField] private Text visitingNameText;
        [SerializeField] private Text visitingStatusText;

        [Header("Move controls")]
        [SerializeField] private Button firstMoveButton;
        [SerializeField] private Text firstMoveButtonText;
        [SerializeField] private Button secondMoveButton;
        [SerializeField] private Text secondMoveButtonText;

        [Header("Result controls")]
        [SerializeField] private Text resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private string exitSceneName = "Overworld";
        [SerializeField, Min(0)] private int progressPointsPerRound = 1;
        [SerializeField, Min(0)] private int progressPointsForWin = 2;
        [SerializeField] private int[] growthFormUnlockLevels = { 5, 10, 15 };

        private TidelingSpecies playerSpecies;
        private TidelingSpecies visitingSpecies;
        private ContestParticipantState playerState;
        private ContestParticipantState visitingState;
        private bool contestFinished;

        private void Start()
        {
            playerSpecies = ContestContext.PlayerSpecies == null ? fallbackPlayerSpecies : ContestContext.PlayerSpecies;
            visitingSpecies = ContestContext.VisitingSpecies == null ? fallbackVisitingSpecies : ContestContext.VisitingSpecies;
            playerState = ContestParticipantState.ForSpecies(playerSpecies);
            visitingState = ContestParticipantState.ForSpecies(visitingSpecies);
            int playerLevel = ResolvePlayerLevel();

            BindCreature(playerSpecies, playerImage, playerNameText);
            BindCreature(visitingSpecies, visitingImage, visitingNameText);
            BindMoveButton(firstMoveButton, firstMoveButtonText,
                playerSpecies?.GetUnlockedContestMove(0, playerLevel), ChooseFirstMove);
            BindMoveButton(secondMoveButton, secondMoveButtonText,
                playerSpecies?.GetUnlockedContestMove(1, playerLevel), ChooseSecondMove);

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(Retry);
                retryButton.gameObject.SetActive(false);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(ExitContest);
            }

            SetResultText(playerSpecies == null || visitingSpecies == null
                ? "Contest friends are still getting ready."
                : "Pick a friendly move.");
            RefreshTuckeredVisuals();
        }

        public void ChooseFirstMove()
        {
            ChooseMove(0);
        }

        public void ChooseSecondMove()
        {
            ChooseMove(1);
        }

        public void Retry()
        {
            contestFinished = false;
            bool playerRested = playerState?.AdvanceRest() ?? false;
            bool visitingRested = visitingState?.AdvanceRest() ?? false;
            int playerLevel = ResolvePlayerLevel();

            SetMoveButtonsInteractable(CanPlayerChooseMove());
            RebindMoveButtons(playerLevel);
            SetResultText(playerRested || visitingRested
                ? "Everyone is ready again. Pick a friendly move."
                : "Pick a friendly move.");
            RefreshTuckeredVisuals();

            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(false);
            }
        }

        public void ExitContest()
        {
            ContestContext.Clear();
            ContestEvents.RaiseContestFinished();

            if (SceneManager.sceneCount > 1)
            {
                SceneManager.UnloadSceneAsync(gameObject.scene);
                return;
            }

            if (!string.IsNullOrWhiteSpace(exitSceneName))
            {
                SceneManager.LoadScene(exitSceneName);
            }
        }

        private void ChooseMove(int moveIndex)
        {
            if (contestFinished || playerSpecies == null || visitingSpecies == null)
            {
                return;
            }

            if (!CanPlayerChooseMove())
            {
                SetResultText("They need a little rest. Try another round?");
                FinishRound();
                return;
            }

            int playerLevel = ResolvePlayerLevel();
            ContestMove playerMove = playerSpecies.GetUnlockedContestMove(moveIndex, playerLevel);
            ContestMove visitingMove = ChooseVisitingMove();
            if (playerMove == null || visitingMove == null)
            {
                SetResultText("They need a little rest. Try another round?");
                FinishRound();
                return;
            }

            float playerScore = ScoreMove(playerMove, visitingSpecies, visitingMove);
            float visitingScore = ScoreMove(visitingMove, playerSpecies, playerMove);
            string visitingName = GetDisplayName(visitingSpecies);

            if (playerScore > visitingScore)
            {
                visitingState?.MarkTuckeredOut();
                AwardProgress(progressPointsForWin);
                SetResultText($"{playerMove.DisplayName} sparkles through. {visitingName} naps a little. Try another round?");
            }
            else if (visitingScore > playerScore)
            {
                playerState?.MarkTuckeredOut();
                AwardProgress(progressPointsPerRound);
                SetResultText($"{visitingName} uses {visitingMove.DisplayName}. Your Tideling needs a quick nap. Try another round?");
            }
            else
            {
                AwardProgress(progressPointsPerRound);
                SetResultText($"Both Tidelings take a happy breather. Try another round?");
            }

            FinishRound();
        }

        private ContestMove ChooseVisitingMove()
        {
            ContestMove first = visitingSpecies.FirstContestMove;
            ContestMove second = visitingSpecies.SecondContestMove;
            if (first == null)
            {
                return second;
            }

            if (second == null)
            {
                return first;
            }

            return ScoreMove(second, playerSpecies) > ScoreMove(first, playerSpecies) ? second : first;
        }

        private static float ScoreMove(ContestMove move, TidelingSpecies defender)
        {
            return ScoreMove(move, defender, null);
        }

        private static float ScoreMove(ContestMove move, TidelingSpecies defender, ContestMove opposingMove)
        {
            if (move == null || defender == null)
            {
                return 0f;
            }

            float score = move.GentlePower * TidelingCurrentRules.GetEffectivenessMultiplier(move.Current, defender.Current);
            if (opposingMove == null)
            {
                return score;
            }

            int categoryAdvantage = ContestMove.ResolveCategoryAdvantage(move.Category, opposingMove.Category);
            return categoryAdvantage > 0 ? score + CategoryAdvantageScoreBonus : score;
        }

        private void FinishRound()
        {
            contestFinished = true;
            SetMoveButtonsInteractable(false);
            RefreshTuckeredVisuals();

            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(true);
            }
        }

        private void SetMoveButtonsInteractable(bool interactable)
        {
            SetButtonInteractable(firstMoveButton, interactable && playerSpecies?.FirstContestMove != null);
            SetButtonInteractable(secondMoveButton, interactable && playerSpecies?.SecondContestMove != null);
        }

        private bool CanPlayerChooseMove()
        {
            return playerState == null || playerState.CanChooseMove;
        }

        private int ResolvePlayerLevel()
        {
            if (playerSpecies == null || GameSaveService.Instance == null)
            {
                return CaughtTideling.MinLevel;
            }

            CaughtTideling caught = GameSaveService.Instance.FindCaught(playerSpecies.Id);
            if (caught == null)
            {
                return CaughtTideling.MinLevel;
            }

            TidelingLevelProgression.Normalize(caught);
            return caught.level;
        }

        private void AwardProgress(int points)
        {
            if (points <= 0 || playerSpecies == null || GameSaveService.Instance == null)
            {
                return;
            }

            CaughtTideling caught = GameSaveService.Instance.FindCaught(playerSpecies.Id);
            int previousLevel = caught == null ? CaughtTideling.MinLevel : caught.level;

            GameSaveService.Instance.RecordGentleProgress(playerSpecies.Id, points);

            if (caught != null)
            {
                int newLevel = caught.level;
                if (newLevel > previousLevel)
                {
                    TryRememberGrowthFormsForLevel(previousLevel, newLevel);
                }
            }
        }

        private void TryRememberGrowthFormsForLevel(int previousLevel, int newLevel)
        {
            if (growthFormUnlockLevels == null || GameSaveService.Instance == null || playerSpecies == null)
            {
                return;
            }

            for (int i = 0; i < growthFormUnlockLevels.Length; i++)
            {
                int unlockLevel = growthFormUnlockLevels[i];
                if (unlockLevel > previousLevel && unlockLevel <= newLevel)
                {
                    string formId = $"growth-form-{unlockLevel}";
                    GameSaveService.Instance.RememberGrowthForm(playerSpecies.Id, formId);
                }
            }
        }

        private void RebindMoveButtons(int level)
        {
            BindMoveButton(firstMoveButton, firstMoveButtonText,
                playerSpecies?.GetUnlockedContestMove(0, level), ChooseFirstMove);
            BindMoveButton(secondMoveButton, secondMoveButtonText,
                playerSpecies?.GetUnlockedContestMove(1, level), ChooseSecondMove);
        }

        private void RefreshTuckeredVisuals()
        {
            UpdateTuckeredDisplay(playerImage, playerStatusText, playerState);
            UpdateTuckeredDisplay(visitingImage, visitingStatusText, visitingState);
        }

        private static void UpdateTuckeredDisplay(Image image, Text statusText, ContestParticipantState state)
        {
            bool tuckeredOut = state != null && state.IsTuckeredOut;
            SetText(statusText, tuckeredOut ? "napping..." : string.Empty);

            if (image != null)
            {
                image.color = tuckeredOut
                    ? new Color(0.5f, 0.5f, 0.5f, 0.6f)
                    : Color.white;
            }
        }

        private static void BindCreature(TidelingSpecies species, Image image, Text nameText)
        {
            if (image != null)
            {
                image.sprite = species == null ? null : species.Sprite;
                image.enabled = species != null && species.Sprite != null;
                image.preserveAspect = true;
            }

            SetText(nameText, GetDisplayName(species));
        }

        private static void BindMoveButton(Button button, Text label, ContestMove move, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
            button.interactable = move != null;
            button.gameObject.SetActive(move != null);
            SetText(label, move == null ? string.Empty : move.DisplayName);
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private void SetResultText(string value)
        {
            SetText(resultText, value);
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private static string GetDisplayName(TidelingSpecies species)
        {
            if (species == null || string.IsNullOrWhiteSpace(species.DisplayName))
            {
                return "Tideling";
            }

            return species.DisplayName;
        }
    }
}
