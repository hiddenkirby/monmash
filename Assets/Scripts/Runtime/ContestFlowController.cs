using Tidepool.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class ContestFlowController : MonoBehaviour
    {
        private const float CategoryAdvantageScoreBonus = 1000f;
        private static readonly TidelingCurrent[] CurrentRingOrder =
        {
            TidelingCurrent.Current,
            TidelingCurrent.Coral,
            TidelingCurrent.Stone,
            TidelingCurrent.Glow,
            TidelingCurrent.Tide
        };

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
        [SerializeField] private Text roundCounterText;
        [SerializeField] private Text resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private string exitSceneName = "Overworld";
        [SerializeField] private string partySelectSceneName = "PartySelect";
        [SerializeField, Min(0)] private int progressPointsPerRound = 1;
        [SerializeField, Min(0)] private int progressPointsForWin = 2;
        [SerializeField, Min(1)] private int roundsToWinContest = 2;
        [SerializeField, Min(1)] private int maxResolvedRounds = 3;
        [SerializeField] private int[] growthFormUnlockLevels = { 5, 10, 15 };

        [Header("Current ring")]
        [SerializeField] private Image[] currentRingNodes;
        [SerializeField] private Text[] currentRingLabels;
        [SerializeField] private Text currentAdvantageText;

        private TidelingSpecies playerSpecies;
        private TidelingSpecies visitingSpecies;
        private ContestParticipantState playerState;
        private ContestParticipantState visitingState;
        private bool contestFinished;
        private int currentRound = 1;
        private int resolvedRounds;
        private int playerRoundWins;
        private int visitingRoundWins;

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
            RefreshRoundCounter();
            RefreshCurrentRing();
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
            ResetContest();
            int playerLevel = ResolvePlayerLevel();

            SetMoveButtonsInteractable(CanPlayerChooseMove());
            RebindMoveButtons(playerLevel);
            SetResultText("Pick a friendly move.");
            RefreshRoundCounter();
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
                UnloadAdditiveSceneIfLoaded(partySelectSceneName);
                return;
            }

            if (!string.IsNullOrWhiteSpace(exitSceneName))
            {
                SceneManager.LoadScene(exitSceneName);
            }
        }

        private static void UnloadAdditiveSceneIfLoaded(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(scene);
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
                visitingRoundWins = roundsToWinContest;
                resolvedRounds = Mathf.Max(resolvedRounds, 1);
                FinishContest();
                return;
            }

            int playerLevel = ResolvePlayerLevel();
            ContestMove playerMove = playerSpecies.GetUnlockedContestMove(moveIndex, playerLevel);
            ContestMove visitingMove = ChooseVisitingMove();
            if (playerMove == null || visitingMove == null)
            {
                visitingRoundWins = roundsToWinContest;
                resolvedRounds = Mathf.Max(resolvedRounds, 1);
                FinishContest();
                return;
            }

            float playerScore = ScoreMove(playerMove, visitingSpecies, visitingMove);
            float visitingScore = ScoreMove(visitingMove, playerSpecies, playerMove);
            string visitingName = GetDisplayName(visitingSpecies);

            if (playerScore > visitingScore)
            {
                visitingState?.MarkTuckeredOut();
                playerRoundWins += 1;
                SetResultText($"{playerMove.DisplayName} sparkles through. {visitingName} naps a little.");
            }
            else if (visitingScore > playerScore)
            {
                playerState?.MarkTuckeredOut();
                visitingRoundWins += 1;
                SetResultText($"{visitingName} uses {visitingMove.DisplayName}. Your Tideling needs a quick nap.");
            }
            else
            {
                SetResultText("Both Tidelings take a happy breather.");
            }

            resolvedRounds += 1;
            if (HasContestResult())
            {
                FinishContest();
                return;
            }

            AdvanceToNextRound();
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

        private bool HasContestResult()
        {
            return playerRoundWins >= roundsToWinContest
                || visitingRoundWins >= roundsToWinContest
                || resolvedRounds >= maxResolvedRounds;
        }

        private void AdvanceToNextRound()
        {
            currentRound = Mathf.Min(resolvedRounds + 1, maxResolvedRounds);
            playerState?.AdvanceRest();
            visitingState?.AdvanceRest();
            int playerLevel = ResolvePlayerLevel();

            RebindMoveButtons(playerLevel);
            SetMoveButtonsInteractable(CanPlayerChooseMove());
            RefreshRoundCounter();
            RefreshTuckeredVisuals();
        }

        private void FinishContest()
        {
            contestFinished = true;
            SetMoveButtonsInteractable(false);
            AwardContestProgress();
            SetContestResultText();
            RefreshRoundCounter();
            RefreshTuckeredVisuals();

            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(true);
            }
        }

        private void ResetContest()
        {
            contestFinished = false;
            currentRound = 1;
            resolvedRounds = 0;
            playerRoundWins = 0;
            visitingRoundWins = 0;
            playerState = ContestParticipantState.ForSpecies(playerSpecies);
            visitingState = ContestParticipantState.ForSpecies(visitingSpecies);
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

        private void AwardContestProgress()
        {
            AwardProgress(playerRoundWins > visitingRoundWins ? progressPointsForWin : progressPointsPerRound);
        }

        private void SetContestResultText()
        {
            if (playerRoundWins > visitingRoundWins)
            {
                SetResultText($"You won the friendly contest {playerRoundWins}-{visitingRoundWins}!");
                return;
            }

            if (visitingRoundWins > playerRoundWins)
            {
                SetResultText($"They won this friendly contest {visitingRoundWins}-{playerRoundWins}. Try again when you like.");
                return;
            }

            SetResultText("That was a close one. Everyone learned something.");
        }

        private void RefreshRoundCounter()
        {
            int visibleRound = contestFinished ? Mathf.Min(resolvedRounds, maxResolvedRounds) : currentRound;
            string value = contestFinished
                ? $"Contest complete - You {playerRoundWins}, Visitor {visitingRoundWins}"
                : $"Round {visibleRound} of {maxResolvedRounds} - You {playerRoundWins}, Visitor {visitingRoundWins}";
            SetText(roundCounterText, value);
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

        private void RefreshCurrentRing()
        {
            for (int i = 0; i < CurrentRingOrder.Length; i++)
            {
                TidelingCurrent current = CurrentRingOrder[i];
                bool isPlayerCurrent = playerSpecies != null && playerSpecies.Current == current;
                bool isVisitingCurrent = visitingSpecies != null && visitingSpecies.Current == current;
                SetCurrentRingNode(i, current, isPlayerCurrent, isVisitingCurrent);
            }

            SetCurrentAdvantageText();
        }

        private void SetCurrentRingNode(int index, TidelingCurrent current, bool isPlayerCurrent, bool isVisitingCurrent)
        {
            Color displayColor = TidelingCurrentRules.GetDisplayColor(current);
            if (currentRingNodes != null && index < currentRingNodes.Length && currentRingNodes[index] != null)
            {
                Image node = currentRingNodes[index];
                node.color = isPlayerCurrent || isVisitingCurrent
                    ? Color.Lerp(displayColor, Color.white, 0.25f)
                    : new Color(displayColor.r, displayColor.g, displayColor.b, 0.35f);
            }

            if (currentRingLabels == null || index >= currentRingLabels.Length || currentRingLabels[index] == null)
            {
                return;
            }

            string prefix = string.Empty;
            if (isPlayerCurrent && isVisitingCurrent)
            {
                prefix = "Both\n";
            }
            else if (isPlayerCurrent)
            {
                prefix = "You\n";
            }
            else if (isVisitingCurrent)
            {
                prefix = "Visitor\n";
            }

            Text label = currentRingLabels[index];
            label.text = $"{prefix}{TidelingCurrentRules.GetDisplayName(current)}";
            label.color = isPlayerCurrent || isVisitingCurrent ? Color.white : new Color(0.08f, 0.18f, 0.22f);
        }

        private void SetCurrentAdvantageText()
        {
            if (currentAdvantageText == null)
            {
                return;
            }

            if (playerSpecies == null || visitingSpecies == null)
            {
                currentAdvantageText.text = "Currents are getting ready.";
                currentAdvantageText.color = new Color(0.08f, 0.18f, 0.22f);
                return;
            }

            TidelingCurrent playerCurrent = playerSpecies.Current;
            TidelingCurrent visitingCurrent = visitingSpecies.Current;
            float playerMultiplier = TidelingCurrentRules.GetEffectivenessMultiplier(playerCurrent, visitingCurrent);
            if (playerMultiplier > TidelingCurrentRules.NeutralMultiplier)
            {
                currentAdvantageText.text = $"You: {TidelingCurrentRules.GetDisplayName(playerCurrent)} -> Visitor: {TidelingCurrentRules.GetDisplayName(visitingCurrent)}";
                currentAdvantageText.color = new Color(0.12f, 0.42f, 0.32f);
                return;
            }

            if (playerMultiplier < TidelingCurrentRules.NeutralMultiplier)
            {
                currentAdvantageText.text = $"Visitor: {TidelingCurrentRules.GetDisplayName(visitingCurrent)} -> You: {TidelingCurrentRules.GetDisplayName(playerCurrent)}";
                currentAdvantageText.color = new Color(0.55f, 0.20f, 0.22f);
                return;
            }

            currentAdvantageText.text = "Currents are even.";
            currentAdvantageText.color = new Color(0.08f, 0.18f, 0.22f);
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
