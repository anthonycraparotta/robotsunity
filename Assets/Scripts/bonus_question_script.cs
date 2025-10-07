using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BonusQuestionScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public GameObject bonusQuestionCard;
    public TextMeshProUGUI questionText;
    public Transform playerIconContainer;
    public GameObject playerIconBonusPrefab;
    public Transform optionListContainer;
    public TextMeshProUGUI timerCountdown;
    public GameObject timerContainer;
    public TextMeshProUGUI tipText;
    public Image desktopBackground;
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public Transform bonusListMobile;
    public Button bonusSubmitButton;
    public Image mobileBackground;
    public GameObject bonusResponse;
    
    [Header("Player Button Prefab")]
    public GameObject playerButtonPrefab;
    
    [Header("State")]
    private bool isMobile = false;
    private string playerID = "";
    private string selectedPlayerID = "";
    private List<GameObject> spawnedPlayerButtons = new List<GameObject>();
    private List<GameObject> spawnedPlayerIcons = new List<GameObject>();
    
    void Start()
    {
        isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        if (isMobile)
        {
            playerID = PlayerAuthSystem.Instance != null ? PlayerAuthSystem.Instance.GetLocalPlayerID() : GetLocalPlayerID();
        }

        // Show appropriate display
        ShowAppropriateDisplay();

        // Initialize the bonus round
        InitializeBonusQuestion();

        // Setup mobile submit button
        if (bonusSubmitButton != null)
        {
            bonusSubmitButton.onClick.AddListener(OnSubmitVote);
            bonusSubmitButton.interactable = false;
        }

        // Set tip text
        if (tipText != null)
        {
            tipText.text = "Vote for a player!";
        }

        // Play appropriate music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBonusQuestionMusic();
        }
    }

    void InitializeBonusQuestion()
    {
        Debug.Log("InitializeBonusQuestion called for question index: " + GameManager.Instance.currentBonusQuestion);

        // Display current bonus question
        DisplayBonusQuestion();

        // Display player selection options (this will recreate icons hidden)
        DisplayPlayerOptions();

        // Reset selected player
        selectedPlayerID = "";

        // Re-enable buttons for new question on mobile
        if (isMobile)
        {
            if (bonusSubmitButton != null)
            {
                bonusSubmitButton.interactable = false;
            }

            // Re-enable all player buttons
            foreach (GameObject btn in spawnedPlayerButtons)
            {
                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = true;
                }

                // Reset button colors
                Image btnImage = btn.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = Color.white;
                }

                // Hide circle indicators
                Transform circle = btn.transform.Find("Circle");
                if (circle != null)
                {
                    circle.gameObject.SetActive(false);
                }
            }
        }
    }

    private int lastBonusQuestionIndex = -1;

    void Update()
    {
        // Check if we moved to next bonus question
        int currentQuestionIndex = GameManager.Instance.currentBonusQuestion;
        if (currentQuestionIndex != lastBonusQuestionIndex && lastBonusQuestionIndex != -1)
        {
            // New bonus question started
            Debug.Log("Detected new bonus question: " + currentQuestionIndex);
            InitializeBonusQuestion();
        }
        lastBonusQuestionIndex = currentQuestionIndex;

        // Update timer display
        UpdateTimerDisplay();

        // Update vote counts on desktop
        if (!isMobile)
        {
            UpdateVoteCounts();
        }

        // Check timer warning for audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.CheckTimerWarning(GameManager.Instance.GetTimeRemaining());
        }
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
    }
    
    void DisplayBonusQuestion()
    {
        string currentQuestion = GameManager.Instance.GetCurrentBonusQuestion();
        
        if (questionText != null)
        {
            questionText.text = currentQuestion;
        }
        
        // Show question number (1/4, 2/4, etc.)
        int questionNum = GameManager.Instance.currentBonusQuestion + 1;
        Debug.Log("Bonus Question " + questionNum + "/4");
    }
    
    void DisplayPlayerOptions()
    {
        List<PlayerData> players = GameManager.Instance.GetAllPlayers();
        
        if (isMobile)
        {
            // Mobile: Show as selectable list
            DisplayMobilePlayerList(players);
        }
        else
        {
            // Desktop: Show player icons
            DisplayDesktopPlayerIcons(players);
        }
    }
    
    void DisplayMobilePlayerList(List<PlayerData> players)
    {
        if (bonusListMobile == null) return;
        
        // Clear existing buttons
        foreach (GameObject btn in spawnedPlayerButtons)
        {
            Destroy(btn);
        }
        spawnedPlayerButtons.Clear();
        
        // Create button for each player
        foreach (PlayerData player in players)
        {
            CreatePlayerButton(player, bonusListMobile);
        }
    }
    
    void CreatePlayerButton(PlayerData player, Transform parent)
    {
        GameObject buttonObj;
        
        if (playerButtonPrefab != null)
        {
            buttonObj = Instantiate(playerButtonPrefab, parent);
        }
        else
        {
            // Create button programmatically
            buttonObj = new GameObject("PlayerButton");
            buttonObj.transform.SetParent(parent);
            buttonObj.AddComponent<RectTransform>();
            buttonObj.AddComponent<Image>();
            buttonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            textObj.AddComponent<RectTransform>();
            textObj.AddComponent<TextMeshProUGUI>();
        }
        
        // Set button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = player.playerName;
        }
        
        // Set player icon
        Image iconImage = buttonObj.transform.Find("PlayerIcon")?.GetComponent<Image>();
        if (iconImage != null && PlayerManager.Instance != null)
        {
            iconImage.sprite = PlayerManager.Instance.GetPlayerIcon(player.iconName);
        }
        
        // Add click listener
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnPlayerSelected(player.playerID, buttonObj));
        }
        
        spawnedPlayerButtons.Add(buttonObj);
    }
    
    void DisplayDesktopPlayerIcons(List<PlayerData> players)
    {
        if (playerIconContainer == null) return;

        // Clear existing icons
        foreach (GameObject icon in spawnedPlayerIcons)
        {
            Destroy(icon);
        }
        spawnedPlayerIcons.Clear();

        // Create icon for each player (initially hidden)
        foreach (PlayerData player in players)
        {
            if (playerIconBonusPrefab != null)
            {
                GameObject iconObj = Instantiate(playerIconBonusPrefab, playerIconContainer);

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

                // Hide icon initially - only show when player votes
                iconObj.SetActive(false);

                spawnedPlayerIcons.Add(iconObj);
            }
        }
    }
    
    void OnPlayerSelected(string votedPlayerID, GameObject buttonObj)
    {
        selectedPlayerID = votedPlayerID;
        
        // Visual feedback
        foreach (GameObject btn in spawnedPlayerButtons)
        {
            Image btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = Color.white;
            }
            
            // Hide circle indicators
            Transform circle = btn.transform.Find("Circle");
            if (circle != null)
            {
                circle.gameObject.SetActive(false);
            }
        }
        
        // Highlight selected
        Image selectedImage = buttonObj.GetComponent<Image>();
        if (selectedImage != null)
        {
            selectedImage.color = Color.yellow;
        }
        
        // Show circle on selected
        Transform selectedCircle = buttonObj.transform.Find("Circle");
        if (selectedCircle != null)
        {
            selectedCircle.gameObject.SetActive(true);
        }
        
        if (isMobile)
        {
            // Enable submit button
            if (bonusSubmitButton != null)
            {
                bonusSubmitButton.interactable = true;
            }
        }
        // Desktop is display-only, doesn't submit votes
        
        Debug.Log("Player selected: " + votedPlayerID);
    }
    
    public void OnSubmitVote()
    {
        if (string.IsNullOrEmpty(selectedPlayerID))
        {
            Debug.LogWarning("No player selected!");
            return;
        }

        SubmitBonusVote();
    }
    
    void SubmitBonusVote()
    {
        // Submit vote via NetworkManager
        if (RWMNetworkManager.Instance != null)
        {
            RWMNetworkManager.Instance.SubmitBonusVoteServerRpc(playerID, selectedPlayerID);
        }
        else
        {
            // Fallback for local testing without network
            GameManager.Instance.SubmitBonusVote(playerID, selectedPlayerID);
        }

        // Disable all buttons
        foreach (GameObject btn in spawnedPlayerButtons)
        {
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }

        if (bonusSubmitButton != null)
        {
            bonusSubmitButton.interactable = false;
        }

        Debug.Log("Bonus vote submitted for: " + selectedPlayerID);
    }
    
    void UpdateTimerDisplay()
    {
        if (timerCountdown != null)
        {
            timerCountdown.text = GameManager.Instance.GetTimerDisplay();
        }
    }
    
    void UpdateVoteCounts()
    {
        // Show icons for players who have voted
        List<PlayerData> allPlayers = GameManager.Instance.GetAllPlayers();
        var bonusVotes = GameManager.Instance.bonusVotes;

        for (int i = 0; i < allPlayers.Count && i < spawnedPlayerIcons.Count; i++)
        {
            PlayerData player = allPlayers[i];
            GameObject iconObj = spawnedPlayerIcons[i];

            // Show icon if player has voted
            bool hasVoted = bonusVotes.ContainsKey(player.playerID);
            iconObj.SetActive(hasVoted);
        }
    }
    
    string GetLocalPlayerID()
    {
        return "player_" + SystemInfo.deviceUniqueIdentifier;
    }
    
    void OnDestroy()
    {
        if (bonusSubmitButton != null)
        {
            bonusSubmitButton.onClick.RemoveListener(OnSubmitVote);
        }
        
        foreach (GameObject btn in spawnedPlayerButtons)
        {
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}