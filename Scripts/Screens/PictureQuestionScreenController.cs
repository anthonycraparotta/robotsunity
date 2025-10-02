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
    /// Controls the Picture Question Screen - displays picture-based trivia questions.
    /// Desktop: Shows picture with question header and double points notice
    /// Mobile: Shows picture in decorative frame
    /// Based on screenspec.md SCREEN: PictureQuestionScreen
    /// </summary>
    public class PictureQuestionScreenController : MonoBehaviour
    {
        [Header("Question Data")]
        [SerializeField] private Question currentQuestion;

        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private Text questionHeaderText;
        [SerializeField] private Text doublePointsNotice;
        [SerializeField] private Image pictureImage;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private Image mobilePictureImage;
        [SerializeField] private Image mobileFrameImage;
        [SerializeField] private MobileAnswerInput mobileAnswerInput;

        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private string nextSceneName = "EliminationScreen";

        private ResponsiveUI responsiveUI;
        private bool isTransitioning = false;
        private bool isDesktop = true;

        // ===========================
        // LIFECYCLE
        // ===========================
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
            LoadQuestionData();

            if (isDesktop)
            {
                SetupDesktop();
            }
            else
            {
                SetupMobile();
            }

            // Fade in from black
            FadeInScreen();
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

        private void LoadQuestionData()
        {
            // Load question data from GameManager or passed parameter
            // For now, using serialized question
        }

        private void SetupDesktop()
        {
            // Set question header
            if (questionHeaderText != null && currentQuestion != null)
            {
                questionHeaderText.text = currentQuestion.question;
            }

            // Set double points notice
            if (doublePointsNotice != null)
            {
                doublePointsNotice.text = "Picture Questions are worth 2x in your testing score";
            }

            // Load picture image
            if (pictureImage != null && currentQuestion != null)
            {
                // Load image from resources or path
                LoadPictureImage(currentQuestion.imageUrl, pictureImage);
            }
        }

        private void SetupMobile()
        {
            // Load picture image
            if (mobilePictureImage != null && currentQuestion != null)
            {
                LoadPictureImage(currentQuestion.imageUrl, mobilePictureImage);
            }

            // Setup answer input
            if (mobileAnswerInput != null)
            {
                mobileAnswerInput.OnAnswerSubmitted += OnMobileAnswerSubmitted;
            }
        }

        private void LoadPictureImage(string imagePath, Image targetImage)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            // TODO: Load image from Resources or URL
            // For now, this is a placeholder
            Sprite loadedSprite = Resources.Load<Sprite>(imagePath);
            if (loadedSprite != null)
            {
                targetImage.sprite = loadedSprite;
            }
        }

        // ===========================
        // ANIMATIONS
        // ===========================

        private void FadeInScreen()
        {
            FadeTransition.Instance.FadeIn(fadeInDuration);
        }

        // ===========================
        // INPUT HANDLERS
        // ===========================

        private void OnMobileAnswerSubmitted(string answer)
        {
            // Handle mobile answer submission
            Debug.Log($"Player submitted answer: {answer}");

            // TODO: Send to server, validate, etc.
        }

        // ===========================
        // SCENE TRANSITION
        // ===========================

        private void FadeOutAndTransition()
        {
            FadeTransition.Instance.FadeOut(fadeOutDuration, () =>
            {
                GoToNextScene();
            });
        }

        private void GoToNextScene()
        {
            Debug.Log($"Transitioning to {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }

        // ===========================
        // CLEANUP
        // ===========================

        private void OnDestroy()
        {
            // Clean up tweens
            DOTween.Kill(this);

            // Unsubscribe from events
            if (mobileAnswerInput != null)
            {
                mobileAnswerInput.OnAnswerSubmitted -= OnMobileAnswerSubmitted;
            }
        }
    }
}
