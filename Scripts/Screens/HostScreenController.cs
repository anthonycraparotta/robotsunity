using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Core;
using RobotsGame.Managers;
using RobotsGame.Network;
using RobotsGame.Data;
using RobotsGame.UI;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Controls the Host/Lobby Screen - room creation and player management.
    /// Desktop: Auto-creates room, shows room code, player list, game mode selection, start button
    /// Mobile: Shows join form with room code input, name/icon selection, then waiting room
    /// Based on screenspec.md SCREEN: HostScreen (Lobby)
    /// </summary>
    public class HostScreenController : MonoBehaviour
    {
        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private Text roomCodeText;
        [SerializeField] private Button eightRoundsButton;
        [SerializeField] private Button twelveRoundsButton;
        [SerializeField] private Image eightRoundsImage;
        [SerializeField] private Image twelveRoundsImage;
        [SerializeField] private Sprite eightRoundsUnselected;
        [SerializeField] private Sprite eightRoundsSelected;
        [SerializeField] private Sprite twelveRoundsUnselected;
        [SerializeField] private Sprite twelveRoundsSelected;
        [SerializeField] private PlayerIconGrid playerListGrid;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Image desktopSpatter;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private GameObject joinFormPanel;
        [SerializeField] private GameObject waitingRoomPanel;
        [SerializeField] private InputField roomCodeInput;
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private PlayerIconGrid iconSelectionGrid;
        [SerializeField] private Image selectedIconPreview;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text errorMessageText;
        [SerializeField] private Text waitingRoomCodeText;
        [SerializeField] private PlayerIconGrid waitingRoomPlayerGrid;
        [SerializeField] private Text playerCountText;

        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private int iconPopSoundCount = 3; // Cycles through 3 pop sounds
        [SerializeField] private string nextSceneName = "RoundArtScreen";

        private ResponsiveUI responsiveUI;
        private NetworkManager networkManager;
        private bool isTransitioning = false;
        private bool isDesktop = true;
        private string currentRoomCode;
        private int selectedGameMode = 8; // Default: 8 rounds
        private string selectedPlayerIcon;
        private List<Player> currentPlayers = new List<Player>();
        private int iconPopSoundIndex = 0;

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
            // Setup game mode buttons
            if (eightRoundsButton != null)
            {
                eightRoundsButton.onClick.AddListener(() => SelectGameMode(8));
            }
            if (twelveRoundsButton != null)
            {
                twelveRoundsButton.onClick.AddListener(() => SelectGameMode(12));
            }

            // Setup start game button
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
                startGameButton.interactable = false; // Disabled until players join
            }

            // Set initial game mode
            SelectGameMode(8);

            // Auto-create room
            CreateRoom();

            // Play voice-over
            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayVoiceOver(GameConstants.Audio.VO_JoinGameCode);
            }

            // Setup spatter animation
            if (desktopSpatter != null)
            {
                AnimateSpatter();
            }

            // Subscribe to network events
            SubscribeToNetworkEvents();
        }

        private void SetupMobile()
        {
            // Show join form initially
            if (joinFormPanel != null)
                joinFormPanel.SetActive(true);
            if (waitingRoomPanel != null)
                waitingRoomPanel.SetActive(false);

            // Setup room code input
            if (roomCodeInput != null)
            {
                roomCodeInput.characterLimit = 5;
                roomCodeInput.onValueChanged.AddListener(OnRoomCodeChanged);
            }

            // Setup player name input
            if (playerNameInput != null)
            {
                playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
            }

            // Setup buttons
            if (joinButton != null)
            {
                joinButton.onClick.AddListener(OnJoinButtonClicked);
                joinButton.interactable = false;
            }
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }

            // Setup icon selection grid
            if (iconSelectionGrid != null)
            {
                // Initialize icon grid (implement icon selection logic)
            }

            // Subscribe to network events
            SubscribeToNetworkEvents();
        }

        // ===========================
        // DESKTOP - ROOM CREATION
        // ===========================

        private void CreateRoom()
        {
            if (networkManager != null)
            {
                networkManager.CreateRoom(selectedGameMode, OnRoomCreated, OnRoomCreationFailed);
            }
            else
            {
                Debug.LogError("NetworkManager not found!");
            }
        }

        private void OnRoomCreated(string roomCode)
        {
            currentRoomCode = roomCode;

            if (roomCodeText != null)
            {
                roomCodeText.text = roomCode;
            }

            Debug.Log($"Room created: {roomCode}");
        }

        private void OnRoomCreationFailed(string error)
        {
            Debug.LogError($"Failed to create room: {error}");
        }

        // ===========================
        // DESKTOP - GAME MODE SELECTION
        // ===========================

        private void SelectGameMode(int rounds)
        {
            selectedGameMode = rounds;

            // Update button visuals
            if (eightRoundsImage != null)
            {
                eightRoundsImage.sprite = (rounds == 8) ? eightRoundsSelected : eightRoundsUnselected;
            }
            if (twelveRoundsImage != null)
            {
                twelveRoundsImage.sprite = (rounds == 12) ? twelveRoundsSelected : twelveRoundsUnselected;
            }

            // Play button press sound
            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }
        }

        // ===========================
        // DESKTOP - START GAME
        // ===========================

        private void OnStartGameClicked()
        {
            if (isTransitioning) return;
            if (currentPlayers.Count == 0) return;

            isTransitioning = true;

            // Play button press sound
            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }

            // Emit game-started event
            if (networkManager != null)
            {
                networkManager.StartGame(currentRoomCode, selectedGameMode);
            }

            // Fade out and transition
            FadeOutAndTransition();
        }

        // ===========================
        // MOBILE - JOIN FORM
        // ===========================

        private void OnRoomCodeChanged(string code)
        {
            // Auto-uppercase
            roomCodeInput.text = code.ToUpper();

            // Validate join button state
            UpdateJoinButtonState();

            // Spectate room if 5 characters entered
            if (code.Length == 5 && networkManager != null)
            {
                networkManager.SpectateRoom(code);
            }
        }

        private void OnPlayerNameChanged(string name)
        {
            // Validate join button state
            UpdateJoinButtonState();

            // TODO: Check against banned words
        }

        private void UpdateJoinButtonState()
        {
            bool canJoin = !string.IsNullOrWhiteSpace(roomCodeInput.text) &&
                          roomCodeInput.text.Length == 5 &&
                          !string.IsNullOrWhiteSpace(playerNameInput.text) &&
                          !string.IsNullOrEmpty(selectedPlayerIcon);

            if (joinButton != null)
            {
                joinButton.interactable = canJoin;
            }
        }

        private void OnJoinButtonClicked()
        {
            string roomCode = roomCodeInput.text.Trim().ToUpper();
            string playerName = playerNameInput.text.Trim();

            if (networkManager != null)
            {
                networkManager.JoinRoom(roomCode, playerName, selectedPlayerIcon, OnJoinSuccess, OnJoinFailed);
            }

            // Play button press sound
            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }
        }

        private void OnJoinSuccess()
        {
            // Switch to waiting room
            if (joinFormPanel != null)
                joinFormPanel.SetActive(false);
            if (waitingRoomPanel != null)
                waitingRoomPanel.SetActive(true);

            // Update waiting room UI
            if (waitingRoomCodeText != null)
            {
                waitingRoomCodeText.text = currentRoomCode;
            }
        }

        private void OnJoinFailed(string error)
        {
            // Show error message
            if (errorMessageText != null)
            {
                errorMessageText.text = error;
                errorMessageText.gameObject.SetActive(true);

                // Flash error message
                DOVirtual.DelayedCall(2f, () =>
                {
                    if (errorMessageText != null)
                        errorMessageText.gameObject.SetActive(false);
                });
            }
        }

        private void OnCancelButtonClicked()
        {
            // Leave spectate mode if active
            if (networkManager != null && !string.IsNullOrEmpty(currentRoomCode))
            {
                networkManager.LeaveSpectate(currentRoomCode);
            }

            // TODO: Return to initial selection (join/create)
        }

        // ===========================
        // NETWORK EVENTS
        // ===========================

        private void SubscribeToNetworkEvents()
        {
            if (networkManager == null) return;

            networkManager.OnPlayerJoined += OnPlayerJoined;
            networkManager.OnPlayerLeft += OnPlayerLeft;
            networkManager.OnGameStarted += OnGameStarted;
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (networkManager == null) return;

            networkManager.OnPlayerJoined -= OnPlayerJoined;
            networkManager.OnPlayerLeft -= OnPlayerLeft;
            networkManager.OnGameStarted -= OnGameStarted;
        }

        private void OnPlayerJoined(Player player)
        {
            currentPlayers.Add(player);

            // Update player list UI
            UpdatePlayerList();

            // Enable start button if we have players (desktop only)
            if (isDesktop && startGameButton != null)
            {
                startGameButton.interactable = currentPlayers.Count > 0;
            }

            // Play icon pop sound (desktop only)
            if (isDesktop && AudioManager.TryGetInstance(out var audioManager))
            {
                string popSound = $"player_icon_pop_{(iconPopSoundIndex % iconPopSoundCount) + 1}";
                audioManager.PlaySFX(popSound);
                iconPopSoundIndex++;
            }
        }

        private void OnPlayerLeft(string playerName)
        {
            currentPlayers.RemoveAll(p => p.name == playerName);

            // Update player list UI
            UpdatePlayerList();

            // Disable start button if no players (desktop only)
            if (isDesktop && startGameButton != null)
            {
                startGameButton.interactable = currentPlayers.Count > 0;
            }
        }

        private void OnGameStarted(int gameMode, int round)
        {
            // Transition to RoundArtScreen
            FadeOutAndTransition();
        }

        private void UpdatePlayerList()
        {
            // Update desktop player grid
            if (isDesktop && playerListGrid != null)
            {
                playerListGrid.UpdatePlayers(currentPlayers);
            }

            // Update mobile waiting room grid
            if (!isDesktop && waitingRoomPlayerGrid != null)
            {
                waitingRoomPlayerGrid.UpdatePlayers(currentPlayers);
            }

            // Update mobile player count
            if (!isDesktop && playerCountText != null)
            {
                playerCountText.text = $"{currentPlayers.Count} player{(currentPlayers.Count != 1 ? "s" : "")} connected";
            }
        }

        // ===========================
        // ANIMATIONS
        // ===========================

        private void AnimateSpatter()
        {
            if (desktopSpatter == null) return;

            desktopSpatter.transform.DOScale(1.05f, 6f)
                .From(0.95f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void FadeOutAndTransition()
        {
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

            // Unsubscribe from network events
            UnsubscribeFromNetworkEvents();

            // Clean up button listeners
            if (eightRoundsButton != null)
                eightRoundsButton.onClick.RemoveAllListeners();
            if (twelveRoundsButton != null)
                twelveRoundsButton.onClick.RemoveAllListeners();
            if (startGameButton != null)
                startGameButton.onClick.RemoveAllListeners();
            if (joinButton != null)
                joinButton.onClick.RemoveAllListeners();
            if (cancelButton != null)
                cancelButton.onClick.RemoveAllListeners();
        }
    }
}
