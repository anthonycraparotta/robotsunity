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
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
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
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            BuildAudioClipDictionary();

            isDesktopMode = Screen.width > GameConstants.UI.MobileMaxWidth;
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
