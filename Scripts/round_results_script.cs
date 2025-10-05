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
    
    [Header("Panel 1 - Your Results")]
    public GameObject panel1;
    public TextMeshProUGUI panel1Headline;
    public Transform panel1ResultsContainer;
    public TextMeshProUGUI playerResponse;
    public TextMeshProUGUI robotResponse;
    public TextMeshProUGUI trueResponse;
    public TextMeshProUGUI playerCumulativeScore;
    public TextMeshProUGUI scoreDiff;
    public TextMeshProUGUI scoreDiffFooled;
    public TextMeshProUGUI scoreDiffNotFooled;
    public TextMeshProUGUI scoreDiffRight;
    public TextMeshProUGUI fooledText;
    public TextMeshProUGUI notFooledText;
    
    [Header("Panel 2 - Answer Reveal")]
    public GameObject panel2;
    public TextMeshProUGUI panel2Headline;
    public Transform panel2ResultsContainer;
    public Image responseBackground;
    public Image trueBackground;
    public Image correctIndicator;
    
    [Header("Panel 3 - Other Players")]
    public GameObject panel3;
    public TextMeshProUGUI panel3Headline;
    public Transform panel3ResultsContainer;
    public TextMeshProUGUI otherTestersFooled;
    public TextMeshProUGUI humansFooledText;
    public TextMeshProUGUI numberOfFooled;
    public Transform otherIconsContainer;
    public Transform playerIconZoneFooled;
    public Transform playerIconZoneNotFooled;
    
    [Header("Navigation")]
    public Button nextRoundButton;
    public Button finalResultsButton;
    public Image roundResultsHero;
    
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
    public Image mobileBackground;
    
    [Header("Prefabs")]
    public GameObject playerIconPrefab;
    
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
        
        // Update round label
        UpdateRoundLabel();
    }
    
    void DisplayDesktopResults(string correctAnswer, string robotAnswer, string eliminatedAnswer)
    {
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
            
            // Get voting results
            var votingResults = GameManager.Instance.GetVotingResults();
            
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
        // Show player's personal results
        if (mobilePlayerScore != null)
        {
            PlayerData player = GameManager.Instance.GetPlayer(playerID);
            if (player != null)
            {
                mobilePlayerScore.text = player.scorePercentage + "%";
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
        string robotAnswer = GameManager.Instance.GetRobotAnswer();
        
        // Count how many players voted for robot
        int fooledCount = 0;
        if (votingResults.ContainsKey(robotAnswer))
        {
            fooledCount = votingResults[robotAnswer];
        }
        
        if (numberOfFooled != null)
        {
            numberOfFooled.text = fooledCount.ToString();
        }
        
        // Display player icons in fooled/not fooled zones
        // Implementation would populate playerIconZoneFooled and playerIconZoneNotFooled
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