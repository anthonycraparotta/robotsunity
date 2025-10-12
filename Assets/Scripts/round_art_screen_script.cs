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
        if (roundBackgrounds != null)
        {
            for (int i = 0; i < roundBackgrounds.Length; i++)
            {
                if (roundBackgrounds[i] != null)
                {
                    roundBackgrounds[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning("roundBackgrounds array is not configured on RoundArtScreen");
        }

        // Hide all mobile backgrounds
        if (mobileRoundBackgrounds != null)
        {
            for (int i = 0; i < mobileRoundBackgrounds.Length; i++)
            {
                if (mobileRoundBackgrounds[i] != null)
                {
                    mobileRoundBackgrounds[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning("mobileRoundBackgrounds array is not configured on RoundArtScreen");
        }

        if (currentRound <= 0)
        {
            Debug.LogWarning("currentRound is out of range: " + currentRound);
            return;
        }

        int roundIndex = currentRound - 1;
        Image[] activeRoundBackgrounds = isMobile ? mobileRoundBackgrounds : roundBackgrounds;

        if (activeRoundBackgrounds == null || roundIndex >= activeRoundBackgrounds.Length)
        {
            Debug.LogWarning("No " + (isMobile ? "mobile" : "desktop") + " background configured for round " + currentRound +
                             ". Available backgrounds: " + (activeRoundBackgrounds == null ? 0 : activeRoundBackgrounds.Length));
            return;
        }

        Image targetBackground = activeRoundBackgrounds[roundIndex];

        if (targetBackground != null)
        {
            if (isMobile)
            {
                Debug.Log("Activating mobileRoundBackgrounds[" + roundIndex + "]");
            }
            else
            {
                Debug.Log("Activating roundBackgrounds[" + roundIndex + "]");
            }

            targetBackground.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError((isMobile ? "mobileRoundBackgrounds" : "roundBackgrounds") + "[" + roundIndex + "] is NULL!");
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
        MobileHaptics.MediumImpact();

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