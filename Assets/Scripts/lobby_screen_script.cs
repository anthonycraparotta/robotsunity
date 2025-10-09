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
    private bool awaitingNetworkConnection = false;
    private bool hasConnectedToHost = false;
    private bool networkCallbacksRegistered = false;
    
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

        RegisterNetworkCallbacks();

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

    void RegisterNetworkCallbacks()
    {
        if (!isMobile || networkCallbacksRegistered)
        {
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        networkCallbacksRegistered = true;
    }

    void UnregisterNetworkCallbacks()
    {
        if (!networkCallbacksRegistered)
        {
            return;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        networkCallbacksRegistered = false;
    }

    void HandleClientConnected(ulong clientId)
    {
        if (!isMobile || NetworkManager.Singleton == null)
        {
            return;
        }

        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        awaitingNetworkConnection = false;
        hasConnectedToHost = true;
    }

    void HandleClientDisconnected(ulong clientId)
    {
        if (!isMobile || NetworkManager.Singleton == null)
        {
            return;
        }

        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        bool wasConnected = hasConnectedToHost;
        hasConnectedToHost = false;

        if (awaitingNetworkConnection)
        {
            awaitingNetworkConnection = false;
            ShowJoinForm();
            ShowErrorMessage("Unable to connect to the host. Please check the room code and try again.", false);
        }
        else if (wasConnected)
        {
            ShowJoinForm();
            ShowErrorMessage("Connection to the host was lost. Please try joining again.", false);
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
        awaitingNetworkConnection = false;
        hasConnectedToHost = false;

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
        // Validate network prerequisites BEFORE proceeding
        if (RWMNetworkManager.Instance == null || NetworkManager.Singleton == null)
        {
            ShowErrorMessage("Network system is not available. Please restart the app and try again.", false);
            return;
        }

        if (!string.IsNullOrEmpty(enteredRoomCode))
        {
            roomCode = enteredRoomCode;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            ShowErrorMessage("Room code is required to join a game.", false);
            return;
        }

        // Store player info for registration after connection
        string playerID = System.Guid.NewGuid().ToString();
        if (PlayerAuthSystem.Instance != null)
        {
            playerID = PlayerAuthSystem.Instance.GetLocalPlayerID();
        }

        // Setup callback to register player AFTER successful connection
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

            // NOW register the player locally
            if (PlayerAuthSystem.Instance != null)
            {
                PlayerAuthSystem.Instance.RegisterPlayer(playerName, selectedPlayerIconName);
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.AddPlayer(playerID, playerName, selectedPlayerIconName);
            }

            // Send to host
            if (RWMNetworkManager.Instance != null)
            {
                RWMNetworkManager.Instance.AddPlayerServerRpc(playerID, playerName, selectedPlayerIconName);
            }

            UpdateWaitingScreenUI(playerName);
            Debug.Log("Player successfully joined and registered: " + playerName);
        }

        // Register callback BEFORE attempting connection
        NetworkManager.Singleton.OnClientConnectedCallback += SendRegistrationWhenConnected;

        // Attempt connection
        ConnectToHostIfNeeded();

        // Show waiting screen (connection in progress)
        UpdateWaitingScreenUI(playerName);
        ShowJoinWait();
    }

    void ConnectToHostIfNeeded()
    {
        RegisterNetworkCallbacks();

        if (RWMNetworkManager.Instance == null || NetworkManager.Singleton == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }

        if (NetworkManager.Singleton.IsClient)
        {
            awaitingNetworkConnection = false;
            hasConnectedToHost = true;
            return;
        }

        bool started = RWMNetworkManager.Instance.JoinGame(roomCode);

        if (!started)
        {
            ShowJoinForm();
            ShowErrorMessage("Unable to start the network client. Please try again.", false);
            return;
        }

        awaitingNetworkConnection = true;
        hasConnectedToHost = false;
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

    void ShowErrorMessage(string message, bool logAsError = true)
    {
        MobileHaptics.Failure();

        if (logAsError)
        {
            Debug.LogError("INPUT ERROR: " + message);
        }
        else
        {
            Debug.LogWarning("INPUT WARNING: " + message);
        }

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

        // Detect new players by comparing counts
        int previousCount = spawnedPlayerIcons.Count;
        int newCount = 0;

        // Count non-host players
        foreach (PlayerData player in players)
        {
            if (!player.isHost)
            {
                newCount++;
            }
        }

        // Only update if player count changed
        if (previousCount == newCount) return;

        // Clear existing icons
        foreach (GameObject icon in spawnedPlayerIcons)
        {
            Destroy(icon);
        }
        spawnedPlayerIcons.Clear();

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
                Transform nameTransform = FindPlayerNameTransform(iconObj);
                if (nameTransform != null)
                {
                    TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                    if (nameText != null)
                    {
                        nameText.text = player.playerName;
                    }
                }

                // Set player icon
                Transform iconTransform = FindPlayerIconTransform(iconObj);
                if (iconTransform != null && PlayerManager.Instance != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = PlayerManager.Instance.GetPlayerIcon(player.iconName);
                    }
                }

                spawnedPlayerIcons.Add(iconObj);

                // Animate bounce scale-up for new player (only for newly added player)
                if (nonHostPlayerCount > previousCount)
                {
                    StartCoroutine(BounceScaleUp(iconObj.transform));
                }
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

    System.Collections.IEnumerator BounceScaleUp(Transform transform)
    {
        if (transform == null) yield break;

        float duration = 0.6f;
        float elapsed = 0f;

        // Start from scale 0
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        Vector3 overshootScale = targetScale * 1.2f; // Overshoot by 20%

        transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Elastic ease out with bounce
            float easedT;
            if (t < 0.5f)
            {
                // First half: scale up to overshoot
                easedT = 1f - Mathf.Pow(1f - (t * 2f), 3f);
                transform.localScale = Vector3.Lerp(startScale, overshootScale, easedT);
            }
            else
            {
                // Second half: bounce back to target
                easedT = 1f - Mathf.Pow(1f - ((t - 0.5f) * 2f), 2f);
                transform.localScale = Vector3.Lerp(overshootScale, targetScale, easedT);
            }

            yield return null;
        }

        transform.localScale = targetScale;
    }

    Transform FindPlayerIconTransform(GameObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        // Direct child search
        Transform icon = obj.transform.Find("PlayerIcon");
        if (icon != null)
        {
            return icon;
        }

        // Could add additional fallback logic here if needed
        return null;
    }

    Transform FindPlayerNameTransform(GameObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        // Direct child search
        Transform nameTransform = obj.transform.Find("PlayerName");
        if (nameTransform != null)
        {
            return nameTransform;
        }

        // Could add additional fallback logic here if needed
        return null;
    }

    void OnDestroy()
    {
        UnregisterNetworkCallbacks();

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