using UnityEngine;
using UnityEngine.UI;
using RobotsGame.Core;

namespace RobotsGame.UI.Utilities
{
    /// <summary>
    /// Manages responsive UI behavior for desktop vs mobile layouts.
    /// Based on unityspec.md RESPONSIVE BEHAVIOR section.
    /// Attach this to the root Canvas of each scene.
    /// </summary>
    public class ResponsiveUI : MonoBehaviour
    {
        [Header("Canvas References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasScaler canvasScaler;

        [Header("Platform Detection")]
        [SerializeField] private bool isDesktop = true;
        [SerializeField] private int currentScreenWidth;

        [Header("Desktop/Mobile GameObjects")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private GameObject mobileContent;

        public bool IsDesktop => isDesktop;
        public bool IsMobile => !isDesktop;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (canvas == null)
                canvas = GetComponent<Canvas>();

            if (canvasScaler == null)
                canvasScaler = GetComponent<CanvasScaler>();

            DetectPlatform();
            ConfigureCanvasScaler();
            SetActiveContent();
        }

        private void Start()
        {
            // Double-check on start in case screen size changed
            DetectPlatform();
            SetActiveContent();
        }

#if UNITY_EDITOR
        private void Update()
        {
            // In editor, check for screen size changes (for testing responsiveness)
            if (currentScreenWidth != Screen.width)
            {
                DetectPlatform();
                ConfigureCanvasScaler();
                SetActiveContent();
            }
        }
#endif

        // ===========================
        // PLATFORM DETECTION
        // ===========================

        private void DetectPlatform()
        {
            currentScreenWidth = Screen.width;

            // Desktop: > 768px width
            // Mobile: <= 768px width
            isDesktop = currentScreenWidth > GameConstants.UI.MobileMaxWidth;

            Debug.Log($"Platform detected: {(isDesktop ? "Desktop" : "Mobile")} (width: {currentScreenWidth}px)");
        }

        private void ConfigureCanvasScaler()
        {
            if (canvasScaler == null)
                return;

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            if (isDesktop)
            {
                // Desktop: 1920x1080 reference
                canvasScaler.referenceResolution = new Vector2(
                    GameConstants.UI.DesktopBaseWidth,
                    GameConstants.UI.DesktopBaseHeight
                );
                canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
            }
            else
            {
                // Mobile: 375x812 reference (iPhone X-style)
                canvasScaler.referenceResolution = new Vector2(
                    GameConstants.UI.MobileBaseWidth,
                    GameConstants.UI.MobileBaseWidth * 2.16f // 375x812 aspect ratio
                );
                canvasScaler.matchWidthOrHeight = 0f; // Match width primarily
            }
        }

        private void SetActiveContent()
        {
            if (desktopContent != null)
                desktopContent.SetActive(isDesktop);

            if (mobileContent != null)
                mobileContent.SetActive(!isDesktop);
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Force platform detection (useful after orientation changes)
        /// </summary>
        public void RefreshPlatformDetection()
        {
            DetectPlatform();
            ConfigureCanvasScaler();
            SetActiveContent();
        }

        /// <summary>
        /// Get safe area for mobile (accounts for notches, etc.)
        /// </summary>
        public Rect GetSafeArea()
        {
            return Screen.safeArea;
        }

        /// <summary>
        /// Apply safe area insets to a RectTransform (mobile only)
        /// </summary>
        public void ApplySafeArea(RectTransform rectTransform)
        {
            if (isDesktop || rectTransform == null)
                return;

            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }

        /// <summary>
        /// Convert viewport units to pixels based on current platform
        /// </summary>
        public float VWToPixels(float vw)
        {
            float referenceWidth = isDesktop ? GameConstants.UI.DesktopBaseWidth : GameConstants.UI.MobileBaseWidth;
            return (vw / 100f) * referenceWidth;
        }

        public float VHToPixels(float vh)
        {
            float referenceHeight = isDesktop ? GameConstants.UI.DesktopBaseHeight : (GameConstants.UI.MobileBaseWidth * 2.16f);
            return (vh / 100f) * referenceHeight;
        }

        /// <summary>
        /// Get current reference resolution
        /// </summary>
        public Vector2 GetReferenceResolution()
        {
            if (canvasScaler != null)
                return canvasScaler.referenceResolution;

            return isDesktop
                ? new Vector2(GameConstants.UI.DesktopBaseWidth, GameConstants.UI.DesktopBaseHeight)
                : new Vector2(GameConstants.UI.MobileBaseWidth, GameConstants.UI.MobileBaseWidth * 2.16f);
        }
    }
}
