using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Data;
using RobotsGame.Managers;
using RobotsGame.UI;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Controls the Winner/Loser Screen - dramatic reveal of winner and last place.
    /// Desktop: Shows winner and loser spotlights with Continue button
    /// Mobile: Black screen (desktop only feature)
    /// Based on screenspec.md SCREEN: WinnerLoserScreen
    /// </summary>
    public class WinnerLoserScreenController : MonoBehaviour
    {
        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private PlayerSpotlightCard winnerSpotlight;
        [SerializeField] private PlayerSpotlightCard loserSpotlight;
        [SerializeField] private Button continueButton;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;

        [Header("Settings")]
        [SerializeField] private float winnerSpotlightDelay = 1f;
        [SerializeField] private float loserSpotlightDelay = 3f;
        [SerializeField] private float continueButtonDelay = 5f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private string nextSceneName = "FinalResults";

        private ResponsiveUI responsiveUI;
        private bool isTransitioning = false;
        private bool isDesktop = true;

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
            if (isDesktop && !isTransitioning)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    if (continueButton != null && continueButton.interactable)
                    {
                        OnContinueClicked();
                    }
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
            // Hide spotlights initially
            if (winnerSpotlight != null)
            {
                winnerSpotlight.gameObject.SetActive(false);
            }
            if (loserSpotlight != null)
            {
                loserSpotlight.gameObject.SetActive(false);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
                continueButton.interactable = false;
                continueButton.gameObject.SetActive(false);
            }

            // Animate spotlights in sequence
            DOVirtual.DelayedCall(winnerSpotlightDelay, () =>
            {
                ShowWinnerSpotlight();
            });

            DOVirtual.DelayedCall(loserSpotlightDelay, () =>
            {
                ShowLoserSpotlight();
            });

            DOVirtual.DelayedCall(continueButtonDelay, () =>
            {
                ShowContinueButton();
            });
        }

        private void SetupMobile()
        {
            // Mobile shows black screen
        }

        private void ShowWinnerSpotlight()
        {
            if (winnerSpotlight == null) return;

            // TODO: Load winner data and show spotlight
            winnerSpotlight.gameObject.SetActive(true);
            winnerSpotlight.transform.localScale = Vector3.zero;
            winnerSpotlight.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }

        private void ShowLoserSpotlight()
        {
            if (loserSpotlight == null) return;

            // TODO: Load loser data and show spotlight
            loserSpotlight.gameObject.SetActive(true);
            loserSpotlight.transform.localScale = Vector3.zero;
            loserSpotlight.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }

        private void ShowContinueButton()
        {
            if (continueButton == null) return;

            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
        }

        private void OnContinueClicked()
        {
            if (isTransitioning) return;

            isTransitioning = true;

            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }

            FadeTransition.Instance.FadeOut(fadeOutDuration, () =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
            if (continueButton != null)
                continueButton.onClick.RemoveAllListeners();
        }
    }
}
