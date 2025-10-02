using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Data;
using RobotsGame.Managers;
using RobotsGame.Core;

namespace RobotsGame.UI
{
    /// <summary>
    /// Manages the player icon grid that shows when players submit answers.
    /// Based on unityspec.md QuestionScreen - Player Icons Response Zone.
    /// </summary>
    public class PlayerIconGrid : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject playerIconPrefab;
        [SerializeField] private Transform iconContainer;

        [Header("Icon Settings")]
        [SerializeField] private bool isPictureQuestion = false;
        [SerializeField] private float iconSizeLarge = 140f; // Text questions
        [SerializeField] private float iconSizeSmall = 70f; // Picture questions

        [Header("Animation Settings")]
        [SerializeField] private float popInDuration = 0.5f;

        private Dictionary<string, GameObject> playerIcons = new Dictionary<string, GameObject>();
        private List<string> playersWithAnswers = new List<string>();

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (iconContainer == null)
                iconContainer = transform;
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Set whether this is a picture question (affects icon size)
        /// </summary>
        public void SetPictureQuestion(bool isPicture)
        {
            isPictureQuestion = isPicture;
        }

        /// <summary>
        /// Show player icon when they submit an answer
        /// </summary>
        public void ShowPlayerIcon(Player player)
        {
            if (player == null) return;

            // Check if already shown
            if (playerIcons.ContainsKey(player.PlayerName))
                return;

            // Create icon GameObject
            GameObject iconObj;
            if (playerIconPrefab != null)
            {
                iconObj = Instantiate(playerIconPrefab, iconContainer);
            }
            else
            {
                iconObj = CreateDefaultPlayerIcon();
            }

            // Setup icon
            SetupPlayerIcon(iconObj, player);

            // Store reference
            playerIcons[player.PlayerName] = iconObj;
            playersWithAnswers.Add(player.PlayerName);

            // Animate in
            AnimateIconIn(iconObj);

            // Play sound
            AudioManager.Instance.PlayPlayerIconPop();
        }

        /// <summary>
        /// Check if all players have submitted
        /// </summary>
        public bool AllPlayersSubmitted(List<Player> allPlayers)
        {
            return playersWithAnswers.Count >= allPlayers.Count;
        }

        /// <summary>
        /// Get list of players who have submitted
        /// </summary>
        public List<string> GetPlayersWithAnswers()
        {
            return new List<string>(playersWithAnswers);
        }

        /// <summary>
        /// Clear all icons
        /// </summary>
        public void ClearIcons()
        {
            foreach (var icon in playerIcons.Values)
            {
                if (icon != null)
                    Destroy(icon);
            }

            playerIcons.Clear();
            playersWithAnswers.Clear();
        }

        // ===========================
        // ICON SETUP
        // ===========================

        private GameObject CreateDefaultPlayerIcon()
        {
            GameObject iconObj = new GameObject("PlayerIcon");
            iconObj.transform.SetParent(iconContainer, false);

            // Add RectTransform
            RectTransform rect = iconObj.AddComponent<RectTransform>();
            float size = isPictureQuestion ? iconSizeSmall : iconSizeLarge;
            rect.sizeDelta = new Vector2(size, size);

            // Add background circle
            Image bgImage = iconObj.AddComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.9f);
            bgImage.type = Image.Type.Filled;

            // Add icon image as child
            GameObject iconImageObj = new GameObject("Icon");
            iconImageObj.transform.SetParent(iconObj.transform, false);
            RectTransform iconRect = iconImageObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;
            Image iconImage = iconImageObj.AddComponent<Image>();

            // Add name label as child
            GameObject labelObj = new GameObject("NameLabel");
            labelObj.transform.SetParent(iconObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0, -5);
            labelRect.sizeDelta = new Vector2(80, 20);

            Image labelBg = labelObj.AddComponent<Image>();
            labelBg.color = new Color(0, 0, 0, 0.7f);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 12;
            labelText.color = GameConstants.Colors.Cream;

            return iconObj;
        }

        private void SetupPlayerIcon(GameObject iconObj, Player player)
        {
            // Find icon image component (prefer dedicated child named "Icon")
            Image iconImage = null;

            Transform iconTransform = iconObj.transform.Find("Icon");
            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }

            if (iconImage == null)
            {
                Image[] images = iconObj.GetComponentsInChildren<Image>(includeInactive: true);

                // Prefer an Image component whose transform name suggests it's the icon.
                foreach (var image in images)
                {
                    if (image == null || image.transform == iconObj.transform)
                        continue;

                    string imageName = image.transform.name;
                    if (!string.IsNullOrEmpty(imageName) && imageName.ToLower().Contains("icon"))
                    {
                        iconImage = image;
                        break;
                    }
                }

                // Next prefer direct children of the icon object so we don't grab background overlays or labels.
                if (iconImage == null)
                {
                    foreach (var image in images)
                    {
                        if (image == null || image.transform == iconObj.transform)
                            continue;

                        if (image.transform.parent == iconObj.transform)
                        {
                            iconImage = image;
                            break;
                        }
                    }
                }

                // Final fallback: just take the first non-root image.
                if (iconImage == null)
                {
                    foreach (var image in images)
                    {
                        if (image == null || image.transform == iconObj.transform)
                            continue;

                        iconImage = image;
                        break;
                    }
                }
            }

            if (iconImage != null)
            {
                // Load player icon sprite
                Sprite playerSprite = LoadPlayerIconSprite(player.Icon);
                if (playerSprite != null)
                {
                    iconImage.sprite = playerSprite;
                }
            }

            // Set name label
            TextMeshProUGUI nameText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = player.PlayerName.ToUpper();
            }

            // Set size based on question type
            RectTransform rect = iconObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                float size = isPictureQuestion ? iconSizeSmall : iconSizeLarge;
                rect.sizeDelta = new Vector2(size, size);
            }
        }

        private Sprite LoadPlayerIconSprite(string iconKey)
        {
            // Try to load from Resources
            string path = $"PlayerIcons/{iconKey}";
            Sprite sprite = Resources.Load<Sprite>(path);

            if (sprite == null)
            {
                Debug.LogWarning($"Could not load player icon: {path}");
            }

            return sprite;
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void AnimateIconIn(GameObject iconObj)
        {
            RectTransform rect = iconObj.GetComponent<RectTransform>();
            if (rect == null) return;

            // Start at scale 0
            rect.localScale = Vector3.zero;

            // Pop in with bounce
            rect.DOScale(Vector3.one, popInDuration)
                .SetEase(Ease.OutBack);
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            ClearIcons();
        }
    }
}
