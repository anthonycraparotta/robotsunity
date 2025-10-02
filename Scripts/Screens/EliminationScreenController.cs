using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Data;
using RobotsGame.Managers;
using RobotsGame.Network;
using RobotsGame.UI;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Main controller for the Elimination Screen.
    /// Players vote to eliminate the robot answer.
    /// Based on unityspec.md SCREEN: EliminationScreen
    /// </summary>
    public class EliminationScreenController : MonoBehaviour
    {
        [Header("Platform Content")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private GameObject mobileContent;

        [Header("Desktop Components")]
        [SerializeField] private Image desktopBackground;
        [SerializeField] private Image heroImage;
        [SerializeField] private EliminationAnswerList answerListDesktop;
        [SerializeField] private EliminationResultPanel resultPanel;
        [SerializeField] private TimerDisplay timerDisplay;
        [SerializeField] private TextMeshProUGUI instructionText;

        [Header("Mobile Components")]
        [SerializeField] private Image mobileBackground;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private EliminationAnswerList answerListMobile;
        [SerializeField] private Button eliminateButton;
        [SerializeField] private TextMeshProUGUI eliminateButtonText;
        [SerializeField] private Image eliminateButtonImage;
        [SerializeField] private CanvasGroup eliminateButtonCanvasGroup;
        [SerializeField] private TimerDisplay timerDisplayMobile;

        [Header("Settings")]
        [SerializeField] private string nextSceneName = "VotingScreen";
        [SerializeField] private float timerDuration = 30f;
        [SerializeField] private float eliminatedHighlightDuration = 4f; // Desktop
        [SerializeField] private float eliminatedHighlightDurationMobile = 2f;

        private ResponsiveUI responsiveUI;
        private bool isDesktop;
        private List<Answer> allAnswers;
        private string selectedAnswer;
        private bool hasVoted = false;
        private VoteResults votingResults;
        private Player localPlayer;

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
            // Get answers from GameManager
            allAnswers = GameManager.Instance.CurrentAnswers;

            // Get local player (simplified - in real game would get from session)
            localPlayer = GameManager.Instance.Players.Count > 0
                ? GameManager.Instance.Players[0]
                : new Player("Player 1", "icon1");

            // Find player's answer
            string playerAnswer = "";
            foreach (var answer in allAnswers)
            {
                if (answer.PlayerName == localPlayer.PlayerName)
                {
                    playerAnswer = answer.Text;
                    break;
                }
            }

            // Subscribe to network events for multiplayer
            if (GameManager.Instance.IsMultiplayer)
            {
                SubscribeToNetworkEvents();
            }

            // Setup UI
            if (isDesktop)
            {
                SetupDesktop(playerAnswer);
            }
            else
            {
                SetupMobile(playerAnswer);
            }

            // Start timer
            StartTimer();

            // Fade in
            FadeTransition.Instance.FadeIn(0.5f);
        }

        private void Update()
        {
            // Check timer expiration
            TimerDisplay activeTimer = isDesktop ? timerDisplay : timerDisplayMobile;
            if (activeTimer != null && activeTimer.IsExpired && !hasVoted)
            {
                AutoSubmitVote();
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

        private void SetupDesktop(string playerAnswer)
        {
            // Setup answer list
            if (answerListDesktop != null)
            {
                answerListDesktop.OnAnswerSelected += OnAnswerSelectedDesktop;
                answerListDesktop.SetupAnswers(allAnswers, playerAnswer, filterOwnAnswer: false);
            }

            // Desktop auto-submits on selection (no separate button)
            // Or could wait for Enter key
        }

        private void SetupMobile(string playerAnswer)
        {
            // Setup header
            if (headerText != null)
            {
                headerText.text = "ELIMINATE THE ROBOT";
            }

            // Setup answer list (filter own answer on mobile)
            if (answerListMobile != null)
            {
                answerListMobile.OnAnswerSelected += OnAnswerSelectedMobile;
                answerListMobile.SetupAnswers(allAnswers, playerAnswer, filterOwnAnswer: true);
            }

            // Setup eliminate button
            if (eliminateButton != null)
            {
                eliminateButton.onClick.AddListener(OnEliminateButtonClicked);
                UpdateEliminateButton();
            }
        }

        private void StartTimer()
        {
            if (isDesktop && timerDisplay != null)
            {
                timerDisplay.SetTime(timerDuration);
                timerDisplay.StartTimer(1f); // 1s delay
            }
            else if (!isDesktop && timerDisplayMobile != null)
            {
                timerDisplayMobile.SetTime(timerDuration);
                timerDisplayMobile.StartTimer(0f);
            }
        }

        // ===========================
        // INTERACTION
        // ===========================

        private void OnAnswerSelectedDesktop(string answer)
        {
            selectedAnswer = answer;

            // Desktop auto-submits on selection
            if (!hasVoted)
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    SubmitVote();
                });
            }
        }

        private void OnAnswerSelectedMobile(string answer)
        {
            selectedAnswer = answer;
            UpdateEliminateButton();
        }

        private void OnEliminateButtonClicked()
        {
            if (string.IsNullOrEmpty(selectedAnswer) || hasVoted)
                return;

            SubmitVote();
        }

        private void UpdateEliminateButton()
        {
            if (eliminateButton == null) return;

            bool canEliminate = !string.IsNullOrEmpty(selectedAnswer) && !hasVoted;

            eliminateButton.interactable = canEliminate;

            if (eliminateButtonCanvasGroup != null)
            {
                eliminateButtonCanvasGroup.alpha = canEliminate ? 1f : 0.5f;
            }

            if (eliminateButtonText != null)
            {
                if (hasVoted)
                    eliminateButtonText.text = "CALCULATING VOTES";
                else if (canEliminate)
                    eliminateButtonText.text = "ELIMINATE THIS ANSWER";
                else
                    eliminateButtonText.text = "SELECT AN ANSWER";
            }

            // Update button color
            if (eliminateButtonImage != null)
            {
                Color targetColor = canEliminate
                    ? GameConstants.Colors.BrightRed
                    : GameConstants.Colors.Grey;

                eliminateButtonImage.color = targetColor;
            }
        }

        // ===========================
        // VOTING
        // ===========================

        private void SubmitVote()
        {
            if (hasVoted) return;

            hasVoted = true;

            // Record vote in GameManager
            GameManager.Instance.RecordEliminationVote(localPlayer.PlayerName, selectedAnswer);

            // Send to server in multiplayer mode
            if (GameManager.Instance.IsMultiplayer)
            {
                NetworkManager.Instance?.SubmitEliminationVote(selectedAnswer);

                // Wait for OnEliminationComplete event from server
                Debug.Log($"Elimination vote sent: {selectedAnswer}. Waiting for server results...");
            }
            else
            {
                // Single-player: simulate immediate results
                votingResults = new VoteResults();
                votingResults.AddVote(selectedAnswer);
                SimulateVotingResults();
            }

            // Pause timer
            TimerDisplay activeTimer = isDesktop ? timerDisplay : timerDisplayMobile;
            if (activeTimer != null)
                activeTimer.PauseTimer();

            // Disable voting
            if (isDesktop && answerListDesktop != null)
                answerListDesktop.DisableVoting();
            else if (!isDesktop && answerListMobile != null)
            {
                answerListMobile.DisableVoting();
                UpdateEliminateButton();
            }

            // Show results after delay only when running offline
            if (!GameManager.Instance.IsMultiplayer)
            {
                DOVirtual.DelayedCall(1f, () =>
                {
                    ShowResults();
                });
            }
        }

        private void AutoSubmitVote()
        {
            // Submit selected answer if any, otherwise don't vote
            if (!string.IsNullOrEmpty(selectedAnswer))
            {
                SubmitVote();
            }
            else
            {
                hasVoted = true;
                UpdateEliminateButton();
            }
        }

        private void SimulateVotingResults()
        {
            // Simplified - in real game, server aggregates all votes
            // For now, just eliminate the selected answer
            votingResults.CalculateElimination();

            TrackEliminatedAnswer();

            // If no elimination calculated (because only one vote), set it
            if (votingResults.EliminatedAnswer == null && !votingResults.TieOccurred)
            {
                // Simulate that our vote was enough
                votingResults.AddVote(selectedAnswer);
                votingResults.CalculateElimination();

                TrackEliminatedAnswer();
            }
        }

        private void TrackEliminatedAnswer()
        {
            if (votingResults == null || votingResults.TieOccurred)
                return;

            string eliminatedAnswer = votingResults.EliminatedAnswer;
            if (string.IsNullOrEmpty(eliminatedAnswer))
                return;

            GameManager.Instance.CurrentQuestion?.AddEliminatedAnswer(eliminatedAnswer);
        }

        // ===========================
        // RESULTS
        // ===========================

        private void ShowResults()
        {
            if (votingResults == null)
            {
                Debug.LogWarning("Elimination results not available yet.");
                return;
            }

            if (!votingResults.TieOccurred && string.IsNullOrEmpty(votingResults.EliminatedAnswer))
            {
                Debug.LogWarning("Elimination results missing eliminated answer.");
                return;
            }

            float highlightDuration = isDesktop ? eliminatedHighlightDuration : eliminatedHighlightDurationMobile;

            if (votingResults.TieOccurred)
            {
                ShowTieResult();
            }
            else
            {
                ShowEliminationResult(votingResults.EliminatedAnswer, highlightDuration);
            }
        }

        private void ShowEliminationResult(string eliminatedAnswer, float highlightDuration)
        {
            // Highlight eliminated answer in list
            if (isDesktop && answerListDesktop != null)
            {
                answerListDesktop.HighlightEliminatedAnswer(eliminatedAnswer, highlightDuration);
            }
            else if (!isDesktop && answerListMobile != null)
            {
                answerListMobile.HighlightEliminatedAnswer(eliminatedAnswer, highlightDuration);
            }

            // Wait for highlight, then show result panel
            DOVirtual.DelayedCall(highlightDuration, () =>
            {
                // Fade out answer list
                CanvasGroup answerListCG = isDesktop
                    ? answerListDesktop?.GetComponent<CanvasGroup>()
                    : answerListMobile?.GetComponent<CanvasGroup>();

                if (answerListCG != null)
                {
                    answerListCG.DOFade(0f, 0.5f);
                }

                // Show result panel (desktop only has full panel)
                if (isDesktop && resultPanel != null)
                {
                    resultPanel.ShowNormalResult(eliminatedAnswer, () =>
                    {
                        TransitionToNextScreen();
                    });
                }
                else
                {
                    // Mobile: simpler result display or skip to transition
                    AudioManager.Instance.PlayVoiceOver(GameConstants.Audio.VO_RobotAnswerGone);

                    DOVirtual.DelayedCall(3f, () =>
                    {
                        TransitionToNextScreen();
                    });
                }
            });
        }

        private void ShowTieResult()
        {
            // Show tie result panel
            if (isDesktop && resultPanel != null)
            {
                resultPanel.ShowTieResult(() =>
                {
                    TransitionToNextScreen();
                });
            }
            else
            {
                // Mobile: simpler tie display
                AudioManager.Instance.PlayVoiceOver(GameConstants.Audio.VO_NoRobotAnswerGone);

                if (headerText != null)
                {
                    headerText.text = "TIE VOTE!";
                    headerText.color = GameConstants.Colors.PrimaryYellow;
                }

                DOVirtual.DelayedCall(3f, () =>
                {
                    TransitionToNextScreen();
                });
            }
        }

        // ===========================
        // SCENE TRANSITION
        // ===========================

        private void TransitionToNextScreen()
        {
            FadeTransition.Instance.FadeOut(0.5f, () =>
            {
                Debug.Log($"Transitioning to {nextSceneName}");
                SceneManager.LoadScene(nextSceneName);
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

            net.OnEliminationVoteCast.AddListener(HandleEliminationVoteCast);
            net.OnEliminationComplete.AddListener(HandleEliminationComplete);
        }

        /// <summary>
        /// Unsubscribe from network events.
        /// </summary>
        private void UnsubscribeFromNetworkEvents()
        {
            var net = NetworkManager.Instance;
            if (net == null) return;

            net.OnEliminationVoteCast.RemoveListener(HandleEliminationVoteCast);
            net.OnEliminationComplete.RemoveListener(HandleEliminationComplete);
        }

        /// <summary>
        /// Handles elimination vote cast notification from server.
        /// </summary>
        private void HandleEliminationVoteCast(string jsonData)
        {
            var data = JsonUtility.FromJson<EliminationVoteCastData>(jsonData);
            Debug.Log($"Player {data.playerName} voted in elimination");
            // Could show visual feedback here if desired
        }

        /// <summary>
        /// Handles elimination complete from server.
        /// </summary>
        private void HandleEliminationComplete(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogWarning("Received empty elimination complete payload.");
                return;
            }

            var data = JsonUtility.FromJson<EliminationCompleteData>(jsonData);
            if (data == null)
            {
                Debug.LogWarning("Failed to parse elimination complete payload.");
                return;
            }

            votingResults = new VoteResults();

            if (data.voteCounts != null)
            {
                foreach (var entry in data.voteCounts)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.answer) || entry.count <= 0)
                        continue;

                    for (int i = 0; i < entry.count; i++)
                    {
                        votingResults.AddVote(entry.answer);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(data.eliminatedAnswer))
            {
                // Fallback: ensure eliminated answer is represented
                votingResults.AddVote(data.eliminatedAnswer);
            }

            votingResults.CalculateElimination();
            TrackEliminatedAnswer();

            // Ensure server provided tie flag is respected if counts were missing
            if (data.tieOccurred && !votingResults.TieOccurred)
            {
                Debug.LogWarning("Server reported tie but vote counts did not reflect it.");
            }

            ShowResults();
        }

        // ===========================
        // DATA CLASSES
        // ===========================

        [System.Serializable]
        private class EliminationVoteCastData
        {
            public string playerName;
            public string vote;
        }

        [System.Serializable]
        private class EliminationCompleteData
        {
            public string eliminatedAnswer;
            public bool tieOccurred;
            public VoteCountData[] voteCounts;
        }

        [System.Serializable]
        private class VoteCountData
        {
            public string answer;
            public int count;
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
            if (answerListDesktop != null)
                answerListDesktop.OnAnswerSelected -= OnAnswerSelectedDesktop;

            if (answerListMobile != null)
                answerListMobile.OnAnswerSelected -= OnAnswerSelectedMobile;

            if (eliminateButton != null)
                eliminateButton.onClick.RemoveAllListeners();

            DOTween.Kill(this);
        }
    }
}
