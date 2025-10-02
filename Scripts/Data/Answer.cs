using System;
using UnityEngine;
using RobotsGame.Core;

namespace RobotsGame.Data
{
    /// <summary>
    /// Represents a player's answer submission.
    /// Based on unityspec.md DATA STRUCTURES section.
    /// </summary>
    [Serializable]
    public class Answer
    {
        [SerializeField] private string text;
        [SerializeField] private GameConstants.AnswerType type;
        [SerializeField] private string playerName;

        public string Text => text;
        public GameConstants.AnswerType Type => type;
        public string PlayerName => playerName;

        public Answer(string text, GameConstants.AnswerType type, string playerName)
        {
            this.text = text;
            this.type = type;
            this.playerName = playerName;
        }

        public bool IsCorrect() => type == GameConstants.AnswerType.Correct;
        public bool IsRobot() => type == GameConstants.AnswerType.Robot;
        public bool IsPlayer() => type == GameConstants.AnswerType.Player;
    }
}
