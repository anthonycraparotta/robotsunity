using System.Collections.Generic;
using UnityEngine;
using RobotsGame.Core;

namespace RobotsGame.Managers
{
    /// <summary>
    /// Manages all audio playback including SFX and voice-overs.
    /// Based on unityspec.md AUDIO CUES section.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        private static readonly string[] PrefabResourcePaths =
        {
            "Audio/AudioManager",
            "Prefabs/AudioManager",
            "Managers/AudioManager"
        };
        private const string ConfigurationResourcePath = "Audio/AudioManagerConfiguration";

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AudioManager>(true);

                    if (_instance == null)
                    {
                        _instance = InstantiateFromConfiguration();
                    }

                    if (_instance == null)
                    {
                        _instance = InstantiateFromResources();
                    }

                    if (_instance == null)
                    {
                        Debug.LogError("AudioManager.Instance was accessed but no configured AudioManager could be found. " +
                                       "Ensure a pre-configured AudioManager exists in the scene or provide a prefab at one of the configured Resources paths: " +
                                       string.Join(", ", PrefabResourcePaths) + ".");
                    }
                }

                return _instance;
            }
        }

        public static bool TryGetInstance(out AudioManager audioManager)
        {
            audioManager = Instance;
            return audioManager != null;
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceOverSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Audio Clips - Sound Effects")]
        [SerializeField] private AudioClip buttonPress;
        [SerializeField] private AudioClip robotSlideOut;
        [SerializeField] private AudioClip responsesSwoosh;
        [SerializeField] private AudioClip timerFinal10Sec;
        [SerializeField] private AudioClip inputAccept;
        [SerializeField] private AudioClip playerIconPop;
        [SerializeField] private AudioClip failure;
        [SerializeField] private AudioClip success;

        [Header("Audio Clips - Voice Overs")]
        [SerializeField] private AudioClip landingRules;
        [SerializeField] private AudioClip questionIntro;
        [SerializeField] private AudioClip questionNudge;
        [SerializeField] private AudioClip timeWarning;
        [SerializeField] private AudioClip robotAnswerGone;
        [SerializeField] private AudioClip noRobotAnswerGone;

        [Header("Settings")]
        [SerializeField] private float sfxVolume = 1.0f;
        [SerializeField] private float voVolume = 1.0f;
        [SerializeField] private float musicVolume = 0.5f;

        private Dictionary<string, AudioClip> audioClips;
        private bool isDesktopMode = true;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                if (IsSceneInstance(this))
                {
                    var previousInstance = _instance;
                    _instance = this;
                    DontDestroyOnLoad(gameObject);

                    if (previousInstance != null && previousInstance != this)
                    {
                        Destroy(previousInstance.gameObject);
                    }
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }

            if (_instance != this)
            {
                return;
            }

            InitializeAudioSources();
            BuildAudioClipDictionary();

            isDesktopMode = Screen.width > GameConstants.UI.MobileMaxWidth;
        }

        private static bool IsSceneInstance(AudioManager manager)
        {
            var scene = manager.gameObject.scene;
            return scene.IsValid() && scene.name != "DontDestroyOnLoad";
        }

        private static AudioManager InstantiateFromConfiguration()
        {
            var configuration = Resources.Load<AudioManagerConfiguration>(ConfigurationResourcePath);
            if (configuration == null)
            {
                return null;
            }

            var prefab = configuration.AudioManagerPrefab;
            if (prefab == null)
            {
                Debug.LogError($"AudioManagerConfiguration at Resources/{ConfigurationResourcePath} does not reference a prefab.");
                return null;
            }

            return InstantiatePrefab(prefab);
        }

        private static AudioManager InstantiateFromResources()
        {
            foreach (var path in PrefabResourcePaths)
            {
                var prefab = Resources.Load<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                var instance = InstantiatePrefab(prefab);
                if (instance != null)
                {
                    return instance;
                }
            }

            return null;
        }

        private static AudioManager InstantiatePrefab(GameObject prefab)
        {
            var instanceGo = UnityEngine.Object.Instantiate(prefab);
            instanceGo.name = prefab.name;

            var audioManager = instanceGo.GetComponent<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError($"AudioManager prefab '{prefab.name}' does not contain an AudioManager component.");
                UnityEngine.Object.Destroy(instanceGo);
                return null;
            }

            DontDestroyOnLoad(instanceGo);
            return audioManager;
        }

        private void InitializeAudioSources()
        {
            // Create audio sources if they don't exist
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }

            if (voiceOverSource == null)
            {
                voiceOverSource = gameObject.AddComponent<AudioSource>();
                voiceOverSource.playOnAwake = false;
                voiceOverSource.volume = voVolume;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.volume = musicVolume;
            }
        }

        private void BuildAudioClipDictionary()
        {
            audioClips = new Dictionary<string, AudioClip>
            {
                // SFX
                { GameConstants.Audio.SFX_ButtonPress, buttonPress },
                { GameConstants.Audio.SFX_RobotSlideOut, robotSlideOut },
                { GameConstants.Audio.SFX_ResponsesSwoosh, responsesSwoosh },
                { GameConstants.Audio.SFX_TimerFinal10Sec, timerFinal10Sec },
                { GameConstants.Audio.SFX_InputAccept, inputAccept },
                { GameConstants.Audio.SFX_PlayerIconPop, playerIconPop },
                { GameConstants.Audio.SFX_Failure, failure },
                { GameConstants.Audio.SFX_Success, success },

                // Voice Overs
                { GameConstants.Audio.VO_LandingRules, landingRules },
                { GameConstants.Audio.VO_QuestionIntro, questionIntro },
                { GameConstants.Audio.VO_QuestionNudge, questionNudge },
                { GameConstants.Audio.VO_TimeWarning, timeWarning },
                { GameConstants.Audio.VO_RobotAnswerGone, robotAnswerGone },
                { GameConstants.Audio.VO_NoRobotAnswerGone, noRobotAnswerGone }
            };
        }

        // ===========================
        // PLAYBACK - SOUND EFFECTS
        // ===========================

        public void PlaySFX(string clipName)
        {
            if (audioClips.TryGetValue(clipName, out AudioClip clip) && clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"SFX clip not found: {clipName}");
            }
        }

        public void PlayButtonPress()
        {
            PlaySFX(GameConstants.Audio.SFX_ButtonPress);
        }

        public void PlayRobotSlideOut()
        {
            PlaySFX(GameConstants.Audio.SFX_RobotSlideOut);
        }

        public void PlayResponsesSwoosh()
        {
            PlaySFX(GameConstants.Audio.SFX_ResponsesSwoosh);
        }

        public void PlayTimerFinal10Sec()
        {
            PlaySFX(GameConstants.Audio.SFX_TimerFinal10Sec);
        }

        public void PlayInputAccept()
        {
            PlaySFX(GameConstants.Audio.SFX_InputAccept);
        }

        public void PlayPlayerIconPop()
        {
            PlaySFX(GameConstants.Audio.SFX_PlayerIconPop);
        }

        public void PlayFailure()
        {
            PlaySFX(GameConstants.Audio.SFX_Failure);
        }

        public void PlaySuccess()
        {
            PlaySFX(GameConstants.Audio.SFX_Success);
        }

        // ===========================
        // PLAYBACK - VOICE OVERS (Desktop Only)
        // ===========================

        public void PlayVoiceOver(string clipName, float delay = 0f)
        {
            // Voice overs only play on desktop
            if (!isDesktopMode)
                return;

            if (audioClips.TryGetValue(clipName, out AudioClip clip) && clip != null)
            {
                if (delay > 0f)
                {
                    StartCoroutine(PlayVoiceOverDelayed(clip, delay));
                }
                else
                {
                    voiceOverSource.PlayOneShot(clip);
                }
            }
            else
            {
                Debug.LogWarning($"Voice over clip not found: {clipName}");
            }
        }

        private System.Collections.IEnumerator PlayVoiceOverDelayed(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            voiceOverSource.PlayOneShot(clip);
        }

        public void StopVoiceOver()
        {
            voiceOverSource.Stop();
        }

        public void StopAllAudio()
        {
            sfxSource.Stop();
            voiceOverSource.Stop();
            musicSource.Stop();
        }

        // ===========================
        // MUSIC
        // ===========================

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        // ===========================
        // VOLUME CONTROL
        // ===========================

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }

        public void SetVOVolume(float volume)
        {
            voVolume = Mathf.Clamp01(volume);
            voiceOverSource.volume = voVolume;
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }

        // ===========================
        // UTILITY
        // ===========================

        /// <summary>
        /// Load audio clip from Resources folder at runtime
        /// </summary>
        public void LoadAudioClip(string clipName, string resourcePath)
        {
            AudioClip clip = Resources.Load<AudioClip>(resourcePath);
            if (clip != null)
            {
                audioClips[clipName] = clip;
                Debug.Log($"Loaded audio clip: {clipName} from {resourcePath}");
            }
            else
            {
                Debug.LogError($"Failed to load audio clip from {resourcePath}");
            }
        }
    }
}
