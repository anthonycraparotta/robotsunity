using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RobotsGame.Core
{
    /// <summary>
    /// Helper for loading scenes safely at runtime.
    /// </summary>
    public static class SceneLoader
    {
        private const string DefaultSceneFolder = "Assets/Scenes";

        /// <summary>
        /// Attempts to load the provided scene name if it exists in the build settings.
        /// Logs a descriptive error if the scene is missing instead of allowing the game to crash.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <returns>True if the load was initiated, false otherwise.</returns>
        public static bool TryLoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("SceneLoader: Cannot load a null or empty scene name.");
                return false;
            }

            string scenePath = $"{DefaultSceneFolder}/{sceneName}.unity";
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

            if (buildIndex < 0 && !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"SceneLoader: Scene '{sceneName}' is not available. Ensure a scene exists at '{scenePath}' and is added to the Build Settings.");
                return false;
            }

            try
            {
                SceneManager.LoadScene(sceneName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SceneLoader: Failed to load scene '{sceneName}'. Exception: {ex.Message}");
                return false;
            }
        }
    }
}
