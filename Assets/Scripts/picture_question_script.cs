using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PictureQuestionScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public GameObject pictureContainer;
    public Image picture;
    public Image pictureFrame;
    public TextMeshProUGUI timerCountdown;
    public Transform playerIconContainer;
    public GameObject playerIconPictureQuestionPrefab;
    public GameObject timerContainer;
    public TextMeshProUGUI tipText;
    public Image desktopBackground;
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public Image mobilePictureFrame;
    public GameObject mobilePictureContainer;
    public Image mobilePicture;
    public TMP_InputField answerInput;
    public Button answerSubmitButton;
    public Image mobileBackground;
    
    [Header("State")]
    private bool isMobile = false;
    private string playerID = "";
    private List<GameObject> spawnedPlayerIcons = new List<GameObject>();
    
    void Start()
    {
        isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        // Get player ID for mobile
        if (isMobile)
        {
            playerID = GetLocalPlayerID();
        }

        // Show appropriate display
        ShowAppropriateDisplay();
        
        // Display picture question
        DisplayPictureQuestion();
        
        // Setup submit button
        if (answerSubmitButton != null)
        {
            answerSubmitButton.onClick.AddListener(OnSubmitAnswer);
        }
        
        // Setup input field
        if (answerInput != null)
        {
            answerInput.characterLimit = 100;
        }
        
        // Update tip text
        if (tipText != null)
        {
            tipText.text = "DOUBLE POINTS! Describe what you see";
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
    
    void DisplayPictureQuestion()
    {
        Question currentQuestion = GameManager.Instance.GetCurrentQuestion();
        
        if (currentQuestion != null)
        {
            // Load picture from URL or Resources
            if (!string.IsNullOrEmpty(currentQuestion.imageURL))
            {
                LoadPicture(currentQuestion.imageURL);
            }
        }
        
        Debug.Log("Picture Question - Round " + GameManager.Instance.GetCurrentRound());
    }
    
    void LoadPicture(string imageURL)
    {
        // Load image from Resources or download from URL
        // For now, placeholder for loading logic
        
        // Example: Load from Resources
        // Sprite pictureSprite = Resources.Load<Sprite>("QuestionImages/" + imageURL);
        
        // Set desktop picture
        if (picture != null)
        {
            // picture.sprite = pictureSprite;
        }
        
        // Set mobile picture
        if (mobilePicture != null)
        {
            // mobilePicture.sprite = pictureSprite;
        }
        
        Debug.Log("Loading picture: " + imageURL);
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
                if (playerIconPictureQuestionPrefab != null)
                {
                    GameObject iconObj = Instantiate(playerIconPictureQuestionPrefab, playerIconContainer);
                    
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
        
        // Update status indicators (show icon when player has submitted)
        for (int i = 0; i < spawnedPlayerIcons.Count && i < players.Count; i++)
        {
            GameObject iconObj = spawnedPlayerIcons[i];
            PlayerData player = players[i];

            // Check if this player has submitted an answer
            bool hasSubmitted = GameManager.Instance.currentRoundAnswers.ContainsKey(player.playerID);

            // Show/hide entire icon based on submission (matching regular Questions behavior)
            iconObj.SetActive(hasSubmitted);
        }
    }
    
    void OnSubmitAnswer()
    {
        MobileHaptics.MediumImpact();

        if (answerInput == null || string.IsNullOrEmpty(answerInput.text))
        {
            Debug.LogWarning("Please enter an answer!");
            MobileHaptics.Failure();
            return;
        }
        
        // Submit answer to GameManager
        GameManager.Instance.SubmitPlayerAnswer(playerID, answerInput.text);
        
        // Disable input and button
        answerInput.interactable = false;
        answerSubmitButton.interactable = false;
        
        Debug.Log("Picture answer submitted: " + answerInput.text);
    }
    
    string GetLocalPlayerID()
    {
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