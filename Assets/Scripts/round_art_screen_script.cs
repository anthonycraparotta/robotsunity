using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoundArtScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject desktopDisplay;
    public GameObject mobileDisplay;
    public Transform backgroundContainer;
    public Button continueButton;
    public Image mobileBackground;
    
    [Header("Round Backgrounds (Round1Intro - Round12Intro)")]
    public Image[] roundBackgrounds = new Image[12];
    
    [Header("Settings")]
    public float autoAdvanceDelay = 3f; // Auto-advance after 3 seconds
    
    void Start()
    {
        // Show appropriate display
        ShowAppropriateDisplay();
        
        // Display the correct round background
        ShowRoundBackground();
        
        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        // Auto-advance after delay
        StartCoroutine(AutoAdvanceAfterDelay());
    }
    
    void ShowAppropriateDisplay()
    {
        bool isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        if (desktopDisplay != null)
        {
            desktopDisplay.SetActive(!isMobile);
        }

        if (mobileDisplay != null)
        {
            mobileDisplay.SetActive(isMobile);
        }
    }
    
    void ShowRoundBackground()
    {
        int currentRound = GameManager.Instance.GetCurrentRound();
        
        // Hide all backgrounds
        for (int i = 0; i < roundBackgrounds.Length; i++)
        {
            if (roundBackgrounds[i] != null)
            {
                roundBackgrounds[i].gameObject.SetActive(false);
            }
        }
        
        // Show the current round's background (array is 0-indexed, rounds are 1-indexed)
        if (currentRound > 0 && currentRound <= roundBackgrounds.Length)
        {
            if (roundBackgrounds[currentRound - 1] != null)
            {
                roundBackgrounds[currentRound - 1].gameObject.SetActive(true);
            }
        }
        
        Debug.Log("Showing Round " + currentRound + " art");
    }
    
    IEnumerator AutoAdvanceAfterDelay()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        AdvanceToQuestion();
    }
    
    void OnContinueClicked()
    {
        // Stop auto-advance coroutine
        StopAllCoroutines();
        AdvanceToQuestion();
    }
    
    void AdvanceToQuestion()
    {
        GameManager.Instance.AdvanceToNextScreen();
    }
    
    void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}