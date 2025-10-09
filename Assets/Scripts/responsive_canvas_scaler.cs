using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically adjusts Canvas Scaler settings based on device type (mobile vs desktop).
/// Attach this to any Canvas GameObject that needs responsive scaling.
/// </summary>
[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvasScaler : MonoBehaviour
{
    [Header("Desktop Settings")]
    [Tooltip("Reference resolution for desktop displays")]
    public Vector2 desktopReferenceResolution = new Vector2(1920, 1080);

    [Header("Mobile Settings")]
    [Tooltip("Reference resolution for mobile displays")]
    public Vector2 mobileReferenceResolution = new Vector2(1080, 1920);

    [Header("Scale Mode Settings")]
    [Tooltip("Match width (0) or height (1) - 0.5 is balanced")]
    [Range(0f, 1f)]
    public float desktopMatchWidthOrHeight = 0.5f;

    [Range(0f, 1f)]
    public float mobileMatchWidthOrHeight = 0.5f;

    private CanvasScaler canvasScaler;

    void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();

        if (canvasScaler == null)
        {
            Debug.LogError("ResponsiveCanvasScaler requires a CanvasScaler component!");
            return;
        }

        ApplyCanvasScalerSettings();
    }

    void ApplyCanvasScalerSettings()
    {
        bool isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        // Set UI Scale Mode
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Set reference resolution based on device type
        if (isMobile)
        {
            canvasScaler.referenceResolution = mobileReferenceResolution;
            canvasScaler.matchWidthOrHeight = mobileMatchWidthOrHeight;
            Debug.Log($"Canvas Scaler configured for MOBILE: {mobileReferenceResolution.x}x{mobileReferenceResolution.y}");
        }
        else
        {
            canvasScaler.referenceResolution = desktopReferenceResolution;
            canvasScaler.matchWidthOrHeight = desktopMatchWidthOrHeight;
            Debug.Log($"Canvas Scaler configured for DESKTOP: {desktopReferenceResolution.x}x{desktopReferenceResolution.y}");
        }

        // Set screen match mode
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    }
}
