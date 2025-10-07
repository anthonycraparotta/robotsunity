using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

        // Clear any placeholder player icons from the scene
        ClearPlayerIconPlaceholders();

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

        // Generate room code if desktop
        if (!isMobile)
        {
            GenerateRoomCode();

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
    
    void GenerateRoomCode()
    {
        // Generate a random 5-character room code
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
        
        Debug.Log("Room Code: " + roomCode);
    }
    
    void OnGameModeSelected(GameManager.GameMode mode)
    {
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

            // Create player ID
            string playerID = System.Guid.NewGuid().ToString();

            // Add player to game manager
            GameManager.Instance.AddPlayer(playerID, sanitizedName, selectedPlayerIconName);

            // Switch to waiting screen
            ShowJoinWait();

            Debug.Log("Player joined: " + sanitizedName);
        }
        else
        {
            // Fallback if ContentFilterManager not available
            string playerID = System.Guid.NewGuid().ToString();
            GameManager.Instance.AddPlayer(playerID, nameInput.text.Trim(), selectedPlayerIconName);
            ShowJoinWait();
            Debug.Log("Player joined: " + nameInput.text);
        }
    }

    void ShowErrorMessage(string message)
    {
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

    void ClearPlayerIconPlaceholders()
    {
        if (playerIconContainer == null) return;

        // Destroy all children (placeholders from the scene)
        foreach (Transform child in playerIconContainer)
        {
            Destroy(child.gameObject);
        }
    }

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
            waitData.text = nonHostPlayerCount + " Players\n" +
                           (GameManager.Instance.gameMode == GameManager.GameMode.EightQuestions ? "8" : "12") + " Questions\n" +
                           roomCode;
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