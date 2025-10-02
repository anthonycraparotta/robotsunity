using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RobotsGame.Core;

namespace RobotsGame.Data
{
    /// <summary>
    /// Loads and manages questions from JSON data files.
    /// Based on screenspec.md question data structure and round logic.
    /// </summary>
    public class QuestionLoader : MonoBehaviour
    {
        [System.Serializable]
        public class QuestionsData
        {
            public List<Question> questions;
        }

        private static QuestionLoader instance;
        public static QuestionLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("QuestionLoader");
                    instance = go.AddComponent<QuestionLoader>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private List<Question> allQuestions;
        private bool isLoaded = false;

        // ===========================
        // LOADING
        // ===========================

        /// <summary>
        /// Load questions from JSON file in Resources folder.
        /// </summary>
        public void LoadQuestionsFromFile(string fileName = "questions")
        {
            TextAsset jsonFile = Resources.Load<TextAsset>($"Data/{fileName}");

            if (jsonFile == null)
            {
                Debug.LogError($"Failed to load questions file: {fileName}");
                allQuestions = new List<Question>();
                return;
            }

            QuestionsData data = JsonUtility.FromJson<QuestionsData>(jsonFile.text);
            allQuestions = data.questions ?? new List<Question>();
            isLoaded = true;

            Debug.Log($"Loaded {allQuestions.Count} questions from {fileName}");
        }

        /// <summary>
        /// Ensure questions are loaded before use.
        /// </summary>
        private void EnsureLoaded()
        {
            if (!isLoaded)
            {
                LoadQuestionsFromFile();
            }
        }

        // ===========================
        // QUESTION RETRIEVAL
        // ===========================

        /// <summary>
        /// Get question for specific round based on game mode.
        /// 8-round mode: rounds 1-8
        /// 12-round mode: rounds 1-12
        /// </summary>
        public Question GetQuestionForRound(int round, int totalRounds)
        {
            EnsureLoaded();

            // Find question matching this round number
            Question question = allQuestions.FirstOrDefault(q => q.round == round);

            if (question == null)
            {
                Debug.LogWarning($"No question found for round {round}. Using fallback.");
                question = CreateFallbackQuestion(round);
            }

            return question;
        }

        /// <summary>
        /// Get picture question for specific round (final round in each mode).
        /// 8-round mode: round 8
        /// 12-round mode: round 12
        /// </summary>
        public Question GetPictureQuestion(int round)
        {
            EnsureLoaded();

            Question question = allQuestions.FirstOrDefault(q =>
                q.round == round && q.type == "picture");

            if (question == null)
            {
                Debug.LogWarning($"No picture question found for round {round}");
                question = GetQuestionForRound(round, round); // Fallback to regular question
            }

            return question;
        }

        /// <summary>
        /// Get bonus round questions (3 questions for bonus round).
        /// </summary>
        public List<Question> GetBonusQuestions(int totalRounds)
        {
            EnsureLoaded();

            // Bonus questions are marked with round = 0 or specific bonus identifiers
            List<Question> bonusQuestions = allQuestions
                .Where(q => q.id != null && q.id.ToLower().Contains("bonus"))
                .Take(3)
                .ToList();

            if (bonusQuestions.Count < 3)
            {
                Debug.LogWarning($"Not enough bonus questions found. Found {bonusQuestions.Count}, need 3.");

                // Fill with fallback bonus questions
                while (bonusQuestions.Count < 3)
                {
                    bonusQuestions.Add(CreateFallbackBonusQuestion(bonusQuestions.Count + 1));
                }
            }

            return bonusQuestions;
        }

        /// <summary>
        /// Get all questions for a specific game mode.
        /// </summary>
        public List<Question> GetQuestionsForGameMode(int totalRounds)
        {
            EnsureLoaded();

            return allQuestions
                .Where(q => q.round > 0 && q.round <= totalRounds)
                .OrderBy(q => q.round)
                .ToList();
        }

        /// <summary>
        /// Check if round has a picture question.
        /// </summary>
        public bool IsPictureQuestionRound(int round, int totalRounds)
        {
            // Picture questions are typically the final round
            return (totalRounds == 8 && round == 8) || (totalRounds == 12 && round == 12);
        }

        // ===========================
        // FALLBACK QUESTIONS
        // ===========================

        /// <summary>
        /// Create fallback question if none found in data.
        /// </summary>
        private Question CreateFallbackQuestion(int round)
        {
            return new Question
            {
                id = $"fallback-{round}",
                question = "What is the best thing about being a robot?",
                correctAnswer = "Never getting tired",
                robotAnswer = "Beep boop beep",
                type = "text",
                round = round,
                imageUrl = null
            };
        }

        /// <summary>
        /// Create fallback bonus question.
        /// </summary>
        private Question CreateFallbackBonusQuestion(int number)
        {
            string[] bonusQuestions = new string[]
            {
                "What's the most creative use for a paperclip?",
                "If you could have dinner with any fictional character, who would it be?",
                "What's the strangest dream you've ever had?"
            };

            return new Question
            {
                id = $"bonus-fallback-{number}",
                question = bonusQuestions[Mathf.Min(number - 1, bonusQuestions.Length - 1)],
                correctAnswer = "",
                robotAnswer = "",
                type = "bonus",
                round = 0,
                imageUrl = null
            };
        }

        // ===========================
        // VALIDATION
        // ===========================

        /// <summary>
        /// Validate that all required questions exist for game mode.
        /// </summary>
        public bool ValidateQuestionsForGameMode(int totalRounds)
        {
            EnsureLoaded();

            for (int round = 1; round <= totalRounds; round++)
            {
                if (!allQuestions.Any(q => q.round == round))
                {
                    Debug.LogWarning($"Missing question for round {round}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get count of available questions.
        /// </summary>
        public int GetQuestionCount()
        {
            EnsureLoaded();
            return allQuestions.Count;
        }
    }
}
