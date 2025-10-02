namespace RobotsGame.Core
{
    /// <summary>
    /// Detects halftime and bonus round transitions based on game mode and current round.
    /// Based on screenspec.md halftime and bonus round logic.
    /// </summary>
    public static class HalftimeDetector
    {
        // ===========================
        // HALFTIME DETECTION
        // ===========================

        /// <summary>
        /// Check if current round is halftime.
        /// 8-round mode: halftime at round 4
        /// 12-round mode: halftime at round 8
        /// </summary>
        public static bool IsHalftimeRound(int round, int totalRounds)
        {
            return round == GetHalftimeRound(totalRounds);
        }

        /// <summary>
        /// Get the halftime round number for game mode.
        /// 8-round mode: returns 4
        /// 12-round mode: returns 8
        /// </summary>
        public static int GetHalftimeRound(int totalRounds)
        {
            // Halftime occurs at 50% of total rounds
            return totalRounds / 2;
        }

        /// <summary>
        /// Check if halftime has passed.
        /// </summary>
        public static bool IsAfterHalftime(int round, int totalRounds)
        {
            return round > GetHalftimeRound(totalRounds);
        }

        /// <summary>
        /// Check if halftime has not yet occurred.
        /// </summary>
        public static bool IsBeforeHalftime(int round, int totalRounds)
        {
            return round < GetHalftimeRound(totalRounds);
        }

        // ===========================
        // BONUS ROUND DETECTION
        // ===========================

        /// <summary>
        /// Check if current round is the bonus round introduction.
        /// Bonus round occurs after halftime (round 4 for 8-game, round 8 for 12-game).
        /// </summary>
        public static bool IsBonusRoundIntro(int round, int totalRounds)
        {
            return round == GetBonusRoundNumber(totalRounds);
        }

        /// <summary>
        /// Get the bonus round number for game mode.
        /// Bonus round is the round immediately after halftime.
        /// 8-round mode: round 5 (after halftime at 4)
        /// 12-round mode: round 9 (after halftime at 8)
        /// </summary>
        public static int GetBonusRoundNumber(int totalRounds)
        {
            return GetHalftimeRound(totalRounds) + 1;
        }

        /// <summary>
        /// Check if bonus round has occurred.
        /// </summary>
        public static bool IsAfterBonusRound(int round, int totalRounds)
        {
            return round > GetBonusRoundNumber(totalRounds);
        }

        /// <summary>
        /// Check if bonus round has not yet occurred.
        /// </summary>
        public static bool IsBeforeBonusRound(int round, int totalRounds)
        {
            return round < GetBonusRoundNumber(totalRounds);
        }

        // ===========================
        // PICTURE QUESTION DETECTION
        // ===========================

        /// <summary>
        /// Check if current round is a picture question round.
        /// Picture questions occur on the final round.
        /// 8-round mode: round 8
        /// 12-round mode: round 12
        /// </summary>
        public static bool IsPictureQuestionRound(int round, int totalRounds)
        {
            return round == totalRounds;
        }

        /// <summary>
        /// Get the picture question round number.
        /// Returns the final round number (8 or 12).
        /// </summary>
        public static int GetPictureQuestionRound(int totalRounds)
        {
            return totalRounds;
        }

        // ===========================
        // GAME PHASE DETECTION
        // ===========================

        /// <summary>
        /// Get the current game phase description.
        /// </summary>
        public static string GetGamePhase(int round, int totalRounds)
        {
            if (round <= 0)
                return "Pre-Game";

            if (IsHalftimeRound(round, totalRounds))
                return "Halftime";

            if (IsBonusRoundIntro(round, totalRounds))
                return "Bonus Round";

            if (round == totalRounds)
                return "Final Round";

            if (round > totalRounds)
                return "Post-Game";

            if (IsBeforeHalftime(round, totalRounds))
                return "First Half";

            return "Second Half";
        }

        /// <summary>
        /// Check if game is complete (all rounds finished).
        /// </summary>
        public static bool IsGameComplete(int round, int totalRounds)
        {
            return round > totalRounds;
        }

        /// <summary>
        /// Get remaining rounds.
        /// </summary>
        public static int GetRemainingRounds(int round, int totalRounds)
        {
            return Mathf.Max(0, totalRounds - round);
        }
    }
}
