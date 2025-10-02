using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using RobotsGame.Core;

namespace RobotsGame.UI.Utilities
{
    /// <summary>
    /// Manages fade in/out transitions for screen changes.
    /// Based on unityspec.md fade transition specifications.
    /// </summary>
    public class FadeTransition : MonoBehaviour
    {
        private static FadeTransition _instance;
        public static FadeTransition Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("FadeTransition");
                    _instance = go.AddComponent<FadeTransition>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Fade Panel")]
        [SerializeField] private GameObject fadePanelPrefab;
        private GameObject fadePanel;
        private CanvasGroup canvasGroup;
        private Canvas canvas;
        private Image fadeImage;

        [Header("Settings")]
        [SerializeField] private float defaultFadeDuration = GameConstants.Delays.FadeTransitionDuration;
        [SerializeField] private Color fadeColor = Color.black;

        private bool isFading = false;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            CreateFadePanel();
        }

        private void CreateFadePanel()
        {
            // Create canvas for fade panel
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = GameConstants.UI.LayerFadeTransitionTop;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // Create fade panel
            fadePanel = new GameObject("FadePanel");
            fadePanel.transform.SetParent(canvasObj.transform, false);

            RectTransform rectTransform = fadePanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            fadeImage = fadePanel.AddComponent<Image>();
            fadeImage.color = fadeColor;
            fadeImage.raycastTarget = true;

            canvasGroup = fadePanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            fadePanel.SetActive(false);
        }

        // ===========================
        // FADE METHODS
        // ===========================

        /// <summary>
        /// Fade to black (or specified color)
        /// </summary>
        public void FadeOut(float duration = -1f, System.Action onComplete = null)
        {
            if (isFading)
            {
                Debug.LogWarning("Already fading, skipping FadeOut");
                return;
            }

            if (duration < 0)
                duration = defaultFadeDuration;

            fadePanel.SetActive(true);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            isFading = true;

            canvasGroup.DOFade(1f, duration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    isFading = false;
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// Fade from black (or specified color) to clear
        /// </summary>
        public void FadeIn(float duration = -1f, System.Action onComplete = null)
        {
            if (isFading)
            {
                Debug.LogWarning("Already fading, skipping FadeIn");
                return;
            }

            if (duration < 0)
                duration = defaultFadeDuration;

            fadePanel.SetActive(true);
            isFading = true;

            canvasGroup.DOFade(0f, duration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    fadePanel.SetActive(false);
                    isFading = false;
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// Fade out, perform action, then fade in
        /// </summary>
        public void FadeOutAndIn(System.Action duringFade, float fadeOutDuration = -1f, float fadeInDuration = -1f, float delayBetween = 0f)
        {
            FadeOut(fadeOutDuration, () =>
            {
                duringFade?.Invoke();

                if (delayBetween > 0f)
                {
                    DOVirtual.DelayedCall(delayBetween, () =>
                    {
                        FadeIn(fadeInDuration);
                    });
                }
                else
                {
                    FadeIn(fadeInDuration);
                }
            });
        }

        /// <summary>
        /// Set fade to specific alpha immediately (no animation)
        /// </summary>
        public void SetFadeAlpha(float alpha)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
            fadePanel.SetActive(alpha > 0f);

            if (alpha >= 1f)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else if (alpha <= 0f)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Change fade color
        /// </summary>
        public void SetFadeColor(Color color)
        {
            fadeColor = color;
            if (fadeImage != null)
                fadeImage.color = color;
        }

        /// <summary>
        /// Check if currently fading
        /// </summary>
        public bool IsFading()
        {
            return isFading;
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            canvasGroup?.DOKill();
        }
    }
}
