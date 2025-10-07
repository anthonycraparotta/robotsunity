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

    [Header("Prefab Component Names")]
    [SerializeField] private string rankComponentName = "Rank";
    [SerializeField] private string playerNameComponentName = "PlayerName";
    [SerializeField] private string scoreComponentName = "Score";
    [SerializeField] private string playerIconComponentName = "PlayerIcon";

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
            rowObj.SetActive(true);
        }
        else
        {
            // Create row programmatically
            rowObj = new GameObject("ResultRow");
            rowObj.transform.SetParent(parent);
            rowObj.AddComponent<RectTransform>();
        }

        // Find and activate components using configurable names
        Transform rankTransform = FindDeepChild(rowObj.transform, rankComponentName);
        Transform nameTransform = FindDeepChild(rowObj.transform, playerNameComponentName);
        Transform scoreTransform = FindDeepChild(rowObj.transform, scoreComponentName);
        Transform iconTransform = FindDeepChild(rowObj.transform, playerIconComponentName);

        // Activate and set rank
        if (rankTransform != null) rankTransform.gameObject.SetActive(true);
        TextMeshProUGUI rankText = rankTransform?.GetComponent<TextMeshProUGUI>();
        if (rankText != null)
        {
            rankText.enabled = true;
            rankText.text = rank.ToString();
        }

        // Activate and set player name
        if (nameTransform != null) nameTransform.gameObject.SetActive(true);
        TextMeshProUGUI nameText = nameTransform?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.enabled = true;
            nameText.text = player.playerName;
        }

        // Activate and set score
        if (scoreTransform != null) scoreTransform.gameObject.SetActive(true);
        TextMeshProUGUI scoreText = scoreTransform?.GetComponent<TextMeshProUGUI>();
        if (scoreText != null)
        {
            scoreText.enabled = true;
            scoreText.text = player.scorePercentage + "%";
        }

        // Activate and set player icon
        if (iconTransform != null) iconTransform.gameObject.SetActive(true);
        Image iconImage = iconTransform?.GetComponent<Image>();
        if (iconImage != null && PlayerManager.Instance != null)
        {
            iconImage.enabled = true;
            Sprite iconSprite = PlayerManager.Instance.GetPlayerIcon(player.iconName);
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
            }
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

    // Helper method to find child by name recursively (search all descendants)
    Transform FindDeepChild(Transform parent, string childName)
    {
        // Check direct children first
        Transform result = parent.Find(childName);
        if (result != null)
            return result;

        // Search all descendants
        foreach (Transform child in parent)
        {
            result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }

        return null;
    }

    void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}