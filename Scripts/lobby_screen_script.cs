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
    public GameObject joinForm;
    public TMP_InputField nameInput;
    public Button joinButton;
    public Transform scrollingPlayerIconContainer;
    public Image selectedIcon;
    public Transform playerIconSelectionContainer;
    public Image namebar;
    public TextMeshProUGUI playerNameDisplay;
    public Image mobileBackground;
    
    [Header("Mobile UI Elements - Join Wait")]
    public GameObject joinWait;
    public TextMeshProUGUI waitingHeadlineText;
    public Image waitPlayerIcon;
    public TextMeshProUGUI waitData;
    public TextMeshProUGUI waitDataHeaders;
    
    [Header("State")]
    private string selectedPlayerIconName = "";
    private string roomCode = "";
    private bool isMobile = false;
    private List<GameObject> spawnedPlayerIcons = new List<GameObject>();
    
    void Start()
    {
        isMobile = Application.isMobilePlatform;
        
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
        
        // Setup mobile join button
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
        }
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
        // Generate a random 4-character room code
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Text.StringBuilder code = new System.Text.StringBuilder();
        
        for (int i = 0; i < 4; i++)
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
    
    void OnStartGameClicked()
    {
        // Check if we have enough players (minimum 2)
        if (GameManager.Instance.GetAllPlayers().Count < 2)
        {
            Debug.LogWarning("Need at least 2 players to start!");
            return;
        }
        
        // Start the game
        GameManager.Instance.StartGame(GameManager.Instance.gameMode);
    }
    
    // === MOBILE JOIN FORM ===
    
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
        if (selectedIcon != null)
        {
            // Load the icon sprite
            // selectedIcon.sprite = Resources.Load<Sprite>("PlayerIcons/" + iconName);
        }
        
        Debug.Log("Player icon selected: " + iconName);
    }
    
    void OnJoinButtonClicked()
    {
        if (nameInput == null || string.IsNullOrEmpty(nameInput.text))
        {
            Debug.LogWarning("Please enter a name!");
            return;
        }
        
        if (string.IsNullOrEmpty(selectedPlayerIconName))
        {
            Debug.LogWarning("Please select an icon!");
            return;
        }
        
        // Create player ID
        string playerID = System.Guid.NewGuid().ToString();
        
        // Add player to game manager
        GameManager.Instance.AddPlayer(playerID, nameInput.text, selectedPlayerIconName);
        
        // Switch to waiting screen
        ShowJoinWait();
        
        Debug.Log("Player joined: " + nameInput.text);
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
        
        // Spawn new icons for each player
        foreach (PlayerData player in players)
        {
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
                if (iconImage != null)
                {
                    // Load the icon sprite
                    // iconImage.sprite = Resources.Load<Sprite>("PlayerIcons/" + player.iconName);
                }
                
                spawnedPlayerIcons.Add(iconObj);
            }
        }
        
        // Update player count on mobile wait screen
        if (isMobile && waitData != null)
        {
            waitData.text = players.Count + " Players\n" + 
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
        
        if (joinButton != null)
        {
            joinButton.onClick.RemoveListener(OnJoinButtonClicked);
        }
    }
}