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

    [Header("Error Display")]
    public TextMeshProUGUI errorMessageText; // Connect in Unity - shows validation errors
    public float errorDisplayDuration = 3f; // How long to show error messages

    [Header("State")]
    private bool isMobile = false;
    private string playerID = "";
    private List<GameObject> spawnedPlayerIcons = new List<GameObject>();
    private Coroutine errorCoroutine;
    
    void Start()
    {
        isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        // Get player ID for mobile
        if (isMobile)
        {
            playerID = PlayerAuthSystem.Instance != null ? PlayerAuthSystem.Instance.GetLocalPlayerID() : GetLocalPlayerID();
        }
        
        // Show appropriate display
        ShowAppropriateDisplay();
        
        // Display question
        DisplayQuestion();
        
        // Show round-specific visuals
        ShowRoundVisuals();
        
        // Setup submit button (mobile only)
        if (isMobile && answerSubmitButton != null)
        {
            answerSubmitButton.onClick.AddListener(OnSubmitAnswer);
        }
        
        // Setup input field
        if (answerInput != null)
        {
            answerInput.characterLimit = 100; // Reasonable character limit
        }

        // Play appropriate music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayQuestionMusic();
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
                    if (iconImage != null && PlayerManager.Instance != null)
                    {
                        iconImage.sprite = PlayerManager.Instance.GetPlayerIcon(player.iconName);
                    }
                    
                    spawnedPlayerIcons.Add(iconObj);
                }
            }
        }
        
        // Update status indicators (show icon when player has submitted)
        for (int i = 0; i < spawnedPlayerIcons.Count && i < players.Count; i++)
        {
            GameObject iconObj = spawnedPlayerIcons[i];
            PlayerData player = players[i];

            // Check if this player has submitted an answer
            bool hasSubmitted = GameManager.Instance.currentRoundAnswers.ContainsKey(player.playerID);

            // Show/hide entire icon based on submission
            iconObj.SetActive(hasSubmitted);
        }
    }
    
    public void OnSubmitAnswer()
    {
        if (answerInput == null || string.IsNullOrEmpty(answerInput.text))
        {
            Debug.LogWarning("Please enter an answer!");
            ShowAnswerError("Please enter an answer!");
            return;
        }

        // Validate answer using ContentFilterManager
        if (ContentFilterManager.Instance != null)
        {
            // Get existing answers for duplicate checking
            List<string> existingAnswers = GameManager.Instance.GetAllExistingAnswers();

            ValidationResult validation = ContentFilterManager.Instance.ValidateAnswer(answerInput.text, existingAnswers);

            if (!validation.isValid)
            {
                Debug.LogWarning("Answer validation failed: " + validation.errorMessage);
                ShowAnswerError(validation.errorMessage);
                return;
            }

            // Use sanitized answer
            string sanitizedAnswer = validation.sanitizedText;

            // Submit answer via NetworkManager
            if (RWMNetworkManager.Instance != null)
            {
                RWMNetworkManager.Instance.SubmitAnswerServerRpc(playerID, sanitizedAnswer);
            }
            else
            {
                // Fallback for local testing without network
                GameManager.Instance.SubmitPlayerAnswer(playerID, sanitizedAnswer);
            }

            // Disable input and button
            answerInput.interactable = false;
            answerSubmitButton.interactable = false;

            Debug.Log("Answer submitted: " + sanitizedAnswer);
        }
        else
        {
            // Fallback if ContentFilterManager not available
            string answer = answerInput.text.Trim();

            if (RWMNetworkManager.Instance != null)
            {
                RWMNetworkManager.Instance.SubmitAnswerServerRpc(playerID, answer);
            }
            else
            {
                GameManager.Instance.SubmitPlayerAnswer(playerID, answer);
            }

            answerInput.interactable = false;
            answerSubmitButton.interactable = false;

            Debug.Log("Answer submitted: " + answer);
        }
    }

    void ShowAnswerError(string message)
    {
        Debug.LogError("ANSWER ERROR: " + message);

        // Display error message in UI
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);

            // Clear previous coroutine if exists
            if (errorCoroutine != null)
            {
                StopCoroutine(errorCoroutine);
            }

            // Auto-hide after duration
            errorCoroutine = StartCoroutine(HideErrorMessageAfterDelay());
        }

        // Play error sound if AudioManager exists
        if (AudioManager.Instance != null)
        {
            // Could add: AudioManager.Instance.PlayErrorSFX();
        }
    }

    System.Collections.IEnumerator HideErrorMessageAfterDelay()
    {
        yield return new WaitForSeconds(errorDisplayDuration);

        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
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