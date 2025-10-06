using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class FinalResultsScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    
    [Header("Winner Section")]
    public GameObject winnerSection;
    public TextMeshProUGUI winnerHeadline;
    public TextMeshProUGUI winnerName;
    public Transform winnerIconContainer;
    public Image winBackground;
    public TextMeshProUGUI scoreDiffWin;
    public TextMeshProUGUI winnerScore;
    
    [Header("Loser Section")]
    public GameObject loserSection;
    public TextMeshProUGUI loserHeadline;
    public Transform loserIconContainer;
    public Image loseBackground;
    public TextMeshProUGUI scoreDiffLose;
    
    [Header("Desktop Navigation")]
    public Button creditsButton; // Go to Credits scene
    public Button newGameButton; // Play Again - back to lobby
    public GameObject buttonSlideout;
    public Image hero;

    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public GameObject mobileResultsContainer;
    public TextMeshProUGUI mobileWinnerName;
    public TextMeshProUGUI mobilePlayerRank;
    public TextMeshProUGUI mobilePlayerScore;
    public TextMeshProUGUI mobilePlayerName;
    public Button mobileShareButton;
    public Button mobileWebButton;
    public Image mobileHero;
    public Image mobileBackground;
    
    [Header("Prefabs")]
    public GameObject playerRowPrefab;
    
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

        // Display final results
        DisplayFinalResults();

        // Preload credits data for faster transition
        PreloadCreditsData();

        // Setup buttons
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OnCreditsClicked);
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }

        if (mobileShareButton != null)
        {
            mobileShareButton.onClick.AddListener(OnShareClicked);
        }

        if (mobileWebButton != null)
        {
            mobileWebButton.onClick.AddListener(OnWebsiteClicked);
        }
    }

    void PreloadCreditsData()
    {
        // Preload the credits JSON file so it's cached for CreditsScreen
        TextAsset creditsFile = Resources.Load<TextAsset>("credits");

        if (creditsFile != null)
        {
            Debug.Log("Credits data preloaded successfully");
        }
        else
        {
            Debug.LogWarning("Could not preload credits data - credits.json not found in Resources folder");
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
    
    void DisplayFinalResults()
    {
        List<PlayerData> rankedPlayers = GameManager.Instance.GetPlayersByRank();
        
        if (rankedPlayers.Count == 0) return;
        
        if (isMobile)
        {
            DisplayMobileFinalResults(rankedPlayers);
        }
        else
        {
            DisplayDesktopFinalResults(rankedPlayers);
        }
    }
    
    void DisplayDesktopFinalResults(List<PlayerData> rankedPlayers)
    {
        // Get winner (highest score)
        PlayerData winner = rankedPlayers[0];
        
        // Get loser (lowest score)
        PlayerData loser = rankedPlayers[rankedPlayers.Count - 1];
        
        // Display winner section
        if (winnerSection != null)
        {
            if (winnerHeadline != null)
            {
                winnerHeadline.text = "MOST HUMAN";
            }
            
            if (winnerName != null)
            {
                winnerName.text = winner.playerName;
            }
            
            if (winnerScore != null)
            {
                winnerScore.text = winner.scorePercentage + "%";
            }
            
            if (scoreDiffWin != null)
            {
                scoreDiffWin.text = "+" + winner.scorePercentage + "%";
            }
            
            // Display winner icon
            if (winnerIconContainer != null)
            {
                DisplayPlayerIcon(winner, winnerIconContainer);
            }
        }
        
        // Display loser section (bottom player)
        if (loserSection != null)
        {
            if (loserHeadline != null)
            {
                loserHeadline.text = "PROBABLY A ROBOT";
            }
            
            if (scoreDiffLose != null)
            {
                scoreDiffLose.text = loser.scorePercentage + "%";
            }
            
            // Display loser icon
            if (loserIconContainer != null)
            {
                DisplayPlayerIcon(loser, loserIconContainer);
            }
        }
        
        // Display all players in ranked order
        DisplayFullLeaderboard(rankedPlayers);
    }
    
    void DisplayMobileFinalResults(List<PlayerData> rankedPlayers)
    {
        PlayerData winner = rankedPlayers[0];
        PlayerData localPlayer = GameManager.Instance.GetPlayer(playerID);
        
        // Show winner
        if (mobileWinnerName != null)
        {
            mobileWinnerName.text = winner.playerName + " WINS!";
        }
        
        // Show local player's results
        if (localPlayer != null)
        {
            if (mobilePlayerName != null)
            {
                mobilePlayerName.text = localPlayer.playerName;
            }
            
            if (mobilePlayerScore != null)
            {
                mobilePlayerScore.text = localPlayer.scorePercentage + "%";
            }
            
            int rank = rankedPlayers.FindIndex(p => p.playerID == playerID) + 1;
            if (mobilePlayerRank != null)
            {
                mobilePlayerRank.text = "Rank: " + rank + " / " + rankedPlayers.Count;
            }
        }
    }
    
    void DisplayPlayerIcon(PlayerData player, Transform container)
    {
        // Clear existing
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        
        // Create player icon
        if (playerRowPrefab != null)
        {
            GameObject iconObj = Instantiate(playerRowPrefab, container);
            
            // Set player name
            TextMeshProUGUI nameText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = player.playerName;
            }
            
            // Set player icon image
            Image iconImage = iconObj.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                // Load player icon
                // iconImage.sprite = Resources.Load<Sprite>("PlayerIcons/" + player.iconName);
            }
        }
    }
    
    void DisplayFullLeaderboard(List<PlayerData> rankedPlayers)
    {
        // This would populate a full leaderboard display
        // Implementation depends on your UI design
    }
    
    void OnCreditsClicked()
    {
        // Go to Credits scene
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("CreditsScreen");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CreditsScreen");
        }
    }

    void OnNewGameClicked()
    {
        // Reset game and go back to lobby
        GameManager.Instance.currentRound = 0;
        GameManager.Instance.isHalftimePlayed = false;
        GameManager.Instance.isBonusRoundPlayed = false;

        // Reset all scores
        foreach (var player in GameManager.Instance.players.Values)
        {
            player.scorePercentage = 0;
        }

        // Go back to lobby
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("LobbyScreen");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScreen");
        }
    }

    void OnShareClicked()
    {
        // Share results functionality
        Debug.Log("Share results clicked");

        // Implementation would generate shareable image or text
        // Example: Take screenshot, share to social media, etc.
    }

    void OnWebsiteClicked()
    {
        // Open game website
        Debug.Log("Website clicked");
        Application.OpenURL("https://robotswearingmoustaches.com"); // Replace with actual URL
    }
    
    string GetLocalPlayerID()
    {
        return "player_" + SystemInfo.deviceUniqueIdentifier;
    }
    
    void OnDestroy()
    {
        if (creditsButton != null)
        {
            creditsButton.onClick.RemoveListener(OnCreditsClicked);
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveListener(OnNewGameClicked);
        }

        if (mobileShareButton != null)
        {
            mobileShareButton.onClick.RemoveListener(OnShareClicked);
        }

        if (mobileWebButton != null)
        {
            mobileWebButton.onClick.RemoveListener(OnWebsiteClicked);
        }
    }
}