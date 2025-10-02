using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using RobotsGame.Core;

namespace RobotsGame.UI
{
    /// <summary>
    /// Shows elimination result: "ROBOT IDENTIFIED" or "TIE VOTE"
    /// Based on unityspec.md EliminationScreen result specifications.
    /// </summary>
    public class EliminationResultPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject normalResultPanel;
        [SerializeField] private GameObject tieResultPanel;
        [SerializeField] private TextMeshProUGUI headlineText;
        [SerializeField] private TextMeshProUGUI subheadlineText;
        [SerializeField] private TextMeshProUGUI eliminatedAnswerText;
        [SerializeField] private TextMeshProUGUI footerText;
        [SerializeField] private Image robotIdentImage; // For tie result
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float slideDelay = 0.05f; // 50ms after fade
        [SerializeField] private float slideDuration = 0.8f;
        [SerializeField] private float textRevealDelay = 0.5f; // After slide
        [SerializeField] private float holdDuration = 5f; // Hold before transition

        private bool isShowing = false;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            // Start hidden
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Show normal elimination result
        /// </summary>
        public void ShowNormalResult(string eliminatedAnswer, System.Action onComplete = null)
        {
            if (isShowing) return;

            isShowing = true;

            // Setup UI
            if (normalResultPanel != null)
                normalResultPanel.SetActive(true);

            if (tieResultPanel != null)
                tieResultPanel.SetActive(false);

            // Set text
            if (headlineText != null)
                headlineText.text = GameConstants.FallbackText.RobotIdentified;

            if (subheadlineText != null)
                subheadlineText.text = "Eliminated Response:";

            if (eliminatedAnswerText != null)
                eliminatedAnswerText.text = $"\"{eliminatedAnswer.ToUpper()}\"";

            if (footerText != null)
                footerText.text = "[YOU WILL NOW CHOOSE FROM THE REMAINING RESPONSES]";

            // Animate in
            AnimateIn(onComplete);
        }

        /// <summary>
        /// Show tie vote result
        /// </summary>
        public void ShowTieResult(System.Action onComplete = null)
        {
            if (isShowing) return;

            isShowing = true;

            // Setup UI
            if (normalResultPanel != null)
                normalResultPanel.SetActive(false);

            if (tieResultPanel != null)
                tieResultPanel.SetActive(true);

            // Animate in
            AnimateIn(onComplete);
        }

        /// <summary>
        /// Hide result panel
        /// </summary>
        public void Hide()
        {
            canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                gameObject.SetActive(false);
                isShowing = false;
            });
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void AnimateIn(System.Action onComplete)
        {
            gameObject.SetActive(true);

            RectTransform rect = GetComponent<RectTransform>();

            // Start position (off-screen down)
            Vector2 startPos = rect.anchoredPosition;
            startPos.y = -150f;
            rect.anchoredPosition = startPos;

            canvasGroup.alpha = 0f;

            Sequence sequence = DOTween.Sequence();

            // Slide up and fade in
            sequence.AppendInterval(slideDelay);
            sequence.Append(rect.DOAnchorPosY(0f, slideDuration).SetEase(Ease.OutCubic));
            sequence.Join(canvasGroup.DOFade(1f, slideDuration));

            // Reveal text elements
            sequence.AppendInterval(textRevealDelay);
            sequence.AppendCallback(() =>
            {
                // Text elements could have individual reveals here
            });

            // Hold
            sequence.AppendInterval(holdDuration);

            // Complete
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            DOTween.Kill(this);
            DOTween.Kill(canvasGroup);
            DOTween.Kill(GetComponent<RectTransform>());
        }
    }
}
