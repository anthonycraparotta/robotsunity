namespace RobotsGame.Core
{
    /// <summary>
    /// Maps round numbers to asset file names for backgrounds, foregrounds, and other round-specific assets.
    /// Based on screenspec.md asset organization and naming conventions.
    /// </summary>
    public static class RoundAssetMapper
    {
        // ===========================
        // BACKGROUND ASSETS
        // ===========================

        /// <summary>
        /// Get background asset name for round.
        /// Examples: "Q1BG.png", "Q2BG.png", etc.
        /// </summary>
        public static string GetBackgroundAsset(int round)
        {
            if (round < 1 || round > 12)
                return null;

            return $"Q{round}BG";
        }

        /// <summary>
        /// Get background resource path for Resources.Load.
        /// </summary>
        public static string GetBackgroundResourcePath(int round)
        {
            string assetName = GetBackgroundAsset(round);
            if (assetName == null)
                return null;

            return $"Backgrounds/{assetName}";
        }

        // ===========================
        // FOREGROUND ASSETS
        // ===========================

        /// <summary>
        /// Get foreground asset name for round.
        /// Examples: "Q1FG.png", "Q2FG.png", etc.
        /// </summary>
        public static string GetForegroundAsset(int round)
        {
            if (round < 1 || round > 12)
                return null;

            return $"Q{round}FG";
        }

        /// <summary>
        /// Get foreground resource path for Resources.Load.
        /// </summary>
        public static string GetForegroundResourcePath(int round)
        {
            string assetName = GetForegroundAsset(round);
            if (assetName == null)
                return null;

            return $"Foregrounds/{assetName}";
        }

        /// <summary>
        /// Get foreground blink variant asset name for round.
        /// Examples: "Q1FG-blink.png", "Q2FG-blink.png", etc.
        /// </summary>
        public static string GetBlinkForegroundAsset(int round)
        {
            if (round < 1 || round > 12)
                return null;

            return $"Q{round}FG-blink";
        }

        /// <summary>
        /// Get foreground blink resource path for Resources.Load.
        /// </summary>
        public static string GetBlinkForegroundResourcePath(int round)
        {
            string assetName = GetBlinkForegroundAsset(round);
            if (assetName == null)
                return null;

            return $"Foregrounds/{assetName}";
        }

        // ===========================
        // ROUND ART SCREENS
        // ===========================

        /// <summary>
        /// Get round art screen asset for desktop.
        /// Examples: "round1art.png", "round2art.png", etc.
        /// Special cases: "picqround8art.png" (8-round mode final), "picqround12art.png" (12-round mode final)
        /// </summary>
        public static string GetRoundArtAsset(int round, int totalRounds)
        {
            // Picture question rounds (final round)
            if (HalftimeDetector.IsPictureQuestionRound(round, totalRounds))
            {
                return $"picqround{totalRounds}art";
            }

            if (round < 1 || round > 12)
                return null;

            return $"round{round}art";
        }

        /// <summary>
        /// Get round art resource path for Resources.Load.
        /// </summary>
        public static string GetRoundArtResourcePath(int round, int totalRounds)
        {
            string assetName = GetRoundArtAsset(round, totalRounds);
            if (assetName == null)
                return null;

            return $"RoundArt/{assetName}";
        }

        /// <summary>
        /// Get mobile round art asset (repeating pattern every 4 rounds).
        /// Rounds 1,5,9 -> roundart1mobile.png
        /// Rounds 2,6,10 -> roundart2mobile.png
        /// Rounds 3,7,11 -> roundart3mobile.png
        /// Rounds 4,8,12 -> roundart4mobile.png
        /// </summary>
        public static string GetMobileRoundArtAsset(int round)
        {
            if (round < 1)
                return null;

            int patternIndex = ((round - 1) % 4) + 1;
            return $"roundart{patternIndex}mobile";
        }

        /// <summary>
        /// Get mobile round art resource path for Resources.Load.
        /// </summary>
        public static string GetMobileRoundArtResourcePath(int round)
        {
            string assetName = GetMobileRoundArtAsset(round);
            if (assetName == null)
                return null;

            return $"Mobile/RoundArt/{assetName}";
        }

        // ===========================
        // ROUND HEADERS
        // ===========================

        /// <summary>
        /// Get round header asset name.
        /// Examples: "round1header.png", "round2header.png", etc.
        /// </summary>
        public static string GetRoundHeaderAsset(int round)
        {
            if (round < 1 || round > 12)
                return null;

            return $"round{round}header";
        }

        /// <summary>
        /// Get round header resource path for Resources.Load.
        /// </summary>
        public static string GetRoundHeaderResourcePath(int round)
        {
            string assetName = GetRoundHeaderAsset(round);
            if (assetName == null)
                return null;

            return $"Headers/{assetName}";
        }

        // ===========================
        // RESULTS ASSETS
        // ===========================

        /// <summary>
        /// Get results header asset for round.
        /// Examples: "resultsheader1.png", "resultsheader2.png", etc.
        /// </summary>
        public static string GetResultsHeaderAsset(int round)
        {
            if (round < 1 || round > 12)
                return null;

            return $"resultsheader{round}";
        }

        /// <summary>
        /// Get results header resource path for Resources.Load.
        /// </summary>
        public static string GetResultsHeaderResourcePath(int round)
        {
            string assetName = GetResultsHeaderAsset(round);
            if (assetName == null)
                return null;

            return $"Results/{assetName}";
        }

        // ===========================
        // ELIMINATION/VOTING BACKGROUNDS
        // ===========================

        /// <summary>
        /// Get elimination screen background asset.
        /// Examples: "eliminationbg1.png", "eliminationbg2.png", etc.
        /// </summary>
        public static string GetEliminationBackgroundAsset(int round)
        {
            if (round < 1 || round > 12)
                return null;

            return $"eliminationbg{round}";
        }

        /// <summary>
        /// Get voting screen background asset.
        /// Examples: "votingbg1.png", "votingbg2.png", etc.
        /// </summary>
        public static string GetVotingBackgroundAsset(int round)
        {
            if (round < 1 || round > 12)
                return null;

            return $"votingbg{round}";
        }

        // ===========================
        // MOBILE BACKGROUNDS
        // ===========================

        /// <summary>
        /// Get mobile elimination background asset.
        /// Examples: "eliminationmobile1.png", etc.
        /// </summary>
        public static string GetMobileEliminationBackgroundAsset(int round)
        {
            if (round < 1)
                return null;

            int patternIndex = ((round - 1) % 4) + 1;
            return $"eliminationmobile{patternIndex}";
        }

        /// <summary>
        /// Get mobile voting background asset.
        /// Examples: "votingmobile1.png", etc.
        /// </summary>
        public static string GetMobileVotingBackgroundAsset(int round)
        {
            if (round < 1)
                return null;

            int patternIndex = ((round - 1) % 4) + 1;
            return $"votingmobile{patternIndex}";
        }

        // ===========================
        // VALIDATION
        // ===========================

        /// <summary>
        /// Validate round number is within valid range.
        /// </summary>
        public static bool IsValidRound(int round)
        {
            return round >= 1 && round <= 12;
        }

        /// <summary>
        /// Get all asset names for a specific round (for preloading).
        /// </summary>
        public static string[] GetAllRoundAssets(int round, int totalRounds)
        {
            if (!IsValidRound(round))
                return new string[0];

            return new string[]
            {
                GetBackgroundAsset(round),
                GetForegroundAsset(round),
                GetBlinkForegroundAsset(round),
                GetRoundArtAsset(round, totalRounds),
                GetRoundHeaderAsset(round),
                GetResultsHeaderAsset(round),
                GetEliminationBackgroundAsset(round),
                GetVotingBackgroundAsset(round)
            };
        }
    }
}
