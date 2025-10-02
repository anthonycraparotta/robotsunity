using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace RobotsGame.UI
{
    /// <summary>
    /// Manages the question card flip animation on desktop.
    /// Shows placeholder text, then flips to reveal the actual question.
    /// Based on unityspec.md QuestionScreen - Question Panel specifications.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class QuestionCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardFrontFace;
        [SerializeField] private GameObject cardBackFace;
        [SerializeField] private TextMeshProUGUI placeholderText;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private CanvasGroup frontCanvasGroup;
        [SerializeField] private CanvasGroup backCanvasGroup;

        [Header("Animation Timing (seconds)")]
        [SerializeField] private float slideInDuration = 0.8f;
        [SerializeField] private float placeholderFadeInDelay = 1.2f;
        [SerializeField] private float placeholderFadeOutDelay = 3.8f;
        [SerializeField] private float cardFlipDelay = 4.2f;
        [SerializeField] private float cardFlipDuration = 0.6f;
        [SerializeField] private float questionRevealDelay = 4.8f;

        [Header("Settings")]
        [SerializeField] private string placeholderMessage = "WRITE A CREATIVE ANSWER TO THIS QUESTION";

        private RectTransform rectTransform;
        private bool isFlipped = false;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (rectTransform == null)
            {
                Debug.LogError($"{nameof(QuestionCard)} requires a RectTransform component.", this);
                return;
            }

            // Ensure canvas groups exist
            if (frontCanvasGroup == null && cardFrontFace != null)
                frontCanvasGroup = cardFrontFace.GetComponent<CanvasGroup>() ?? cardFrontFace.AddComponent<CanvasGroup>();

            if (backCanvasGroup == null && cardBackFace != null)
                backCanvasGroup = cardBackFace.GetComponent<CanvasGroup>() ?? cardBackFace.AddComponent<CanvasGroup>();

            // Set initial states
            if (frontCanvasGroup != null)
                frontCanvasGroup.alpha = 0f;

            if (backCanvasGroup != null)
            {
                backCanvasGroup.alpha = 0f;
                cardBackFace.SetActive(false);
            }

            if (placeholderText != null)
                placeholderText.text = placeholderMessage;
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Start the full card animation sequence
        /// </summary>
        public void PlayCardSequence(string question)
        {
            if (questionText != null)
                questionText.text = question.ToUpper();

            Sequence cardSequence = DOTween.Sequence();

            // 1. Slide in (0-800ms)
            cardSequence.Append(rectTransform.DOAnchorPos(rectTransform.anchoredPosition, slideInDuration).SetEase(Ease.OutQuad));

            // 2. Fade in placeholder (1200ms)
            cardSequence.Insert(placeholderFadeInDelay, frontCanvasGroup.DOFade(1f, 0.5f));

            // 3. Fade out placeholder (3800ms)
            cardSequence.Insert(placeholderFadeOutDelay, frontCanvasGroup.DOFade(0f, 0.4f));

            // 4. Flip card (4200ms)
            cardSequence.InsertCallback(cardFlipDelay, () => FlipCard());

            // 5. Reveal question text (4800ms)
            cardSequence.InsertCallback(questionRevealDelay, () => RevealQuestion());
        }

        /// <summary>
        /// Flip the card from front to back face
        /// </summary>
        private void FlipCard()
        {
            if (isFlipped) return;

            isFlipped = true;

            // 3D flip animation using rotation
            Sequence flipSequence = DOTween.Sequence();

            // Rotate to 90 degrees (hide front)
            flipSequence.Append(rectTransform.DORotate(new Vector3(0, 90, 0), cardFlipDuration / 2f).SetEase(Ease.InQuad));

            // At 90 degrees, swap faces
            flipSequence.AppendCallback(() =>
            {
                cardFrontFace.SetActive(false);
                cardBackFace.SetActive(true);
            });

            // Rotate from 90 to 0 (show back)
            flipSequence.Append(rectTransform.DORotate(new Vector3(0, 0, 0), cardFlipDuration / 2f).SetEase(Ease.OutQuad));
        }

        /// <summary>
        /// Reveal the question text on the back face
        /// </summary>
        private void RevealQuestion()
        {
            if (backCanvasGroup != null)
            {
                backCanvasGroup.DOFade(1f, 0.5f);
            }
        }

        /// <summary>
        /// Instant show question (for mobile or when skipping animation)
        /// </summary>
        public void ShowQuestionInstant(string question)
        {
            if (questionText != null)
                questionText.text = question.ToUpper();

            cardFrontFace.SetActive(false);
            cardBackFace.SetActive(true);
            backCanvasGroup.alpha = 1f;
            isFlipped = true;
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            DOTween.Kill(rectTransform);
            DOTween.Kill(frontCanvasGroup);
            DOTween.Kill(backCanvasGroup);
        }
    }
}
