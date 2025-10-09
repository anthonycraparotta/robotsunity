using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    // === DEBUG FLAG - SET TO FALSE TO REMOVE ALL DEBUG LOGS ===
    private const bool ENABLE_DEBUG_LOGS = true;
    [Header("Desktop UI Elements")]
    public GameObject desktopDisplay;
    public Image gameLogo;
    public Image background;
    public Image tips;
    
    [Header("Mobile UI Elements")]
    public GameObject mobileDisplay;
    public Image mobileBackground;
    
    [Header("Settings")]
    public float minimumLoadTime = 2f; // Show loading screen for at least 2 seconds
    public bool clickToAdvance = true;
    
    private bool isLoaded = false;
    private float loadTimer = 0f;
    
    void Start()
    {
        if (ENABLE_DEBUG_LOGS)
        {
            Debug.Log("[LoadingScreen] Start called");
            Debug.Log($"[LoadingScreen] Screen size: {Screen.width}x{Screen.height}");
        }

        // Show appropriate display
        ShowAppropriateDisplay();

        // Start loading process
        StartCoroutine(LoadGame());
    }
    
    void Update()
    {
        loadTimer += Time.deltaTime;
        
        // Allow clicking logo to advance (if enabled and loaded)
        if (clickToAdvance && isLoaded && loadTimer >= minimumLoadTime)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                AdvanceToLanding();
            }
        }
    }
    
    void ShowAppropriateDisplay()
    {
        if (ENABLE_DEBUG_LOGS)
            Debug.Log($"[LoadingScreen] ShowAppropriateDisplay called");

        bool isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        if (ENABLE_DEBUG_LOGS)
        {
            Debug.Log($"[LoadingScreen] DeviceDetector.Instance exists: {DeviceDetector.Instance != null}");
            if (DeviceDetector.Instance != null)
            {
                Debug.Log($"[LoadingScreen] isMobile: {isMobile}");
            }
        }

        if (desktopDisplay != null)
        {
            desktopDisplay.SetActive(!isMobile);
            if (ENABLE_DEBUG_LOGS)
                Debug.Log($"[LoadingScreen] desktopDisplay set to: {!isMobile}");
        }

        if (mobileDisplay != null)
        {
            mobileDisplay.SetActive(isMobile);
            if (ENABLE_DEBUG_LOGS)
                Debug.Log($"[LoadingScreen] mobileDisplay set to: {isMobile}");
        }
    }

    IEnumerator LoadGame()
    {
        if (ENABLE_DEBUG_LOGS)
            Debug.Log("[LoadingScreen] LoadGame coroutine started - calling CoreSystemsBootstrapper.EnsureInitialized()");

        // Initialize all global managers
        CoreSystemsBootstrapper.EnsureInitialized();

        if (ENABLE_DEBUG_LOGS)
            Debug.Log("[LoadingScreen] CoreSystemsBootstrapper.EnsureInitialized() completed");

        // Simulate loading or perform actual loading tasks
        yield return new WaitForSeconds(0.5f);

        // Load any necessary assets
        // yield return StartCoroutine(LoadAssets());

        isLoaded = true;

        if (ENABLE_DEBUG_LOGS)
            Debug.Log("[LoadingScreen] Loading complete, waiting for minimum load time");

        // Wait for minimum load time
        while (loadTimer < minimumLoadTime)
        {
            yield return null;
        }

        // Auto-advance if click-to-advance is disabled
        if (!clickToAdvance)
        {
            AdvanceToLanding();
        }
    }
    
    void AdvanceToLanding()
    {
        if (ENABLE_DEBUG_LOGS)
            Debug.Log("[LoadingScreen] Advancing to IntroVideoScreen");

        UnityEngine.SceneManagement.SceneManager.LoadScene("IntroVideoScreen");
        GameManager.Instance.currentGameState = GameManager.GameState.IntroVideo;
    }
    
    // Optional: Add actual asset loading here
    IEnumerator LoadAssets()
    {
        // Example: Load questions from JSON
        // TextAsset questionsJson = Resources.Load<TextAsset>("questions");
        // if (questionsJson != null)
        // {
        //     // Parse JSON and populate GameManager
        // }
        
        yield return null;
    }
}