using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
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
        private bool questionDataReady = false;
        private bool panelSequenceStarted = false;
        private bool autoStartWhenDataReady = false;
        private bool autoStartCalculateLocalScores = false;
        private Coroutine waitForQuestionDataCoroutine;

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

            var gameManager = GameManager.Instance;
            bool isMultiplayerGame = gameManager != null && gameManager.IsMultiplayer;

            if (gameManager == null)
            {
                Debug.LogWarning("ResultsScreenController: GameManager instance not found. Waiting for initialization.");
            }

            autoStartWhenDataReady = !isMultiplayerGame;
            autoStartCalculateLocalScores = !isMultiplayerGame;

            if (isMultiplayerGame)
            {
                SubscribeToNetworkEvents();

                Debug.Log("Waiting for server to calculate round scores...");

                DOVirtual.DelayedCall(10f, () =>
                {
                    if (!panelSequenceStarted)
                    {
                        Debug.LogWarning("Timeout waiting for OnFinalRoundScores. Calculating scores locally...");
                        StartPanelSequence(true);
                    }
                });
            }

            waitForQuestionDataCoroutine = StartCoroutine(WaitForQuestionData());

            if (isDesktop)
            {
                SetupDesktop();
            }
            else
            {
                SetupMobile();
            }

            FadeTransition.Instance.FadeIn(0.5f);
        }

        /// <summary>
        /// Calculate scores locally for single-player mode.
        /// </summary>
        private void CalculateLocalScores()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("ResultsScreenController: Cannot calculate local scores without GameManager.");
                return;
            }

            gameManager.CalculateRoundScores();
        }

        private IEnumerator WaitForQuestionData()
        {
            while (!TryCacheGameData())
            {
                yield return null;
            }

            questionDataReady = true;

            if (autoStartWhenDataReady)
            {
                StartPanelSequence(autoStartCalculateLocalScores);
            }

            waitForQuestionDataCoroutine = null;
        }

        private bool TryCacheGameData()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return false;
            }

            currentQuestion = gameManager.CurrentQuestion;
            allAnswers = gameManager.CurrentAnswers;
            allPlayers = gameManager.Players;

            if (currentQuestion == null || allAnswers == null || allPlayers == null)
            {
                return false;
            }

            return true;
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

        private void StartPanelSequence(bool calculateLocalScores)
        {
            if (panelSequenceStarted)
                return;

            if (!TryCacheGameData())
            {
                if (!autoStartWhenDataReady)
                {
                    Debug.Log("ResultsScreenController: Panel sequence requested before question data was ready. Waiting...");
                }

                autoStartWhenDataReady = true;
                autoStartCalculateLocalScores = calculateLocalScores;
                return;
            }

            questionDataReady = true;

            if (calculateLocalScores)
            {
                CalculateLocalScores();
            }

            panelSequenceStarted = true;
            autoStartWhenDataReady = false;

            BeginPanelSequence();
        }

        private void BeginPanelSequence()
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

            if (!questionDataReady || currentQuestion == null || allAnswers == null)
            {
                Debug.LogWarning("ResultsScreenController: Skipping correct answer panel until question data is ready.");
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("ResultsScreenController: GameManager unavailable while showing correct answer panel.");
                return;
            }

            correctAnswerPanel.gameObject.SetActive(true);

            // Get players who got it right
            List<Player> playersWhoGotIt = new List<Player>();
            foreach (var answer in allAnswers)
            {
                if (answer.Type == GameConstants.AnswerType.Player &&
                    AnswerValidator.IsCorrectAnswer(answer.Text, currentQuestion.CorrectAnswer))
                {
                    Player player = gameManager.GetPlayer(answer.PlayerName);
                    if (player != null)
                        playersWhoGotIt.Add(player);
                }
            }

            // Get points value
            var scoring = GameConstants.Scoring.GetScoring(gameManager.GameMode);

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

            if (!questionDataReady || currentQuestion == null || allPlayers == null)
            {
                Debug.LogWarning("ResultsScreenController: Skipping robot decoy panel until question data is ready.");
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("ResultsScreenController: GameManager unavailable while showing robot decoy panel.");
                return;
            }

            robotDecoyPanel.gameObject.SetActive(true);

            var eliminationResults = gameManager.EliminationResults;
            var votingResults = gameManager.VotingResults;

            HashSet<string> notFooledNames = new HashSet<string>();
            if (eliminationResults != null)
            {
                foreach (var kvp in eliminationResults.GetPlayerVotes())
                {
                    if (kvp.Value == currentQuestion.RobotAnswer)
                    {
                        notFooledNames.Add(kvp.Key);
                    }
                }
            }

            HashSet<string> fooledNames = new HashSet<string>();
            if (votingResults != null)
            {
                foreach (var kvp in votingResults.GetPlayerVotes())
                {
                    if (kvp.Value == currentQuestion.RobotAnswer)
                    {
                        fooledNames.Add(kvp.Key);
                    }
                }
            }

            foreach (var name in fooledNames)
            {
                notFooledNames.Remove(name);
            }

            List<Player> notFooled = new List<Player>();
            foreach (var name in notFooledNames)
            {
                var player = gameManager.GetPlayer(name);
                if (player != null)
                {
                    notFooled.Add(player);
                }
            }

            List<Player> fooled = new List<Player>();
            foreach (var name in fooledNames)
            {
                var player = gameManager.GetPlayer(name);
                if (player != null)
                {
                    fooled.Add(player);
                }
            }

            if (notFooled.Count == 0 && fooled.Count == 0)
            {
                notFooled.AddRange(allPlayers);
            }
            else
            {
                foreach (var player in allPlayers)
                {
                    if (!notFooledNames.Contains(player.PlayerName) && !fooledNames.Contains(player.PlayerName))
                    {
                        notFooled.Add(player);
                    }
                }
            }

            var scoring = GameConstants.Scoring.GetScoring(gameManager.GameMode);

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

            if (!questionDataReady || allAnswers == null)
            {
                Debug.LogWarning("ResultsScreenController: Skipping player answers panel until answer data is ready.");
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("ResultsScreenController: GameManager unavailable while showing player answers panel.");
                return;
            }

            playerAnswersPanel.gameObject.SetActive(true);

            Dictionary<string, int> voteCounts = new Dictionary<string, int>();
            var votingResults = gameManager.VotingResults;
            if (votingResults != null)
            {
                foreach (var kvp in votingResults.GetVoteCounts())
                {
                    voteCounts[kvp.Key] = kvp.Value;
                }
            }

            foreach (var answer in allAnswers)
            {
                if (answer.Type == GameConstants.AnswerType.Player && !voteCounts.ContainsKey(answer.Text))
                {
                    voteCounts[answer.Text] = 0;
                }
            }

            var scoring = GameConstants.Scoring.GetScoring(gameManager.GameMode);

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

            if (!questionDataReady)
            {
                Debug.LogWarning("ResultsScreenController: Skipping standings panel until question data is ready.");
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("ResultsScreenController: GameManager unavailable while showing standings panel.");
                return;
            }

            standingsPanel.gameObject.SetActive(true);

            // Get ranked players
            List<Player> rankedPlayers = gameManager.GetRankedPlayers();
            if (rankedPlayers == null)
            {
                Debug.LogWarning("ResultsScreenController: No ranked players available for standings panel.");
                return;
            }

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
                SceneManager.LoadScene(targetScene);
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
            StartPanelSequence(false);
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

            if (waitForQuestionDataCoroutine != null)
            {
                StopCoroutine(waitForQuestionDataCoroutine);
                waitForQuestionDataCoroutine = null;
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
