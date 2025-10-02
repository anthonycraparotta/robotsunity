using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Core;
using RobotsGame.Data;

namespace RobotsGame.UI.Results
{
    /// <summary>
    /// Panel 1: Shows correct answer and who got it right.
    /// Based on unityspec.md ResultsScreen Panel 1 specifications.
    /// </summary>
    public class CorrectAnswerPanel : ResultPanelBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI headlineText;
        [SerializeField] private TextMeshProUGUI answerText;
        [SerializeField] private Transform playerIconContainer;
        [SerializeField] private TextMeshProUGUI fallbackText;
        [SerializeField] private GameObject scoreCircle;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Settings")]
        [SerializeField] private GameObject playerIconPrefab;
        [SerializeField] private float iconCascadeDelay = 0.3f; // 300ms between icons
        [SerializeField] private float scoreCircleDelay = 1f; // After icons

        private List<GameObject> playerIcons = new List<GameObject>();

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Setup and show panel
        /// </summary>
        public void ShowPanel(string correctAnswer, List<Player> playersWhoGotIt, int pointsValue, System.Action onComplete = null)
        {
            // Setup texts
            if (headlineText != null)
                headlineText.text = "TRUE ANSWER";

            if (answerText != null)
                answerText.text = correctAnswer.ToUpper();

            // Setup icons or fallback
            if (playersWhoGotIt != null && playersWhoGotIt.Count > 0)
            {
                if (fallbackText != null)
                    fallbackText.gameObject.SetActive(false);

                SetupPlayerIcons(playersWhoGotIt);
            }
            else
            {
                if (fallbackText != null)
                {
                    fallbackText.text = GameConstants.FallbackText.NoTesters;
                    fallbackText.gameObject.SetActive(true);
                }

                if (playerIconContainer != null)
                    playerIconContainer.gameObject.SetActive(false);
            }

            // Setup score circle
            if (scoreText != null)
            {
                scoreText.text = $"+{pointsValue}";
            }

            if (scoreCircle != null)
                scoreCircle.SetActive(false);

            // Show panel
            Show(onComplete);

            // Cascade icons and score
            AnimateContent();
        }

        // ===========================
        // PLAYER ICONS
        // ===========================

        private void SetupPlayerIcons(List<Player> players)
        {
            ClearPlayerIcons();

            foreach (var player in players)
            {
                GameObject iconObj = CreatePlayerIcon(player);
                playerIcons.Add(iconObj);

                // Start hidden for cascade
                iconObj.transform.localScale = Vector3.zero;
                CanvasGroup cg = iconObj.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = iconObj.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
            }
        }

        private GameObject CreatePlayerIcon(Player player)
        {
            GameObject iconObj;

            if (playerIconPrefab != null)
            {
                iconObj = Instantiate(playerIconPrefab, playerIconContainer);
            }
            else
            {
                iconObj = new GameObject($"Icon_{player.PlayerName}");
                iconObj.transform.SetParent(playerIconContainer, false);

                RectTransform rect = iconObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(80, 80);

                Image img = iconObj.AddComponent<Image>();
                img.color = Color.white;

                // Load icon sprite
                Sprite iconSprite = Resources.Load<Sprite>($"PlayerIcons/{player.Icon}");
                if (iconSprite != null)
                    img.sprite = iconSprite;
            }

            return iconObj;
        }

        private void ClearPlayerIcons()
        {
            foreach (var icon in playerIcons)
            {
                if (icon != null)
                    Destroy(icon);
            }
            playerIcons.Clear();
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void AnimateContent()
        {
            // Wait for panel fade in, then cascade icons
            DOVirtual.DelayedCall(fadeInDuration + 0.5f, () =>
            {
                CascadePlayerIcons();
            });

            // Show score circle after icons
            DOVirtual.DelayedCall(fadeInDuration + 0.5f + (playerIcons.Count * iconCascadeDelay) + scoreCircleDelay, () =>
            {
                ShowScoreCircle();
            });
        }

        private void CascadePlayerIcons()
        {
            for (int i = 0; i < playerIcons.Count; i++)
            {
                GameObject icon = playerIcons[i];
                float delay = i * iconCascadeDelay;

                DOVirtual.DelayedCall(delay, () =>
                {
                    AnimateIconIn(icon);
                });
            }
        }

        private void AnimateIconIn(GameObject icon)
        {
            if (icon == null) return;

            CanvasGroup cg = icon.GetComponent<CanvasGroup>();

            // Bounce animation: 0 → 1.15 → 0.95 → 1
            Sequence seq = DOTween.Sequence();
            seq.Append(icon.transform.DOScale(1.15f, 0.25f).SetEase(Ease.OutCubic));
            seq.Append(icon.transform.DOScale(0.95f, 0.1f));
            seq.Append(icon.transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad));

            if (cg != null)
            {
                cg.DOFade(1f, 0.5f);
            }
        }

        private void ShowScoreCircle()
        {
            if (scoreCircle == null) return;

            scoreCircle.SetActive(true);
            scoreCircle.transform.localScale = Vector3.zero;

            scoreCircle.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }

        // ===========================
        // CLEANUP
        // ===========================
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearPlayerIcons();
        }
    }
}
