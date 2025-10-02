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
    /// Panel 2: Shows robot answer and who was fooled vs not fooled.
    /// Based on unityspec.md ResultsScreen Panel 2 specifications.
    /// </summary>
    public class RobotDecoyPanel : ResultPanelBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI headlineText;
        [SerializeField] private TextMeshProUGUI decoyAnswerText;
        [SerializeField] private GameObject notFooledSection;
        [SerializeField] private TextMeshProUGUI notFooledLabel;
        [SerializeField] private Transform notFooledIconContainer;
        [SerializeField] private TextMeshProUGUI notFooledFallback;
        [SerializeField] private GameObject fooledSection;
        [SerializeField] private TextMeshProUGUI fooledLabel;
        [SerializeField] private Transform fooledIconContainer;
        [SerializeField] private TextMeshProUGUI fooledFallback;

        [Header("Settings")]
        [SerializeField] private GameObject playerIconPrefab;
        [SerializeField] private float iconCascadeDelay = 0.3f;
        [SerializeField] private float sectionDelay = 0.5f;

        private List<GameObject> notFooledIcons = new List<GameObject>();
        private List<GameObject> fooledIcons = new List<GameObject>();

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Setup and show panel
        /// </summary>
        public void ShowPanel(string robotAnswer, List<Player> notFooled, List<Player> fooled,
                             int notFooledPoints, int fooledPenalty, System.Action onComplete = null)
        {
            // Setup texts
            if (headlineText != null)
                headlineText.text = "ROBOT DECOY ANSWER";

            if (decoyAnswerText != null)
            {
                decoyAnswerText.text = robotAnswer.ToUpper();
            }

            // Setup not fooled section
            if (notFooledLabel != null)
                notFooledLabel.text = $"NOT FOOLED (+{notFooledPoints}):";

            SetupSection(notFooled, notFooledIconContainer, notFooledFallback, notFooledIcons);

            // Setup fooled section
            if (fooledLabel != null)
                fooledLabel.text = $"FOOLED ({fooledPenalty}):";

            SetupSection(fooled, fooledIconContainer, fooledFallback, fooledIcons);

            // Show panel
            Show(onComplete);

            // Animate content
            AnimateContent();
        }

        // ===========================
        // SETUP
        // ===========================

        private void SetupSection(List<Player> players, Transform iconContainer, TextMeshProUGUI fallback, List<GameObject> iconList)
        {
            ClearIcons(iconList);

            if (players != null && players.Count > 0)
            {
                if (fallback != null)
                    fallback.gameObject.SetActive(false);

                foreach (var player in players)
                {
                    GameObject iconObj = CreatePlayerIcon(player, iconContainer);
                    iconList.Add(iconObj);

                    // Start hidden
                    iconObj.transform.localScale = Vector3.zero;
                    CanvasGroup cg = iconObj.GetComponent<CanvasGroup>();
                    if (cg == null)
                        cg = iconObj.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                }
            }
            else
            {
                if (fallback != null)
                {
                    fallback.text = GameConstants.FallbackText.NoTesters;
                    fallback.gameObject.SetActive(true);
                }

                if (iconContainer != null)
                    iconContainer.gameObject.SetActive(false);
            }
        }

        private GameObject CreatePlayerIcon(Player player, Transform parent)
        {
            GameObject iconObj;

            if (playerIconPrefab != null)
            {
                iconObj = Instantiate(playerIconPrefab, parent);
            }
            else
            {
                iconObj = new GameObject($"Icon_{player.PlayerName}");
                iconObj.transform.SetParent(parent, false);

                RectTransform rect = iconObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(70, 70);

                Image img = iconObj.AddComponent<Image>();
                img.color = Color.white;

                Sprite iconSprite = Resources.Load<Sprite>($"PlayerIcons/{player.Icon}");
                if (iconSprite != null)
                    img.sprite = iconSprite;
            }

            return iconObj;
        }

        private void ClearIcons(List<GameObject> iconList)
        {
            foreach (var icon in iconList)
            {
                if (icon != null)
                    Destroy(icon);
            }
            iconList.Clear();
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void AnimateContent()
        {
            // Decoy answer slides in
            if (decoyAnswerText != null)
            {
                RectTransform rect = decoyAnswerText.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Vector2 startPos = rect.anchoredPosition;
                    startPos.y = 20f;
                    rect.anchoredPosition = startPos;

                    DOVirtual.DelayedCall(fadeInDuration, () =>
                    {
                        rect.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutQuad);
                        decoyAnswerText.GetComponent<CanvasGroup>()?.DOFade(1f, 0.5f);
                    });
                }
            }

            // Not fooled section after decoy
            DOVirtual.DelayedCall(fadeInDuration + sectionDelay, () =>
            {
                AnimateSection(notFooledSection, notFooledIcons, 0f);
            });

            // Fooled section after not fooled
            float fooledDelay = fadeInDuration + sectionDelay + (notFooledIcons.Count * iconCascadeDelay) + sectionDelay;
            DOVirtual.DelayedCall(fooledDelay, () =>
            {
                AnimateSection(fooledSection, fooledIcons, 0f);
            });
        }

        private void AnimateSection(GameObject section, List<GameObject> icons, float delay)
        {
            if (section == null) return;

            // Slide in section
            RectTransform rect = section.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 startPos = rect.anchoredPosition;
                startPos.y = 20f;
                rect.anchoredPosition = startPos;

                rect.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutQuad);
            }

            // Cascade icons
            for (int i = 0; i < icons.Count; i++)
            {
                GameObject icon = icons[i];
                float iconDelay = i * iconCascadeDelay;

                DOVirtual.DelayedCall(iconDelay, () =>
                {
                    AnimateIconIn(icon);
                });
            }
        }

        private void AnimateIconIn(GameObject icon)
        {
            if (icon == null) return;

            CanvasGroup cg = icon.GetComponent<CanvasGroup>();

            icon.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

            if (cg != null)
            {
                cg.DOFade(1f, 0.5f);
            }
        }

        // ===========================
        // CLEANUP
        // ===========================
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearIcons(notFooledIcons);
            ClearIcons(fooledIcons);
        }
    }
}
