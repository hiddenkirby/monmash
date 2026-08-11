using Tidepool.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tidepool.Runtime
{
    public class ContestFlowController : MonoBehaviour
    {
        [Header("Prototype fallback")]
        [SerializeField] private TidelingSpecies fallbackPlayerSpecies;
        [SerializeField] private TidelingSpecies fallbackVisitingSpecies;

        [Header("Creature views")]
        [SerializeField] private Image playerImage;
        [SerializeField] private Text playerNameText;
        [SerializeField] private Image visitingImage;
        [SerializeField] private Text visitingNameText;

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

            BindCreature(playerSpecies, playerImage, playerNameText);
            BindCreature(visitingSpecies, visitingImage, visitingNameText);
            BindMoveButton(firstMoveButton, firstMoveButtonText, playerSpecies?.FirstContestMove, ChooseFirstMove);
            BindMoveButton(secondMoveButton, secondMoveButtonText, playerSpecies?.SecondContestMove, ChooseSecondMove);

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

            SetMoveButtonsInteractable(CanPlayerChooseMove());
            SetResultText(playerRested || visitingRested
                ? "Everyone is ready again. Pick a friendly move."
                : "Pick a friendly move.");

            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(false);
            }
        }

        public void ExitContest()
        {
            ContestContext.Clear();

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

            ContestMove playerMove = playerSpecies.GetContestMove(moveIndex);
            ContestMove visitingMove = ChooseVisitingMove();
            if (playerMove == null || visitingMove == null)
            {
                SetResultText("They need a little rest. Try another round?");
                FinishRound();
                return;
            }

            float playerScore = ScoreMove(playerMove, visitingSpecies);
            float visitingScore = ScoreMove(visitingMove, playerSpecies);
            string visitingName = GetDisplayName(visitingSpecies);

            if (playerScore > visitingScore)
            {
                visitingState?.MarkTuckeredOut();
                SetResultText($"{playerMove.DisplayName} sparkles through. {visitingName} takes a little rest.");
            }
            else if (visitingScore > playerScore)
            {
                playerState?.MarkTuckeredOut();
                SetResultText($"{visitingName} uses {visitingMove.DisplayName}. Try another round?");
            }
            else
            {
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
            if (move == null || defender == null)
            {
                return 0f;
            }

            return move.GentlePower * TidelingCurrentRules.GetEffectivenessMultiplier(move.Current, defender.Current);
        }

        private void FinishRound()
        {
            contestFinished = true;
            SetMoveButtonsInteractable(false);

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
