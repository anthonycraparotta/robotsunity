using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestionScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI timerCountdown;
    public Transform playerIconContainer;
    public GameObject playerIconQuestionPrefab;
    public Transform backgroundContainer;
    public Transform robotForegrounds;
    public GameObject questionCard;
    public GameObject timerContainer;
    public TextMeshProUGUI tipText;
    public Image desktopBackground;
    
    [Header("Round Backgrounds (Round1BG - Round12BG)")]
    public Image[] roundBackgrounds = new Image[12];
    
    [Header("Robot Foregrounds (Robot1 - Robot12)")]
    public Image[] robotImages = new Image[12];
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public TMP_InputField answerInput;
    public Button answerSubmitButton;
    public Image mobileBackground;
    
    [Header("State")]
    private bool isMobile = false;
    private string playerID = "";
    private List<GameObject> spawnedPlayerIcons = new List<GameObject>();
    
    void Start()
    {
        isMobile = Application.isMobilePlatform;
        
        // Get player ID for mobile
        if (isMobile)
        {
            // In a real implementation, this would come from authentication/session
            playerID = GetLocalPlayerID();
        }
        
        // Show appropriate display
        ShowAppropriateDisplay();
        
        // Display question
        DisplayQuestion();
        
        // Show round-specific visuals
        ShowRoundVisuals();
        
        // Setup submit button
        if (answerSubmitButton != null)
        {
            answerSubmitButton.onClick.AddListener(OnSubmitAnswer);
        }
        
        // Setup input field
        if (answerInput != null)
        {
            answerInput.characterLimit = 100; // Reasonable character limit
        }
    }
    
    void Update()
    {
        // Update timer display
        UpdateTimerDisplay();
        
        // Update player status indicators (desktop only)
        if (!isMobile)
        {
            UpdatePlayerStatusIndicators();
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
    
    void DisplayQuestion()
    {
        Question currentQuestion = GameManager.Instance.GetCurrentQuestion();
        
        if (currentQuestion != null && questionText != null)
        {
            questionText.text = currentQuestion.questionText;
        }
        
        // Set tip text based on question type
        if (tipText != null)
        {
            if (GameManager.Instance.IsPlayerQuestion())
            {
                tipText.text = "Share your opinion!";
            }
            else
            {
                tipText.text = "Type your answer below";
            }
        }
    }
    
    void ShowRoundVisuals()
    {
        int currentRound = GameManager.Instance.GetCurrentRound();
        
        // Hide all round backgrounds
        for (int i = 0; i < roundBackgrounds.Length; i++)
        {
            if (roundBackgrounds[i] != null)
            {
                roundBackgrounds[i].gameObject.SetActive(false);
            }
        }
        
        // Show current round background
        if (currentRound > 0 && currentRound <= roundBackgrounds.Length)
        {
            if (roundBackgrounds[currentRound - 1] != null)
            {
                roundBackgrounds[currentRound - 1].gameObject.SetActive(true);
            }
        }
        
        // Hide all robot foregrounds
        for (int i = 0; i < robotImages.Length; i++)
        {
            if (robotImages[i] != null)
            {
                robotImages[i].gameObject.SetActive(false);
            }
        }
        
        // Show current round robot
        if (currentRound > 0 && currentRound <= robotImages.Length)
        {
            if (robotImages[currentRound - 1] != null)
            {
                robotImages[currentRound - 1].gameObject.SetActive(true);
            }
        }
    }
    
    void UpdateTimerDisplay()
    {
        if (timerCountdown != null)
        {
            timerCountdown.text = GameManager.Instance.GetTimerDisplay();
        }
    }
    
    void UpdatePlayerStatusIndicators()
    {
        if (playerIconContainer == null) return;
        
        List<PlayerData> players = GameManager.Instance.GetAllPlayers();
        
        // Clear existing icons if count changed
        if (spawnedPlayerIcons.Count != players.Count)
        {
            foreach (GameObject icon in spawnedPlayerIcons)
            {
                Destroy(icon);
            }
            spawnedPlayerIcons.Clear();
            
            // Spawn new icons
            foreach (PlayerData player in players)
            {
                if (playerIconQuestionPrefab != null)
                {
                    GameObject iconObj = Instantiate(playerIconQuestionPrefab, playerIconContainer);
                    
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
        }
        
        // Update status indicators (show if player has submitted)
        for (int i = 0; i < spawnedPlayerIcons.Count && i < players.Count; i++)
        {
            GameObject iconObj = spawnedPlayerIcons[i];
            PlayerData player = players[i];
            
            // Check if this player has submitted an answer
            bool hasSubmitted = GameManager.Instance.currentRoundAnswers.ContainsKey(player.playerID);
            
            // Update circle indicator
            Image circle = iconObj.transform.Find("Circle")?.GetComponent<Image>();
            if (circle != null)
            {
                circle.enabled = hasSubmitted;
            }
        }
    }
    
    void OnSubmitAnswer()
    {
        if (answerInput == null || string.IsNullOrEmpty(answerInput.text))
        {
            Debug.LogWarning("Please enter an answer!");
            return;
        }
        
        // Submit answer to GameManager
        GameManager.Instance.SubmitPlayerAnswer(playerID, answerInput.text);
        
        // Disable input and button
        answerInput.interactable = false;
        answerSubmitButton.interactable = false;
        
        Debug.Log("Answer submitted: " + answerInput.text);
    }
    
    string GetLocalPlayerID()
    {
        // In a real implementation, this would retrieve the authenticated player's ID
        // For now, return a placeholder
        return "player_" + SystemInfo.deviceUniqueIdentifier;
    }
    
    void OnDestroy()
    {
        if (answerSubmitButton != null)
        {
            answerSubmitButton.onClick.RemoveListener(OnSubmitAnswer);
        }
    }
}