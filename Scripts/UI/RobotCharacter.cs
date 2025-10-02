using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using RobotsGame.Managers;
using RobotsGame.Core;

namespace RobotsGame.UI
{
    /// <summary>
    /// Manages the robot character foreground image with slide-in animation and blinking.
    /// Based on unityspec.md QuestionScreen - Foreground Overlay specifications.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class RobotCharacter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image foregroundImage;
        [SerializeField] private Image blinkImage;

        [Header("Round Settings")]
        [SerializeField] private int currentRound = 1;

        [Header("Animation Settings")]
        [SerializeField] private float slideInDelay = 0.1f; // 100ms
        [SerializeField] private float slideInDuration = 1f; // 1000ms
        [SerializeField] private float blinkInterval = 5f; // Every 5 seconds
        [SerializeField] private float blinkDuration = 0.2f; // 200ms

        [Header("Round-Specific Sprites")]
        [SerializeField] private Sprite[] foregroundSprites = new Sprite[12]; // Q1FG through Q12FG
        [SerializeField] private Sprite[] blinkSprites = new Sprite[12]; // Q1FG-blink through Q12FG-blink

        private RectTransform rectTransform;
        private bool isBlinking = false;
        private Coroutine blinkCoroutine;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (rectTransform == null)
            {
                Debug.LogError($"{nameof(RobotCharacter)} requires a RectTransform component.", this);
                return;
            }

            if (blinkImage != null)
            {
                blinkImage.enabled = false;
            }
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Setup robot for specific round and play slide-in animation
        /// </summary>
        public void SetupRobot(int round)
        {
            currentRound = Mathf.Clamp(round, 1, 12);

            // Set sprite for this round
            if (foregroundImage != null && foregroundSprites.Length >= currentRound)
            {
                foregroundImage.sprite = foregroundSprites[currentRound - 1];
            }

            if (blinkImage != null && blinkSprites.Length >= currentRound)
            {
                blinkImage.sprite = blinkSprites[currentRound - 1];
            }

            // Position off-screen to the left for slide-in
            Vector2 startPos = rectTransform.anchoredPosition;
            startPos.x = -Screen.width; // Off-screen left
            rectTransform.anchoredPosition = startPos;
        }

        /// <summary>
        /// Play the slide-in animation from left
        /// </summary>
        public void SlideIn()
        {
            DOVirtual.DelayedCall(slideInDelay, () =>
            {
                // Play sound effect
                AudioManager.Instance.PlayRobotSlideOut();

                // Slide to original position (0, y)
                Vector2 targetPos = Vector2.zero;
                rectTransform.DOAnchorPos(targetPos, slideInDuration)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() =>
                    {
                        StartBlinking();
                    });
            });
        }

        /// <summary>
        /// Start the blinking animation loop
        /// </summary>
        public void StartBlinking()
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);

            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        /// <summary>
        /// Stop the blinking animation
        /// </summary>
        public void StopBlinking()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }

            if (blinkImage != null)
                blinkImage.enabled = false;
        }

        // ===========================
        // BLINKING
        // ===========================

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(blinkInterval);

                // Show blink image
                if (blinkImage != null)
                {
                    blinkImage.enabled = true;
                }

                yield return new WaitForSeconds(blinkDuration);

                // Hide blink image
                if (blinkImage != null)
                {
                    blinkImage.enabled = false;
                }
            }
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            StopBlinking();
            DOTween.Kill(rectTransform);
        }
    }
}
