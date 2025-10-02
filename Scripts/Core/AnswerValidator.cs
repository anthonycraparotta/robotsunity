using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace RobotsGame.Core
{
    /// <summary>
    /// Validates player answers against duplicates, correct answers, and profanity.
    /// Based on unityspec.md validation thresholds and edge cases.
    /// </summary>
    public static class AnswerValidator
    {
        private static HashSet<string> bannedWords;
        private static bool bannedWordsLoaded = false;

        // ===========================
        // SIMILARITY CHECKING
        // ===========================

        /// <summary>
        /// Check if answer is too similar to correct answer (85% similarity OR within 1 character)
        /// </summary>
        public static bool IsCorrectAnswer(string playerAnswer, string correctAnswer)
        {
            if (string.IsNullOrEmpty(playerAnswer) || string.IsNullOrEmpty(correctAnswer))
                return false;

            string normalized1 = NormalizeText(playerAnswer);
            string normalized2 = NormalizeText(correctAnswer);

            // Exact match
            if (normalized1 == normalized2)
                return true;

            // Within 1 character difference
            if (GetLevenshteinDistance(normalized1, normalized2) <= GameConstants.Validation.CorrectAnswerMaxDifference)
                return true;

            // 85% similarity
            float similarity = CalculateSimilarity(normalized1, normalized2);
            return similarity >= GameConstants.Validation.CorrectAnswerSimilarity;
        }

        /// <summary>
        /// Check if answer is duplicate of another player's answer (90% similarity OR within 1 character)
        /// </summary>
        public static bool IsDuplicate(string answer1, string answer2)
        {
            if (string.IsNullOrEmpty(answer1) || string.IsNullOrEmpty(answer2))
                return false;

            string normalized1 = NormalizeText(answer1);
            string normalized2 = NormalizeText(answer2);

            // Exact match
            if (normalized1 == normalized2)
                return true;

            // Within 1 character difference
            if (GetLevenshteinDistance(normalized1, normalized2) <= GameConstants.Validation.DuplicateAnswerMaxDifference)
                return true;

            // 90% similarity
            float similarity = CalculateSimilarity(normalized1, normalized2);
            return similarity >= GameConstants.Validation.DuplicateAnswerSimilarity;
        }

        /// <summary>
        /// Check if answer contains profanity or inappropriate content
        /// </summary>
        public static bool ContainsProfanity(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            LoadBannedWords();

            string normalized = NormalizeProfanityText(text);

            foreach (string bannedWord in bannedWords)
            {
                if (!string.IsNullOrEmpty(bannedWord) && normalized.Contains(bannedWord))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if answer meets minimum length requirement
        /// </summary>
        public static bool IsValidLength(string text)
        {
            return !string.IsNullOrEmpty(text) && text.Trim().Length >= GameConstants.Validation.MinimumAnswerLength;
        }

        // ===========================
        // TEXT NORMALIZATION
        // ===========================

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // Convert to lowercase, trim whitespace, remove extra spaces
            return text.ToLower().Trim().Replace("  ", " ");
        }

        // ===========================
        // SIMILARITY ALGORITHMS
        // ===========================

        /// <summary>
        /// Calculate similarity ratio between 0.0 and 1.0
        /// Uses Levenshtein distance normalized to string length
        /// </summary>
        private static float CalculateSimilarity(string s1, string s2)
        {
            if (s1 == s2) return 1.0f;
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0f;

            int distance = GetLevenshteinDistance(s1, s2);
            int maxLength = Mathf.Max(s1.Length, s2.Length);

            return 1.0f - ((float)distance / maxLength);
        }

        /// <summary>
        /// Calculate Levenshtein distance (edit distance) between two strings
        /// </summary>
        private static int GetLevenshteinDistance(string s1, string s2)
        {
            int[,] matrix = new int[s1.Length + 1, s2.Length + 1];

            // Initialize first column and row
            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            // Calculate distances
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                    matrix[i, j] = Mathf.Min(
                        Mathf.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost
                    );
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        // ===========================
        // BANNED WORDS LOADING
        // ===========================

        private static void LoadBannedWords()
        {
            if (bannedWordsLoaded)
                return;

            bannedWords = new HashSet<string>();

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string dataDirectory = Path.Combine(projectRoot, "data");

            string[] candidateFiles = new string[]
            {
                Path.Combine(dataDirectory, "banned_words.txt"),
                Path.Combine(dataDirectory, "bannedwords.txt"),
                Path.Combine(dataDirectory, "bannedwords.json"),
                Path.Combine(dataDirectory, "bannedwords.js"),
            };

            bool loaded = false;

            foreach (string candidate in candidateFiles)
            {
                if (TryLoadBannedWordsFromFile(candidate))
                {
                    loaded = true;
                    break;
                }
            }

            if (!loaded)
            {
                Debug.LogWarning("Banned words file not found. Profanity filter disabled.");
            }

            bannedWordsLoaded = true;
        }

        private static bool TryLoadBannedWordsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                IEnumerable<string> words;

                if (extension == ".js" || extension == ".json")
                {
                    string content = File.ReadAllText(filePath);
                    words = ExtractWordsFromStructuredContent(content);
                }
                else
                {
                    words = File.ReadAllLines(filePath);
                }

                int added = 0;
                foreach (string word in words)
                {
                    if (string.IsNullOrWhiteSpace(word))
                        continue;

                    if (extension != ".js" && extension != ".json")
                    {
                        string trimmed = word.Trim();
                        if (trimmed.StartsWith("#"))
                            continue;
                    }

                    string normalized = NormalizeProfanityText(word);
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        bannedWords.Add(normalized);
                        added++;
                    }
                }

                Debug.Log($"Loaded {bannedWords.Count} banned words from {filePath}");
                return added > 0;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load banned words from {filePath}: {e.Message}");
            }

            return false;
        }

        /// <summary>
        /// Force reload banned words (useful for testing)
        /// </summary>
        public static void ReloadBannedWords()
        {
            bannedWordsLoaded = false;
            LoadBannedWords();
        }

        private static IEnumerable<string> ExtractWordsFromStructuredContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                yield break;

            MatchCollection matches = Regex.Matches(content, "'([^']*)'|\"([^\"]*)\"");

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        yield return value;
                    }
                }
            }
        }

        private static string NormalizeProfanityText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            string normalized = text.ToLowerInvariant().Normalize(NormalizationForm.FormKD);
            StringBuilder builder = new StringBuilder(normalized.Length);

            foreach (char c in normalized)
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
