using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Tidepool.Domain;
using Tidepool.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tidepool.Editor
{
    public static class ContestBalanceVerifier
    {
        private const string SpeciesDatabasePath = "Assets/Data/Databases/SpeciesDatabase.asset";
        private const int SimulationCount = 100000;
        private const int SimulationSeed = 134;
        private const int VerificationLevel = 3;
        private const float ReasonableChoiceRate = 0.75f;
        private const float MinimumTargetWinRate = 0.68f;
        private const float MaximumTargetWinRate = 0.75f;

        private static readonly MethodInfo ChooseVisitingMoveMethod = FindControllerMethod("ChooseVisitingMove");
        private static readonly MethodInfo ScoreMoveMethod = FindControllerMethod(
            "ScoreMove",
            typeof(ContestMove),
            typeof(TidelingSpecies),
            typeof(ContestMove));
        private static readonly MethodInfo AwardContestProgressMethod = FindControllerMethod("AwardContestProgress");

        [MenuItem("Tools/Tidepool/Verify Contest Balance")]
        public static void VerifyContestBalance()
        {
            SpeciesDatabase database = AssetDatabase.LoadAssetAtPath<SpeciesDatabase>(SpeciesDatabasePath);
            if (database == null || database.All == null || database.All.Count < 2)
            {
                throw new InvalidOperationException($"Contest balance verification needs a populated database at {SpeciesDatabasePath}.");
            }

            GameObject controllerObject = new GameObject("ContestBalanceSimulationController");
            UnityEngine.Random.State previousRandomState = UnityEngine.Random.state;
            float winRate;
            try
            {
                UnityEngine.Random.InitState(SimulationSeed);
                ContestFlowController controller = controllerObject.AddComponent<ContestFlowController>();
                winRate = SimulateReasonablePlay(database.All, controller);
            }
            finally
            {
                UnityEngine.Random.state = previousRandomState;
                UnityEngine.Object.DestroyImmediate(controllerObject);
            }

            if (winRate < MinimumTargetWinRate || winRate > MaximumTargetWinRate)
            {
                throw new InvalidOperationException(
                    $"Reasonable-play contest win rate was {winRate:P1}; expected {MinimumTargetWinRate:P0}-{MaximumTargetWinRate:P0}.");
            }

            VerifyProgressAndSaveSafety(database.All[0]);
            Debug.Log(
                $"Contest balance verification passed: {SimulationCount:N0} contests, "
                + $"{winRate:P1} reasonable-play win rate, win=2 points, loss=1 point, save data preserved.");
        }

        private static float SimulateReasonablePlay(
            IReadOnlyList<TidelingSpecies> species,
            ContestFlowController controller)
        {
            System.Random random = new System.Random(SimulationSeed);
            int playerContestWins = 0;

            for (int simulation = 0; simulation < SimulationCount; simulation++)
            {
                TidelingSpecies player = species[random.Next(species.Count)];
                TidelingSpecies visitor = PickDifferentSpecies(species, player, random);
                if (SimulateContest(player, visitor, controller, random))
                {
                    playerContestWins += 1;
                }
            }

            return (float)playerContestWins / SimulationCount;
        }

        private static bool SimulateContest(
            TidelingSpecies player,
            TidelingSpecies visitor,
            ContestFlowController controller,
            System.Random random)
        {
            SetPrivateField(controller, "playerSpecies", player);
            SetPrivateField(controller, "visitingSpecies", visitor);
            SetPrivateField(controller, "consecutivePlannedVisitingCategoryCount", 0);

            int playerWins = 0;
            int visitorWins = 0;
            for (int round = 0; round < 3 && playerWins < 2 && visitorWins < 2; round++)
            {
                ContestMove visitorMove = (ContestMove)ChooseVisitingMoveMethod.Invoke(controller, null);
                ContestMove playerMove = random.NextDouble() < ReasonableChoiceRate
                    ? ChooseBestResponse(player, visitor, visitorMove)
                    : ChooseRandomUnlockedMove(player, random);

                float playerScore = ScoreMove(playerMove, visitor, visitorMove);
                float visitorScore = ScoreMove(visitorMove, player, playerMove);
                if (playerScore > visitorScore)
                {
                    playerWins += 1;
                }
                else if (visitorScore > playerScore)
                {
                    visitorWins += 1;
                }
            }

            return playerWins > visitorWins;
        }

        private static ContestMove ChooseBestResponse(
            TidelingSpecies player,
            TidelingSpecies visitor,
            ContestMove visitorMove)
        {
            ContestMove bestMove = null;
            float bestMargin = float.NegativeInfinity;
            float bestScore = float.NegativeInfinity;

            for (int index = 0; index < 2; index++)
            {
                ContestMove candidate = player.GetUnlockedContestMove(index, VerificationLevel);
                if (candidate == null)
                {
                    continue;
                }

                float playerScore = ScoreMove(candidate, visitor, visitorMove);
                float margin = playerScore - ScoreMove(visitorMove, player, candidate);
                if (margin > bestMargin || (Mathf.Approximately(margin, bestMargin) && playerScore > bestScore))
                {
                    bestMove = candidate;
                    bestMargin = margin;
                    bestScore = playerScore;
                }
            }

            return bestMove ?? throw new InvalidOperationException($"{player.DisplayName} has no unlocked contest move at level {VerificationLevel}.");
        }

        private static ContestMove ChooseRandomUnlockedMove(TidelingSpecies species, System.Random random)
        {
            ContestMove first = species.GetUnlockedContestMove(0, VerificationLevel);
            ContestMove second = species.GetUnlockedContestMove(1, VerificationLevel);
            if (first == null)
            {
                return second ?? throw new InvalidOperationException($"{species.DisplayName} has no unlocked contest move at level {VerificationLevel}.");
            }

            return second == null || random.Next(2) == 0 ? first : second;
        }

        private static float ScoreMove(ContestMove move, TidelingSpecies defender, ContestMove opposingMove)
        {
            return (float)ScoreMoveMethod.Invoke(null, new object[] { move, defender, opposingMove });
        }

        private static TidelingSpecies PickDifferentSpecies(
            IReadOnlyList<TidelingSpecies> species,
            TidelingSpecies excluded,
            System.Random random)
        {
            TidelingSpecies selected;
            do
            {
                selected = species[random.Next(species.Count)];
            }
            while (selected == excluded);

            return selected;
        }

        private static void VerifyProgressAndSaveSafety(TidelingSpecies playerSpecies)
        {
            string saveFileName = $"contest-balance-verification-{Guid.NewGuid():N}.json";
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            GameSaveService previousInstance = GameSaveService.Instance;
            GameObject saveObject = new GameObject("ContestBalanceVerificationSaveService");
            GameObject controllerObject = new GameObject("ContestBalanceVerificationController");

            try
            {
                GameSaveService saveService = saveObject.AddComponent<GameSaveService>();
                SetPrivateField(saveService, "saveFileName", saveFileName);
                SetStaticAutoProperty(typeof(GameSaveService), "Instance", saveService);

                ContestFlowController controller = controllerObject.AddComponent<ContestFlowController>();
                SetPrivateField(controller, "playerSpecies", playerSpecies);
                VerifyProgressAward(saveService, controller, playerSpecies.Id, 2, 0);
                VerifyProgressAward(saveService, controller, playerSpecies.Id, 0, 2);
            }
            finally
            {
                SetStaticAutoProperty(typeof(GameSaveService), "Instance", previousInstance);
                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(saveObject);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }
        }

        private static void VerifyProgressAward(
            GameSaveService saveService,
            ContestFlowController controller,
            string playerSpeciesId,
            int playerRoundWins,
            int visitorRoundWins)
        {
            SetAutoProperty(saveService, "Data", CreateVerificationSave(playerSpeciesId));
            SetPrivateField(controller, "playerRoundWins", playerRoundWins);
            SetPrivateField(controller, "visitingRoundWins", visitorRoundWins);

            AwardContestProgressMethod.Invoke(controller, null);
            int expectedPoints = playerRoundWins > visitorRoundWins ? 2 : 1;
            AssertSaveMatchesExpected(saveService.Data, playerSpeciesId, expectedPoints);
            saveService.Load();
            AssertSaveMatchesExpected(saveService.Data, playerSpeciesId, expectedPoints);
        }

        private static SaveData CreateVerificationSave(string playerSpeciesId)
        {
            return new SaveData
            {
                caught = new List<CaughtTideling>
                {
                    CreateCaught(playerSpeciesId, "Ripple", ZoneId.TidepoolShallows, 4, 0),
                    CreateCaught("save-safety-friend", "Pebble", ZoneId.SeagrassMeadow, 3, 1)
                },
                seenSpeciesIds = new List<string> { playerSpeciesId, "save-safety-friend" },
                triggeredStoryBeatIds = new List<string> { "first-catch" },
                completedQuestIds = new List<string> { "look-around" },
                unlockedZoneIds = new List<ZoneId>
                {
                    ZoneId.TidepoolShallows,
                    ZoneId.SeagrassMeadow,
                    ZoneId.KelpCurtain
                },
                playerTile = new SerializableVector2Int(7, 3),
                currentZone = ZoneId.KelpCurtain
            };
        }

        private static CaughtTideling CreateCaught(
            string speciesId,
            string nickname,
            ZoneId zone,
            int level,
            int levelProgress)
        {
            return new CaughtTideling
            {
                speciesId = speciesId,
                nickname = nickname,
                caughtAtUtc = "2026-08-28T12:00:00.0000000Z",
                caughtInZone = zone,
                timesSeen = 3,
                level = level,
                levelProgress = levelProgress,
                activeGrowthFormId = TidelingGrowthForms.OriginalFormId,
                rememberedGrowthFormIds = new List<string> { TidelingGrowthForms.OriginalFormId }
            };
        }

        private static void AssertSaveMatchesExpected(SaveData actual, string playerSpeciesId, int expectedPoints)
        {
            SaveData expected = CreateVerificationSave(playerSpeciesId);
            TidelingLevelProgression.AddProgress(expected.caught[0], expectedPoints);
            if (!string.Equals(JsonUtility.ToJson(actual), JsonUtility.ToJson(expected), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Contest progress changed unrelated save data.");
            }
        }

        private static MethodInfo FindControllerMethod(string methodName, params Type[] argumentTypes)
        {
            MethodInfo method = typeof(ContestFlowController).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                argumentTypes,
                null);
            return method ?? throw new MissingMethodException(typeof(ContestFlowController).Name, methodName);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }

            field.SetValue(target, value);
        }

        private static void SetAutoProperty(object target, string propertyName, object value)
        {
            SetPrivateField(target, $"<{propertyName}>k__BackingField", value);
        }

        private static void SetStaticAutoProperty(Type targetType, string propertyName, object value)
        {
            FieldInfo field = targetType.GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(targetType.Name, propertyName);
            }

            field.SetValue(null, value);
        }
    }
}
