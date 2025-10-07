using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BonusResultsScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public Transform resultsContainer;
    public TextMeshProUGUI resultsHeadline;
    public Image desktopBackground;
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public Transform mobileResultsContainer;
    public TextMeshProUGUI mobileResultsHeadline;
    public Image mobileBackground;
    
    [Header("Navigation")]
    public Button continueButton;
    
    [Header("Prefabs")]
    public GameObject resultRowPrefab;
    
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
        
        // Display bonus round results
        DisplayBonusResults();
        
        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        // Set headline
        if (resultsHeadline != null)
        {
            resultsHeadline.text = "BONUS ROUND COMPLETE!";
        }
        
        if (mobileResultsHeadline != null)
        {
            mobileResultsHeadline.text = "Bonus Round Results";
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
    
    void DisplayBonusResults()
    {
        // Show updated scores after bonus round
        List<PlayerData> rankedPlayers = GameManager.Instance.GetPlayersByRank();
        
        Transform container = isMobile ? mobileResultsContainer : resultsContainer;
        
        if (container == null) return;
        
        // Clear existing content
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        
        // Display each player's result
        int rank = 1;
        foreach (PlayerData player in rankedPlayers)
        {
            CreateResultRow(player, rank, container);
            rank++;
        }
    }
    
    void CreateResultRow(PlayerData player, int rank, Transform parent)
    {
        GameObject rowObj;
        
        if (resultRowPrefab != null)
        {
            rowObj = Instantiate(resultRowPrefab, parent);
        }
        else
        {
            // Create row programmatically
            rowObj = new GameObject("ResultRow");
            rowObj.transform.SetParent(parent);
            rowObj.AddComponent<RectTransform>();
        }
        
        // Set rank
        TextMeshProUGUI rankText = rowObj.transform.Find("Rank")?.GetComponent<TextMeshProUGUI>();
        if (rankText != null)
        {
            rankText.text = rank.ToString();
        }
        
        // Set player name
        TextMeshProUGUI nameText = rowObj.transform.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = player.playerName;
        }
        
        // Set score
        TextMeshProUGUI scoreText = rowObj.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();
        if (scoreText != null)
        {
            scoreText.text = player.scorePercentage + "%";
        }
        
        // Set player icon
        Image iconImage = rowObj.transform.Find("PlayerIcon")?.GetComponent<Image>();
        if (iconImage != null)
        {
            // Load player icon
            // iconImage.sprite = Resources.Load<Sprite>("PlayerIcons/" + player.iconName);
        }
    }
    
    public void OnContinueClicked()
    {
        // Continue to next round (Round 5 for 8Q, Round 7 for 12Q)
        GameManager.Instance.AdvanceToNextScreen();
    }
    
    string GetLocalPlayerID()
    {
        return "player_" + SystemInfo.deviceUniqueIdentifier;
    }
    
    void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}