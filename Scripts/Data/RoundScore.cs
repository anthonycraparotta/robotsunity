using System;
using UnityEngine;

namespace RobotsGame.Data
{
    /// <summary>
    /// Tracks detailed scoring breakdown for a single round.
    /// Based on unityspec.md DATA STRUCTURES section.
    /// </summary>
    [Serializable]
    public class RoundScore
    {
        [SerializeField] private string playerName;
        [SerializeField] private int total;
        [SerializeField] private int correctAnswerPoints;
        [SerializeField] private int robotIdentifiedPoints;
        [SerializeField] private int votesReceivedCount;
        [SerializeField] private int votesReceivedPoints;
        [SerializeField] private int fooledPoints;
        [SerializeField] private string icon;

        public string PlayerName => playerName;
        public int Total => total;
        public int CorrectAnswerPoints => correctAnswerPoints;
        public int RobotIdentifiedPoints => robotIdentifiedPoints;
        public int VotesReceivedCount => votesReceivedCount;
        public int VotesReceivedPoints => votesReceivedPoints;
        public int FooledPoints => fooledPoints;
        public string Icon => icon;

        public RoundScore(string playerName, string icon)
        {
            this.playerName = playerName;
            this.icon = icon;
            this.total = 0;
            this.correctAnswerPoints = 0;
            this.robotIdentifiedPoints = 0;
            this.votesReceivedCount = 0;
            this.votesReceivedPoints = 0;
            this.fooledPoints = 0;
        }

        public void SetCorrectAnswer(int points)
        {
            correctAnswerPoints = points;
            RecalculateTotal();
        }

        public void SetRobotIdentified(int points)
        {
            robotIdentifiedPoints = points;
            RecalculateTotal();
        }

        public void AddVoteReceived(int pointsPerVote)
        {
            votesReceivedCount++;
            votesReceivedPoints += pointsPerVote;
            RecalculateTotal();
        }

        public void SetFooled(int penaltyPoints)
        {
            fooledPoints = penaltyPoints; // Already negative
            RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            total = correctAnswerPoints + robotIdentifiedPoints + votesReceivedPoints + fooledPoints;
        }

        public bool HasPositiveScore() => total > 0;
        public bool HasNegativeScore() => total < 0;
    }
}
