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
    /// Controls the Halftime Results Screen - shows mid-game standings after round 4 (8-game) or round 6 (12-game).
    /// Desktop: Shows current standings with Continue button
    /// Mobile: Shows player placement and score
    /// Based on screenspec.md SCREEN: HalftimeResultsScreen
    /// </summary>
    public class HalftimeResultsScreenController : MonoBehaviour
    {
        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private Text titleText;
        [SerializeField] private PlayerIconGrid standingsGrid;
        [SerializeField] private Button continueButton;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text placementText;
        [SerializeField] private Text scoreText;

        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private string nextSceneName = "RoundArtScreen";

        private ResponsiveUI responsiveUI;
        private bool isTransitioning = false;
        private bool isDesktop = true;
        private List<RoundScore> currentStandings = new List<RoundScore>();

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
                titleText.text = "HALFTIME";
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            // TODO: Load and display standings
        }

        private void SetupMobile()
        {
            // TODO: Load and display player's placement and score
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
