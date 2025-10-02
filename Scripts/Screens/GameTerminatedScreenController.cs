using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Managers;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Controls the Game Terminated Screen - shown when host disconnects or game ends unexpectedly.
    /// Desktop: Shows termination message with Return to Lobby button
    /// Mobile: Shows termination message with Return to Lobby button
    /// Based on screenspec.md SCREEN: GameTerminatedScreen
    /// </summary>
    public class GameTerminatedScreenController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private Button returnToLobbyButton;

        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private string nextSceneName = "HostScreen";

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
            SetupUI();
            FadeTransition.Instance.FadeIn(fadeInDuration);
        }

        private void Update()
        {
            if (!isTransitioning)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnReturnToLobbyClicked();
                }
            }
        }

        // ===========================
        // SETUP
        // ===========================

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
        }

        private void SetupUI()
        {
            // Set title
            if (titleText != null)
            {
                titleText.text = "GAME TERMINATED";
            }

            // Set message
            if (messageText != null)
            {
                messageText.text = "The host has disconnected.\nThe game has been terminated.";
            }

            // Setup button
            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.onClick.AddListener(OnReturnToLobbyClicked);

                // Add ButtonEffects component if not already present
                ButtonEffects buttonEffects = returnToLobbyButton.GetComponent<ButtonEffects>();
                if (buttonEffects == null)
                {
                    buttonEffects = returnToLobbyButton.gameObject.AddComponent<ButtonEffects>();
                }
            }
        }

        // ===========================
        // BUTTON HANDLERS
        // ===========================

        private void OnReturnToLobbyClicked()
        {
            if (isTransitioning) return;

            isTransitioning = true;

            // Play button press sound (handled by ButtonEffects)

            // Fade out and transition
            FadeTransition.Instance.FadeOut(fadeOutDuration, () =>
            {
                GoToNextScene();
            });
        }

        // ===========================
        // SCENE TRANSITION
        // ===========================

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

            // Clean up button listener
            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.onClick.RemoveAllListeners();
            }
        }
    }
}
