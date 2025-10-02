using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Core;
using RobotsGame.Data;
using RobotsGame.Managers;
using RobotsGame.Network;
using RobotsGame.UI;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Controls the Bonus Question Screen - players vote on most human-like answers.
    /// Desktop: Shows all player answers with voting interface
    /// Mobile: Shows answer buttons with vote selection
    /// Based on screenspec.md SCREEN: BonusQuestionScreen
    /// </summary>
    public class BonusQuestionScreenController : MonoBehaviour
    {
        [Header("Question Data")]
        [SerializeField] private int questionNumber = 1;

        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private Text questionText;
        [SerializeField] private VotingAnswerList answerList;
        [SerializeField] private TimerDisplay timerDisplay;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private VotingAnswerList mobileAnswerList;
        [SerializeField] private Button submitButton;

        [Header("Settings")]
        [SerializeField] private float votingDuration = 30f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private string nextSceneName = "BonusResultsScreen";

        private ResponsiveUI responsiveUI;
        private NetworkManager networkManager;
        private bool isTransitioning = false;
        private bool isDesktop = true;
        private string selectedAnswer;
        private float timeRemaining;
        private bool timerRunning = false;

        private void Awake()
        {
            responsiveUI = GetComponentInParent<ResponsiveUI>();
            if (responsiveUI == null)
            {
                responsiveUI = FindObjectOfType<ResponsiveUI>();
            }

            networkManager = FindObjectOfType<NetworkManager>();
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

            // Start timer
            timeRemaining = votingDuration;
            timerRunning = true;
        }

        private void Update()
        {
            if (timerRunning)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0)
                {
                    timeRemaining = 0;
                    timerRunning = false;
                    OnTimerExpired();
                }

                if (timerDisplay != null)
                {
                    timerDisplay.UpdateTime(timeRemaining);
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
            // TODO: Load question and answers
        }

        private void SetupMobile()
        {
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitVote);
                submitButton.interactable = false;
            }

            // TODO: Setup answer selection
        }

        private void OnSubmitVote()
        {
            if (isTransitioning || string.IsNullOrEmpty(selectedAnswer)) return;

            isTransitioning = true;
            timerRunning = false;

            // Submit vote to server
            if (networkManager != null)
            {
                networkManager.SubmitBonusVote(selectedAnswer, questionNumber);
            }

            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }
        }

        private void OnTimerExpired()
        {
            if (isDesktop && !isTransitioning)
            {
                // Auto-submit or transition
                FadeTransition.Instance.FadeOut(fadeOutDuration, () =>
                {
                    SceneManager.LoadScene(nextSceneName);
                });
            }
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
            if (submitButton != null)
                submitButton.onClick.RemoveAllListeners();
        }
    }
}
