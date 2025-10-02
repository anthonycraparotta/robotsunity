using System.Collections.Generic;
using System.Linq;
using RobotsGame.Data;

namespace RobotsGame.Managers
{
    /// <summary>
    /// Manages player icon assignment and availability.
    /// Based on screenspec.md icon selection logic (20 available icons).
    /// </summary>
    public static class IconManager
    {
        // ===========================
        // CONSTANTS
        // ===========================

        public const int TOTAL_ICONS = 20;
        public const int MIN_ICON_NUMBER = 1;
        public const int MAX_ICON_NUMBER = 20;

        // ===========================
        // ICON AVAILABILITY
        // ===========================

        /// <summary>
        /// Get list of all available icon keys (icon1 through icon20).
        /// </summary>
        public static List<string> GetAllIconKeys()
        {
            List<string> icons = new List<string>();
            for (int i = MIN_ICON_NUMBER; i <= MAX_ICON_NUMBER; i++)
            {
                icons.Add($"icon{i}");
            }
            return icons;
        }

        /// <summary>
        /// Get list of available (unassigned) icons.
        /// </summary>
        public static List<string> GetAvailableIcons(List<Player> existingPlayers)
        {
            if (existingPlayers == null || existingPlayers.Count == 0)
                return GetAllIconKeys();

            HashSet<string> takenIcons = new HashSet<string>(
                existingPlayers.Where(p => p.icon != null).Select(p => p.icon)
            );

            return GetAllIconKeys().Where(icon => !takenIcons.Contains(icon)).ToList();
        }

        /// <summary>
        /// Check if specific icon is available (not taken).
        /// </summary>
        public static bool IsIconAvailable(string iconKey, List<Player> players)
        {
            if (string.IsNullOrEmpty(iconKey))
                return false;

            if (!ValidateIconKey(iconKey))
                return false;

            if (players == null || players.Count == 0)
                return true;

            return !players.Any(p => p.icon == iconKey);
        }

        /// <summary>
        /// Get next available icon (first unassigned icon).
        /// Returns null if all icons are taken.
        /// </summary>
        public static string GetNextAvailableIcon(List<Player> players)
        {
            List<string> available = GetAvailableIcons(players);
            return available.FirstOrDefault();
        }

        /// <summary>
        /// Get taken (assigned) icons.
        /// </summary>
        public static List<string> GetTakenIcons(List<Player> players)
        {
            if (players == null || players.Count == 0)
                return new List<string>();

            return players.Where(p => p.icon != null).Select(p => p.icon).ToList();
        }

        // ===========================
        // VALIDATION
        // ===========================

        /// <summary>
        /// Validate icon key is in valid range (icon1-icon20).
        /// </summary>
        public static bool ValidateIconKey(string iconKey)
        {
            if (string.IsNullOrEmpty(iconKey))
                return false;

            if (!iconKey.StartsWith("icon"))
                return false;

            string numberPart = iconKey.Substring(4);
            if (!int.TryParse(numberPart, out int iconNumber))
                return false;

            return iconNumber >= MIN_ICON_NUMBER && iconNumber <= MAX_ICON_NUMBER;
        }

        /// <summary>
        /// Check if room is full (all icons assigned).
        /// </summary>
        public static bool IsRoomFull(List<Player> players)
        {
            return players != null && players.Count >= TOTAL_ICONS;
        }

        /// <summary>
        /// Get number of available slots.
        /// </summary>
        public static int GetAvailableSlots(List<Player> players)
        {
            int currentPlayers = players?.Count ?? 0;
            return Mathf.Max(0, TOTAL_ICONS - currentPlayers);
        }

        // ===========================
        // ICON MANAGEMENT
        // ===========================

        /// <summary>
        /// Auto-assign icon to player if their selected icon is taken.
        /// Returns the icon key (original or alternative).
        /// </summary>
        public static string AutoAssignIcon(string preferredIcon, List<Player> players)
        {
            // If preferred icon is available, use it
            if (IsIconAvailable(preferredIcon, players))
                return preferredIcon;

            // Otherwise, get next available icon
            string availableIcon = GetNextAvailableIcon(players);

            if (availableIcon == null)
            {
                UnityEngine.Debug.LogWarning("No available icons - room is full");
                return null;
            }

            return availableIcon;
        }

        /// <summary>
        /// Get player by icon key.
        /// </summary>
        public static Player GetPlayerByIcon(string iconKey, List<Player> players)
        {
            if (players == null || string.IsNullOrEmpty(iconKey))
                return null;

            return players.FirstOrDefault(p => p.icon == iconKey);
        }

        /// <summary>
        /// Get icon number from icon key.
        /// Example: "icon5" -> 5
        /// </summary>
        public static int GetIconNumber(string iconKey)
        {
            if (string.IsNullOrEmpty(iconKey) || !iconKey.StartsWith("icon"))
                return -1;

            string numberPart = iconKey.Substring(4);
            if (int.TryParse(numberPart, out int iconNumber))
                return iconNumber;

            return -1;
        }

        /// <summary>
        /// Get icon key from number.
        /// Example: 5 -> "icon5"
        /// </summary>
        public static string GetIconKey(int iconNumber)
        {
            if (iconNumber < MIN_ICON_NUMBER || iconNumber > MAX_ICON_NUMBER)
                return null;

            return $"icon{iconNumber}";
        }

        // ===========================
        // ICON RESOURCES
        // ===========================

        /// <summary>
        /// Get icon sprite path for Resources.Load.
        /// Example: "icon5" -> "Icons/icon5"
        /// </summary>
        public static string GetIconResourcePath(string iconKey)
        {
            if (string.IsNullOrEmpty(iconKey))
                return null;

            return $"Icons/{iconKey}";
        }

        /// <summary>
        /// Get icon sprite from Resources.
        /// </summary>
        public static UnityEngine.Sprite GetIconSprite(string iconKey)
        {
            string path = GetIconResourcePath(iconKey);
            if (path == null)
                return null;

            return UnityEngine.Resources.Load<UnityEngine.Sprite>(path);
        }
    }
}
