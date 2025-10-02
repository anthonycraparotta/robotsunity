using UnityEngine;

namespace RobotsGame.Utilities
{
    /// <summary>
    /// Utility functions for dynamic text sizing and formatting.
    /// Based on screenspec.md text scaling logic.
    /// </summary>
    public static class TextUtility
    {
        // ===========================
        // DYNAMIC FONT SIZING
        // ===========================

        /// <summary>
        /// Calculate dynamic font size to fit text within container width.
        /// Automatically scales down if text is too wide.
        /// </summary>
        /// <param name="text">The text to measure</param>
        /// <param name="maxWidth">Maximum container width in pixels</param>
        /// <param name="baseFontSize">Starting font size</param>
        /// <param name="font">Unity Font to use for measurement</param>
        /// <param name="minFontSize">Minimum allowed font size (default 10)</param>
        /// <param name="paddingBuffer">Extra padding buffer (default 30)</param>
        /// <returns>Calculated font size that fits within bounds</returns>
        public static float CalculateDynamicFontSize(
            string text,
            float maxWidth,
            float baseFontSize,
            Font font = null,
            float minFontSize = 10f,
            float paddingBuffer = 30f)
        {
            if (string.IsNullOrEmpty(text))
                return baseFontSize;

            // Adjust max width for padding
            float effectiveWidth = maxWidth - paddingBuffer;
            if (effectiveWidth <= 0)
                return minFontSize;

            // Estimate text width at base font size
            float textWidth = MeasureTextWidth(text, baseFontSize, font);

            if (textWidth <= effectiveWidth)
            {
                // Text fits at base size
                return baseFontSize;
            }

            // Calculate scale factor
            float scaleFactor = effectiveWidth / textWidth;
            float scaledSize = baseFontSize * scaleFactor;

            // Clamp to minimum size
            return Mathf.Max(scaledSize, minFontSize);
        }

        /// <summary>
        /// Measure text width at specific font size.
        /// Uses approximate calculation based on character count and average width.
        /// </summary>
        /// <param name="text">The text to measure</param>
        /// <param name="fontSize">Font size to measure at</param>
        /// <param name="font">Unity Font (optional)</param>
        /// <returns>Estimated width in pixels</returns>
        public static float MeasureTextWidth(string text, float fontSize, Font font = null)
        {
            if (string.IsNullOrEmpty(text))
                return 0f;

            // If we have a Unity Font, use CharacterInfo for accurate measurement
            if (font != null)
            {
                font.RequestCharactersInTexture(text, (int)fontSize);
                float width = 0f;

                foreach (char c in text)
                {
                    CharacterInfo characterInfo;
                    if (font.GetCharacterInfo(c, out characterInfo, (int)fontSize))
                    {
                        width += characterInfo.advance;
                    }
                }

                return width;
            }

            // Fallback: approximate width based on character count
            // Average character width is roughly 60% of font size
            float avgCharWidth = fontSize * 0.6f;
            return text.Length * avgCharWidth;
        }

        // ===========================
        // TEXT FORMATTING
        // ===========================

        /// <summary>
        /// Truncate text with ellipsis if too long.
        /// </summary>
        /// <param name="text">Text to truncate</param>
        /// <param name="maxLength">Maximum character length</param>
        /// <param name="ellipsis">Ellipsis string (default "...")</param>
        /// <returns>Truncated text</returns>
        public static string TruncateText(string text, int maxLength, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.Length <= maxLength)
                return text;

            int truncateLength = Mathf.Max(0, maxLength - ellipsis.Length);
            return text.Substring(0, truncateLength) + ellipsis;
        }

        /// <summary>
        /// Word wrap text at maximum line length.
        /// </summary>
        /// <param name="text">Text to wrap</param>
        /// <param name="maxLineLength">Maximum characters per line</param>
        /// <returns>Text with line breaks inserted</returns>
        public static string WordWrap(string text, int maxLineLength)
        {
            if (string.IsNullOrEmpty(text) || maxLineLength <= 0)
                return text;

            string[] words = text.Split(' ');
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            int currentLineLength = 0;

            foreach (string word in words)
            {
                if (currentLineLength + word.Length + 1 > maxLineLength && currentLineLength > 0)
                {
                    result.AppendLine();
                    currentLineLength = 0;
                }

                if (currentLineLength > 0)
                {
                    result.Append(" ");
                    currentLineLength++;
                }

                result.Append(word);
                currentLineLength += word.Length;
            }

            return result.ToString();
        }

        // ===========================
        // FONT SIZE HELPERS
        // ===========================

        /// <summary>
        /// Get font size based on answer count (for answer lists).
        /// Fewer answers = larger font size.
        /// </summary>
        /// <param name="answerCount">Number of answers</param>
        /// <param name="baseFontSize">Base font size for few answers</param>
        /// <param name="minFontSize">Minimum font size for many answers</param>
        /// <returns>Calculated font size</returns>
        public static float GetFontSizeByAnswerCount(int answerCount, float baseFontSize = 44f, float minFontSize = 31f)
        {
            // Based on screenspec.md answer count sizing:
            // 2-3 answers: 2.29vw (44px at 1920px)
            // 4-5 answers: 2.03vw (39px)
            // 6+ answers: 1.61vw (31px)

            if (answerCount <= 3)
                return baseFontSize; // 44px
            else if (answerCount <= 5)
                return baseFontSize * 0.89f; // 39px
            else
                return minFontSize; // 31px
        }

        /// <summary>
        /// Get font size based on question text length.
        /// Shorter questions = larger font size.
        /// </summary>
        /// <param name="questionLength">Length of question text</param>
        /// <param name="baseFontSize">Base font size for short questions</param>
        /// <param name="minFontSize">Minimum font size for long questions</param>
        /// <returns>Calculated font size</returns>
        public static float GetFontSizeByQuestionLength(int questionLength, float baseFontSize = 96f, float minFontSize = 48f)
        {
            // Based on screenspec.md question length sizing:
            // Short questions (< 30 chars): 5.0vw (96px at 1920px)
            // Medium questions (30-60 chars): 3.75vw (72px)
            // Long questions (> 60 chars): 2.5vw (48px)

            if (questionLength < 30)
                return baseFontSize; // 96px
            else if (questionLength < 60)
                return baseFontSize * 0.75f; // 72px
            else
                return minFontSize; // 48px
        }

        // ===========================
        // RICH TEXT
        // ===========================

        /// <summary>
        /// Add color tag to text.
        /// </summary>
        public static string Colorize(string text, Color color)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hexColor}>{text}</color>";
        }

        /// <summary>
        /// Add bold tag to text.
        /// </summary>
        public static string Bold(string text)
        {
            return $"<b>{text}</b>";
        }

        /// <summary>
        /// Add italic tag to text.
        /// </summary>
        public static string Italic(string text)
        {
            return $"<i>{text}</i>";
        }

        /// <summary>
        /// Add size tag to text.
        /// </summary>
        public static string Resize(string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }
    }
}
