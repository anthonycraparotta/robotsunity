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

    [Header("Prefab Component Names")]
    [SerializeField] private string playerNameComponentName = "PlayerName";
    [SerializeField] private string playerScoreComponentName = "Score";
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
            rowObj.SetActive(true);
        }
        else
        {
            // Create row programmatically
            rowObj = new GameObject("PlayerRow");
            rowObj.transform.SetParent(parent);
            rowObj.AddComponent<RectTransform>();
        }

        // Find and activate components using configurable names
        Transform nameTransform = FindDeepChild(rowObj.transform, playerNameComponentName);
        Transform scoreTransform = FindDeepChild(rowObj.transform, playerScoreComponentName);
        Transform iconTransform = FindDeepChild(rowObj.transform, playerIconComponentName);

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
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}