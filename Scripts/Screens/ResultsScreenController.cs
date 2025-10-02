using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Data;
using RobotsGame.Managers;
using RobotsGame.Network;
using RobotsGame.UI.Results;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Main controller for the Results Screen.
    /// Shows 4 sequential panels: Correct Answer, Robot Decoy, Player Answers, Standings.
    /// Based on unityspec.md SCREEN: ResultsScreen
    /// </summary>
    public class ResultsScreenController : MonoBehaviour
    {
        [Header("Platform Content")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private GameObject mobileContent;

        [Header("Desktop Components")]
        [SerializeField] private Image desktopBackground;
        [SerializeField] private Image roundResultsBanner;
        [SerializeField] private CorrectAnswerPanel correctAnswerPanel;
        [SerializeField] private RobotDecoyPanel robotDecoyPanel;
        [SerializeField] private PlayerAnswersPanel playerAnswersPanel;
        [SerializeField] private StandingsPanel standingsPanel;
        [SerializeField] private GameObject nextRoundButton;
        [SerializeField] private GameObject buttonSlideout;

        [Header("Mobile Components")]
        [SerializeField] private Image mobileBackground;
        // Mobile simplified UI components would go here

        [Header("Settings")]
        [SerializeField] private string nextSceneName = "QuestionScreen";
        [SerializeField] private Sprite[] roundResultsBanners = new Sprite[12];
        [SerializeField] private float panelHoldDuration = 5f;
        [SerializeField] private float buttonSlideDelay = 0.8f;

        private ResponsiveUI responsiveUI;
        private bool isDesktop;
        private Question currentQuestion;
        private List<Answer> allAnswers;
        private List<Player> allPlayers;
        private int currentPanel = 0;
        private bool canProceed = false;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            responsiveUI = GetComponentInParent<ResponsiveUI>();
            if (responsiveUI == null)
                responsiveUI = FindObjectOfType<ResponsiveUI>();

            isDesktop = responsiveUI != null ? responsiveUI.IsDesktop : (Screen.width > GameConstants.UI.MobileMaxWidth);

            SetupPlatformContent();
        }

        private void Start()
        {
            // Get data from GameManager
            currentQuestion = GameManager.Instance.CurrentQuestion;
            allAnswers = GameManager.Instance.CurrentAnswers;
            allPlayers = GameManager.Instance.Players;

            // Subscribe to network events for multiplayer
            if (GameManager.Instance.IsMultiplayer)
            {
                SubscribeToNetworkEvents();

                // Wait for OnFinalRoundScores before showing panels
                Debug.Log("Waiting for server to calculate round scores...");

                // Add timeout fallback (10 seconds)
                DOVirtual.DelayedCall(10f, () =>
                {
                    if (currentPanel == 0) // Still waiting for results
                    {
                        Debug.LogWarning("Timeout waiting for OnFinalRoundScores. Calculating scores locally...");
                        CalculateLocalScores();
                        StartPanelSequence();
                    }
                });
            }
            else
            {
                // Single-player: calculate scores locally and show panels
                CalculateLocalScores();
                StartPanelSequence();
            }

            if (isDesktop)
            {
                SetupDesktop();
            }
            else
            {
                SetupMobile();
            }

            // Fade in
            FadeTransition.Instance.FadeIn(0.5f);
        }

        /// <summary>
        /// Calculate scores locally for single-player mode.
        /// </summary>
        private void CalculateLocalScores()
        {
            // Create mock vote results for single-player
            VoteResults eliminationResults = new VoteResults();
            VoteResults votingResults = new VoteResults();

            // Calculate round scores
            GameManager.Instance.CalculateRoundScores(eliminationResults, votingResults);
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
            // Set round banner
            int round = GameManager.Instance.CurrentRound;
            if (roundResultsBanner != null && round > 0 && round <= roundResultsBanners.Length)
            {
                roundResultsBanner.sprite = roundResultsBanners[round - 1];
                roundResultsBanner.transform.rotation = Quaternion.Euler(0, 0, -8f);
            }

            // Hide panels initially
            if (correctAnswerPanel != null)
                correctAnswerPanel.gameObject.SetActive(false);

            if (robotDecoyPanel != null)
                robotDecoyPanel.gameObject.SetActive(false);

            if (playerAnswersPanel != null)
                playerAnswersPanel.gameObject.SetActive(false);

            if (standingsPanel != null)
                standingsPanel.gameObject.SetActive(false);

            // Hide button initially
            if (nextRoundButton != null)
                nextRoundButton.SetActive(false);

            if (buttonSlideout != null)
                buttonSlideout.SetActive(false);

            // Set panel hold durations
            if (correctAnswerPanel != null)
                correctAnswerPanel.SetHoldDuration(panelHoldDuration);

            if (robotDecoyPanel != null)
                robotDecoyPanel.SetHoldDuration(panelHoldDuration);

            if (playerAnswersPanel != null)
                playerAnswersPanel.SetHoldDuration(panelHoldDuration);

            if (standingsPanel != null)
                standingsPanel.SetHoldDuration(panelHoldDuration);
        }

        private void SetupMobile()
        {
            // Mobile simplified UI setup
            // Would show single-screen summary or auto-advance
        }

        // ===========================
        // PANEL SEQUENCE
        // ===========================

        private void StartPanelSequence()
        {
            currentPanel = 0;
            ShowNextPanel();
        }

        private void ShowNextPanel()
        {
            currentPanel++;

            switch (currentPanel)
            {
                case 1:
                    ShowCorrectAnswerPanel();
                    break;

                case 2:
                    ShowRobotDecoyPanel();
                    break;

                case 3:
                    ShowPlayerAnswersPanel();
                    break;

                case 4:
                    ShowStandingsPanel();
                    break;

                default:
                    ShowNextRoundButton();
                    break;
            }
        }

        // ===========================
        // PANEL 1: CORRECT ANSWER
        // ===========================

        private void ShowCorrectAnswerPanel()
        {
            if (correctAnswerPanel == null) return;

            correctAnswerPanel.gameObject.SetActive(true);

            // Get players who got it right
            List<Player> playersWhoGotIt = new List<Player>();
            foreach (var answer in allAnswers)
            {
                if (answer.Type == GameConstants.AnswerType.Player &&
                    AnswerValidator.IsCorrectAnswer(answer.Text, currentQuestion.CorrectAnswer))
                {
                    Player player = GameManager.Instance.GetPlayer(answer.PlayerName);
                    if (player != null)
                        playersWhoGotIt.Add(player);
                }
            }

            // Get points value
            var scoring = GameConstants.Scoring.GetScoring(GameManager.Instance.GameMode);

            correctAnswerPanel.ShowPanel(currentQuestion.CorrectAnswer, playersWhoGotIt, scoring.correct, () =>
            {
                correctAnswerPanel.Hide(() =>
                {
                    ShowNextPanel();
                });
            });
        }

        // ===========================
        // PANEL 2: ROBOT DECOY
        // ===========================

        private void ShowRobotDecoyPanel()
        {
            if (robotDecoyPanel == null) return;

            robotDecoyPanel.gameObject.SetActive(true);

            // Get players who were not fooled vs fooled
            List<Player> notFooled = new List<Player>();
            List<Player> fooled = new List<Player>();

            // Simplified: In real game, would check elimination votes
            // For now, assume all players not fooled
            notFooled.AddRange(allPlayers);

            var scoring = GameConstants.Scoring.GetScoring(GameManager.Instance.GameMode);

            robotDecoyPanel.ShowPanel(currentQuestion.RobotAnswer, notFooled, fooled,
                                     scoring.robotId, scoring.fooled, () =>
            {
                robotDecoyPanel.Hide(() =>
                {
                    ShowNextPanel();
                });
            });
        }

        // ===========================
        // PANEL 3: PLAYER ANSWERS
        // ===========================

        private void ShowPlayerAnswersPanel()
        {
            if (playerAnswersPanel == null) return;

            playerAnswersPanel.gameObject.SetActive(true);

            // Get vote counts (simplified - would come from voting phase)
            Dictionary<string, int> voteCounts = new Dictionary<string, int>();
            foreach (var answer in allAnswers)
            {
                if (answer.Type == GameConstants.AnswerType.Player)
                {
                    voteCounts[answer.Text] = Random.Range(0, 3); // Mock votes
                }
            }

            var scoring = GameConstants.Scoring.GetScoring(GameManager.Instance.GameMode);

            playerAnswersPanel.ShowPanel(allAnswers, voteCounts, scoring.vote, () =>
            {
                playerAnswersPanel.Hide(() =>
                {
                    ShowNextPanel();
                });
            });
        }

        // ===========================
        // PANEL 4: STANDINGS
        // ===========================

        private void ShowStandingsPanel()
        {
            if (standingsPanel == null) return;

            standingsPanel.gameObject.SetActive(true);

            // Get ranked players
            List<Player> rankedPlayers = GameManager.Instance.GetRankedPlayers();

            standingsPanel.ShowPanel(rankedPlayers, () =>
            {
                ShowNextPanel();
            });
        }

        // ===========================
        // NEXT ROUND BUTTON
        // ===========================

        private void ShowNextRoundButton()
        {
            canProceed = true;

            if (nextRoundButton == null) return;

            // Show slideout background
            if (buttonSlideout != null)
            {
                buttonSlideout.SetActive(true);
                RectTransform slideoutRect = buttonSlideout.GetComponent<RectTransform>();
                if (slideoutRect != null)
                {
                    Vector2 startPos = slideoutRect.anchoredPosition;
                    startPos.x = -800f;
                    slideoutRect.anchoredPosition = startPos;

                    slideoutRect.DOAnchorPosX(50f, buttonSlideDelay).SetEase(Ease.InOutQuad);
                }
            }

            // Show button
            DOVirtual.DelayedCall(buttonSlideDelay, () =>
            {
                nextRoundButton.SetActive(true);

                Button btn = nextRoundButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(OnNextRoundClicked);
                }

                // Add ButtonEffects if not present
                ButtonEffects effects = nextRoundButton.GetComponent<ButtonEffects>();
                if (effects == null)
                {
                    nextRoundButton.AddComponent<ButtonEffects>();
                }
            });
        }

        private void OnNextRoundClicked()
        {
            if (!canProceed) return;

            canProceed = false;

            // Check if last round
            bool isLastRound = GameManager.Instance.IsLastRound();

            string targetScene = isLastRound ? "FinalResults" : nextSceneName;

            FadeTransition.Instance.FadeOut(0.5f, () =>
            {
                Debug.Log($"Transitioning to {targetScene}");
                if (!SceneLoader.TryLoadScene(targetScene))
                {
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        FadeTransition.Instance.FadeIn(1f);
                    });
                }
            });
        }

        // ===========================
        // NETWORK INTEGRATION
        // ===========================

        /// <summary>
        /// Subscribe to network events for multiplayer.
        /// </summary>
        private void SubscribeToNetworkEvents()
        {
            var net = NetworkManager.Instance;
            if (net == null) return;

            net.OnFinalRoundScores.AddListener(HandleFinalRoundScores);
        }

        /// <summary>
        /// Unsubscribe from network events.
        /// </summary>
        private void UnsubscribeFromNetworkEvents()
        {
            var net = NetworkManager.Instance;
            if (net == null) return;

            net.OnFinalRoundScores.RemoveListener(HandleFinalRoundScores);
        }

        /// <summary>
        /// Handles final round scores from server.
        /// </summary>
        private void HandleFinalRoundScores(string jsonData)
        {
            Debug.Log("Received round scores from server. Starting panel sequence...");

            // Server has calculated all scores, safe to show results
            StartPanelSequence();
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            // Unsubscribe from network events
            if (GameManager.Instance != null && GameManager.Instance.IsMultiplayer)
            {
                UnsubscribeFromNetworkEvents();
            }

            // Unsubscribe from UI events
            if (nextRoundButton != null)
            {
                Button btn = nextRoundButton.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.RemoveAllListeners();
            }

            DOTween.Kill(this);
        }
    }
}
