using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using RobotsGame.Core;
using RobotsGame.Data;
using RobotsGame.Managers;

namespace RobotsGame.Tests
{
    public class GameManagerVoteResetTests
    {
        private GameObject testObject;
        private GameManager gameManager;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("GameManagerTestObject");
            gameManager = testObject.AddComponent<GameManager>();

            // Seed with a single player so score containers are created
            gameManager.Players.Add(new Player("Player One", "icon1"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testObject);
            var instanceField = typeof(GameManager).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            instanceField?.SetValue(null, null);
        }

        [Test]
        public void StartNextRound_ClearsVotesAndEliminationState()
        {
            var initialQuestion = new Question(
                "q0",
                "Who is the robot?",
                "Correct",
                "Robot",
                GameConstants.QuestionType.Text,
                0
            );

            // Begin the first round so vote dictionaries are initialized
            gameManager.StartNextRound(initialQuestion);

            gameManager.RecordEliminationVote("Player One", "Answer A");
            gameManager.RecordFinalVote("Player One", "Answer B");

            var nextQuestion = new Question(
                "q1",
                "Who is the imposter?",
                "Correct",
                "Robot",
                GameConstants.QuestionType.Text,
                1
            );
            nextQuestion.AddEliminatedAnswer("Stale Answer");

            gameManager.StartNextRound(nextQuestion);

            Assert.IsNull(gameManager.GetPlayerEliminationVote("Player One"), "Elimination vote should be cleared between rounds.");
            Assert.IsNull(gameManager.GetPlayerFinalVote("Player One"), "Final vote should be cleared between rounds.");
            Assert.IsEmpty(nextQuestion.EliminatedAnswers, "Eliminated answers should be cleared for the new round question.");
        }
    }
}
