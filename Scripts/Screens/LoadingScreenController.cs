using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Managers;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Controls the Loading Screen - initial entry point with logo animation.
    /// Desktop: Shows animated logo with floating animation, tips, and spatter overlays
    /// Mobile: Shows logo with "TAP TO START" pulsing text
    /// Based on screenspec.md SCREEN: LoadingScreen
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private Image logo;
        [SerializeField] private Image loadingTips;
        [SerializeField] private Image desktopSpatter1;
        [SerializeField] private Image desktopSpatter2;
        [SerializeField] private CanvasGroup blackOverlay;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private Image mobileLogo;
        [SerializeField] private Text tapToStartText;

        [Header("Settings")]
        [SerializeField] private float blackOverlayHoldDelay = 1f; // 1 second before fade starts
        [SerializeField] private float blackOverlayFadeDuration = 0.5f;
        [SerializeField] private float logoAppearDelay = 1.5f;
        [SerializeField] private float logoScaleDuration = 0.5f;
        [SerializeField] private float logoFloatStartDelay = 2f;
        [SerializeField] private float logoFloatDuration = 7.8f;
        [SerializeField] private float tipsAppearDelay = 2.5f;
        [SerializeField] private float tipsFadeDuration = 0.5f;
        [SerializeField] private float exitFadeDuration = 1f;
        [SerializeField] private string nextSceneName = "IntroVideoScreen";

        private ResponsiveUI responsiveUI;
        private bool isTransitioning = false;
        private bool isDesktop = true;
        private Tween logoFloatTween;
        private Tween spatter1Tween;
        private Tween spatter2Tween;

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
            // Handle click/keyboard input
            if (!isTransitioning)
            {
                if (isDesktop)
                {
                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        OnScreenClicked();
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        OnScreenTapped();
                    }
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
            // Start with logo hidden
            if (logo != null)
            {
                logo.transform.localScale = Vector3.zero;
            }

            // Start with tips hidden
            if (loadingTips != null)
            {
                var tipsGroup = loadingTips.GetComponent<CanvasGroup>();
                if (tipsGroup == null)
                {
                    tipsGroup = loadingTips.gameObject.AddComponent<CanvasGroup>();
                }
                tipsGroup.alpha = 0f;
            }

            // Setup black overlay
            if (blackOverlay != null)
            {
                blackOverlay.alpha = 1f;
                blackOverlay.blocksRaycasts = false;

                // Fade out after delay
                DOVirtual.DelayedCall(blackOverlayHoldDelay, () =>
                {
                    FadeOutBlackOverlay();
                });
            }

            // Animate logo appearance
            DOVirtual.DelayedCall(logoAppearDelay, () =>
            {
                AnimateLogo();
            });

            // Animate tips appearance
            DOVirtual.DelayedCall(tipsAppearDelay, () =>
            {
                AnimateTips();
            });

            // Start floating logo animation
            DOVirtual.DelayedCall(logoFloatStartDelay, () =>
            {
                StartLogoFloating();
            });

            // Start spatter animations
            StartSpatterAnimations();
        }

        private void SetupMobile()
        {
            // Mobile: logo is visible, "TAP TO START" pulses
            if (mobileLogo != null)
            {
                mobileLogo.gameObject.SetActive(true);
            }

            if (tapToStartText != null)
            {
                StartTapToStartPulse();
            }
        }

        // ===========================
        // DESKTOP ANIMATIONS
        // ===========================

        private void FadeOutBlackOverlay()
        {
            if (blackOverlay == null) return;

            blackOverlay.DOFade(0f, blackOverlayFadeDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    blackOverlay.gameObject.SetActive(false);
                });
        }

        private void AnimateLogo()
        {
            if (logo == null) return;

            // Scale up from 0 to 1 with elastic bounce
            logo.transform.DOScale(1f, logoScaleDuration)
                .SetEase(Ease.OutElastic);
        }

        private void AnimateTips()
        {
            if (loadingTips == null) return;

            var tipsGroup = loadingTips.GetComponent<CanvasGroup>();
            if (tipsGroup == null) return;

            tipsGroup.DOFade(1f, tipsFadeDuration)
                .SetEase(Ease.InOutQuad);
        }

        private void StartLogoFloating()
        {
            if (logo == null) return;

            // Floating animation: scale and vertical movement
            Sequence floatSequence = DOTween.Sequence();

            floatSequence.Append(
                logo.transform.DOScale(1.05f, logoFloatDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );
            floatSequence.Join(
                logo.transform.DOMoveY(logo.transform.position.y - Screen.height * 0.0185f, logoFloatDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );

            floatSequence.Append(
                logo.transform.DOScale(1f, logoFloatDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );
            floatSequence.Join(
                logo.transform.DOMoveY(logo.transform.position.y, logoFloatDuration / 2f)
                    .SetEase(Ease.InOutSine)
            );

            floatSequence.SetLoops(-1, LoopType.Restart);
            logoFloatTween = floatSequence;
        }

        private void StartSpatterAnimations()
        {
            // Spatter pulse animation: scale between 0.95 and 1.05
            if (desktopSpatter1 != null)
            {
                spatter1Tween = desktopSpatter1.transform.DOScale(1.05f, 6f)
                    .From(0.95f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            if (desktopSpatter2 != null)
            {
                spatter2Tween = desktopSpatter2.transform.DOScale(1.05f, 6f)
                    .From(0.95f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        // ===========================
        // MOBILE ANIMATIONS
        // ===========================

        private void StartTapToStartPulse()
        {
            if (tapToStartText == null) return;

            Sequence pulseSequence = DOTween.Sequence();

            pulseSequence.Append(
                tapToStartText.DOFade(0.7f, 1f)
                    .SetEase(Ease.InOutSine)
            );
            pulseSequence.Join(
                tapToStartText.transform.DOScale(1.05f, 1f)
                    .SetEase(Ease.InOutSine)
            );

            pulseSequence.Append(
                tapToStartText.DOFade(1f, 1f)
                    .SetEase(Ease.InOutSine)
            );
            pulseSequence.Join(
                tapToStartText.transform.DOScale(1f, 1f)
                    .SetEase(Ease.InOutSine)
            );

            pulseSequence.SetLoops(-1, LoopType.Restart);
        }

        // ===========================
        // INPUT HANDLERS
        // ===========================

        private void OnScreenClicked()
        {
            if (isTransitioning) return;

            isTransitioning = true;

            // Play button press sound
            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }

            // Fade to black and transition
            FadeOutAndTransition();
        }

        private void OnScreenTapped()
        {
            if (isTransitioning) return;

            isTransitioning = true;

            // Fade out and transition (mobile)
            FadeOutAndTransition();
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

            Debug.Log($"Transitioning to {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }

        // ===========================
        // CLEANUP
        // ===========================

        private void OnDestroy()
        {
            // Kill all tweens
            DOTween.Kill(this);
            if (logoFloatTween != null) logoFloatTween.Kill();
            if (spatter1Tween != null) spatter1Tween.Kill();
            if (spatter2Tween != null) spatter2Tween.Kill();
        }
    }
}
