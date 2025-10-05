using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class VotingScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public TextMeshProUGUI headerText;
    public Transform answerListContainer;
    public TextMeshProUGUI timerCountdown;
    public Image hero;
    public Image desktopBackground;
    public GameObject timerContainer;
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public TextMeshProUGUI mobileHeaderText;
    public Transform voteListMobile;
    public Button votingSubmitButton;
    public Image mobileBackground;
    
    [Header("Answer Button Prefab")]
    public GameObject answerButtonPrefab;
    
    [Header("State")]
    private bool isMobile = false;
    private string playerID = "";
    private string selectedAnswer = "";
    private List<GameObject> spawnedAnswerButtons = new List<GameObject>();
    
    void Start()
    {
        isMobile = Application.isMobilePlatform;
        
        // Get player ID for mobile
        if (isMobile)
        {
            playerID = GetLocalPlayerID();
        }
        
        // Show appropriate display
        ShowAppropriateDisplay();
        
        // Set header text
        if (headerText != null)
        {
            headerText.text = "VOTE FOR THE CORRECT ANSWER";
        }
        
        if (mobileHeaderText != null)
        {
            mobileHeaderText.text = "Vote for Correct Answer";
        }
        
        // Display remaining answers
        DisplayAnswers();
        
        // Setup mobile submit button
        if (votingSubmitButton != null)
        {
            votingSubmitButton.onClick.AddListener(OnSubmitVote);
            votingSubmitButton.interactable = false; // Disabled until selection made
        }
    }
    
    void Update()
    {
        // Update timer display
        UpdateTimerDisplay();
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
    
    void DisplayAnswers()
    {
        List<string> answers;
        Transform container;
        
        if (isMobile)
        {
            // Mobile: Filter out player's own answer (if still remaining)
            answers = GameManager.Instance.GetVotingAnswersForMobile(playerID);
            container = voteListMobile;
        }
        else
        {
            // Desktop: Show all remaining answers
            answers = GameManager.Instance.GetRemainingAnswers();
            container = answerListContainer;
        }
        
        if (container == null)
        {
            Debug.LogError("Answer container is null!");
            return;
        }
        
        // Clear existing buttons
        foreach (GameObject btn in spawnedAnswerButtons)
        {
            Destroy(btn);
        }
        spawnedAnswerButtons.Clear();
        
        // Create button for each answer
        foreach (string answer in answers)
        {
            CreateAnswerButton(answer, container);
        }
    }
    
    void CreateAnswerButton(string answerText, Transform parent)
    {
        GameObject buttonObj;
        
        if (answerButtonPrefab != null)
        {
            // Use prefab if provided
            buttonObj = Instantiate(answerButtonPrefab, parent);
        }
        else
        {
            // Create button programmatically
            buttonObj = new GameObject("AnswerButton");
            buttonObj.transform.SetParent(parent);
            buttonObj.AddComponent<RectTransform>();
            buttonObj.AddComponent<Image>();
            buttonObj.AddComponent<Button>();
            
            // Add text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            textObj.AddComponent<RectTransform>();
            textObj.AddComponent<TextMeshProUGUI>();
        }
        
        // Set button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = answerText;
        }
        
        // Add click listener
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnAnswerSelected(answerText, buttonObj));
        }
        
        spawnedAnswerButtons.Add(buttonObj);
    }
    
    void OnAnswerSelected(string answer, GameObject buttonObj)
    {
        selectedAnswer = answer;
        
        // Visual feedback - highlight selected button
        foreach (GameObject btn in spawnedAnswerButtons)
        {
            Image btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
            {
                // Reset all buttons to default color
                btnImage.color = Color.white;
            }
        }
        
        // Highlight selected button
        Image selectedImage = buttonObj.GetComponent<Image>();
        if (selectedImage != null)
        {
            selectedImage.color = Color.green; // Or your highlight color
        }
        
        if (isMobile)
        {
            // Enable submit button on mobile
            if (votingSubmitButton != null)
            {
                votingSubmitButton.interactable = true;
            }
        }
        else
        {
            // Desktop: Auto-submit on click
            SubmitVotingVote();
        }
        
        Debug.Log("Answer selected for voting: " + answer);
    }
    
    void OnSubmitVote()
    {
        if (string.IsNullOrEmpty(selectedAnswer))
        {
            Debug.LogWarning("No answer selected!");
            return;
        }
        
        SubmitVotingVote();
    }
    
    void SubmitVotingVote()
    {
        GameManager.Instance.SubmitVotingVote(playerID, selectedAnswer);
        
        // Disable all buttons
        foreach (GameObject btn in spawnedAnswerButtons)
        {
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }
        
        // Disable submit button
        if (votingSubmitButton != null)
        {
            votingSubmitButton.interactable = false;
        }
        
        Debug.Log("Voting vote submitted: " + selectedAnswer);
    }
    
    void UpdateTimerDisplay()
    {
        if (timerCountdown != null)
        {
            timerCountdown.text = GameManager.Instance.GetTimerDisplay();
        }
    }
    
    string GetLocalPlayerID()
    {
        return "player_" + SystemInfo.deviceUniqueIdentifier;
    }
    
    void OnDestroy()
    {
        if (votingSubmitButton != null)
        {
            votingSubmitButton.onClick.RemoveListener(OnSubmitVote);
        }
        
        // Clean up button listeners
        foreach (GameObject btn in spawnedAnswerButtons)
        {
            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}