using System.Collections.Generic;
using System.Linq;
using RobotsGame.Data;

namespace RobotsGame.Core
{
    /// <summary>
    /// Calculates player rankings and placements based on scores.
    /// Based on screenspec.md placement logic for results screens.
    /// </summary>
    public static class PlacementCalculator
    {
        /// <summary>
        /// Result of placement calculation with rank and tie information.
        /// </summary>
        public class PlacementResult
        {
            public string PlayerName { get; set; }
            public int Score { get; set; }
            public int Placement { get; set; }
            public bool IsTied { get; set; }
            public string Icon { get; set; }
        }

        /// <summary>
        /// Calculate placements for all players, handling ties appropriately.
        /// Players with same score get same placement, next placement skips accordingly.
        /// Example: [100, 100, 80] → [1st, 1st, 3rd]
        /// </summary>
        public static List<PlacementResult> CalculatePlacements(List<Player> players)
        {
            if (players == null || players.Count == 0)
                return new List<PlacementResult>();

            // Sort by score descending
            var sortedPlayers = players.OrderByDescending(p => p.score).ToList();

            var results = new List<PlacementResult>();
            int currentPlacement = 1;
            int previousScore = int.MinValue;
            int tieCount = 0;

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                var player = sortedPlayers[i];
                bool isTied = false;

                if (player.score == previousScore)
                {
                    // Same score as previous player - tied for same placement
                    isTied = true;
                    tieCount++;
                    // Mark previous player as tied too
                    if (results.Count > 0)
                        results[results.Count - 1].IsTied = true;
                }
                else
                {
                    // Different score - advance placement by 1 + number of tied players
                    if (tieCount > 0)
                        currentPlacement += tieCount + 1;
                    tieCount = 0;
                }

                results.Add(new PlacementResult
                {
                    PlayerName = player.name,
                    Score = player.score,
                    Placement = currentPlacement,
                    IsTied = isTied,
                    Icon = player.icon
                });

                previousScore = player.score;
            }

            return results;
        }

        /// <summary>
        /// Get placement suffix for display (1st, 2nd, 3rd, 4th, etc.).
        /// </summary>
        public static string GetPlacementSuffix(int placement)
        {
            return ScoringUtility.GetPlacementSuffix(placement);
        }

        /// <summary>
        /// Get status message for mobile results screen based on placement.
        /// </summary>
        public static string GetStatusMessage(int placement, int totalPlayers)
        {
            if (totalPlayers == 1)
            {
                return "[You were the only player!]";
            }

            if (placement == 1)
            {
                return "[You were the MOST HUMAN]";
            }
            else if (placement == totalPlayers)
            {
                return "[You were the MOST ROBOT-LIKE]";
            }
            else if (placement == 2)
            {
                return "[You were the 2nd MOST HUMAN]";
            }
            else if (placement == 3)
            {
                return "[You were the 3rd MOST HUMAN]";
            }
            else
            {
                string suffix = GetPlacementSuffix(placement);
                return $"[You placed #{placement} out of {totalPlayers}]";
            }
        }

        /// <summary>
        /// Get winner (first place player).
        /// Returns null if no players.
        /// </summary>
        public static PlacementResult GetWinner(List<PlacementResult> placements)
        {
            return placements?.FirstOrDefault(p => p.Placement == 1);
        }

        /// <summary>
        /// Get loser (last place player).
        /// Returns null if no players or only one player.
        /// </summary>
        public static PlacementResult GetLoser(List<PlacementResult> placements)
        {
            if (placements == null || placements.Count <= 1)
                return null;

            return placements.OrderByDescending(p => p.Placement).FirstOrDefault();
        }

        /// <summary>
        /// Get all players tied for first place.
        /// </summary>
        public static List<PlacementResult> GetWinners(List<PlacementResult> placements)
        {
            return placements?.Where(p => p.Placement == 1).ToList() ?? new List<PlacementResult>();
        }

        /// <summary>
        /// Get all players tied for last place.
        /// </summary>
        public static List<PlacementResult> GetLosers(List<PlacementResult> placements)
        {
            if (placements == null || placements.Count == 0)
                return new List<PlacementResult>();

            int lastPlacement = placements.Max(p => p.Placement);
            return placements.Where(p => p.Placement == lastPlacement).ToList();
        }

        /// <summary>
        /// Check if player is in first place.
        /// </summary>
        public static bool IsWinner(PlacementResult placement)
        {
            return placement?.Placement == 1;
        }

        /// <summary>
        /// Check if player is in last place.
        /// </summary>
        public static bool IsLoser(PlacementResult placement, List<PlacementResult> allPlacements)
        {
            if (placement == null || allPlacements == null || allPlacements.Count <= 1)
                return false;

            int lastPlacement = allPlacements.Max(p => p.Placement);
            return placement.Placement == lastPlacement;
        }

        /// <summary>
        /// Get placement for specific player by name.
        /// </summary>
        public static PlacementResult GetPlayerPlacement(List<PlacementResult> placements, string playerName)
        {
            return placements?.FirstOrDefault(p => p.PlayerName == playerName);
        }
    }
}
