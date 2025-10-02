using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Managers;
using RobotsGame.Network;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Controls the Round Art Screen - displays round artwork with countdown timer.
    /// Desktop: Shows round art, skip button with timer that auto-starts after 15s
    /// Mobile: Shows round art, waits for host to start
    /// Based on screenspec.md SCREEN: RoundArtScreen
    /// </summary>
    public class RoundArtScreenController : MonoBehaviour
    {
        [Header("Round Settings")]
        [SerializeField] private int currentRound = 1;
        [SerializeField] private int totalRounds = 8; // 8 or 12

        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject buttonTimerAssembly;
        [SerializeField] private RectTransform buttonTimerContainer;
        [SerializeField] private Button skipButton;
        [SerializeField] private Text timerText;
        [SerializeField] private Image hostCircle;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private Image mobileBackgroundImage;
        [SerializeField] private Text warningMessageText;

        [Header("Background Sprites - Desktop")]
        [SerializeField] private Sprite[] roundArtSprites; // round1art through round12art
        [SerializeField] private Sprite picqRound8Art;
        [SerializeField] private Sprite picqRound12Art;

        [Header("Background Sprites - Mobile")]
        [SerializeField] private Sprite roundArt1Mobile;
        [SerializeField] private Sprite roundArt2Mobile;
        [SerializeField] private Sprite roundArt3Mobile;
        [SerializeField] private Sprite roundArt4Mobile;

        [Header("Settings")]
        [SerializeField] private float timerDuration = 15f;
        [SerializeField] private float buttonSlideInDelay = 1.5f;
        [SerializeField] private float buttonSlideDuration = 0.8f;
        [SerializeField] private float hostCircleScaleDelay = 1f;
        [SerializeField] private float hostCircleScaleDuration = 0.5f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float buttonOffScreenX = -1200f; // Off-screen right
        [SerializeField] private float buttonOnScreenX = 56f; // On-screen position
        [SerializeField] private string nextSceneName = "QuestionScreen";

        private ResponsiveUI responsiveUI;
        private NetworkManager networkManager;
        private bool isTransitioning = false;
        private bool isDesktop = true;
        private float timeRemaining;
        private bool timerRunning = false;
        private bool skipButtonEnabled = false;

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

            networkManager = FindObjectOfType<NetworkManager>();
        }

        private void Start()
        {
            InitializePlatform();
            SetRoundBackground();

            if (isDesktop)
            {
                SetupDesktop();
            }
            else
            {
                SetupMobile();
            }

            // Fade in from black
            FadeInScreen();
        }

        private void Update()
        {
            // Update timer
            if (timerRunning && isDesktop)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0)
                {
                    timeRemaining = 0;
                    timerRunning = false;
                    OnTimerExpired();
                }

                UpdateTimerDisplay();
            }

            // Keyboard input (desktop only)
            if (isDesktop && !isTransitioning && skipButtonEnabled)
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

        private void SetRoundBackground()
        {
            if (isDesktop && backgroundImage != null)
            {
                // Desktop background selection
                if (currentRound == 8 && totalRounds == 8 && picqRound8Art != null)
                {
                    backgroundImage.sprite = picqRound8Art;
                }
                else if (currentRound == 12 && totalRounds == 12 && picqRound12Art != null)
                {
                    backgroundImage.sprite = picqRound12Art;
                }
                else if (currentRound >= 1 && currentRound <= roundArtSprites.Length)
                {
                    backgroundImage.sprite = roundArtSprites[currentRound - 1];
                }
            }
            else if (!isDesktop && mobileBackgroundImage != null)
            {
                // Mobile background selection (repeating pattern)
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
        }

        private void SetupDesktop()
        {
            // Setup skip button
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipButtonClicked);
                skipButton.interactable = false; // Disabled initially

                // Add ButtonEffects component if not already present
                ButtonEffects buttonEffects = skipButton.GetComponent<ButtonEffects>();
                if (buttonEffects == null)
                {
                    buttonEffects = skipButton.gameObject.AddComponent<ButtonEffects>();
                }
            }

            // Position button assembly off-screen
            if (buttonTimerContainer != null)
            {
                buttonTimerContainer.anchoredPosition = new Vector2(buttonOffScreenX, buttonTimerContainer.anchoredPosition.y);
            }

            // Setup host circle (hidden initially)
            if (hostCircle != null)
            {
                hostCircle.transform.localScale = Vector3.zero;
            }

            // Start timer
            timeRemaining = timerDuration;
            timerRunning = true;
            UpdateTimerDisplay();

            // Slide in button assembly after delay
            DOVirtual.DelayedCall(buttonSlideInDelay, () =>
            {
                SlideInButtonAssembly();
            });

            // Scale in host circle after delay
            DOVirtual.DelayedCall(hostCircleScaleDelay, () =>
            {
                ScaleInHostCircle();
            });

            // Subscribe to network events
            SubscribeToNetworkEvents();
        }

        private void SetupMobile()
        {
            // Mobile waits for host
            if (warningMessageText != null)
            {
                warningMessageText.gameObject.SetActive(false);
            }

            // Subscribe to network events
            SubscribeToNetworkEvents();
        }

        // ===========================
        // ANIMATIONS
        // ===========================

        private void FadeInScreen()
        {
            FadeTransition.Instance.FadeIn(fadeInDuration);
        }

        private void SlideInButtonAssembly()
        {
            if (buttonTimerContainer == null) return;

            buttonTimerContainer.DOAnchorPosX(buttonOnScreenX, buttonSlideDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    // Enable skip button after slide-in
                    if (skipButton != null)
                    {
                        skipButton.interactable = true;
                        skipButtonEnabled = true;
                    }
                });
        }

        private void ScaleInHostCircle()
        {
            if (hostCircle == null) return;

            hostCircle.transform.DOScale(1f, hostCircleScaleDuration)
                .SetEase(Ease.InOutQuad);
        }

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(timeRemaining);
                timerText.text = seconds.ToString();
            }
        }

        // ===========================
        // BUTTON HANDLERS
        // ===========================

        private void OnSkipButtonClicked()
        {
            if (isTransitioning) return;

            isTransitioning = true;
            timerRunning = false;

            // Disable button
            if (skipButton != null)
            {
                skipButton.interactable = false;
            }

            // Play button press sound (handled by ButtonEffects)

            // Emit start-round event
            if (networkManager != null)
            {
                networkManager.StartRound(currentRound);
            }

            // Fade out and transition after delay
            DOVirtual.DelayedCall(fadeOutDuration, () =>
            {
                FadeOutAndTransition();
            });
        }

        private void OnTimerExpired()
        {
            // Auto-start round when timer expires
            if (!isTransitioning && isDesktop)
            {
                OnSkipButtonClicked();
            }
        }

        // ===========================
        // NETWORK EVENTS
        // ===========================

        private void SubscribeToNetworkEvents()
        {
            if (networkManager == null) return;

            networkManager.OnRoundStarted += OnRoundStarted;
            networkManager.OnRoundStartWarning += OnRoundStartWarning;
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (networkManager == null) return;

            networkManager.OnRoundStarted -= OnRoundStarted;
            networkManager.OnRoundStartWarning -= OnRoundStartWarning;
        }

        private void OnRoundStarted(int round)
        {
            // Transition to question screen
            FadeOutAndTransition();
        }

        private void OnRoundStartWarning(string message)
        {
            // Show warning message (mobile)
            if (!isDesktop && warningMessageText != null)
            {
                warningMessageText.text = message;
                warningMessageText.gameObject.SetActive(true);

                // Hide after 3 seconds
                DOVirtual.DelayedCall(3f, () =>
                {
                    if (warningMessageText != null)
                        warningMessageText.gameObject.SetActive(false);
                });
            }
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

            // Unsubscribe from network events
            UnsubscribeFromNetworkEvents();

            // Clean up button listeners
            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
            }
        }
    }
}
