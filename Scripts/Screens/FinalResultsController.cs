using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
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
    /// Main controller for the Final Results Screen.
    /// Shows winner and last place with action buttons.
    /// Based on unityspec.md SCREEN: FinalResults
    /// </summary>
    public class FinalResultsController : MonoBehaviour
    {
        [Header("Platform Content")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private GameObject mobileContent;

        [Header("Desktop Components")]
        [SerializeField] private Image desktopBackground;
        [SerializeField] private Image heroImage;
        [SerializeField] private PlayerSpotlightCard winnerCard;
        [SerializeField] private PlayerSpotlightCard lastPlaceCard;
        [SerializeField] private GameObject actionButtons;
        [SerializeField] private GameObject buttonSlideout;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button creditsButton;

        [Header("Mobile Components")]
        [SerializeField] private Image mobileBackground;
        [SerializeField] private Image playerIcon;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI placementText;
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private Button shareResultsButton;
        [SerializeField] private Button gamesButton;

        [Header("Settings")]
        [SerializeField] private float heroFadeDelay = 0.5f;
        [SerializeField] private float winnerCardDelay = 1f;
        [SerializeField] private float lastPlaceCardDelay = 1.1f; // 100ms after winner
        [SerializeField] private float buttonsSlideDelay = 2.5f;
        [SerializeField] private float buttonSlideDuration = 0.8f;

        private ResponsiveUI responsiveUI;
        private bool isDesktop;
        private List<Player> rankedPlayers;
        private Player localPlayer;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            responsiveUI = GetComponentInParent<ResponsiveUI>();
            if (responsiveUI == null)
                responsiveUI = FindObjectOfType<ResponsiveUI>();
        }

        private void Start()
        {
            InitializePlatform();

            // Get ranked players
            rankedPlayers = GameManager.Instance.GetRankedPlayers();

            // Get local player
            localPlayer = GameManager.Instance.Players.Count > 0
                ? GameManager.Instance.Players[0]
                : new Player("Player 1", "icon1");

            if (isDesktop)
            {
                SetupDesktop();
            }
            else
            {
                SetupMobile();
            }

            // Fade in
            FadeTransition.Instance.FadeIn(0.5f, () =>
            {
                StartAnimations();
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
            // Hide cards initially
            if (winnerCard != null)
                winnerCard.gameObject.SetActive(false);

            if (lastPlaceCard != null)
                lastPlaceCard.gameObject.SetActive(false);

            // Hide buttons initially
            if (actionButtons != null)
                actionButtons.SetActive(false);

            if (buttonSlideout != null)
                buttonSlideout.SetActive(false);

            // Hero image starts hidden
            if (heroImage != null)
            {
                CanvasGroup heroCG = heroImage.GetComponent<CanvasGroup>();
                if (heroCG == null)
                    heroCG = heroImage.gameObject.AddComponent<CanvasGroup>();
                heroCG.alpha = 0f;
            }

            // Setup button listeners
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
                ButtonEffects effects = newGameButton.GetComponent<ButtonEffects>();
                if (effects == null)
                    newGameButton.gameObject.AddComponent<ButtonEffects>();
            }

            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(OnCreditsClicked);
                ButtonEffects effects = creditsButton.GetComponent<ButtonEffects>();
                if (effects == null)
                    creditsButton.gameObject.AddComponent<ButtonEffects>();
            }
        }

        private void SetupMobile()
        {
            // Find local player's rank
            int playerRank = 1;
            for (int i = 0; i < rankedPlayers.Count; i++)
            {
                if (rankedPlayers[i].PlayerName == localPlayer.PlayerName)
                {
                    playerRank = i + 1;
                    break;
                }
            }

            // Setup player info
            if (playerIcon != null)
            {
                Sprite iconSprite = Resources.Load<Sprite>($"PlayerIcons/{localPlayer.Icon}");
                if (iconSprite != null)
                    playerIcon.sprite = iconSprite;
            }

            if (playerNameText != null)
            {
                playerNameText.text = localPlayer.PlayerName.ToUpper();
            }

            if (placementText != null)
            {
                string suffix = GetPlacementSuffix(playerRank);
                placementText.text = $"{playerRank}{suffix} place of {rankedPlayers.Count} players";
            }

            if (totalScoreText != null)
            {
                totalScoreText.text = localPlayer.Score.ToString();
            }

            // Setup button listeners
            if (shareResultsButton != null)
            {
                shareResultsButton.onClick.AddListener(OnShareResultsClicked);
            }

            if (gamesButton != null)
            {
                gamesButton.onClick.AddListener(OnGamesClicked);
            }
        }

        private string GetPlacementSuffix(int rank)
        {
            if (rank == 1) return "st";
            if (rank == 2) return "nd";
            if (rank == 3) return "rd";
            return "th";
        }

        // ===========================
        // ANIMATIONS
        // ===========================

        private void StartAnimations()
        {
            if (isDesktop)
            {
                AnimateDesktop();
            }
            else
            {
                AnimateMobile();
            }
        }

        private void AnimateDesktop()
        {
            // Hero image fade in
            if (heroImage != null)
            {
                CanvasGroup heroCG = heroImage.GetComponent<CanvasGroup>();
                if (heroCG != null)
                {
                    DOVirtual.DelayedCall(heroFadeDelay, () =>
                    {
                        heroCG.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
                    });
                }
            }

            // Winner card
            if (winnerCard != null && rankedPlayers.Count > 0)
            {
                Player winner = rankedPlayers[0];
                winnerCard.gameObject.SetActive(true);
                winnerCard.ShowCard(winner, 1, rankedPlayers.Count, PlayerSpotlightCard.SpotlightType.Winner, winnerCardDelay);
            }

            // Last place card (if more than one player)
            if (lastPlaceCard != null && rankedPlayers.Count > 1)
            {
                Player lastPlace = rankedPlayers[rankedPlayers.Count - 1];
                lastPlaceCard.gameObject.SetActive(true);
                lastPlaceCard.ShowCard(lastPlace, rankedPlayers.Count, rankedPlayers.Count,
                                      PlayerSpotlightCard.SpotlightType.LastPlace, lastPlaceCardDelay);
            }

            // Action buttons slide in
            DOVirtual.DelayedCall(buttonsSlideDelay, () =>
            {
                ShowActionButtons();
            });
        }

        private void AnimateMobile()
        {
            // Mobile is simpler - everything shows immediately
            // Could add stagger effects if desired
        }

        private void ShowActionButtons()
        {
            if (actionButtons == null) return;

            // Show slideout background
            if (buttonSlideout != null)
            {
                buttonSlideout.SetActive(true);
                RectTransform slideoutRect = buttonSlideout.GetComponent<RectTransform>();
                if (slideoutRect != null)
                {
                    Vector2 startPos = slideoutRect.anchoredPosition;
                    startPos.x = -700f;
                    slideoutRect.anchoredPosition = startPos;

                    slideoutRect.DOAnchorPosX(50f, buttonSlideDuration).SetEase(Ease.InOutQuad);
                }
            }

            // Show buttons
            DOVirtual.DelayedCall(buttonSlideDuration * 0.5f, () =>
            {
                actionButtons.SetActive(true);
            });
        }

        // ===========================
        // BUTTON HANDLERS
        // ===========================

        private void OnNewGameClicked()
        {
            // Reset game and return to landing page
            FadeTransition.Instance.FadeOut(0.5f, () =>
            {
                GameManager.Instance.ResetGame();
                Debug.Log("Transitioning to LandingPage (new game)");
                SceneManager.LoadScene("LandingPage");
            });
        }

        private void OnCreditsClicked()
        {
            Debug.Log("Credits clicked - would show credits screen");
            // Could show modal or transition to credits scene
        }

        private void OnShareResultsClicked()
        {
            Debug.Log("Share Results clicked - would open share dialog");
            // Mobile share functionality
        }

        private void OnGamesClicked()
        {
            Debug.Log("Games clicked - would return to game selection");
            // Return to game lobby or list
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            if (newGameButton != null)
                newGameButton.onClick.RemoveAllListeners();

            if (creditsButton != null)
                creditsButton.onClick.RemoveAllListeners();

            if (shareResultsButton != null)
                shareResultsButton.onClick.RemoveAllListeners();

            if (gamesButton != null)
                gamesButton.onClick.RemoveAllListeners();

            DOTween.Kill(this);
        }
    }
}
