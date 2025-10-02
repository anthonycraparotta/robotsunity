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
    /// Controls the Credits Screen - scrolling credits with video and navigation buttons.
    /// Desktop: Scrolling credits text, floating video, button assembly
    /// Mobile: Static credits image with clickable area
    /// Based on screenspec.md SCREEN: CreditsScreen
    /// </summary>
    public class CreditsScreenController : MonoBehaviour
    {
        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private ScrollRect creditsScrollRect;
        [SerializeField] private RectTransform creditsContent;
        [SerializeField] private VideoPlayer creditsVideo;
        [SerializeField] private GameObject buttonAssembly;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button gamesButton;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private Image creditsForeground;

        [Header("Settings")]
        [SerializeField] private float scrollSpeed = 3.25f; // Pixels per 30ms
        [SerializeField] private float scrollInterval = 0.03f; // 30ms
        [SerializeField] private float buttonSlideInDelay = 2f;
        [SerializeField] private float videoScaleInDuration = 0.5f;
        [SerializeField] private float videoFloatDuration = 6f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private string nextSceneName = "HostScreen";

        private ResponsiveUI responsiveUI;
        private bool isTransitioning = false;
        private bool isDesktop = true;
        private bool isScrolling = false;
        private float scrollTimer = 0f;
        private int highlightedButtonIndex = 0; // 0 = games, 1 = new game

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

            FadeTransition.Instance.FadeIn(fadeInDuration);
        }

        private void Update()
        {
            // Auto-scroll credits (desktop)
            if (isDesktop && isScrolling)
            {
                scrollTimer += Time.deltaTime;
                if (scrollTimer >= scrollInterval)
                {
                    scrollTimer = 0f;
                    ScrollCredits();
                }
            }

            // Keyboard controls (desktop)
            if (isDesktop && !isTransitioning)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    HighlightButton(0);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    HighlightButton(1);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    ClickHighlightedButton();
                }
            }
        }

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
            // Setup video
            if (creditsVideo != null)
            {
                creditsVideo.isLooping = true;
                creditsVideo.Play();

                // Animate video scale in
                creditsVideo.transform.localScale = Vector3.zero;
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    creditsVideo.transform.DOScale(1f, videoScaleInDuration).SetEase(Ease.OutBack);
                });

                // Start floating animation
                StartVideoFloating();
            }

            // Setup buttons
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
            }
            if (gamesButton != null)
            {
                gamesButton.onClick.AddListener(OnGamesClicked);
            }

            // Hide button assembly initially
            if (buttonAssembly != null)
            {
                buttonAssembly.SetActive(false);
                DOVirtual.DelayedCall(buttonSlideInDelay, () =>
                {
                    buttonAssembly.SetActive(true);
                    HighlightButton(0); // Default to games button
                });
            }

            // Start scrolling credits
            isScrolling = true;

            // Play credits music
            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayMusic("credits");
            }
        }

        private void SetupMobile()
        {
            // Mobile: scale in foreground after delay
            if (creditsForeground != null)
            {
                creditsForeground.transform.localScale = Vector3.zero;
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    creditsForeground.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
                });
            }
        }

        private void ScrollCredits()
        {
            if (creditsScrollRect == null || creditsContent == null) return;

            // Scroll upward
            float newPosition = creditsScrollRect.verticalNormalizedPosition + (scrollSpeed / creditsContent.rect.height);
            creditsScrollRect.verticalNormalizedPosition = Mathf.Clamp01(newPosition);

            // Stop scrolling when reached top
            if (creditsScrollRect.verticalNormalizedPosition >= 1f)
            {
                isScrolling = false;
            }
        }

        private void StartVideoFloating()
        {
            if (creditsVideo == null) return;

            Sequence floatSequence = DOTween.Sequence();
            floatSequence.Append(
                creditsVideo.transform.DOMoveY(creditsVideo.transform.position.y + 20f, videoFloatDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );
            floatSequence.Append(
                creditsVideo.transform.DOMoveY(creditsVideo.transform.position.y, videoFloatDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );
            floatSequence.SetLoops(-1, LoopType.Restart);
        }

        private void HighlightButton(int index)
        {
            highlightedButtonIndex = index;

            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }

            // Visual highlight (scale effect)
            if (gamesButton != null && newGameButton != null)
            {
                gamesButton.transform.localScale = (index == 0) ? Vector3.one * 1.05f : Vector3.one;
                newGameButton.transform.localScale = (index == 1) ? Vector3.one * 1.05f : Vector3.one;
            }
        }

        private void ClickHighlightedButton()
        {
            if (highlightedButtonIndex == 0)
            {
                OnGamesClicked();
            }
            else
            {
                OnNewGameClicked();
            }
        }

        private void OnNewGameClicked()
        {
            if (isTransitioning) return;

            isTransitioning = true;

            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
                audioManager.StopMusic();
            }

            FadeTransition.Instance.FadeOut(fadeOutDuration, () =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }

        private void OnGamesClicked()
        {
            // Currently disabled - same as New Game
            OnNewGameClicked();
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
            if (newGameButton != null)
                newGameButton.onClick.RemoveAllListeners();
            if (gamesButton != null)
                gamesButton.onClick.RemoveAllListeners();

            if (creditsVideo != null && creditsVideo.isPlaying)
            {
                creditsVideo.Stop();
            }
        }
    }
}
