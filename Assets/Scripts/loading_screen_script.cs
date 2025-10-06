using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
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
    
    IEnumerator LoadGame()
    {
        // Initialize all global managers
        InitializeGlobalManagers();

        // Simulate loading or perform actual loading tasks
        yield return new WaitForSeconds(0.5f);

        // Load any necessary assets
        // yield return StartCoroutine(LoadAssets());

        isLoaded = true;

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

    void InitializeGlobalManagers()
    {
        // Create a DontDestroyOnLoad parent object for all managers
        GameObject managersParent = new GameObject("_GlobalManagers");
        DontDestroyOnLoad(managersParent);

        // Initialize each manager as a child of the parent
        CreateManager<GameManager>("GameManager", managersParent.transform);
        CreateManager<AudioManager>("AudioManager", managersParent.transform);
        CreateManager<PlayerManager>("PlayerManager", managersParent.transform);
        CreateManager<SceneTransitionManager>("SceneTransitionManager", managersParent.transform);
        CreateManager<DeviceDetector>("DeviceDetector", managersParent.transform);
        CreateManager<ContentFilterManager>("ContentFilterManager", managersParent.transform);
        CreateManager<RWMNetworkManager>("NetworkManager", managersParent.transform);
        CreateManager<PlayerAuthSystem>("PlayerAuthSystem", managersParent.transform);
        CreateManager<DebugManager>("DebugManager", managersParent.transform);

        Debug.Log("All global managers initialized");
    }

    void CreateManager<T>(string managerName, Transform parent) where T : MonoBehaviour
    {
        GameObject managerObj = new GameObject(managerName);
        managerObj.transform.SetParent(parent);
        managerObj.AddComponent<T>();
        Debug.Log($"Created {managerName}");
    }
    
    void AdvanceToLanding()
    {
        Debug.Log("Loading complete - advancing to Landing Screen");
        GameManager.Instance.AdvanceToNextScreen();
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