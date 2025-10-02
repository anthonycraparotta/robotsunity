using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    /// Main controller for the Voting Screen.
    /// Players vote for the correct answer from remaining options.
    /// Based on unityspec.md SCREEN: VotingScreen
    /// </summary>
    public class VotingScreenController : MonoBehaviour
    {
        [Header("Platform Content")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private GameObject mobileContent;

        [Header("Desktop Components")]
        [SerializeField] private Image desktopBackground;
        [SerializeField] private Image heroImage;
        [SerializeField] private VotingAnswerList answerListDesktop;
        [SerializeField] private TimerDisplay timerDisplay;
        [SerializeField] private TextMeshProUGUI instructionText;

        [Header("Mobile Components")]
        [SerializeField] private Image mobileBackground;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private VotingAnswerList answerListMobile;
        [SerializeField] private Button submitButton;
        [SerializeField] private TextMeshProUGUI submitButtonText;
        [SerializeField] private Image submitButtonImage;
        [SerializeField] private CanvasGroup submitButtonCanvasGroup;
        [SerializeField] private TimerDisplay timerDisplayMobile;

        [Header("Settings")]
        [SerializeField] private string nextSceneName = "ResultsScreen";
        [SerializeField] private float timerDuration = 30f;
        [SerializeField] private float correctAnswerHoldDuration = 4f;

        private ResponsiveUI responsiveUI;
        private bool isDesktop;
        private List<Answer> remainingAnswers;
        private Question currentQuestion;
        private string selectedAnswer;
        private bool hasVoted = false;
        private bool votingComplete = false;
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
            // Get question and answers from GameManager
            currentQuestion = GameManager.Instance.CurrentQuestion;

            // Get remaining answers (after elimination)
            var allAnswers = GameManager.Instance.CurrentAnswers;
            var eliminatedAnswers = currentQuestion != null ? currentQuestion.EliminatedAnswers : null;

            if (eliminatedAnswers != null && eliminatedAnswers.Count > 0)
            {
                remainingAnswers = new List<Answer>();
                foreach (var answer in allAnswers)
                {
                    if (!eliminatedAnswers.Contains(answer.Text))
                    {
                        remainingAnswers.Add(answer);
                    }
                }
            }
            else
            {
                remainingAnswers = new List<Answer>(allAnswers);
            }

            // Get local player
            localPlayer = GameManager.Instance.Players.Count > 0
                ? GameManager.Instance.Players[0]
                : new Player("Player 1", "icon1");

            // Find player's answer
            string playerAnswer = "";
            foreach (var answer in remainingAnswers)
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
            // Instruction text
            if (instructionText != null)
            {
                instructionText.text = "Answer this ordinary multiple-choice trivia question";
            }

            // Setup answer list
            if (answerListDesktop != null)
            {
                answerListDesktop.OnAnswerSelected += OnAnswerSelectedDesktop;
                answerListDesktop.SetupAnswers(remainingAnswers, playerAnswer, filterOwnAnswer: false);
            }
        }

        private void SetupMobile(string playerAnswer)
        {
            // Setup header
            if (headerText != null)
            {
                headerText.text = "CHOOSE THE RIGHT ANSWER";
            }

            // Setup answer list (filter own answer on mobile)
            if (answerListMobile != null)
            {
                answerListMobile.OnAnswerSelected += OnAnswerSelectedMobile;
                answerListMobile.SetupAnswers(remainingAnswers, playerAnswer, filterOwnAnswer: true);
            }

            // Setup submit button
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitButtonClicked);
                UpdateSubmitButton();
            }
        }

        private void StartTimer()
        {
            if (isDesktop && timerDisplay != null)
            {
                timerDisplay.SetTime(timerDuration);
                timerDisplay.StartTimer(1f);
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
            UpdateSubmitButton();
        }

        private void OnSubmitButtonClicked()
        {
            if (string.IsNullOrEmpty(selectedAnswer) || hasVoted || votingComplete)
                return;

            SubmitVote();
        }

        private void UpdateSubmitButton()
        {
            if (submitButton == null) return;

            bool canSubmit = !string.IsNullOrEmpty(selectedAnswer) && !hasVoted && !votingComplete;

            submitButton.interactable = canSubmit;

            if (submitButtonCanvasGroup != null)
            {
                submitButtonCanvasGroup.alpha = canSubmit ? 1f : 0.5f;
            }

            if (submitButtonText != null)
            {
                if (votingComplete || hasVoted)
                    submitButtonText.text = "VOTE CAST!";
                else if (canSubmit)
                    submitButtonText.text = "SUBMIT FINAL ANSWER";
                else
                    submitButtonText.text = "SELECT AN ANSWER";
            }

            // Update button color
            if (submitButtonImage != null)
            {
                Color targetColor = canSubmit
                    ? GameConstants.Colors.BrightGreen
                    : GameConstants.Colors.Grey;

                submitButtonImage.color = targetColor;
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
            GameManager.Instance.RecordFinalVote(localPlayer.PlayerName, selectedAnswer);

            // Send to server in multiplayer mode
            if (GameManager.Instance.IsMultiplayer)
            {
                NetworkManager.Instance?.SubmitFinalVote(selectedAnswer);

                // Wait for OnAllVotesSubmitted event from server
                Debug.Log($"Final vote sent: {selectedAnswer}. Waiting for server results...");
            }
            else
            {
                // Single-player: show results immediately
                ShowResults();
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
                UpdateSubmitButton();
            }

            // Show results after delay (only for single-player)
            if (!GameManager.Instance.IsMultiplayer)
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    ShowResults();
                });
            }
        }

        private void AutoSubmitVote()
        {
            // Submit selected answer if any
            if (!string.IsNullOrEmpty(selectedAnswer))
            {
                SubmitVote();
            }
            else
            {
                hasVoted = true;
                UpdateSubmitButton();
            }
        }

        // ===========================
        // RESULTS
        // ===========================

        private void ShowResults()
        {
            votingComplete = true;

            // Check if player voted correctly
            bool playerGotItRight = selectedAnswer == currentQuestion.CorrectAnswer;

            // Highlight correct answer
            if (isDesktop && answerListDesktop != null)
            {
                answerListDesktop.HighlightCorrectAnswer(currentQuestion.CorrectAnswer, playerGotItRight);
            }
            else if (!isDesktop && answerListMobile != null)
            {
                answerListMobile.HighlightCorrectAnswer(currentQuestion.CorrectAnswer, playerGotItRight);
            }

            // Hold for duration, then transition
            DOVirtual.DelayedCall(correctAnswerHoldDuration, () =>
            {
                TransitionToNextScreen();
            });
        }

        // ===========================
        // SCENE TRANSITION
        // ===========================

        private void TransitionToNextScreen()
        {
            FadeTransition.Instance.FadeOut(0.5f, () =>
            {
                Debug.Log($"Transitioning to {nextSceneName}");
                // SceneManager.LoadScene(nextSceneName);

                // Temporary: fade back in
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    FadeTransition.Instance.FadeIn(1f);
                });
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

            net.OnFinalVoteCast.AddListener(HandleFinalVoteCast);
            net.OnAllVotesSubmitted.AddListener(HandleAllVotesSubmitted);
        }

        /// <summary>
        /// Unsubscribe from network events.
        /// </summary>
        private void UnsubscribeFromNetworkEvents()
        {
            var net = NetworkManager.Instance;
            if (net == null) return;

            net.OnFinalVoteCast.RemoveListener(HandleFinalVoteCast);
            net.OnAllVotesSubmitted.RemoveListener(HandleAllVotesSubmitted);
        }

        /// <summary>
        /// Handles final vote cast notification from server.
        /// </summary>
        private void HandleFinalVoteCast(string jsonData)
        {
            var data = JsonUtility.FromJson<FinalVoteCastData>(jsonData);
            Debug.Log($"Player {data.playerName} voted for: {data.vote}");
            // Could show visual feedback here if desired
        }

        /// <summary>
        /// Handles all votes submitted from server.
        /// </summary>
        private void HandleAllVotesSubmitted(string jsonData)
        {
            votingComplete = true;
            ShowResults();
        }

        // ===========================
        // DATA CLASSES
        // ===========================

        [System.Serializable]
        private class FinalVoteCastData
        {
            public string playerName;
            public string vote;
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

            if (submitButton != null)
                submitButton.onClick.RemoveAllListeners();

            DOTween.Kill(this);
        }
    }
}
