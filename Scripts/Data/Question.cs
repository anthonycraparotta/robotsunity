using System;
using System.Collections.Generic;
using UnityEngine;
using RobotsGame.Core;

namespace RobotsGame.Data
{
    /// <summary>
    /// Represents a game question with correct answer and robot decoy.
    /// Based on unityspec.md DATA STRUCTURES section.
    /// </summary>
    [Serializable]
    public class Question
    {
        [SerializeField] private string id;
        [SerializeField] private string questionText;
        [SerializeField] private string correctAnswer;
        [SerializeField] private string robotAnswer;
        [SerializeField] private GameConstants.QuestionType type;
        [SerializeField] private int round;
        [SerializeField] private string imageUrl;

        // Elimination tracking for voting phase
        [SerializeField] private List<string> eliminatedAnswers = new List<string>();

        public string Id => id;
        public string QuestionText => questionText;
        public string CorrectAnswer => correctAnswer;
        public string RobotAnswer => robotAnswer;
        public GameConstants.QuestionType Type => type;
        public int Round => round;
        public string ImageUrl => imageUrl;
        public List<string> EliminatedAnswers => eliminatedAnswers;

        public Question(string id, string questionText, string correctAnswer, string robotAnswer,
                       GameConstants.QuestionType type, int round, string imageUrl = "")
        {
            this.id = id;
            this.questionText = questionText;
            this.correctAnswer = correctAnswer;
            this.robotAnswer = robotAnswer;
            this.type = type;
            this.round = round;
            this.imageUrl = imageUrl;
            this.eliminatedAnswers = new List<string>();
        }

        public bool IsPictureQuestion() => type == GameConstants.QuestionType.Picture;

        /// <summary>
        /// Adds an answer to the elimination list.
        /// </summary>
        public void AddEliminatedAnswer(string answer)
        {
            if (!string.IsNullOrEmpty(answer) && !eliminatedAnswers.Contains(answer))
            {
                eliminatedAnswers.Add(answer);
            }
        }

        /// <summary>
        /// Checks if an answer has been eliminated.
        /// </summary>
        public bool IsAnswerEliminated(string answer)
        {
            return eliminatedAnswers.Contains(answer);
        }

        /// <summary>
        /// Gets all non-eliminated answers from a given list.
        /// </summary>
        public List<string> GetNonEliminatedAnswers(List<string> allAnswers)
        {
            List<string> nonEliminated = new List<string>();
            foreach (string answer in allAnswers)
            {
                if (!IsAnswerEliminated(answer))
                {
                    nonEliminated.Add(answer);
                }
            }
            return nonEliminated;
        }

        /// <summary>
        /// Clears all eliminated answers (for new round).
        /// </summary>
        public void ClearEliminatedAnswers()
        {
            eliminatedAnswers.Clear();
        }
    }
}
