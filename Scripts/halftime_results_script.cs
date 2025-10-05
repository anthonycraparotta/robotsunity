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
    public Image mobileBackground;
    
    [Header("Prefabs")]
    public GameObject playerScoreRowPrefab;
    
    [Header("State")]
    private bool isMobile = false;
    private string playerID = "";
    
    void Start()
    {
        isMobile = Application.isMobilePlatform;
        
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
    
    void OnContinueClicked()
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