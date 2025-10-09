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
    public GameObject mobileQuestionContainer;
    public TextMeshProUGUI mobileQuestionText;
    public TMP_InputField answerInput;
    public Button answerSubmitButton;
    public Image mobileBackground;
    public TextMeshProUGUI submissionConfirmationText; // Optional: Shows "Answer Submitted!" message

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

        // Update player status indicators (desktop only - mobile gets updates via ClientRpc)
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

    // Called by network ClientRpc or locally to update buzz-in feedback
    public void OnPlayerSubmittedAnswer(string playerID)
    {
        if (isMobile)
        {
            // Mobile: Show visual feedback that answer was submitted
            UpdateMobileBuzzInFeedback(playerID);
        }
        else
        {
            // Desktop: Update will be handled by UpdatePlayerStatusIndicators in Update loop
            // No action needed here - desktop polls the state
        }
    }

    void UpdateMobileBuzzInFeedback(string playerID)
    {
        // If this is the local player, show confirmation
        if (playerID == this.playerID)
        {
            // Disable input and button to show submission is complete
            if (answerInput != null)
            {
                answerInput.interactable = false;
            }

            if (answerSubmitButton != null)
            {
                answerSubmitButton.interactable = false;
            }

            // Show confirmation message if available
            if (submissionConfirmationText != null)
            {
                submissionConfirmationText.text = "Answer Submitted!";
                submissionConfirmationText.gameObject.SetActive(true);
            }

            // Play confirmation sound if available
            if (AudioManager.Instance != null)
            {
                // Could add: AudioManager.Instance.PlaySubmitSFX();
            }

            Debug.Log("Your answer has been submitted!");
        }

        // Future: Could show other players' buzz-in status on mobile if desired
        // For example, display a list of player icons who have buzzed in
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

        Debug.Log($"DisplayQuestion - currentQuestion is null: {currentQuestion == null}");
        Debug.Log($"DisplayQuestion - questionText is null: {questionText == null}");

        if (currentQuestion != null)
        {
            Debug.Log($"DisplayQuestion - questionText content: '{currentQuestion.questionText}'");

            string displayText = currentQuestion.questionText;

            // Replace [player.name] placeholder with random player name for Player Questions
            if (GameManager.Instance.IsPlayerQuestion() && displayText.Contains("[player.name]"))
            {
                List<PlayerData> allPlayers = GameManager.Instance.GetAllPlayers();
                if (allPlayers.Count > 0)
                {
                    // Pick a random player
                    PlayerData randomPlayer = allPlayers[Random.Range(0, allPlayers.Count)];
                    displayText = displayText.Replace("[player.name]", randomPlayer.playerName);
                    Debug.Log($"Replaced [player.name] with: {randomPlayer.playerName}");
                }
            }

            // Set desktop question text
            if (questionText != null)
            {
                questionText.text = displayText;
                Debug.Log($"DisplayQuestion - Set questionText to: '{questionText.text}'");
            }

            // Set mobile question text
            if (mobileQuestionText != null)
            {
                mobileQuestionText.text = displayText;
                Debug.Log($"DisplayQuestion - Set mobileQuestionText to: '{mobileQuestionText.text}'");
            }
        }
        else
        {
            Debug.LogError("DisplayQuestion - currentQuestion is NULL!");
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

        // Animate question card sliding down (Desktop only)
        if (!isMobile && questionCard != null)
        {
            StartCoroutine(SlideDownQuestionCard(questionCard.GetComponent<RectTransform>()));
        }
    }

    System.Collections.IEnumerator SlideDownQuestionCard(RectTransform rectTransform)
    {
        if (rectTransform == null) yield break;

        float duration = 0.8f;
        float elapsed = 0f;

        // Store original position
        Vector2 targetPosition = rectTransform.anchoredPosition;

        // Start position (off screen above)
        Vector2 startPosition = new Vector2(targetPosition.x, targetPosition.y + Screen.height);
        rectTransform.anchoredPosition = startPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease out cubic
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
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
        
        // Show current round robot with slide-in animation
        if (currentRound > 0 && currentRound <= robotImages.Length)
        {
            if (robotImages[currentRound - 1] != null)
            {
                GameObject robotObj = robotImages[currentRound - 1].gameObject;
                robotObj.SetActive(true);

                // Start slide-in animation from left
                StartCoroutine(SlideInFromLeft(robotObj.GetComponent<RectTransform>()));
            }
        }
    }

    System.Collections.IEnumerator SlideInFromLeft(RectTransform rectTransform)
    {
        if (rectTransform == null) yield break;

        float duration = 1f;
        float elapsed = 0f;

        // Store original position
        Vector2 targetPosition = rectTransform.anchoredPosition;

        // Start position (off screen to the left)
        Vector2 startPosition = new Vector2(targetPosition.x - Screen.width, targetPosition.y);
        rectTransform.anchoredPosition = startPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease out cubic
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
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
                    Transform nameTransform = FindPlayerNameTransform(iconObj);
                    if (nameTransform != null)
                    {
                        TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                        if (nameText != null)
                        {
                            nameText.text = player.playerName;
                        }
                    }

                    // Set player icon
                    Transform iconTransform = FindPlayerIconTransform(iconObj);
                    if (iconTransform != null && PlayerManager.Instance != null)
                    {
                        Image iconImage = iconTransform.GetComponent<Image>();
                        if (iconImage != null)
                        {
                            iconImage.sprite = PlayerManager.Instance.GetPlayerIcon(player.iconName);
                        }
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

            // Check previous state
            bool wasActive = iconObj.activeSelf;

            // Show/hide entire icon based on submission
            iconObj.SetActive(hasSubmitted);

            // Animate bounce when player just buzzed in
            if (hasSubmitted && !wasActive)
            {
                StartCoroutine(BounceScaleUp(iconObj.transform));
            }
        }
    }

    System.Collections.IEnumerator BounceScaleUp(Transform transform)
    {
        if (transform == null) yield break;

        float duration = 0.6f;
        float elapsed = 0f;

        // Start from scale 0
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        Vector3 overshootScale = targetScale * 1.2f; // Overshoot by 20%

        transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Elastic ease out with bounce
            float easedT;
            if (t < 0.5f)
            {
                // First half: scale up to overshoot
                easedT = 1f - Mathf.Pow(1f - (t * 2f), 3f);
                transform.localScale = Vector3.Lerp(startScale, overshootScale, easedT);
            }
            else
            {
                // Second half: bounce back to target
                easedT = 1f - Mathf.Pow(1f - ((t - 0.5f) * 2f), 2f);
                transform.localScale = Vector3.Lerp(overshootScale, targetScale, easedT);
            }

            yield return null;
        }

        transform.localScale = targetScale;
    }
    
    public void OnSubmitAnswer()
    {
        MobileHaptics.MediumImpact();

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
        MobileHaptics.Failure();

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

    Transform FindPlayerIconTransform(GameObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        // Direct child search
        Transform icon = obj.transform.Find("PlayerIcon");
        if (icon != null)
        {
            return icon;
        }

        // Could add additional fallback logic here if needed
        return null;
    }

    Transform FindPlayerNameTransform(GameObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        // Direct child search
        Transform nameTransform = obj.transform.Find("PlayerName");
        if (nameTransform != null)
        {
            return nameTransform;
        }

        // Could add additional fallback logic here if needed
        return null;
    }

    void OnDestroy()
    {
        if (answerSubmitButton != null)
        {
            answerSubmitButton.onClick.RemoveListener(OnSubmitAnswer);
        }
    }
}