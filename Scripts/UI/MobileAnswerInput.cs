using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using RobotsGame.Core;
using RobotsGame.Managers;

namespace RobotsGame.UI
{
    /// <summary>
    /// Manages the mobile answer input field with validation and flash warnings.
    /// Based on unityspec.md QuestionScreen - Mobile InputField specifications.
    /// </summary>
    public class MobileAnswerInput : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Image inputBackground;
        [SerializeField] private TextMeshProUGUI placeholderText;
        [SerializeField] private Button submitButton;
        [SerializeField] private CanvasGroup submitButtonCanvasGroup;

        [Header("Flash Warning Settings")]
        [SerializeField] private float flashDuration = 2f; // 2000ms

        [Header("Colors")]
        [SerializeField] private Color normalBorderColor;
        [SerializeField] private Color warningBorderColor;
        [SerializeField] private Color normalBackgroundColor;
        [SerializeField] private Color submittedBackgroundColor;
        [SerializeField] private Color warningBackgroundColor;
        [SerializeField] private Color normalTextColor;
        [SerializeField] private Color warningTextColor;

        private bool hasSubmitted = false;
        private bool isFlashing = false;
        private string currentAnswer = "";
        private Coroutine flashCoroutine;

        public bool HasSubmitted => hasSubmitted;
        public string CurrentAnswer => currentAnswer;
        public TMP_InputField InputField => inputField;

        // Events
        public System.Action<string> OnAnswerSubmitted;
        public System.Action<string> OnAnswerChanged;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (inputField == null)
                inputField = GetComponent<TMP_InputField>();

            if (inputBackground == null)
                inputBackground = GetComponent<Image>();

            if (placeholderText == null && inputField != null)
                placeholderText = inputField.placeholder as TextMeshProUGUI;

            SetupColors();
            SetupListeners();
        }

        private void SetupColors()
        {
            normalBorderColor = GameConstants.Colors.Cream;
            warningBorderColor = GameConstants.Colors.BrightRed;
            normalBackgroundColor = new Color(1f, 251f/255f, 188f/255f, 0.9f);
            submittedBackgroundColor = new Color(3f/255f, 2f/255f, 49f/255f, 0.25f);
            warningBackgroundColor = GameConstants.Colors.BrightRed;
            normalTextColor = GameConstants.Colors.DarkBlue;
            warningTextColor = Color.white;
        }

        private void SetupListeners()
        {
            if (inputField != null)
            {
                inputField.onValueChanged.AddListener(OnInputChanged);
            }

            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitClicked);
            }
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Enable or disable the input field
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (inputField != null)
                inputField.interactable = enabled && !hasSubmitted && !isFlashing;

            UpdateSubmitButton();
        }

        /// <summary>
        /// Mark answer as submitted
        /// </summary>
        public void MarkAsSubmitted()
        {
            hasSubmitted = true;

            if (inputField != null)
            {
                inputField.interactable = false;
                SetPlaceholder("Answer submitted!");
            }

            if (inputBackground != null)
            {
                inputBackground.color = submittedBackgroundColor;
            }

            UpdateSubmitButton();
        }

        /// <summary>
        /// Set placeholder text
        /// </summary>
        public void SetPlaceholder(string text)
        {
            if (placeholderText != null)
                placeholderText.text = text;
        }

        /// <summary>
        /// Clear input field
        /// </summary>
        public void ClearInput()
        {
            if (inputField != null)
                inputField.text = "";
            currentAnswer = "";
        }

        // ===========================
        // FLASH WARNINGS
        // ===========================

        /// <summary>
        /// Show flash warning for correct answer (decoy warning)
        /// </summary>
        public void FlashDecoyWarning(string originalAnswer)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashWarningRoutine(
                GameConstants.Warnings.CorrectAnswer,
                originalAnswer,
                restoreText: true
            ));

            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayFailure();
            }
        }

        /// <summary>
        /// Show flash warning for duplicate answer
        /// </summary>
        public void FlashDuplicateWarning()
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashWarningRoutine(
                GameConstants.Warnings.DuplicateResponse,
                "",
                restoreText: false
            ));

            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayFailure();
            }
        }

        /// <summary>
        /// Show flash warning for profanity
        /// </summary>
        public void FlashProfanityWarning()
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashWarningRoutine(
                GameConstants.Warnings.InappropriateContent,
                "",
                restoreText: false
            ));

            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayFailure();
            }
        }

        private IEnumerator FlashWarningRoutine(string warningMessage, string textToRestore, bool restoreText)
        {
            isFlashing = true;
            string originalText = inputField.text;

            // Disable input during flash
            inputField.interactable = false;

            // Flash to warning state
            inputField.text = warningMessage;
            if (inputBackground != null)
                inputBackground.DOColor(warningBackgroundColor, 0.2f);
            inputField.textComponent.DOColor(warningTextColor, 0.2f);

            // Wait for flash duration
            yield return new WaitForSeconds(flashDuration);

            // Restore to normal state
            if (restoreText && !string.IsNullOrEmpty(textToRestore))
            {
                inputField.text = textToRestore;
                currentAnswer = textToRestore;
            }
            else
            {
                inputField.text = "";
                currentAnswer = "";
            }

            if (inputBackground != null)
                inputBackground.DOColor(normalBackgroundColor, 0.2f);
            inputField.textComponent.DOColor(normalTextColor, 0.2f);

            // Re-enable input
            inputField.interactable = !hasSubmitted;
            isFlashing = false;

            UpdateSubmitButton();
        }

        // ===========================
        // EVENT HANDLERS
        // ===========================

        private void OnInputChanged(string value)
        {
            currentAnswer = value;
            OnAnswerChanged?.Invoke(value);
            UpdateSubmitButton();
        }

        private void OnSubmitClicked()
        {
            if (string.IsNullOrEmpty(currentAnswer) || hasSubmitted || isFlashing)
                return;

            OnAnswerSubmitted?.Invoke(currentAnswer);
        }

        private void UpdateSubmitButton()
        {
            if (submitButton == null) return;

            bool canSubmit = !string.IsNullOrEmpty(currentAnswer) && !hasSubmitted && !isFlashing;

            submitButton.interactable = canSubmit;

            if (submitButtonCanvasGroup != null)
            {
                submitButtonCanvasGroup.alpha = canSubmit ? 1f : 0.5f;
            }
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            if (inputField != null)
                inputField.onValueChanged.RemoveListener(OnInputChanged);

            if (submitButton != null)
                submitButton.onClick.RemoveListener(OnSubmitClicked);

            DOTween.Kill(inputBackground);
            DOTween.Kill(inputField?.textComponent);
        }
    }
}
