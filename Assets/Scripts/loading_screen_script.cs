using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

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
        // Initialize each manager as a root GameObject (they handle DontDestroyOnLoad themselves)
        EnsureManagerExists<GameManager>("GameManager");
        EnsureManagerExists<QuestionLoader>("QuestionLoader");
        EnsureManagerExists<PictureQuestionLoader>("PictureQuestionLoader");
        EnsureManagerExists<AudioManager>("AudioManager");
        EnsureManagerExists<PlayerManager>("PlayerManager");
        EnsureManagerExists<SceneTransitionManager>("SceneTransitionManager");
        EnsureManagerExists<DeviceDetector>("DeviceDetector");
        EnsureManagerExists<ContentFilterManager>("ContentFilterManager");

        // Create Unity Netcode NetworkManager first (required for other NetworkBehaviours)
        EnsureNetworkManager();

        // Create custom network managers with NetworkObject components
        EnsureNetworkBehaviourExists<RWMNetworkManager>("NetworkManager");
        EnsureNetworkBehaviourExists<PlayerAuthSystem>("PlayerAuthSystem");

        EnsureManagerExists<DebugManager>("DebugManager");

        Debug.Log("All global managers initialized");
    }

    T EnsureManagerExists<T>(string managerName) where T : MonoBehaviour
    {
        T existing = FindExistingComponent<T>();
        if (existing != null)
        {
            Debug.Log($"Found existing {typeof(T).Name} on {existing.gameObject.name}");
            return existing;
        }

        GameObject managerObj = new GameObject(managerName);
        T created = managerObj.AddComponent<T>();
        Debug.Log($"Created {managerName}");
        return created;
    }

    void EnsureNetworkManager()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Found existing Unity Netcode NetworkManager singleton");
            return;
        }

        NetworkManager existing = FindExistingComponent<NetworkManager>();
        if (existing != null)
        {
            if (!existing.gameObject.activeSelf)
            {
                existing.gameObject.SetActive(true);
            }

            Debug.Log("Using scene-configured NetworkManager");
            return;
        }

        GameObject netManagerObj = new GameObject("Unity_NetworkManager");
        netManagerObj.AddComponent<NetworkManager>();

        // Configure with basic transport (UnityTransport is the default)
        var transport = netManagerObj.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        if (transport == null)
        {
            transport = netManagerObj.AddComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        }

        Debug.Log("Created Unity NetworkManager singleton");
    }

    void EnsureNetworkBehaviourExists<T>(string managerName) where T : NetworkBehaviour
    {
        T existing = FindExistingComponent<T>();
        if (existing != null)
        {
            if (!existing.gameObject.activeSelf)
            {
                existing.gameObject.SetActive(true);
            }

            NetworkObject existingNetworkObject = existing.GetComponent<NetworkObject>();
            if (existingNetworkObject == null)
            {
                existingNetworkObject = existing.gameObject.AddComponent<NetworkObject>();
                Debug.LogWarning($"Added missing NetworkObject to existing {typeof(T).Name}");
            }

            Debug.Log($"Found existing {typeof(T).Name} on {existing.gameObject.name}");
            return;
        }

        // NetworkBehaviours MUST have a NetworkObject component to function
        GameObject managerObj = new GameObject(managerName);
        managerObj.AddComponent<NetworkObject>();
        managerObj.AddComponent<T>();
        Debug.Log($"Created {managerName} with NetworkObject");
    }

    static U FindExistingComponent<U>() where U : Component
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindAnyObjectByType<U>(FindObjectsInactive.Include);
#else
        U[] existing = Object.FindObjectsOfType<U>(true);
        return existing.Length > 0 ? existing[0] : null;
#endif
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