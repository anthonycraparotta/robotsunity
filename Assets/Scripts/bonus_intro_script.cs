using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BonusIntroScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public Image[] introImages; // Multiple Image components for intro art
    public Button continueButton;
    public GameObject buttonSlideout;
    public Image desktopBackground;
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public Image mobileBackground;
    
    [Header("Settings")]
    public float autoAdvanceDelay = 4f; // Auto-advance after 4 seconds
    
    void Start()
    {
        // Show appropriate display
        ShowAppropriateDisplay();
        
        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        // Auto-advance to bonus questions
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
    
    IEnumerator AutoAdvanceAfterDelay()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        AdvanceToBonusQuestions();
    }
    
    void OnContinueClicked()
    {
        // Stop auto-advance
        StopAllCoroutines();
        AdvanceToBonusQuestions();
    }
    
    void AdvanceToBonusQuestions()
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