using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class RoundResultsScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public GameObject resultsPanelContainer;
    
    [Header("Panel 1 - True, Robot Answers")]
    public GameObject panel1;
    public Image panel1Background;
    public TextMeshProUGUI panel1Headline;
    public TextMeshProUGUI trueResponse;
    public TextMeshProUGUI robotResponse;
    public TextMeshProUGUI scoreDiffTrue; // Score difference for getting it right
    public GameObject resultsPlayerIconPrefab; // Icon prefab for Panel 1

    [Header("Panel 2 - Player Responses")]
    public GameObject panel2;
    public Image panel2Background;
    public TextMeshProUGUI panel2Headline;
    public Transform panel2ResultsContainer; // Container for ResultsPlayerResponse prefabs
    public GameObject resultsPlayerResponsePrefab; // Prefab with: ScoreDiff, PlayerResponse, Number of Testers Fooled

    [Header("Panel 3 - Game Standings")]
    public GameObject panel3;
    public Image panel3Background;
    public TextMeshProUGUI panel3Headline;
    public Transform panel3ResultsContainer; // Container for ResultsPlayerRank prefabs
    public GameObject resultsPlayerRankPrefab;
    public GameObject panel3ResultsPlayerIconPrefab; // Icon prefab for Panel 3
    
    [Header("Navigation")]
    public Button nextRoundButton;
    public Button finalResultsButton;

    [Header("Round Hero Images (Hero1 - Hero12)")]
    public Image[] heroImages = new Image[12];

    [Header("Round Labels")]
    public TextMeshProUGUI[] roundLabels = new TextMeshProUGUI[12];
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public GameObject mobileResultsContainer;
    public TextMeshProUGUI roundResultsMobileHeadline;
    public TextMeshProUGUI mobilePlayerScore;
    public TextMeshProUGUI mobileScoreDiff;
    public TextMeshProUGUI mobileTrueResponse;
    public TextMeshProUGUI mobileRobotResponse;
    public TextMeshProUGUI mobilePlayerRank;
    public Image mobilePlayerIcon;
    public Image mobileBackground;

    [Header("Prefabs")]
    public GameObject playerIconPrefab; // For PlayerIconZone containers (fooled/not fooled zones)
    
    [Header("State")]
    private bool isMobile = false;
    private string playerID = "";
    
    void Start()
    {
        isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        if (isMobile)
        {
            playerID = PlayerAuthSystem.Instance != null ? PlayerAuthSystem.Instance.GetLocalPlayerID() : GetLocalPlayerID();
        }
        
        // Show appropriate display
        ShowAppropriateDisplay();
        
        // Display results
        DisplayResults();
        
        // Setup navigation buttons
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.AddListener(OnNextRoundClicked);
        }
        
        if (finalResultsButton != null)
        {
            finalResultsButton.onClick.AddListener(OnFinalResultsClicked);
        }
        
        // Show/hide appropriate button
        UpdateNavigationButtons();

        // Play results music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayResultsMusic();
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
    
    void DisplayResults()
    {
        string correctAnswer = GameManager.Instance.GetCorrectAnswer();
        string robotAnswer = GameManager.Instance.GetRobotAnswer();
        string eliminatedAnswer = GameManager.Instance.GetEliminatedAnswer();
        
        if (isMobile)
        {
            DisplayMobileResults(correctAnswer, robotAnswer);
        }
        else
        {
            DisplayDesktopResults(correctAnswer, robotAnswer, eliminatedAnswer);
        }
        
        // Update round label and hero image
        UpdateRoundLabel();
        UpdateRoundHeroImage();
    }
    
    void DisplayDesktopResults(string correctAnswer, string robotAnswer, string eliminatedAnswer)
    {
        // Get voting results
        var votingResults = GameManager.Instance.GetVotingResults();

        // Panel 1: Player's answer and score changes
        if (panel1 != null)
        {
            // Set answers
            if (trueResponse != null)
            {
                trueResponse.text = correctAnswer;
            }

            if (this.robotResponse != null)
            {
                this.robotResponse.text = robotAnswer;
            }

            // Calculate score changes for each player
            // This would show all players' results in a real implementation
        }

        // Panel 2: Highlight correct answer
        if (panel2 != null)
        {
            // Visual indication of correct vs robot answer
        }

        // Panel 3: Show who was fooled
        if (panel3 != null)
        {
            DisplayFooledPlayers(votingResults);
        }
    }
    
    void DisplayMobileResults(string correctAnswer, string robotAnswer)
    {
        PlayerData player = GameManager.Instance.GetPlayer(playerID);

        if (player != null)
        {
            // Show player's personal results
            if (mobilePlayerScore != null)
            {
                mobilePlayerScore.text = player.scorePercentage + "%";
            }

            // Show player icon
            if (mobilePlayerIcon != null && PlayerManager.Instance != null)
            {
                mobilePlayerIcon.sprite = PlayerManager.Instance.GetPlayerIcon(player.iconName);
            }
        }

        // Show correct and robot answers
        if (mobileTrueResponse != null)
        {
            mobileTrueResponse.text = correctAnswer;
        }

        if (mobileRobotResponse != null)
        {
            mobileRobotResponse.text = robotAnswer;
        }

        // Show player's rank
        if (mobilePlayerRank != null)
        {
            List<PlayerData> rankedPlayers = GameManager.Instance.GetPlayersByRank();
            int rank = rankedPlayers.FindIndex(p => p.playerID == playerID) + 1;
            mobilePlayerRank.text = "Rank: " + rank;
        }
    }
    
    void DisplayFooledPlayers(Dictionary<string, int> votingResults)
    {
        // This data is now displayed via ResultsPlayerResponse prefabs in Panel 2
        // Each prefab shows the number of testers fooled by that specific answer
    }
    
    void UpdateRoundLabel()
    {
        int currentRound = GameManager.Instance.GetCurrentRound();

        // Hide all round labels
        foreach (var label in roundLabels)
        {
            if (label != null)
            {
                label.gameObject.SetActive(false);
            }
        }

        // Show current round label
        if (currentRound > 0 && currentRound <= roundLabels.Length)
        {
            if (roundLabels[currentRound - 1] != null)
            {
                roundLabels[currentRound - 1].gameObject.SetActive(true);
            }
        }
    }

    void UpdateRoundHeroImage()
    {
        int currentRound = GameManager.Instance.GetCurrentRound();

        // Hide all hero images
        foreach (var hero in heroImages)
        {
            if (hero != null)
            {
                hero.gameObject.SetActive(false);
            }
        }

        // Show current round's hero image
        if (currentRound > 0 && currentRound <= heroImages.Length)
        {
            if (heroImages[currentRound - 1] != null)
            {
                heroImages[currentRound - 1].gameObject.SetActive(true);
            }
        }
    }
    
    void UpdateNavigationButtons()
    {
        int currentRound = GameManager.Instance.GetCurrentRound();
        int totalRounds = (GameManager.Instance.gameMode == GameManager.GameMode.EightQuestions) ? 8 : 12;
        
        bool isLastRound = currentRound >= totalRounds;
        
        if (nextRoundButton != null)
        {
            nextRoundButton.gameObject.SetActive(!isLastRound);
        }
        
        if (finalResultsButton != null)
        {
            finalResultsButton.gameObject.SetActive(isLastRound);
        }
    }
    
    void OnNextRoundClicked()
    {
        GameManager.Instance.AdvanceToNextScreen();
    }
    
    void OnFinalResultsClicked()
    {
        // Force to final results even if game logic would go elsewhere
        GameManager.Instance.currentGameState = GameManager.GameState.FinalResults;
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
            nextRoundButton.onClick.RemoveListener(OnNextRoundClicked);
        }
        
        if (finalResultsButton != null)
        {
            finalResultsButton.onClick.RemoveListener(OnFinalResultsClicked);
        }
    }
}