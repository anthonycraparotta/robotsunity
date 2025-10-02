using UnityEngine;

namespace RobotsGame.Managers
{
    /// <summary>
    /// Scriptable object configuration that provides a pre-configured AudioManager prefab.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioManagerConfiguration", menuName = "RobotsGame/Audio/AudioManager Configuration")]
    public class AudioManagerConfiguration : ScriptableObject
    {
        [SerializeField] private GameObject audioManagerPrefab;

        public GameObject AudioManagerPrefab => audioManagerPrefab;
    }
}
