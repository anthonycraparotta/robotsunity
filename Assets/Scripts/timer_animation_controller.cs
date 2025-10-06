using UnityEngine;
using UnityEngine.UI;

public class TimerAnimationController : MonoBehaviour
{
    [Header("Timer Components")]
    public Animator timerAnimator;
    public Image timerCircle;
    public Image timerBlob;
    
    [Header("Timer Sprite Sets")]
    public string questionTimerPath = "sprites/timer/question/";
    public string votingTimerPath = "sprites/timer/voting/";
    
    [Header("Animation Settings")]
    public float warningThreshold = 10f; // Start warning animation at 10 seconds
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    
    [Header("Timer Type")]
    public TimerType timerType = TimerType.Question;
    
    public enum TimerType
    {
        Question,    // 60 seconds - Question/PlayerQuestion/PictureQuestion
        Voting       // 30 seconds - Elimination/Voting/Bonus
    }
    
    private bool isWarning = false;
    
    void Start()
    {
        LoadTimerSprites();
        
        if (timerAnimator == null)
        {
            timerAnimator = GetComponent<Animator>();
        }
    }
    
    void LoadTimerSprites()
    {
        string basePath = (timerType == TimerType.Question) ? questionTimerPath : votingTimerPath;
        
        // Load timer circle sprite
        Sprite circleSprite = Resources.Load<Sprite>(basePath + "timer_circle");
        if (circleSprite != null && timerCircle != null)
        {
            timerCircle.sprite = circleSprite;
        }
        
        // Load timer blob sprite
        Sprite blobSprite = Resources.Load<Sprite>(basePath + "timer_blob");
        if (blobSprite != null && timerBlob != null)
        {
            timerBlob.sprite = blobSprite;
        }
    }
    
    void Update()
    {
        float timeRemaining = GameManager.Instance.GetTimeRemaining();
        
        UpdateTimerVisuals(timeRemaining);
        UpdateWarningState(timeRemaining);
    }
    
    void UpdateTimerVisuals(float timeRemaining)
    {
        // Rotate or scale timer based on remaining time
        if (timerBlob != null)
        {
            // Rotate the blob
            float rotationSpeed = Mathf.Lerp(50f, 200f, 1f - (timeRemaining / 60f));
            timerBlob.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        
        // Update fill amount based on time (if using filled circle)
        if (timerCircle != null && timerCircle.type == Image.Type.Filled)
        {
            float maxTime = (timerType == TimerType.Question) ? 60f : 30f;
            timerCircle.fillAmount = timeRemaining / maxTime;
        }
    }
    
    void UpdateWarningState(float timeRemaining)
    {
        bool shouldWarn = timeRemaining <= warningThreshold && timeRemaining > 0;
        
        if (shouldWarn && !isWarning)
        {
            // Start warning
            isWarning = true;
            StartWarningAnimation();
        }
        else if (!shouldWarn && isWarning)
        {
            // Stop warning
            isWarning = false;
            StopWarningAnimation();
        }
    }
    
    void StartWarningAnimation()
    {
        // Trigger warning animation
        if (timerAnimator != null)
        {
            timerAnimator.SetBool("Warning", true);
        }
        
        // Change color to warning
        if (timerCircle != null)
        {
            timerCircle.color = warningColor;
        }
        
        if (timerBlob != null)
        {
            timerBlob.color = warningColor;
        }
        
        Debug.Log("Timer warning started");
    }
    
    void StopWarningAnimation()
    {
        // Stop warning animation
        if (timerAnimator != null)
        {
            timerAnimator.SetBool("Warning", false);
        }
        
        // Reset to normal color
        if (timerCircle != null)
        {
            timerCircle.color = normalColor;
        }
        
        if (timerBlob != null)
        {
            timerBlob.color = normalColor;
        }
    }
    
    public void ResetTimer()
    {
        isWarning = false;
        StopWarningAnimation();
        
        if (timerBlob != null)
        {
            timerBlob.transform.rotation = Quaternion.identity;
        }
    }
}
