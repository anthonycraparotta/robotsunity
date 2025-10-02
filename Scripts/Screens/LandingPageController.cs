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
    /// Controls the Landing Page screen - entry point of the game.
    /// Desktop: Shows video player and start button with fade effects
    /// Mobile: Shows black screen only
    /// Based on unityspec.md SCREEN: LandingPage
    /// </summary>
    public class LandingPageController : MonoBehaviour
    {
        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private Button startButton;
        [SerializeField] private CanvasGroup initialFadeOverlay;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;

        [Header("Settings")]
        [SerializeField] private float initialFadeDelay = 0.1f; // 100ms
        [SerializeField] private float initialFadeDuration = 1f;
        [SerializeField] private float exitFadeDelay = 1f; // 1000ms before fade starts
        [SerializeField] private float exitFadeDuration = 1f;
        [SerializeField] private string nextSceneName = "JoinRoom"; // Or "Lobby"

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

            isDesktop = responsiveUI != null ? responsiveUI.IsDesktop : (Screen.width > GameConstants.UI.MobileMaxWidth);

            SetupPlatformContent();
        }

        private void Start()
        {
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
            // Keyboard input (desktop only)
            if (isDesktop && !isTransitioning)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnStartButtonClicked();
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

        private void SetupDesktop()
        {
            // Setup video player
            if (videoPlayer != null)
            {
                videoPlayer.isLooping = true;
                videoPlayer.playOnAwake = true;

                // Mute initially (will be unmuted after user interaction for browser compatibility)
                videoPlayer.SetDirectAudioMute(0, false);

                // Start playing
                videoPlayer.Play();

                // Play voice over for rules
                AudioManager.Instance.PlayVoiceOver(GameConstants.Audio.VO_LandingRules);
            }

            // Setup start button
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);

                // Add ButtonEffects component if not already present
                ButtonEffects buttonEffects = startButton.GetComponent<ButtonEffects>();
                if (buttonEffects == null)
                {
                    buttonEffects = startButton.gameObject.AddComponent<ButtonEffects>();
                }
            }

            // Initial fade in
            if (initialFadeOverlay != null)
            {
                initialFadeOverlay.alpha = 1f;
                initialFadeOverlay.interactable = false;
                initialFadeOverlay.blocksRaycasts = false;

                DOVirtual.DelayedCall(initialFadeDelay, () =>
                {
                    FadeInScreen();
                });
            }
        }

        private void SetupMobile()
        {
            // Mobile shows blank black screen
            // Start button still works for transition, but no fade effects
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClickedMobile);
            }
        }

        // ===========================
        // ANIMATIONS
        // ===========================

        private void FadeInScreen()
        {
            if (initialFadeOverlay == null) return;

            initialFadeOverlay.DOFade(0f, initialFadeDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    initialFadeOverlay.gameObject.SetActive(false);
                });
        }

        // ===========================
        // BUTTON HANDLERS
        // ===========================

        private void OnStartButtonClicked()
        {
            if (isTransitioning) return;

            isTransitioning = true;

            // Stop any voice overs
            AudioManager.Instance.StopVoiceOver();

            // Button press sound is handled by ButtonEffects component

            // Wait delay, then fade out and transition
            DOVirtual.DelayedCall(exitFadeDelay, () =>
            {
                FadeOutAndTransition();
            });
        }

        private void OnStartButtonClickedMobile()
        {
            if (isTransitioning) return;

            isTransitioning = true;

            // Mobile: immediate transition, no fade
            AudioManager.Instance.PlayButtonPress();
            GoToNextScene();
        }

        private void FadeOutAndTransition()
        {
            FadeTransition.Instance.FadeOut(exitFadeDuration, () =>
            {
                GoToNextScene();
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

            // Prevent any further attempts while we load the next scene.
            isTransitioning = false;

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
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
            }

            // Stop video
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
        }
    }
}
