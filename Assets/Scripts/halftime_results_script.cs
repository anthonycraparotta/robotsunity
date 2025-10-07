using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class HalftimeResultsScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    
    [Header("Winner Section")]
    public GameObject winnerSection;
    public TextMeshProUGUI winnerHeadline;
    public Transform winnerIconContainer;
    public Image winBackground;
    public TextMeshProUGUI scoreDiffWin;
    
    [Header("Loser Section")]
    public GameObject loserSection;
    public TextMeshProUGUI loserHeadline;
    public Transform loserIconContainer;
    public Image loseBackground;
    public TextMeshProUGUI scoreDiffLose;
    
    [Header("Navigation")]
    public Button nextRoundButton;
    public GameObject buttonSlideout;
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public GameObject mobileResultsContainer;
    public TextMeshProUGUI mobilePlayerName;
    public TextMeshProUGUI mobilePlayerScore;
    public TextMeshProUGUI mobilePlayerRank;
    public Image mobileBackground;

    [Header("Prefabs")]
    public GameObject playerScoreRowPrefab; // Desktop prefab
    public GameObject playerIconHalftimePrefab; // Mobile prefab
    
    [Header("State")]
    private bool isMobile = false;
    private string playerID = "";
    
    void Start()
    {
        isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();
        
        if (isMobile)
        {
            playerID = GetLocalPlayerID();
        }
        
        // Show appropriate display
        ShowAppropriateDisplay();
        
        // Display halftime results
        DisplayHalftimeResults();
        
        // Setup continue button
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.AddListener(OnContinueClicked);
        }
        
        // Set headlines
        if (winnerHeadline != null)
        {
            winnerHeadline.text = "LEADING THE PACK";
        }
        
        if (loserHeadline != null)
        {
            loserHeadline.text = "STILL IN THE RUNNING";
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
    
    void DisplayHalftimeResults()
    {
        List<PlayerData> rankedPlayers = GameManager.Instance.GetPlayersByRank();

        if (rankedPlayers.Count == 0) return;

        if (isMobile)
        {
            DisplayMobileHalftimeResults(rankedPlayers);
        }
        else
        {
            DisplayDesktopHalftimeResults(rankedPlayers);
        }
    }

    void DisplayDesktopHalftimeResults(List<PlayerData> rankedPlayers)
    {
        // Split players into top half (winners) and bottom half (losers)
        int midpoint = Mathf.CeilToInt(rankedPlayers.Count / 2f);

        List<PlayerData> winners = rankedPlayers.Take(midpoint).ToList();
        List<PlayerData> losers = rankedPlayers.Skip(midpoint).ToList();

        // Display winner section
        if (winnerSection != null && winnerIconContainer != null)
        {
            DisplayPlayerGroup(winners, winnerIconContainer, true);
        }

        // Display loser section
        if (loserSection != null && loserIconContainer != null)
        {
            DisplayPlayerGroup(losers, loserIconContainer, false);
        }
    }

    void DisplayMobileHalftimeResults(List<PlayerData> rankedPlayers)
    {
        PlayerData localPlayer = GameManager.Instance.GetPlayer(playerID);

        if (localPlayer != null)
        {
            // Display player name
            if (mobilePlayerName != null)
            {
                mobilePlayerName.text = localPlayer.playerName;
            }

            // Display player score
            if (mobilePlayerScore != null)
            {
                mobilePlayerScore.text = localPlayer.scorePercentage + "%";
            }

            // Display player rank
            if (mobilePlayerRank != null)
            {
                int rank = rankedPlayers.FindIndex(p => p.playerID == playerID) + 1;
                mobilePlayerRank.text = "Rank: " + rank + " / " + rankedPlayers.Count;
            }

            // Display player icon
            if (mobileResultsContainer != null && playerIconHalftimePrefab != null)
            {
                // Clear existing icons
                foreach (Transform child in mobileResultsContainer.transform)
                {
                    if (child.GetComponent<Image>() != null) // Only destroy icon objects
                    {
                        Destroy(child.gameObject);
                    }
                }

                // Instantiate player icon
                GameObject iconObj = Instantiate(playerIconHalftimePrefab, mobileResultsContainer.transform);

                // Set player icon sprite
                Image iconImage = iconObj.GetComponent<Image>();
                if (iconImage != null && PlayerManager.Instance != null)
                {
                    iconImage.sprite = PlayerManager.Instance.GetPlayerIcon(localPlayer.iconName);
                }
            }
        }
    }
    
    void DisplayPlayerGroup(List<PlayerData> players, Transform container, bool isWinnerSection)
    {
        // Clear existing content
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        
        // Display each player
        foreach (PlayerData player in players)
        {
            GameObject playerRow = CreatePlayerRow(player, container);
            
            // Additional styling for winner/loser sections could go here
        }
    }
    
    GameObject CreatePlayerRow(PlayerData player, Transform parent)
    {
        GameObject rowObj;
        
        if (playerScoreRowPrefab != null)
        {
            rowObj = Instantiate(playerScoreRowPrefab, parent);
        }
        else
        {
            // Create row programmatically
            rowObj = new GameObject("PlayerRow");
            rowObj.transform.SetParent(parent);
            rowObj.AddComponent<RectTransform>();
        }
        
        // Set player name
        TextMeshProUGUI nameText = rowObj.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = player.playerName;
        }
        
        // Find and set score text
        TextMeshProUGUI[] allText = rowObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (allText.Length > 1)
        {
            allText[1].text = player.scorePercentage + "%";
        }
        
        // Set player icon
        Image iconImage = rowObj.GetComponentInChildren<Image>();
        if (iconImage != null)
        {
            // Load player icon
            // iconImage.sprite = Resources.Load<Sprite>("PlayerIcons/" + player.iconName);
        }
        
        return rowObj;
    }
    
    public void OnContinueClicked()
    {
        // Continue to Bonus Round
        GameManager.Instance.AdvanceToNextScreen();
    }
    
    string GetLocalPlayerID()
    {
        return "player_" + SystemInfo.deviceUniqueIdentifier;
    }
    
    void OnDestroy()
    {
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}