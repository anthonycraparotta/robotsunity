using System.Collections.Generic;
using System.Linq;
using RobotsGame.Data;

namespace RobotsGame.Core
{
    /// <summary>
    /// Validates player names for lobby join.
    /// Based on screenspec.md name validation rules.
    /// </summary>
    public static class NameValidator
    {
        // ===========================
        // VALIDATION
        // ===========================

        /// <summary>
        /// Validate player name meets all requirements.
        /// Returns tuple (isValid, errorMessage)
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateName(string name, List<Player> existingPlayers = null)
        {
            // Check if empty or whitespace
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "Name cannot be empty");
            }

            // Trim whitespace
            name = name.Trim();

            // Check minimum length
            if (name.Length < 1)
            {
                return (false, "Name must be at least 1 character");
            }

            // Check maximum length
            if (name.Length > 20)
            {
                return (false, "Name must be 20 characters or less");
            }

            // Check for banned words/profanity
            if (ContainsBannedWords(name))
            {
                return (false, "INAPPROPRIATE CONTENT");
            }

            // Check for duplicate names
            if (existingPlayers != null && IsDuplicateName(name, existingPlayers))
            {
                return (false, "DUPLICATE NAME");
            }

            return (true, "");
        }

        /// <summary>
        /// Check if name is valid (quick check without error message).
        /// </summary>
        public static bool IsValidName(string name, List<Player> existingPlayers = null)
        {
            var (isValid, _) = ValidateName(name, existingPlayers);
            return isValid;
        }

        // ===========================
        // DUPLICATE DETECTION
        // ===========================

        /// <summary>
        /// Check if name is already taken by another player.
        /// Case-insensitive comparison.
        /// </summary>
        public static bool IsDuplicateName(string name, List<Player> players)
        {
            if (players == null || players.Count == 0)
                return false;

            string normalizedName = name.Trim().ToLower();

            return players.Any(p =>
                p.name != null &&
                p.name.Trim().ToLower() == normalizedName
            );
        }

        // ===========================
        // PROFANITY DETECTION
        // ===========================

        /// <summary>
        /// Check if name contains banned words or profanity.
        /// Uses AnswerValidator's banned words logic.
        /// </summary>
        public static bool ContainsBannedWords(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Reuse AnswerValidator's profanity detection
            return AnswerValidator.ContainsProfanity(name);
        }

        // ===========================
        // SANITIZATION
        // ===========================

        /// <summary>
        /// Sanitize name by trimming whitespace and limiting length.
        /// Does not check for banned words.
        /// </summary>
        public static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            name = name.Trim();

            // Limit to 20 characters
            if (name.Length > 20)
                name = name.Substring(0, 20);

            return name;
        }

        /// <summary>
        /// Suggest alternative name if duplicate.
        /// Appends number to make unique.
        /// </summary>
        public static string SuggestAlternativeName(string name, List<Player> players)
        {
            if (!IsDuplicateName(name, players))
                return name;

            string baseName = name;
            int counter = 2;

            // Try appending numbers until unique
            while (IsDuplicateName($"{baseName}{counter}", players) && counter < 100)
            {
                counter++;
            }

            return $"{baseName}{counter}";
        }

        // ===========================
        // FORMATTING
        // ===========================

        /// <summary>
        /// Format name for display (trim, proper casing if needed).
        /// </summary>
        public static string FormatNameForDisplay(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Unknown";

            return name.Trim();
        }
    }
}
