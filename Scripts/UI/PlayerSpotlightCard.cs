using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Data;

namespace RobotsGame.UI
{
    /// <summary>
    /// Spotlight card showing winner or last place.
    /// Based on unityspec.md FinalResults Spotlight Card specifications.
    /// </summary>
    public class PlayerSpotlightCard : MonoBehaviour
    {
        public enum SpotlightType
        {
            Winner,
            LastPlace
        }

        [Header("UI References")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image cardBorder;
        [SerializeField] private Image playerIconImage;
        [SerializeField] private Image iconWrapper;
        [SerializeField] private TextMeshProUGUI badgeLabel;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI rankLabel;
        [SerializeField] private TextMeshProUGUI scoreLabel;
        [SerializeField] private TextMeshProUGUI scoreValue;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] private SpotlightType type = SpotlightType.Winner;
        [SerializeField] private float slideDistance = 30f;
        [SerializeField] private float animationDuration = 0.6f;

        private RectTransform rectTransform;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            // Start hidden
            canvasGroup.alpha = 0f;
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Setup and show card
        /// </summary>
        public void ShowCard(Player player, int rank, int totalPlayers, SpotlightType spotlightType, float delay = 0f)
        {
            type = spotlightType;

            // Setup colors and styling
            SetupCardStyle();

            // Setup content
            SetupContent(player, rank, totalPlayers);

            // Animate in
            DOVirtual.DelayedCall(delay, () =>
            {
                AnimateIn();
            });
        }

        // ===========================
        // SETUP
        // ===========================

        private void SetupCardStyle()
        {
            Color borderColor;
            Color glowColor;
            Color textColor;
            float scale;

            if (type == SpotlightType.Winner)
            {
                borderColor = GameConstants.Colors.BrightGreen;
                glowColor = new Color(17f/255f, 255f/255f, 206f/255f, 0.35f);
                textColor = GameConstants.Colors.BrightGreen;
                scale = 1.04f;
            }
            else
            {
                borderColor = GameConstants.Colors.BrightRed;
                glowColor = new Color(254f/255f, 29f/255f, 74f/255f, 0.35f);
                textColor = GameConstants.Colors.BrightRed;
                scale = 1f;
            }

            // Apply border color
            if (cardBorder != null)
            {
                cardBorder.color = borderColor;
            }

            // Apply scale
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * scale;
            }

            // Apply text colors
            if (playerNameText != null)
                playerNameText.color = textColor;

            if (scoreValue != null)
                scoreValue.color = textColor;

            if (iconWrapper != null)
                iconWrapper.GetComponent<Outline>()?.DOColor(borderColor, 0f);
        }

        private void SetupContent(Player player, int rank, int totalPlayers)
        {
            // Badge label
            if (badgeLabel != null)
            {
                badgeLabel.text = type == SpotlightType.Winner ? "WINNER" : "LAST PLACE";
            }

            // Player name
            if (playerNameText != null)
            {
                playerNameText.text = player.PlayerName.ToUpper();
            }

            // Subtitle
            if (subtitleText != null)
            {
                subtitleText.text = type == SpotlightType.Winner ? "MOST HUMAN" : "ROBOT IDENTIFIED";
            }

            // Rank
            if (rankLabel != null)
            {
                rankLabel.text = $"#{rank}";
            }

            // Score label
            if (scoreLabel != null)
            {
                scoreLabel.text = "SCORE";
            }

            // Score value (will animate)
            if (scoreValue != null)
            {
                scoreValue.text = "0";
            }

            // Load player icon
            if (playerIconImage != null)
            {
                Sprite iconSprite = Resources.Load<Sprite>($"PlayerIcons/{player.Icon}");
                if (iconSprite != null)
                    playerIconImage.sprite = iconSprite;
            }

            // Store final score for animation
            int finalScore = player.Score;
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void AnimateIn()
        {
            // Start position offset
            Vector2 startPos = rectTransform.anchoredPosition;
            startPos.y -= slideDistance;
            rectTransform.anchoredPosition = startPos;

            // Slide up and fade in
            Sequence seq = DOTween.Sequence();
            seq.Append(rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + slideDistance, animationDuration).SetEase(Ease.OutQuad));
            seq.Join(canvasGroup.DOFade(1f, animationDuration));

            // Animate score count-up
            seq.AppendCallback(() =>
            {
                AnimateScore();
            });
        }

        private void AnimateScore()
        {
            if (scoreValue == null) return;

            // Get player from GameManager
            Player player = null;
            foreach (var p in GameManager.Instance.Players)
            {
                if (p.PlayerName.ToUpper() == playerNameText.text)
                {
                    player = p;
                    break;
                }
            }

            if (player == null) return;

            // Count up from 0 to score
            DOVirtual.Float(0, player.Score, 1.5f, (value) =>
            {
                scoreValue.text = Mathf.RoundToInt(value).ToString();
            }).SetEase(Ease.OutQuad);
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            DOTween.Kill(rectTransform);
            DOTween.Kill(canvasGroup);
            DOTween.Kill(scoreValue);
        }
    }
}
