using UnityEngine;
using UnityEngine.Events;

public class DeviceDetector : MonoBehaviour
{
    public static DeviceDetector Instance;
    
    [Header("Device Type")]
    public DeviceType currentDevice = DeviceType.Desktop;
    
    [Header("Device Detection Settings")]
    public bool autoDetectOnStart = true;
    public bool forceDeviceType = false;
    public DeviceType forcedDeviceType = DeviceType.Desktop;
    
    [Header("Events")]
    public UnityEvent onDesktopDetected;
    public UnityEvent onMobileDetected;
    
    public enum DeviceType
    {
        Desktop,
        Mobile
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (autoDetectOnStart)
        {
            DetectDevice();
        }
    }
    
    // === DEVICE DETECTION ===
    
    public void DetectDevice()
    {
        if (forceDeviceType)
        {
            currentDevice = forcedDeviceType;
            Debug.Log("Device type forced to: " + currentDevice);
        }
        else
        {
            // Use Unity's built-in platform detection
            currentDevice = DetermineDeviceType();
            Debug.Log("Device detected as: " + currentDevice);
        }
        
        // Trigger events
        if (currentDevice == DeviceType.Desktop)
        {
            onDesktopDetected?.Invoke();
        }
        else
        {
            onMobileDetected?.Invoke();
        }
    }
    
    DeviceType DetermineDeviceType()
    {
        // Check if running on mobile platform
        if (Application.isMobilePlatform)
        {
            return DeviceType.Mobile;
        }
        
        // Check specific platforms
        RuntimePlatform platform = Application.platform;
        
        switch (platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                return DeviceType.Mobile;
                
            case RuntimePlatform.WebGLPlayer:
                // For WebGL, check screen size and touch support
                return DetectWebGLDevice();
                
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.LinuxEditor:
            default:
                return DeviceType.Desktop;
        }
    }
    
    DeviceType DetectWebGLDevice()
    {
        // For WebGL builds, use additional heuristics
        
        // Check for touch support
        if (Input.touchSupported)
        {
            return DeviceType.Mobile;
        }
        
        // Check screen size (mobile devices typically < 768px width)
        if (Screen.width < 768)
        {
            return DeviceType.Mobile;
        }
        
        // Check aspect ratio (mobile devices typically have portrait or narrow aspect)
        float aspectRatio = (float)Screen.width / Screen.height;
        if (aspectRatio < 1.0f || (aspectRatio < 1.5f && Screen.width < 1024))
        {
            return DeviceType.Mobile;
        }
        
        // Default to desktop for WebGL
        return DeviceType.Desktop;
    }
    
    // === PUBLIC QUERY METHODS ===
    
    public bool IsMobile()
    {
        return currentDevice == DeviceType.Mobile;
    }
    
    public bool IsDesktop()
    {
        return currentDevice == DeviceType.Desktop;
    }
    
    public string GetDeviceTypeString()
    {
        return currentDevice.ToString().ToLower();
    }
    
    // === INPUT DETECTION ===
    
    public bool IsUsingTouch()
    {
        // Check if user is actively using touch input
        return Input.touchCount > 0 || (Input.touchSupported && currentDevice == DeviceType.Mobile);
    }
    
    public bool IsUsingMouse()
    {
        // Check if user is using mouse input
        return Input.mousePresent && currentDevice == DeviceType.Desktop;
    }
    
    // === SCREEN ORIENTATION (Mobile) ===
    
    public ScreenOrientation GetOrientation()
    {
        return Screen.orientation;
    }
    
    public bool IsPortrait()
    {
        return Screen.height > Screen.width;
    }
    
    public bool IsLandscape()
    {
        return Screen.width > Screen.height;
    }
    
    // === MANUAL OVERRIDE ===
    
    public void SetDeviceType(DeviceType type)
    {
        currentDevice = type;
        forceDeviceType = true;
        forcedDeviceType = type;
        
        Debug.Log("Device type manually set to: " + currentDevice);
        
        // Trigger events
        if (currentDevice == DeviceType.Desktop)
        {
            onDesktopDetected?.Invoke();
        }
        else
        {
            onMobileDetected?.Invoke();
        }
    }
    
    public void ResetToAutoDetect()
    {
        forceDeviceType = false;
        DetectDevice();
    }
}
