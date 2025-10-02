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
    /// Controls the Player Question Video Screen - plays player question video before certain rounds.
    /// Desktop: Full-screen video player that auto-advances when video ends
    /// Mobile: Static background (no video)
    /// Based on screenspec.md SCREEN: PlayerQuestionVideoScreen
    /// </summary>
    public class PlayerQuestionVideoScreenController : MonoBehaviour
    {
        [Header("Round Settings")]
        [SerializeField] private int currentRound = 1;

        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage videoDisplay;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private Image mobileBackgroundImage;

        [Header("Mobile Background Sprites")]
        [SerializeField] private Sprite roundArt1Mobile;
        [SerializeField] private Sprite roundArt2Mobile;
        [SerializeField] private Sprite roundArt3Mobile;
        [SerializeField] private Sprite roundArt4Mobile;

        [Header("Settings")]
        [SerializeField] private float fadeInDelay = 0.1f;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 1f;
        [SerializeField] private string nextSceneName = "QuestionScreen";

        private ResponsiveUI responsiveUI;
        private bool isTransitioning = false;
        private bool isDesktop = true;

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

            // Fade in from black
            DOVirtual.DelayedCall(fadeInDelay, () =>
            {
                FadeInScreen();
            });
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
        }

        private void SetupMobile()
        {
            // Mobile shows static background based on round pattern
            SetMobileBackground();
        }

        private void SetMobileBackground()
        {
            if (mobileBackgroundImage == null) return;

            // Pattern repeats every 4 rounds (same as RoundArtScreen)
            int patternIndex = ((currentRound - 1) % 4) + 1;
            switch (patternIndex)
            {
                case 1:
                    mobileBackgroundImage.sprite = roundArt1Mobile;
                    break;
                case 2:
                    mobileBackgroundImage.sprite = roundArt2Mobile;
                    break;
                case 3:
                    mobileBackgroundImage.sprite = roundArt3Mobile;
                    break;
                case 4:
                    mobileBackgroundImage.sprite = roundArt4Mobile;
                    break;
            }
        }

        // ===========================
        // ANIMATIONS
        // ===========================

        private void FadeInScreen()
        {
            FadeTransition.Instance.FadeIn(fadeInDuration);
        }

        // ===========================
        // VIDEO HANDLERS
        // ===========================

        private void OnVideoEnd(VideoPlayer vp)
        {
            if (isTransitioning) return;

            isTransitioning = true;

            // Cleanup video
            if (videoPlayer != null)
            {
                videoPlayer.Pause();
                videoPlayer.targetTexture = null;
                videoPlayer.loopPointReached -= OnVideoEnd;
            }

            // Fade to black and transition
            FadeOutAndTransition();
        }

        private void FadeOutAndTransition()
        {
            FadeTransition.Instance.FadeOut(fadeOutDuration, () =>
            {
                GoToNextScene();
            });
        }

        // ===========================
        // SCENE TRANSITION
        // ===========================

        private void GoToNextScene()
        {
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
