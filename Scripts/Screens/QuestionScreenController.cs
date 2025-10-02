using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    /// Main controller for the Question Screen.
    /// Manages desktop and mobile views, answer submission, timer, and multiplayer sync.
    /// Based on unityspec.md SCREEN: QuestionScreen
    /// </summary>
    public class QuestionScreenController : MonoBehaviour
    {
        [Header("Platform Content")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private GameObject mobileContent;

        [Header("Desktop Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RobotCharacter robotCharacter;
        [SerializeField] private QuestionCard questionCard;
        [SerializeField] private PlayerIconGrid playerIconGrid;
        [SerializeField] private AnswersOverlay answersOverlay;
        [SerializeField] private TimerDisplay timerDisplay;

        [Header("Mobile Components")]
        [SerializeField] private Image mobileBackground;
        [SerializeField] private TextMeshProUGUI questionTextMobile;
        [SerializeField] private MobileAnswerInput mobileInput;
        [SerializeField] private TimerDisplay timerDisplayMobile;

        [Header("Settings")]
        [SerializeField] private string nextSceneName = "EliminationScreen";
        [SerializeField] private Sprite[] roundBackgrounds = new Sprite[12]; // Q1BG through Q12BG

        private ResponsiveUI responsiveUI;
        private bool isDesktop;
        private Question currentQuestion;
        private List<Answer> allAnswers = new List<Answer>();
        private HashSet<string> submittedPlayerNames = new HashSet<string>();
        private bool hasLocalPlayerSubmitted = false;
        private string localPlayerAnswer = "";

        // State tracking
        private bool allAnswersReceived = false;
        private bool hasPlayedQuestionIntroVO = false;
        private bool hasPlayedNudgeVO = false;

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
            // Get current question from GameManager
            currentQuestion = GameManager.Instance.CurrentQuestion;

            if (currentQuestion != null)
            {
                SetupQuestion(currentQuestion);
            }
            else
            {
                Debug.LogError("No question loaded in GameManager!");
            }

            // Setup answer validation callbacks
            if (mobileInput != null)
            {
                mobileInput.OnAnswerChanged += ValidateAnswerRealTime;
                mobileInput.OnAnswerSubmitted += SubmitAnswer;
            }

            // Subscribe to network events for multiplayer
            if (GameManager.Instance.IsMultiplayer)
            {
                SubscribeToNetworkEvents();
            }

            // Start timer
            if (isDesktop && timerDisplay != null)
            {
                timerDisplay.StartTimer();
                timerDisplay.OnTimerExpired += HandleTimerExpired;
            }
            else if (!isDesktop && timerDisplayMobile != null)
            {
                timerDisplayMobile.StartTimer(0f); // Immediate start on mobile
                timerDisplayMobile.OnTimerExpired += HandleTimerExpired;
            }

            // Play question intro VO (desktop only)
            if (isDesktop && !hasPlayedQuestionIntroVO)
            {
                DOVirtual.DelayedCall(GameConstants.Delays.QuestionIntroDelay, () =>
                {
                    AudioManager.Instance.PlayVoiceOver(GameConstants.Audio.VO_QuestionIntro);
                    hasPlayedQuestionIntroVO = true;
                });
            }

            // Fade in from black
            FadeTransition.Instance.FadeIn(1f);
        }

        private void OnDestroy()
        {
            // Unsubscribe from network events
            if (GameManager.Instance != null && GameManager.Instance.IsMultiplayer)
            {
                UnsubscribeFromNetworkEvents();
            }

            // Unsubscribe from mobile input events
            if (mobileInput != null)
            {
                mobileInput.OnAnswerChanged -= ValidateAnswerRealTime;
                mobileInput.OnAnswerSubmitted -= SubmitAnswer;
            }

            // Unsubscribe from timer events
            if (timerDisplay != null)
                timerDisplay.OnTimerExpired -= HandleTimerExpired;

            if (timerDisplayMobile != null)
                timerDisplayMobile.OnTimerExpired -= HandleTimerExpired;

            // Kill DOTween animations
            DOTween.Kill(this);
        }

        /// <summary>
        /// Subscribe to network events for multiplayer.
        /// </summary>
        private void SubscribeToNetworkEvents()
        {
            var net = NetworkManager.Instance;
            if (net == null) return;

            net.OnPlayerAnswered.AddListener(HandlePlayerAnsweredNetwork);
            net.OnAllAnswersSubmitted.AddListener(HandleAllAnswersSubmittedNetwork);
        }

        /// <summary>
        /// Unsubscribe from network events.
        /// </summary>
        private void UnsubscribeFromNetworkEvents()
        {
            var net = NetworkManager.Instance;
            if (net == null) return;

            net.OnPlayerAnswered.RemoveListener(HandlePlayerAnsweredNetwork);
            net.OnAllAnswersSubmitted.RemoveListener(HandleAllAnswersSubmittedNetwork);
        }

        /// <summary>
        /// Handles player answered notification from server.
        /// </summary>
        private void HandlePlayerAnsweredNetwork(string jsonData)
        {
            // Show player icon on desktop
            if (isDesktop && playerIconGrid != null)
            {
                var data = JsonUtility.FromJson<PlayerAnsweredData>(jsonData);
                playerIconGrid.ShowPlayerIcon(data.playerName);
                AudioManager.Instance.PlaySFX(GameConstants.Audio.SFX_PlayerIconPop);
            }
        }

        /// <summary>
        /// Handles all answers submitted from server.
        /// </summary>
        private void HandleAllAnswersSubmittedNetwork(string jsonData)
        {
            allAnswersReceived = true;
            TransitionToElimination();
        }

        /// <summary>
        /// Handles timer expiration.
        /// </summary>
        private void HandleTimerExpired()
        {
            if (GameManager.Instance.IsHost)
            {
                // Host notifies server and transitions
                NetworkManager.Instance?.RequestTransitionToElimination();
            }

            TransitionToElimination();
        }

        private void Update()
        {
            // Check timer expiration
            TimerDisplay activeTimer = isDesktop ? timerDisplay : timerDisplayMobile;
            if (activeTimer != null && activeTimer.IsExpired && !hasLocalPlayerSubmitted)
            {
                AutoSubmitAnswer();
            }

            // Check for nudge VO at 30 seconds (desktop only)
            if (isDesktop && !hasPlayedNudgeVO && !hasLocalPlayerSubmitted)
            {
                if (activeTimer != null && activeTimer.TimeRemaining <= 30f)
                {
                    AudioManager.Instance.PlayVoiceOver(GameConstants.Audio.VO_QuestionNudge);
                    hasPlayedNudgeVO = true;
                }
            }

            // Desktop: Enter key to submit
            if (isDesktop && Input.GetKeyDown(KeyCode.Return) && !hasLocalPlayerSubmitted)
            {
                // Desktop would have input field, simplified for now
                // SubmitAnswer(desktopInputField.text);
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

        private void SetupQuestion(Question question)
        {
            int round = GameManager.Instance.CurrentRound;

            if (isDesktop)
            {
                SetupDesktop(question, round);
            }
            else
            {
                SetupMobile(question, round);
            }

            // Pre-seed correct and robot answers for duplicate detection
            allAnswers.Add(new Answer(question.CorrectAnswer, GameConstants.AnswerType.Correct, "System"));
            allAnswers.Add(new Answer(question.RobotAnswer, GameConstants.AnswerType.Robot, "Robot"));
        }

        private void SetupDesktop(Question question, int round)
        {
            // Set background
            if (backgroundImage != null && round > 0 && round <= roundBackgrounds.Length)
            {
                backgroundImage.sprite = roundBackgrounds[round - 1];
            }

            // Setup robot character
            if (robotCharacter != null)
            {
                robotCharacter.SetupRobot(round);
                robotCharacter.SlideIn();
            }

            // Setup question card
            if (questionCard != null)
            {
                questionCard.PlayCardSequence(question.QuestionText);
            }

            // Setup player icon grid
            if (playerIconGrid != null)
            {
                playerIconGrid.SetPictureQuestion(question.Type == GameConstants.QuestionType.Picture);
            }
        }

        private void SetupMobile(Question question, int round)
        {
            // Set background (mobile uses different backgrounds)
            // Would set mobilecontrollerbg.png, questionmobile.png, or picqmobile.png

            // Show question text
            if (questionTextMobile != null)
            {
                DOVirtual.DelayedCall(1f, () =>
                {
                    questionTextMobile.text = question.QuestionText.ToUpper();
                    questionTextMobile.GetComponent<CanvasGroup>()?.DOFade(1f, 0.2f);
                });
            }
        }

        // ===========================
        // ANSWER VALIDATION
        // ===========================

        private void ValidateAnswerRealTime(string answer)
        {
            if (string.IsNullOrEmpty(answer))
                return;

            // Check against correct answer
            if (AnswerValidator.IsCorrectAnswer(answer, currentQuestion.CorrectAnswer))
            {
                if (mobileInput != null && !isDesktop)
                {
                    mobileInput.FlashDecoyWarning(answer);
                }
                return;
            }

            // Check for duplicates
            foreach (var existingAnswer in allAnswers)
            {
                if (AnswerValidator.IsDuplicate(answer, existingAnswer.Text))
                {
                    // Don't flash on every keystroke, only on submit
                    return;
                }
            }

            // Check for profanity
            if (AnswerValidator.ContainsProfanity(answer))
            {
                // Don't flash on every keystroke, only on submit
                return;
            }
        }

        // ===========================
        // ANSWER SUBMISSION
        // ===========================

        private void SubmitAnswer(string answer)
        {
            if (hasLocalPlayerSubmitted)
                return;

            if (string.IsNullOrEmpty(answer))
            {
                answer = GameConstants.FallbackText.NoAnswer;
            }

            // Validate before submitting
            if (!ValidateAnswerForSubmission(answer))
                return;

            // Get local player (simplified - in real game would get from session)
            Player localPlayer = GameManager.Instance.Players.Count > 0
                ? GameManager.Instance.Players[0]
                : new Player("Player 1", "icon1");

            // Create answer object
            Answer playerAnswer = new Answer(answer, GameConstants.AnswerType.Player, localPlayer.PlayerName);

            // Submit to GameManager
            GameManager.Instance.SubmitAnswer(playerAnswer);
            allAnswers.Add(playerAnswer);
            submittedPlayerNames.Add(localPlayer.PlayerName);

            hasLocalPlayerSubmitted = true;
            localPlayerAnswer = answer;

            // Send to server in multiplayer mode
            if (GameManager.Instance.IsMultiplayer)
            {
                NetworkManager.Instance?.SubmitAnswer(answer);
            }

            // UI feedback
            if (isDesktop)
            {
                // Show player icon on desktop
                if (playerIconGrid != null)
                {
                    playerIconGrid.ShowPlayerIcon(localPlayer);
                }
            }
            else
            {
                // Mark input as submitted on mobile
                if (mobileInput != null)
                {
                    mobileInput.MarkAsSubmitted();
                    AudioManager.Instance.PlayInputAccept();
                }
            }

            // Check if all players have submitted
            CheckAllAnswersReceived();
        }

        private bool ValidateAnswerForSubmission(string answer)
        {
            // Check correct answer
            if (AnswerValidator.IsCorrectAnswer(answer, currentQuestion.CorrectAnswer))
            {
                if (mobileInput != null && !isDesktop)
                    mobileInput.FlashDecoyWarning(answer);
                return false;
            }

            // Check duplicates
            foreach (var existingAnswer in allAnswers)
            {
                if (AnswerValidator.IsDuplicate(answer, existingAnswer.Text))
                {
                    if (mobileInput != null && !isDesktop)
                        mobileInput.FlashDuplicateWarning();
                    return false;
                }
            }

            // Check profanity
            if (AnswerValidator.ContainsProfanity(answer))
            {
                if (mobileInput != null && !isDesktop)
                    mobileInput.FlashProfanityWarning();
                return false;
            }

            return true;
        }

        private void AutoSubmitAnswer()
        {
            string answer = isDesktop ? "" : (mobileInput != null ? mobileInput.CurrentAnswer : "");

            if (string.IsNullOrEmpty(answer))
            {
                answer = GameConstants.FallbackText.NoAnswer;
            }

            SubmitAnswer(answer);
        }

        private void CheckAllAnswersReceived()
        {
            if (allAnswersReceived)
                return;

            List<Player> allPlayers = GameManager.Instance.Players;

            // Check if all players have submitted
            if (submittedPlayerNames.Count >= allPlayers.Count)
            {
                allAnswersReceived = true;

                // Pause timer
                if (isDesktop && timerDisplay != null)
                    timerDisplay.PauseTimer();
                else if (!isDesktop && timerDisplayMobile != null)
                    timerDisplayMobile.PauseTimer();

                // Show answers overlay on desktop
                if (isDesktop && answersOverlay != null)
                {
                    List<Answer> playerAnswers = new List<Answer>();
                    foreach (var answer in allAnswers)
                    {
                        if (answer.Type == GameConstants.AnswerType.Player)
                        {
                            playerAnswers.Add(answer);
                        }
                    }
                    answersOverlay.ShowOverlay(playerAnswers);
                }

                // Transition after delay
                DOVirtual.DelayedCall(GameConstants.Delays.EarlySubmissionDelay + 3f, () =>
                {
                    TransitionToNextScreen();
                });
            }
        }

        // ===========================
        // SCENE TRANSITION
        // ===========================

        private void TransitionToElimination()
        {
            if (GameManager.Instance.IsMultiplayer && GameManager.Instance.IsHost)
            {
                // Host uses sync delay before transitioning
                MultiplayerSync.Instance.SyncScreenTransition("elimination", TransitionToNextScreen);
            }
            else
            {
                // Client or single-player transitions immediately
                TransitionToNextScreen();
            }
        }

        private void TransitionToNextScreen()
        {
            // Add robot and correct answers to GameManager
            GameManager.Instance.AddRobotAndCorrectAnswers();

            FadeTransition.Instance.FadeOut(1f, () =>
            {
                Debug.Log($"Transitioning to {nextSceneName}");
                if (!SceneLoader.TryLoadScene(nextSceneName))
                {
                    FadeTransition.Instance.FadeIn(0.5f);
                }
            });
        }

        // ===========================
        // DATA CLASSES
        // ===========================

        [System.Serializable]
        private class PlayerAnsweredData
        {
            public string playerName;
        }

    }
}
