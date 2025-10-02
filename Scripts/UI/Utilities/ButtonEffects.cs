using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Managers;

namespace RobotsGame.UI.Utilities
{
    /// <summary>
    /// Standard button hover and press effects.
    /// Based on unityspec.md Standard Button Pattern.
    /// Attach this to any Button GameObject to get standard effects.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private Button button;

        [Header("Effect Settings")]
        [SerializeField] private bool playSound = true;
        [SerializeField] private bool enableHoverEffect = true;
        [SerializeField] private bool enablePressEffect = true;

        [Header("Scale Settings")]
        [SerializeField] private float hoverScale = GameConstants.UI.ButtonHoverScale;
        [SerializeField] private float pressScale = GameConstants.UI.ButtonPressScale;
        [SerializeField] private float normalScale = 1f;
        [SerializeField] private float transitionDuration = GameConstants.UI.ButtonTransitionDuration;

        private Vector3 originalScale;
        private bool isHovering = false;
        private bool isPressed = false;
        private bool isMobile = false;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            originalScale = transform.localScale;
            isMobile = Screen.width <= GameConstants.UI.MobileMaxWidth;

            // Disable hover effects on mobile
            if (isMobile)
                enableHoverEffect = false;
        }

        private void OnEnable()
        {
            // Reset to normal scale when enabled
            transform.DOScale(originalScale * normalScale, 0f);
        }

        // ===========================
        // POINTER EVENTS
        // ===========================

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enableHoverEffect || !button.interactable)
                return;

            isHovering = true;

            if (!isPressed)
            {
                transform.DOScale(originalScale * hoverScale, transitionDuration)
                    .SetEase(Ease.OutQuad);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enableHoverEffect)
                return;

            isHovering = false;

            if (!isPressed)
            {
                transform.DOScale(originalScale * normalScale, transitionDuration)
                    .SetEase(Ease.OutQuad);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!enablePressEffect || !button.interactable)
                return;

            isPressed = true;

            transform.DOScale(originalScale * pressScale, transitionDuration)
                .SetEase(Ease.OutQuad);

            // Play sound on press
            if (playSound && AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!enablePressEffect)
                return;

            isPressed = false;

            // Return to hover scale if still hovering, otherwise normal
            float targetScale = (isHovering && enableHoverEffect) ? hoverScale : normalScale;

            transform.DOScale(originalScale * targetScale, transitionDuration)
                .SetEase(Ease.OutQuad);
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Manually trigger button press animation and sound
        /// </summary>
        public void PressButton()
        {
            if (!button.interactable)
                return;

            // Animate press and release
            Sequence pressSequence = DOTween.Sequence();
            pressSequence.Append(transform.DOScale(originalScale * pressScale, transitionDuration * 0.5f).SetEase(Ease.OutQuad));
            pressSequence.Append(transform.DOScale(originalScale * normalScale, transitionDuration * 0.5f).SetEase(Ease.OutQuad));

            if (playSound && AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }

            // Trigger button click
            button.onClick.Invoke();
        }

        /// <summary>
        /// Set button interactable state with visual feedback
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;

            if (!interactable)
            {
                // Reset to normal scale when disabled
                transform.DOScale(originalScale * normalScale, transitionDuration);
                isHovering = false;
                isPressed = false;
            }
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            // Kill any active tweens
            transform.DOKill();
        }
    }
}
