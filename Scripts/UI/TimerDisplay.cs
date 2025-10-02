using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Managers;

namespace RobotsGame.UI
{
    /// <summary>
    /// Manages the countdown timer display with visual states and audio cues.
    /// Based on unityspec.md QuestionScreen timer specifications.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TimerDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerBackground;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Timer Settings")]
        [SerializeField] private float totalTime = 60f; // Total seconds
        [SerializeField] private bool autoStart = false;

        [Header("Visual States")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;

        private float timeRemaining;
        private bool isRunning = false;
        private int urgencyLevel = 0; // 0=normal, 1=warning(30s), 2=critical(18s), 3=final(12s)
        private bool hasPlayedTimeWarningVO = false;
        private bool hasPlayedFinalSFX = false;
        private RectTransform rectTransform;

        public float TimeRemaining => timeRemaining;
        public bool IsRunning => isRunning;
        public bool IsExpired => timeRemaining <= 0f;

        public event Action TimerExpired;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (!TryGetComponent(out rectTransform))
            {
                Debug.LogError($"{nameof(TimerDisplay)} requires a RectTransform component.", this);
                return;
            }

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            timeRemaining = totalTime;
            UpdateTimerDisplay();

            // Start hidden, will scale in
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        private void Start()
        {
            if (autoStart)
                StartTimer();
        }

        private void Update()
        {
            if (isRunning && timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining <= 0)
                {
                    timeRemaining = 0;
                    isRunning = false;
                    OnTimerExpired();
                }

                UpdateTimerDisplay();
                CheckUrgencyLevel();
            }
        }

        // ===========================
        // TIMER CONTROL
        // ===========================

        /// <summary>
        /// Start the countdown timer with scale-in animation
        /// </summary>
        public void StartTimer(float delay = 1.2f)
        {
            // Scale in animation
            DOVirtual.DelayedCall(delay, () =>
            {
                rectTransform.localScale = Vector3.zero;
                rectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                canvasGroup.DOFade(1f, 0.5f);

                isRunning = true;
            });
        }

        /// <summary>
        /// Pause the timer
        /// </summary>
        public void PauseTimer()
        {
            isRunning = false;
        }

        /// <summary>
        /// Resume the timer
        /// </summary>
        public void ResumeTimer()
        {
            if (timeRemaining > 0)
                isRunning = true;
        }

        /// <summary>
        /// Stop and reset the timer
        /// </summary>
        public void StopTimer()
        {
            isRunning = false;
            timeRemaining = 0;
            UpdateTimerDisplay();
        }

        /// <summary>
        /// Set timer to specific time
        /// </summary>
        public void SetTime(float seconds)
        {
            totalTime = seconds;
            timeRemaining = seconds;
            UpdateTimerDisplay();
        }

        // ===========================
        // DISPLAY UPDATE
        // ===========================

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(timeRemaining);
                timerText.text = seconds.ToString();
            }
        }

        private void CheckUrgencyLevel()
        {
            float percentRemaining = timeRemaining / totalTime;

            // Final 10 seconds (12s for audio cue)
            if (timeRemaining <= GameConstants.TimerThresholds.FinalTenSeconds && urgencyLevel < 3)
            {
                urgencyLevel = 3;
                SetVisualState(urgencyLevel);

                if (!hasPlayedFinalSFX)
                {
                    if (AudioManager.TryGetInstance(out var finalAudioManager))
                    {
                        finalAudioManager.PlayTimerFinal10Sec();
                    }

                    hasPlayedFinalSFX = true;
                }
            }
            // Critical state (18s for voice-over)
            else if (timeRemaining <= GameConstants.TimerThresholds.TimeWarningAudio && urgencyLevel < 2)
            {
                urgencyLevel = 2;
                SetVisualState(urgencyLevel);

                if (!hasPlayedTimeWarningVO)
                {
                    if (AudioManager.TryGetInstance(out var warningAudioManager))
                    {
                        warningAudioManager.PlayVoiceOver(GameConstants.Audio.VO_TimeWarning,
                                                          GameConstants.Delays.TimeWarningDelay);
                    }

                    hasPlayedTimeWarningVO = true;
                }
            }
            // Warning state (25% remaining)
            else if (percentRemaining <= GameConstants.TimerThresholds.WarningThreshold && urgencyLevel < 1)
            {
                urgencyLevel = 1;
                SetVisualState(urgencyLevel);
            }
        }

        private void SetVisualState(int level)
        {
            Color targetColor = normalColor;

            switch (level)
            {
                case 0:
                    targetColor = normalColor;
                    break;
                case 1:
                    targetColor = warningColor;
                    break;
                case 2:
                case 3:
                    targetColor = criticalColor;
                    if (level == 3)
                    {
                        // Shake animation for critical state
                        rectTransform.DOShakePosition(0.5f, strength: 10f, vibrato: 10, randomness: 90)
                            .SetLoops(-1, LoopType.Restart);
                    }
                    break;
            }

            // Animate color change
            if (timerText != null)
                timerText.DOColor(targetColor, 0.3f);

            if (timerBackground != null)
                timerBackground.DOColor(targetColor, 0.3f);
        }

        // ===========================
        // EVENTS
        // ===========================

        private void OnTimerExpired()
        {
            TimerExpired?.Invoke();
            Debug.Log("Timer expired!");
            // Controller will handle submission
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            DOTween.Kill(rectTransform);
            DOTween.Kill(timerText);
            DOTween.Kill(timerBackground);
            DOTween.Kill(canvasGroup);
        }
    }
}
