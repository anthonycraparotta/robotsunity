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

    [Header("Desktop Round Backgrounds (Round1Intro - Round12Intro)")]
    public Image[] roundBackgrounds = new Image[12];

    [Header("Mobile Round Backgrounds (MobileRound1Intro - MobileRound12Intro)")]
    public Image[] mobileRoundBackgrounds = new Image[12];
    
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
        Debug.Log("ShowRoundBackground - Direct access to GameManager.Instance.currentRound: " + GameManager.Instance.currentRound);

        int currentRound = GameManager.Instance.GetCurrentRound();
        bool isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        Debug.Log("ShowRoundBackground called - currentRound from GetCurrentRound(): " + currentRound);
        Debug.Log("GameManager instance ID in RoundArtScreen: " + GameManager.Instance.GetInstanceID());
        Debug.Log("roundBackgrounds array length: " + roundBackgrounds.Length);

        // Hide all desktop backgrounds
        for (int i = 0; i < roundBackgrounds.Length; i++)
        {
            if (roundBackgrounds[i] != null)
            {
                roundBackgrounds[i].gameObject.SetActive(false);
            }
        }

        // Hide all mobile backgrounds
        for (int i = 0; i < mobileRoundBackgrounds.Length; i++)
        {
            if (mobileRoundBackgrounds[i] != null)
            {
                mobileRoundBackgrounds[i].gameObject.SetActive(false);
            }
        }

        // Show the current round's background (array is 0-indexed, rounds are 1-indexed)
        if (currentRound > 0 && currentRound <= roundBackgrounds.Length)
        {
            if (isMobile)
            {
                // Show mobile background
                if (mobileRoundBackgrounds[currentRound - 1] != null)
                {
                    Debug.Log("Activating mobileRoundBackgrounds[" + (currentRound - 1) + "]");
                    mobileRoundBackgrounds[currentRound - 1].gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("mobileRoundBackgrounds[" + (currentRound - 1) + "] is NULL!");
                }
            }
            else
            {
                // Show desktop background
                if (roundBackgrounds[currentRound - 1] != null)
                {
                    Debug.Log("Activating roundBackgrounds[" + (currentRound - 1) + "]");
                    roundBackgrounds[currentRound - 1].gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("roundBackgrounds[" + (currentRound - 1) + "] is NULL!");
                }
            }
        }
        else
        {
            Debug.LogWarning("currentRound is out of range: " + currentRound);
        }

        Debug.Log("Showing Round " + currentRound + " art");
    }
    
    IEnumerator AutoAdvanceAfterDelay()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        AdvanceToQuestion();
    }
    
    public void OnContinueClicked()
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