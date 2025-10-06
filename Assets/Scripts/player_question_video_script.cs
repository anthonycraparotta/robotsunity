using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayerQuestionVideoScreen : MonoBehaviour
{
    [Header("Video Elements")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;

    [Header("Settings")]
    public bool skipOnClick = true;
    public bool autoAdvanceOnVideoEnd = true;

    void Start()
    {
        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoError;

            // Start preparing the video
            videoPlayer.Prepare();
        }
    }

    void Update()
    {
        // Check for skip input
        if (skipOnClick && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            SkipVideo();
        }
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("Player question video prepared and ready to play");

        // Auto-play when ready
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }

    void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log("Player question video finished");

        if (autoAdvanceOnVideoEnd)
        {
            AdvanceToNextScreen();
        }
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError("Video error: " + message);

        // If video fails, just advance
        AdvanceToNextScreen();
    }

    void SkipVideo()
    {
        Debug.Log("Skipping player question video");

        // Stop video
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        AdvanceToNextScreen();
    }

    void AdvanceToNextScreen()
    {
        // Continue to next screen (PlayerQuestionScreen)
        GameManager.Instance.AdvanceToNextScreen();
    }

    void OnDestroy()
    {
        // Clean up video listeners
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }
}
