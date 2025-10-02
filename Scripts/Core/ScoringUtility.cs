using UnityEngine;

namespace RobotsGame.Core
{
    /// <summary>
    /// Utility functions for score calculations and formatting.
    /// Based on screenspec.md scoring rules for 8-round and 12-round game modes.
    /// </summary>
    public static class ScoringUtility
    {
        // ===========================
        // POINT VALUES
        // ===========================

        /// <summary>
        /// Get base points for correct answer based on game mode.
        /// 8-round: 8 points | 12-round: 6 points
        /// </summary>
        public static int GetBasePoints(int totalRounds)
        {
            return totalRounds == 8 ? 8 : 6;
        }

        /// <summary>
        /// Get half points (for robot identified, votes received) based on game mode.
        /// 8-round: 4 points | 12-round: 3 points
        /// </summary>
        public static int GetHalfPoints(int totalRounds)
        {
            return totalRounds == 8 ? 4 : 3;
        }

        /// <summary>
        /// Get penalty points (fooled by robot) based on game mode.
        /// 8-round: -8 points | 12-round: -6 points
        /// </summary>
        public static int GetPenaltyPoints(int totalRounds)
        {
            return totalRounds == 8 ? -8 : -6;
        }

        /// <summary>
        /// Get double points for picture questions (worth 2x).
        /// 8-round: 16 points | 12-round: 12 points
        /// </summary>
        public static int GetDoublePoints(int totalRounds)
        {
            return GetBasePoints(totalRounds) * 2;
        }

        /// <summary>
        /// Get bonus question points based on game mode.
        /// 8-round: 4 points per vote | 12-round: 3 points per vote
        /// </summary>
        public static int GetBonusQuestionPoints(int totalRounds)
        {
            return GetHalfPoints(totalRounds);
        }

        // ===========================
        // FORMATTING
        // ===========================

        /// <summary>
        /// Convert raw points to percentage string format.
        /// Positive: "+X%" | Negative: "-X%" | Zero: "0%"
        /// </summary>
        public static string PointsToPercentage(int points)
        {
            if (points > 0)
                return $"+{points}%";
            else if (points < 0)
                return $"{points}%";
            else
                return "0%";
        }

        /// <summary>
        /// Format score for display with proper sign and percentage.
        /// Example: 12 → "+12%" | -8 → "-8%" | 0 → "0%"
        /// </summary>
        public static string FormatScore(int points)
        {
            return PointsToPercentage(points);
        }

        /// <summary>
        /// Format score change for display (always shows sign).
        /// Example: 8 → "+8" | -4 → "-4" | 0 → "+0"
        /// </summary>
        public static string FormatScoreChange(int points)
        {
            if (points >= 0)
                return $"+{points}";
            else
                return points.ToString();
        }

        /// <summary>
        /// Get color for score display based on positive/negative value.
        /// Positive: #11ffce (green) | Negative: #fe1d4a (red) | Zero: #fffbbc (cream)
        /// </summary>
        public static Color GetScoreColor(int points)
        {
            if (points > 0)
                return GameConstants.Colors.BrightGreen; // #11ffce
            else if (points < 0)
                return GameConstants.Colors.BrightRed; // #fe1d4a
            else
                return GameConstants.Colors.OffWhite; // #fffbbc
        }

        // ===========================
        // CALCULATIONS
        // ===========================

        /// <summary>
        /// Calculate total round score for a player.
        /// </summary>
        public static int CalculateRoundScore(
            bool answeredCorrectly,
            bool identifiedRobot,
            int votesReceived,
            bool fooledByRobot,
            bool isPictureQuestion,
            int totalRounds)
        {
            int score = 0;

            // Correct answer points (doubled for picture questions)
            if (answeredCorrectly)
            {
                score += isPictureQuestion ? GetDoublePoints(totalRounds) : GetBasePoints(totalRounds);
            }

            // Robot identified points
            if (identifiedRobot)
            {
                score += GetHalfPoints(totalRounds);
            }

            // Votes received points
            score += votesReceived * GetHalfPoints(totalRounds);

            // Fooled by robot penalty
            if (fooledByRobot)
            {
                score += GetPenaltyPoints(totalRounds);
            }

            return score;
        }

        /// <summary>
        /// Calculate bonus round score for a player.
        /// Points earned for each vote received.
        /// </summary>
        public static int CalculateBonusScore(int votesReceived, int totalRounds)
        {
            return votesReceived * GetBonusQuestionPoints(totalRounds);
        }

        /// <summary>
        /// Get placement suffix for display (1st, 2nd, 3rd, 4th, etc.).
        /// </summary>
        public static string GetPlacementSuffix(int placement)
        {
            if (placement <= 0) return "";

            string suffix;
            switch (placement % 10)
            {
                case 1 when placement % 100 != 11:
                    suffix = "st";
                    break;
                case 2 when placement % 100 != 12:
                    suffix = "nd";
                    break;
                case 3 when placement % 100 != 13:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }

            return $"{placement}{suffix}";
        }

        /// <summary>
        /// Format placement with total players (e.g., "1st place of 5 players").
        /// </summary>
        public static string FormatPlacement(int placement, int totalPlayers)
        {
            string placementText = GetPlacementSuffix(placement);
            string playerText = totalPlayers == 1 ? "player" : "players";
            return $"{placementText} place of {totalPlayers} {playerText}";
        }
    }
}
