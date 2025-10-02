using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Managers;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Controls the Intro Video Screen - plays intro video with skip button.
    /// Desktop: Full-screen video player with skip button that slides in after 3s
    /// Mobile: Static background (no video)
    /// Based on screenspec.md SCREEN: IntroVideoScreen
    /// </summary>
    public class IntroVideoScreenController : MonoBehaviour
    {
        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage videoDisplay;
        [SerializeField] private GameObject skipButtonAssembly;
        [SerializeField] private Button skipButton;
        [SerializeField] private RectTransform skipButtonContainer;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;

        [Header("Settings")]
        [SerializeField] private float skipButtonSlideDelay = 3f;
        [SerializeField] private float skipButtonSlideDuration = 0.8f;
        [SerializeField] private float skipButtonOffScreenX = -300f; // Off-screen right position
        [SerializeField] private float skipButtonOnScreenX = 50f; // On-screen position
        [SerializeField] private float fadeOutDuration = 1f;
        [SerializeField] private float transitionDelay = 1.5f;
        [SerializeField] private string nextSceneName = "LandingPage";

        private ResponsiveUI responsiveUI;
        private bool isTransitioning = false;
        private bool isDesktop = true;
        private bool skipButtonVisible = false;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            responsiveUI = GetComponentInParent<ResponsiveUI>();
            if (responsiveUI == null)
            {
                responsiveUI = FindObjectOfType<ResponsiveUI>();
            }
        }

        private void Start()
        {
            InitializePlatform();

            if (isDesktop)
            {
                SetupDesktop();
            }
            else
            {
                SetupMobile();
            }
        }

        private void Update()
        {
            // Keyboard input (desktop only, after skip button appears)
            if (isDesktop && !isTransitioning && skipButtonVisible)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnSkipButtonClicked();
                }
            }
        }

        // ===========================
        // SETUP
        // ===========================

        private void SetupPlatformContent()
        {
            if (desktopContent != null)
                desktopContent.SetActive(isDesktop);

            if (mobileContent != null)
                mobileContent.SetActive(!isDesktop);
        }

        private void InitializePlatform()
        {
            if (responsiveUI != null)
            {
                isDesktop = responsiveUI.IsDesktop;
            }
            else
            {
                isDesktop = Screen.width > GameConstants.UI.MobileMaxWidth;
            }

            SetupPlatformContent();
        }

        private void SetupDesktop()
        {
            // Setup video player
            if (videoPlayer != null)
            {
                videoPlayer.isLooping = false;
                videoPlayer.playOnAwake = true;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;

                // Create render texture if needed
                if (videoDisplay != null && videoPlayer.targetTexture == null)
                {
                    RenderTexture rt = new RenderTexture(1920, 1080, 0);
                    videoPlayer.targetTexture = rt;
                    videoDisplay.texture = rt;
                }

                // Register video end callback
                videoPlayer.loopPointReached += OnVideoEnd;

                // Start playing
                videoPlayer.Play();
            }

            // Setup skip button (hidden initially, off-screen)
            if (skipButtonAssembly != null)
            {
                skipButtonAssembly.SetActive(true);

                if (skipButtonContainer != null)
                {
                    // Position off-screen to the right
                    skipButtonContainer.anchoredPosition = new Vector2(skipButtonOffScreenX, skipButtonContainer.anchoredPosition.y);
                }

                if (skipButton != null)
                {
                    skipButton.onClick.AddListener(OnSkipButtonClicked);

                    // Add ButtonEffects component if not already present
                    ButtonEffects buttonEffects = skipButton.GetComponent<ButtonEffects>();
                    if (buttonEffects == null)
                    {
                        buttonEffects = skipButton.gameObject.AddComponent<ButtonEffects>();
                    }
                }

                // Slide in after delay
                DOVirtual.DelayedCall(skipButtonSlideDelay, () =>
                {
                    SlideInSkipButton();
                });
            }
        }

        private void SetupMobile()
        {
            // Mobile shows static background only
            // No interaction needed
        }

        // ===========================
        // ANIMATIONS
        // ===========================

        private void SlideInSkipButton()
        {
            if (skipButtonContainer == null) return;

            skipButtonContainer.DOAnchorPosX(skipButtonOnScreenX, skipButtonSlideDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    skipButtonVisible = true;
                });
        }

        // ===========================
        // BUTTON HANDLERS
        // ===========================

        private void OnSkipButtonClicked()
        {
            if (isTransitioning) return;

            isTransitioning = true;

            // Button press sound is handled by ButtonEffects component

            // Cleanup and transition
            CleanupVideoAndTransition();
        }

        private void OnVideoEnd(VideoPlayer vp)
        {
            if (isTransitioning) return;

            isTransitioning = true;

            // Cleanup and transition
            CleanupVideoAndTransition();
        }

        // ===========================
        // VIDEO CLEANUP & TRANSITION
        // ===========================

        private void CleanupVideoAndTransition()
        {
            // Cleanup video
            if (videoPlayer != null)
            {
                videoPlayer.Pause();
                videoPlayer.targetTexture = null;
                videoPlayer.loopPointReached -= OnVideoEnd;
            }

            // Fade to black and transition
            FadeTransition.Instance.FadeOut(fadeOutDuration, () =>
            {
                DOVirtual.DelayedCall(transitionDelay - fadeOutDuration, () =>
                {
                    GoToNextScene();
                });
            });
        }

        // ===========================
        // SCENE TRANSITION
        // ===========================

        private void GoToNextScene()
        {
            if (!isTransitioning)
            {
                Debug.LogWarning("GoToNextScene called without an active transition. Ignoring.");
                return;
            }

            Debug.Log($"Transitioning to {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }

        // ===========================
        // CLEANUP
        // ===========================

        private void OnDestroy()
        {
            // Clean up tweens
            DOTween.Kill(this);

            // Clean up button listeners
            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
            }

            // Clean up video
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoEnd;
                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Stop();
                }
            }
        }
    }
}
