using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace RobotsGame.UI.Results
{
    /// <summary>
    /// Base class for result panels with common fade in/out behavior.
    /// Based on unityspec.md ResultsScreen panel specifications.
    /// </summary>
    public class ResultPanelBase : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform rectTransform;

        [Header("Animation Settings")]
        [SerializeField] protected float fadeInDuration = 0.5f;
        [SerializeField] protected float fadeOutDuration = 0.5f;
        [SerializeField] protected float holdDuration = 5f;

        protected bool isShowing = false;

        // ===========================
        // LIFECYCLE
        // ===========================
        protected virtual void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

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
        /// Show panel with fade in
        /// </summary>
        public virtual void Show(System.Action onComplete = null)
        {
            if (isShowing) return;

            isShowing = true;
            gameObject.SetActive(true);

            // Fade in
            canvasGroup.DOFade(1f, fadeInDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Hold for duration
                    DOVirtual.DelayedCall(holdDuration, () =>
                    {
                        onComplete?.Invoke();
                    });
                });
        }

        /// <summary>
        /// Hide panel with fade out
        /// </summary>
        public virtual void Hide(System.Action onComplete = null)
        {
            if (!isShowing) return;

            canvasGroup.DOFade(0f, fadeOutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    isShowing = false;
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// Set hold duration for this panel
        /// </summary>
        public void SetHoldDuration(float duration)
        {
            holdDuration = duration;
        }

        // ===========================
        // CLEANUP
        // ===========================
        protected virtual void OnDestroy()
        {
            DOTween.Kill(canvasGroup);
            DOTween.Kill(rectTransform);
            DOTween.Kill(this);
        }
    }
}
