using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Core;
using RobotsGame.Data;
using RobotsGame.Managers;
using RobotsGame.UI;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Controls the Bonus Results Screen - shows results of bonus round voting.
    /// Desktop: Shows vote results and score changes
    /// Mobile: Shows player's votes received and score change
    /// Based on screenspec.md SCREEN: BonusResultsScreen
    /// </summary>
    public class BonusResultsScreenController : MonoBehaviour
    {
        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private Text titleText;
        [SerializeField] private PlayerIconGrid resultsGrid;
        [SerializeField] private Button continueButton;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private Text votesReceivedText;
        [SerializeField] private Text scoreChangeText;

        [Header("Settings")]
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
                    OnContinueClicked();
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
            if (titleText != null)
            {
                titleText.text = "BONUS RESULTS";
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            // TODO: Load and display results
        }

        private void SetupMobile()
        {
            // TODO: Load and display player's votes received
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
