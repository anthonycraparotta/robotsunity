using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CreditsScreen : MonoBehaviour
{
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public VideoPlayer creditsVideo;
    public RawImage creditsVideoDisplay;
    public Transform creditsScrollZone;
    public Button websiteButton;
    public Button newGameButton;
    public GameObject buttonSlideout;
    public Image desktopBackground;
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public RawImage mobileVideoDisplay;
    public Image mobileBackground;
    
    [Header("Settings")]
    public float scrollSpeed = 20f;
    public bool autoScroll = false;
    
    void Start()
    {
        // Show appropriate display
        ShowAppropriateDisplay();
        
        // Setup video player
        if (creditsVideo != null)
        {
            creditsVideo.prepareCompleted += OnVideoPrepared;
            creditsVideo.loopPointReached += OnVideoFinished;
            creditsVideo.Prepare();
        }
        
        // Setup buttons
        if (websiteButton != null)
        {
            websiteButton.onClick.AddListener(OnWebsiteClicked);
        }
        
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }
    }
    
    void Update()
    {
        // Auto-scroll credits if enabled
        if (autoScroll && creditsScrollZone != null)
        {
            RectTransform scrollRect = creditsScrollZone.GetComponent<RectTransform>();
            if (scrollRect != null)
            {
                Vector2 pos = scrollRect.anchoredPosition;
                pos.y += scrollSpeed * Time.deltaTime;
                scrollRect.anchoredPosition = pos;
            }
        }
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
    
    void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("Credits video prepared");
        
        // Auto-play when ready
        if (creditsVideo != null)
        {
            creditsVideo.Play();
        }
    }
    
    void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log("Credits video finished");
        
        // Loop or advance
        // For now, just loop the video or stop
    }
    
    void OnWebsiteClicked()
    {
        Debug.Log("Website button clicked");
        Application.OpenURL("https://robotswearingmoustaches.com"); // Replace with actual URL
    }
    
    void OnNewGameClicked()
    {
        Debug.Log("New Game button clicked");
        
        // Reset game and return to landing
        GameManager.Instance.currentRound = 0;
        GameManager.Instance.isHalftimePlayed = false;
        GameManager.Instance.isBonusRoundPlayed = false;
        
        // Clear all players
        GameManager.Instance.players.Clear();
        
        GameManager.Instance.AdvanceToNextScreen();
    }
    
    void OnDestroy()
    {
        // Clean up listeners
        if (creditsVideo != null)
        {
            creditsVideo.prepareCompleted -= OnVideoPrepared;
            creditsVideo.loopPointReached -= OnVideoFinished;
        }
        
        if (websiteButton != null)
        {
            websiteButton.onClick.RemoveListener(OnWebsiteClicked);
        }
        
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveListener(OnNewGameClicked);
        }
    }
}