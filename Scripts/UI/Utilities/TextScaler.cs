using UnityEngine;
using TMPro;
using RobotsGame.Core;

namespace RobotsGame.UI.Utilities
{
    /// <summary>
    /// Dynamically scales text to fit within a container.
    /// Based on unityspec.md Element Scaling Rules.
    /// Attach this to TextMeshProUGUI components that need dynamic sizing.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextScaler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private RectTransform containerRect;

        [Header("Settings")]
        [SerializeField] private bool autoScale = true;
        [SerializeField] private float minFontSize = GameConstants.UI.MinFontSize;
        [SerializeField] private float maxFontSize = 100f;
        [SerializeField] private float paddingBuffer = GameConstants.UI.TextPaddingBuffer;
        [SerializeField] private bool scaleOnTextChange = true;

        private string lastText = "";

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (textComponent == null)
                textComponent = GetComponent<TextMeshProUGUI>();

            if (containerRect == null)
                containerRect = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (autoScale)
                ScaleTextToFit();
        }

        private void LateUpdate()
        {
            if (scaleOnTextChange && textComponent.text != lastText)
            {
                lastText = textComponent.text;
                if (autoScale)
                    ScaleTextToFit();
            }
        }

        // ===========================
        // SCALING METHODS
        // ===========================

        /// <summary>
        /// Scale text to fit within container bounds
        /// </summary>
        public void ScaleTextToFit()
        {
            if (textComponent == null || containerRect == null)
                return;

            if (string.IsNullOrEmpty(textComponent.text))
                return;

            // Get container dimensions
            float containerWidth = containerRect.rect.width - paddingBuffer;
            float containerHeight = containerRect.rect.height - paddingBuffer;

            // Start with max font size
            textComponent.fontSize = maxFontSize;
            textComponent.enableAutoSizing = false;

            // Force update to get accurate measurements
            Canvas.ForceUpdateCanvases();
            textComponent.ForceMeshUpdate();

            // Get text dimensions at max size
            Vector2 textSize = textComponent.GetPreferredValues();

            // Calculate scale factor to fit width
            float widthScale = containerWidth / textSize.x;
            float heightScale = containerHeight / textSize.y;

            // Use the smaller scale to ensure it fits in both dimensions
            float scaleFactor = Mathf.Min(widthScale, heightScale, 1f);

            // Apply scaled font size
            float newFontSize = Mathf.Clamp(maxFontSize * scaleFactor, minFontSize, maxFontSize);
            textComponent.fontSize = newFontSize;

            // Force update again
            Canvas.ForceUpdateCanvases();
            textComponent.ForceMeshUpdate();
        }

        /// <summary>
        /// Scale text based on character count (alternative method)
        /// Used for answer lists where font size varies by count
        /// </summary>
        public void ScaleByCharacterCount(int characterCount, float baseFontSize)
        {
            if (textComponent == null)
                return;

            // Longer text gets smaller font
            float sizeFactor = 1f;

            if (characterCount > 100)
                sizeFactor = 0.7f;
            else if (characterCount > 50)
                sizeFactor = 0.85f;

            float newFontSize = Mathf.Clamp(baseFontSize * sizeFactor, minFontSize, maxFontSize);
            textComponent.fontSize = newFontSize;
        }

        /// <summary>
        /// Scale text based on number of answers (for answer lists)
        /// Desktop: 2.29vw down to 1.61vw based on answer count
        /// </summary>
        public void ScaleByAnswerCount(int answerCount, ResponsiveUI responsiveUI)
        {
            if (textComponent == null || responsiveUI == null)
                return;

            float baseFontSize;

            if (responsiveUI.IsDesktop)
            {
                // Desktop: 2.29vw to 1.61vw based on count
                float vwSize = answerCount <= 3 ? 2.29f : (answerCount <= 6 ? 2.0f : 1.61f);
                baseFontSize = responsiveUI.VWToPixels(vwSize);
            }
            else
            {
                // Mobile: smaller base size
                float vwSize = answerCount <= 3 ? 4.5f : (answerCount <= 6 ? 4.0f : 3.5f);
                baseFontSize = responsiveUI.VWToPixels(vwSize);
            }

            textComponent.fontSize = Mathf.Clamp(baseFontSize, minFontSize, maxFontSize);
        }

        /// <summary>
        /// Set font size directly with clamping
        /// </summary>
        public void SetFontSize(float fontSize)
        {
            if (textComponent == null)
                return;

            textComponent.fontSize = Mathf.Clamp(fontSize, minFontSize, maxFontSize);
        }

        /// <summary>
        /// Enable/disable auto-sizing based on text changes
        /// </summary>
        public void SetAutoScale(bool enable)
        {
            autoScale = enable;

            if (enable)
                ScaleTextToFit();
        }

        /// <summary>
        /// Truncate text with ellipsis if too long
        /// </summary>
        public void TruncateIfNeeded(int maxCharacters)
        {
            if (textComponent == null || string.IsNullOrEmpty(textComponent.text))
                return;

            if (textComponent.text.Length > maxCharacters)
            {
                textComponent.text = textComponent.text.Substring(0, maxCharacters - 3) + "...";
            }
        }

        // ===========================
        // UTILITY
        // ===========================

        /// <summary>
        /// Get current text dimensions
        /// </summary>
        public Vector2 GetTextDimensions()
        {
            if (textComponent == null)
                return Vector2.zero;

            Canvas.ForceUpdateCanvases();
            textComponent.ForceMeshUpdate();

            return textComponent.GetPreferredValues();
        }

        /// <summary>
        /// Check if text overflows container
        /// </summary>
        public bool IsOverflowing()
        {
            if (textComponent == null || containerRect == null)
                return false;

            Vector2 textSize = GetTextDimensions();
            float containerWidth = containerRect.rect.width - paddingBuffer;
            float containerHeight = containerRect.rect.height - paddingBuffer;

            return textSize.x > containerWidth || textSize.y > containerHeight;
        }
    }
}
