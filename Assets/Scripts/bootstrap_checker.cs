using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Add this to any scene that might be loaded directly during development.
/// It ensures managers are loaded before the scene starts.
/// </summary>
public class BootstrapChecker : MonoBehaviour
{
    void Awake()
    {
        // Check if GameManager exists (it's created in LoadingScreen)
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found! Loading LoadingScreen first...");
            SceneManager.LoadScene("LoadingScreen");
        }
    }
}
