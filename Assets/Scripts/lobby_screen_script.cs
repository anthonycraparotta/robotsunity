using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public class LobbyScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public TextMeshProUGUI roomCodeDisplay; // "Data" in JoinWait
    public Transform playerIconContainer;
    public GameObject playerIconLobbyPrefab;
    public Button startTestButton;
    public Button eightQButton;
    public Button twelveQButton;
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI dataHeaders;
    public Image desktopBackground;
    
    [Header("Mobile UI Elements - Join Form")]
    public GameObject mobileDisplay;
    public GameObject joinScreen;
    public Button joinGameButton; // Button on JoinScreen to show JoinForm
    public GameObject joinForm;
    public TMP_InputField nameInput;
    public TMP_InputField roomCodeInput;
    public Button joinButton; // Button on JoinForm to submit name/icon
    public Transform scrollingPlayerIconContainer;
    public Image selectedIcon;
    public Transform playerIconSelectionContainer;
    public Image namebar;
    public TextMeshProUGUI playerNameDisplay;
    public Image joinScreenBackground;
    public Image joinFormBackground;

    [Header("Mobile UI Elements - Join Wait")]
    public GameObject joinWait;
    public Image joinWaitBackground;
    public TextMeshProUGUI waitingHeadlineText;
    public Image waitPlayerIcon;
    public TextMeshProUGUI waitData;
    public TextMeshProUGUI waitDataHeaders;

    [Header("Error Display")]
    public TextMeshProUGUI errorMessageText; // Connect in Unity - shows validation errors
    public float errorDisplayDuration = 3f; // How long to show error messages

    [Header("State")]
    private string selectedPlayerIconName = "";
    private string roomCode = "";
    private bool isMobile = false;
    private List<GameObject> spawnedPlayerIcons = new List<GameObject>();
    private Coroutine errorCoroutine;
    
    void Start()
    {
        isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        // Set game state to Lobby
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentGameState = GameManager.GameState.Lobby;
            Debug.Log("LobbyScreen loaded - set currentGameState to Lobby");
        }

        // Setup desktop buttons
        if (startTestButton != null)
        {
            startTestButton.onClick.AddListener(OnStartGameClicked);
        }

        if (eightQButton != null)
        {
            eightQButton.onClick.AddListener(() => OnGameModeSelected(GameManager.GameMode.EightQuestions));
        }

        if (twelveQButton != null)
        {
            twelveQButton.onClick.AddListener(() => OnGameModeSelected(GameManager.GameMode.TwelveQuestions));
        }

        // Setup mobile join game button (shows the form)
        if (joinGameButton != null)
        {
            joinGameButton.onClick.AddListener(OnJoinGameButtonClicked);
        }

        // Setup mobile join button (submits the form)
        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinButtonClicked);
        }

        // Show appropriate display
        ShowAppropriateDisplay();

        // Initialize networking flow based on device type
        if (!isMobile)
        {
            SetupDesktopHost();

            // Add desktop host as a player
            AddDesktopHostPlayer();
        }

        // Play landing page music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLandingPageMusic();
        }
    }

    void AddDesktopHostPlayer()
    {
        // Create host player ID
        string hostPlayerID = PlayerAuthSystem.Instance != null ?
            PlayerAuthSystem.Instance.GetLocalPlayerID() :
            "host_" + SystemInfo.deviceUniqueIdentifier;

        // Use a default icon for the host
        string hostIconName = PlayerManager.Instance != null ?
            PlayerManager.Instance.GetRandomIconName() :
            "player icon (1)";

        // Desktop host is NOT a player - it's just a display screen
        // Players join via mobile devices only
        Debug.Log("Desktop is host - not added as a player");
    }
    
    void Update()
    {
        // Update player list
        UpdatePlayerList();
    }
    
    void ShowAppropriateDisplay()
    {
        if (desktopDisplay != null)
        {
            desktopDisplay.SetActive(!isMobile);
        }
        
        if (mobileDisplay != null)
        {
            mobileDisplay.SetActive(isMobile);
        }
        
        // Mobile starts on join form
        if (isMobile)
        {
            ShowJoinForm();
        }
    }
    
    void SetupDesktopHost()
    {
        if (RWMNetworkManager.Instance != null && NetworkManager.Singleton != null)
        {
            if (!NetworkManager.Singleton.IsListening)
            {
                RWMNetworkManager.Instance.StartHost();
            }

            roomCode = RWMNetworkManager.Instance.GetRoomCode();

            if (roomCodeDisplay != null)
            {
                roomCodeDisplay.text = roomCode;
            }
        }
        else
        {
            // Fallback: generate a local code so the UI isn't empty when networking is unavailable
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            System.Text.StringBuilder code = new System.Text.StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                code.Append(chars[Random.Range(0, chars.Length)]);
            }

            roomCode = code.ToString();

            if (roomCodeDisplay != null)
            {
                roomCodeDisplay.text = roomCode;
            }

            Debug.LogWarning("Networking not available - using locally generated room code");
        }
    }
    
    void OnGameModeSelected(GameManager.GameMode mode)
    {
        MobileHaptics.SelectionChanged();

        GameManager.Instance.gameMode = mode;
        Debug.Log("Game mode selected: " + mode);
        
        // Update UI to show selected mode
        if (eightQButton != null && twelveQButton != null)
        {
            // Visual feedback for selected button (you can add color changes, etc.)
            if (mode == GameManager.GameMode.EightQuestions)
            {
                Debug.Log("8 Questions selected");
            }
            else
            {
                Debug.Log("12 Questions selected");
            }
        }
    }
    
    public void OnEightQuestionsClicked()
    {
        OnGameModeSelected(GameManager.GameMode.EightQuestions);
    }

    public void OnTwelveQuestionsClicked()
    {
        OnGameModeSelected(GameManager.GameMode.TwelveQuestions);
    }

    public void OnStartGameClicked()
    {
        MobileHaptics.MediumImpact();

        // Check if we have enough players (minimum 2)
        if (GameManager.Instance.GetAllPlayers().Count < 2)
        {
            Debug.LogWarning("Need at least 2 players to start!");
            return;
        }

        Debug.Log("Start button clicked - currentGameState: " + GameManager.Instance.currentGameState + ", currentRound: " + GameManager.Instance.currentRound);

        // Advance to Round Art (which will load Round 1)
        GameManager.Instance.AdvanceToNextScreen();
    }

    // === MOBILE JOIN FORM ===

    public void OnJoinGameButtonClicked()
    {
        MobileHaptics.LightImpact();

        // User clicked "Join Game" button - show the form
        ShowJoinForm();
    }

    void ShowJoinForm()
    {
        if (joinForm != null)
        {
            joinForm.SetActive(true);
        }

        if (joinWait != null)
        {
            joinWait.SetActive(false);
        }
    }
    
    void ShowJoinWait()
    {
        if (joinForm != null)
        {
            joinForm.SetActive(false);
        }
        
        if (joinWait != null)
        {
            joinWait.SetActive(true);
        }
    }
    
    public void OnPlayerIconSelected(string iconName)
    {
        MobileHaptics.SelectionChanged();

        // Check if icon is available (not already selected by another player)
        if (PlayerManager.Instance != null && !PlayerManager.Instance.IsIconAvailable(iconName))
        {
            Debug.LogWarning($"Icon {iconName} is already selected by another player!");
            ShowErrorMessage("This icon is already taken. Please choose another one.");
            return;
        }

        selectedPlayerIconName = iconName;

        // Update selected icon display
        if (selectedIcon != null && PlayerManager.Instance != null)
        {
            selectedIcon.sprite = PlayerManager.Instance.GetPlayerIcon(iconName);
        }

        Debug.Log("Player icon selected: " + iconName);
    }
    
    public void OnJoinButtonClicked()
    {
        MobileHaptics.MediumImpact();

        if (nameInput == null || string.IsNullOrEmpty(nameInput.text))
        {
            Debug.LogWarning("Please enter a name!");
            ShowErrorMessage("Please enter a name!");
            return;
        }

        if (string.IsNullOrEmpty(selectedPlayerIconName))
        {
            Debug.LogWarning("Please select an icon!");
            ShowErrorMessage("Please select an icon!");
            return;
        }

        string enteredRoomCode = roomCodeInput != null ? roomCodeInput.text.Trim().ToUpper() : "";

        if (isMobile && string.IsNullOrEmpty(enteredRoomCode))
        {
            Debug.LogWarning("Please enter the room code!");
            ShowErrorMessage("Please enter the room code!");
            return;
        }

        // Validate name using ContentFilterManager
        if (ContentFilterManager.Instance != null)
        {
            ValidationResult validation = ContentFilterManager.Instance.ValidatePlayerName(nameInput.text);

            if (!validation.isValid)
            {
                Debug.LogWarning("Name validation failed: " + validation.errorMessage);
                ShowErrorMessage(validation.errorMessage);
                return;
            }

            // Use sanitized name
            string sanitizedName = validation.sanitizedText;

            CompleteJoinFlow(sanitizedName, enteredRoomCode);
        }
        else
        {
            // Fallback if ContentFilterManager not available
            CompleteJoinFlow(nameInput.text.Trim(), enteredRoomCode);
        }
    }

    void CompleteJoinFlow(string playerName, string enteredRoomCode)
    {
        if (!string.IsNullOrEmpty(enteredRoomCode))
        {
            roomCode = enteredRoomCode;
        }

        ConnectToHostIfNeeded();

        string playerID = System.Guid.NewGuid().ToString();

        if (PlayerAuthSystem.Instance != null)
        {
            playerID = PlayerAuthSystem.Instance.GetLocalPlayerID();
            PlayerAuthSystem.Instance.RegisterPlayer(playerName, selectedPlayerIconName);
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayer(playerID, playerName, selectedPlayerIconName);
        }

        if (RWMNetworkManager.Instance != null && NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsClient)
            {
                RWMNetworkManager.Instance.AddPlayerServerRpc(playerID, playerName, selectedPlayerIconName);
            }
            else
            {
                void SendRegistrationWhenConnected(ulong clientId)
                {
                    if (NetworkManager.Singleton == null)
                    {
                        return;
                    }

                    if (clientId != NetworkManager.Singleton.LocalClientId)
                    {
                        return;
                    }

                    NetworkManager.Singleton.OnClientConnectedCallback -= SendRegistrationWhenConnected;

                    if (RWMNetworkManager.Instance != null)
                    {
                        RWMNetworkManager.Instance.AddPlayerServerRpc(playerID, playerName, selectedPlayerIconName);
                    }
                }

                NetworkManager.Singleton.OnClientConnectedCallback += SendRegistrationWhenConnected;
            }
        }

        UpdateWaitingScreenUI(playerName);
        ShowJoinWait();

        Debug.Log("Player joined: " + playerName);
    }

    void ConnectToHostIfNeeded()
    {
        if (RWMNetworkManager.Instance == null || NetworkManager.Singleton == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            return;
        }

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            RWMNetworkManager.Instance.JoinGame(roomCode);
        }
    }

    void UpdateWaitingScreenUI(string playerName)
    {
        if (playerNameDisplay != null)
        {
            playerNameDisplay.text = playerName;
        }

        if (waitPlayerIcon != null && PlayerManager.Instance != null)
        {
            waitPlayerIcon.sprite = PlayerManager.Instance.GetPlayerIcon(selectedPlayerIconName);
        }
    }

    void ShowErrorMessage(string message)
    {
        MobileHaptics.Failure();

        Debug.LogError("INPUT ERROR: " + message);

        // Display error message in UI
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);

            // Clear previous coroutine if exists
            if (errorCoroutine != null)
            {
                StopCoroutine(errorCoroutine);
            }

            // Auto-hide after duration
            errorCoroutine = StartCoroutine(HideErrorMessageAfterDelay());
        }

        // Play error sound if AudioManager exists
        if (AudioManager.Instance != null)
        {
            // Could add: AudioManager.Instance.PlayErrorSFX();
        }
    }

    System.Collections.IEnumerator HideErrorMessageAfterDelay()
    {
        yield return new WaitForSeconds(errorDisplayDuration);

        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
    }
    
    // === PLAYER LIST UPDATES ===

    void UpdatePlayerList()
    {
        if (playerIconContainer == null) return;

        List<PlayerData> players = GameManager.Instance.GetAllPlayers();

        // Clear existing icons
        foreach (GameObject icon in spawnedPlayerIcons)
        {
            Destroy(icon);
        }
        spawnedPlayerIcons.Clear();

        // Count non-host players
        int nonHostPlayerCount = 0;

        // Spawn new icons for each player (excluding host)
        foreach (PlayerData player in players)
        {
            // Skip host player
            if (player.isHost)
            {
                continue;
            }

            nonHostPlayerCount++;

            if (playerIconLobbyPrefab != null)
            {
                GameObject iconObj = Instantiate(playerIconLobbyPrefab, playerIconContainer);

                // Set player name
                TextMeshProUGUI nameText = iconObj.transform.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = player.playerName;
                }

                // Set player icon
                Image iconImage = iconObj.transform.Find("PlayerIcon")?.GetComponent<Image>();
                if (iconImage != null && PlayerManager.Instance != null)
                {
                    iconImage.sprite = PlayerManager.Instance.GetPlayerIcon(player.iconName);
                }

                spawnedPlayerIcons.Add(iconObj);
            }
        }

        // Update player count on mobile wait screen (excluding host)
        if (isMobile && waitData != null)
        {
            string displayRoomCode = !string.IsNullOrEmpty(roomCode) ? roomCode :
                (RWMNetworkManager.Instance != null ? RWMNetworkManager.Instance.GetRoomCode() : "");

            waitData.text = nonHostPlayerCount + " Players\n" +
                           (GameManager.Instance.gameMode == GameManager.GameMode.EightQuestions ? "8" : "12") + " Questions\n" +
                           displayRoomCode;
        }

        if (!isMobile && roomCodeDisplay != null && string.IsNullOrEmpty(roomCode))
        {
            string hostRoomCode = RWMNetworkManager.Instance != null ? RWMNetworkManager.Instance.GetRoomCode() : "";
            if (!string.IsNullOrEmpty(hostRoomCode))
            {
                roomCode = hostRoomCode;
                roomCodeDisplay.text = roomCode;
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up listeners
        if (startTestButton != null)
        {
            startTestButton.onClick.RemoveListener(OnStartGameClicked);
        }
        
        if (eightQButton != null)
        {
            eightQButton.onClick.RemoveAllListeners();
        }
        
        if (twelveQButton != null)
        {
            twelveQButton.onClick.RemoveAllListeners();
        }
        
        if (joinGameButton != null)
        {
            joinGameButton.onClick.RemoveListener(OnJoinGameButtonClicked);
        }

        if (joinButton != null)
        {
            joinButton.onClick.RemoveListener(OnJoinButtonClicked);
        }
    }
}