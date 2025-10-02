using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Core;
using RobotsGame.Data;

namespace RobotsGame.UI
{
    /// <summary>
    /// Manages the answer list for elimination voting.
    /// Desktop: Shows all answers with click selection
    /// Mobile: Filters out own answer, shows with tap selection
    /// Based on unityspec.md EliminationScreen specifications.
    /// </summary>
    public class EliminationAnswerList : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject answerButtonPrefab;
        [SerializeField] private Transform answerContainer;

        [Header("Settings")]
        [SerializeField] private bool isMobile = false;
        [SerializeField] private float cascadeStartDelay = 1f;
        [SerializeField] private float cascadeItemDelay = 0.2f;
        [SerializeField] private float revealDuration = 0.5f;

        [Header("Colors")]
        [SerializeField] private Color normalBgColor;
        [SerializeField] private Color selectedBgColor;
        [SerializeField] private Color eliminatedBgColor;
        [SerializeField] private Color normalTextColor;
        [SerializeField] private Color selectedTextColor;
        [SerializeField] private Color eliminatedTextColor;

        private List<GameObject> answerButtons = new List<GameObject>();
        private Dictionary<string, GameObject> answerButtonMap = new Dictionary<string, GameObject>();
        private string selectedAnswer = null;
        private string ownAnswer = null;
        private bool hasVoted = false;

        public string SelectedAnswer => selectedAnswer;
        public System.Action<string> OnAnswerSelected;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (answerContainer == null)
                answerContainer = transform;

            SetupColors();
        }

        private void SetupColors()
        {
            normalBgColor = GameConstants.Colors.Cream;
            selectedBgColor = GameConstants.Colors.BrightRed;
            eliminatedBgColor = GameConstants.Colors.BrightRed;
            normalTextColor = GameConstants.Colors.DarkBlue;
            selectedTextColor = GameConstants.Colors.PrimaryYellow;
            eliminatedTextColor = GameConstants.Colors.PrimaryYellow;
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Populate answer list with cascade animation
        /// </summary>
        public void SetupAnswers(List<Answer> answers, string playerOwnAnswer, bool filterOwnAnswer = false)
        {
            ClearAnswers();

            ownAnswer = playerOwnAnswer;

            // Filter and shuffle
            List<Answer> displayAnswers = new List<Answer>(answers);

            // Mobile: filter out own answer
            if (filterOwnAnswer && !string.IsNullOrEmpty(playerOwnAnswer))
            {
                displayAnswers.RemoveAll(a => a.Text == playerOwnAnswer);
            }

            // Shuffle answers
            ShuffleList(displayAnswers);

            // Create buttons
            for (int i = 0; i < displayAnswers.Count; i++)
            {
                Answer answer = displayAnswers[i];
                CreateAnswerButton(answer, i);
            }

            // Start cascade reveal
            RevealAnswersCascade();
        }

        /// <summary>
        /// Highlight eliminated answer
        /// </summary>
        public void HighlightEliminatedAnswer(string answer, float holdDuration = 4f)
        {
            if (string.IsNullOrEmpty(answer)) return;

            if (answerButtonMap.TryGetValue(answer, out GameObject buttonObj))
            {
                HighlightButton(buttonObj, holdDuration);
            }
        }

        /// <summary>
        /// Disable voting (after vote submitted)
        /// </summary>
        public void DisableVoting()
        {
            hasVoted = true;

            foreach (var kvp in answerButtonMap)
            {
                Button button = kvp.Value.GetComponent<Button>();
                if (button != null)
                    button.interactable = false;

                // Grey out non-selected answers
                if (kvp.Key != selectedAnswer)
                {
                    Image bg = kvp.Value.GetComponent<Image>();
                    TextMeshProUGUI text = kvp.Value.GetComponentInChildren<TextMeshProUGUI>();

                    if (bg != null)
                        bg.color = GameConstants.Colors.Grey;

                    if (text != null)
                        text.color = normalTextColor;
                }
            }
        }

        /// <summary>
        /// Clear selected answer
        /// </summary>
        public void ClearSelection()
        {
            if (selectedAnswer != null && answerButtonMap.TryGetValue(selectedAnswer, out GameObject prevButton))
            {
                SetButtonNormalState(prevButton);
            }

            selectedAnswer = null;
        }

        /// <summary>
        /// Clear all answers
        /// </summary>
        public void ClearAnswers()
        {
            foreach (var button in answerButtons)
            {
                if (button != null)
                    Destroy(button);
            }

            answerButtons.Clear();
            answerButtonMap.Clear();
            selectedAnswer = null;
        }

        // ===========================
        // ANSWER BUTTON CREATION
        // ===========================

        private void CreateAnswerButton(Answer answer, int index)
        {
            GameObject buttonObj;

            if (answerButtonPrefab != null)
            {
                buttonObj = Instantiate(answerButtonPrefab, answerContainer);
            }
            else
            {
                buttonObj = CreateDefaultAnswerButton();
            }

            // Setup button
            SetupAnswerButton(buttonObj, answer, index);

            // Store references
            answerButtons.Add(buttonObj);
            answerButtonMap[answer.Text] = buttonObj;

            // Start hidden for cascade
            CanvasGroup canvasGroup = buttonObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = buttonObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 pos = rect.anchoredPosition;
                pos.x = -100f; // Off to left
                rect.anchoredPosition = pos;
            }
        }

        private GameObject CreateDefaultAnswerButton()
        {
            GameObject buttonObj = new GameObject("AnswerButton");
            buttonObj.transform.SetParent(answerContainer, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 80); // Auto width, 80 height

            Image bg = buttonObj.AddComponent<Image>();
            bg.color = normalBgColor;

            Button button = buttonObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(20, 10);
            textRect.offsetMax = new Vector2(-20, -10);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.color = normalTextColor;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Left;
            text.fontStyle = FontStyles.Bold;
            text.textWrappingMode = TextWrappingModes.Normal;

            return buttonObj;
        }

        private void SetupAnswerButton(GameObject buttonObj, Answer answer, int index)
        {
            // Set text
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = answer.Text.ToUpper();

                // Dynamic font size based on answer count
                float baseFontSize = GetDynamicFontSize(answerButtons.Count + 1);
                text.fontSize = baseFontSize;
            }

            // Setup button click
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                // Check if this is own answer (desktop only)
                bool isOwnAnswer = !isMobile && answer.Text == ownAnswer;

                // Check if this is fallback answer
                bool isFallback = answer.Text == GameConstants.FallbackText.NoAnswer;

                if (isOwnAnswer || isFallback)
                {
                    button.interactable = false;
                }
                else
                {
                    button.onClick.AddListener(() => OnAnswerButtonClicked(answer.Text));
                }
            }
        }

        private float GetDynamicFontSize(int answerCount)
        {
            // Based on spec: dynamic sizing from 4.59vw (1 answer) to 1.61vw (9+ answers)
            // Simplified to pixel values for Unity
            switch (answerCount)
            {
                case 1: return 88f;
                case 2: return 77f * 1.3f; // 30% larger
                case 3: return 66f * 1.2f; // 20% larger
                case 4: return 57f;
                case 5: return 51f;
                case 6: return 44f;
                case 7: return 40f;
                case 8: return 35f;
                default: return 31f; // 9+
            }
        }

        // ===========================
        // INTERACTION
        // ===========================

        private void OnAnswerButtonClicked(string answerText)
        {
            if (hasVoted) return;

            // Deselect previous
            if (selectedAnswer != null && answerButtonMap.TryGetValue(selectedAnswer, out GameObject prevButton))
            {
                SetButtonNormalState(prevButton);
            }

            // Select new
            selectedAnswer = answerText;

            if (answerButtonMap.TryGetValue(answerText, out GameObject newButton))
            {
                SetButtonSelectedState(newButton);
            }

            OnAnswerSelected?.Invoke(answerText);
        }

        private void SetButtonNormalState(GameObject buttonObj)
        {
            Image bg = buttonObj.GetComponent<Image>();
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (bg != null)
                bg.DOColor(normalBgColor, 0.3f);

            if (text != null)
                text.DOColor(normalTextColor, 0.3f);
        }

        private void SetButtonSelectedState(GameObject buttonObj)
        {
            Image bg = buttonObj.GetComponent<Image>();
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (bg != null)
                bg.DOColor(selectedBgColor, 0.3f);

            if (text != null)
                text.DOColor(selectedTextColor, 0.3f);
        }

        private void HighlightButton(GameObject buttonObj, float holdDuration)
        {
            Image bg = buttonObj.GetComponent<Image>();
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            RectTransform rect = buttonObj.GetComponent<RectTransform>();

            if (bg != null)
                bg.DOColor(eliminatedBgColor, 0.3f);

            if (text != null)
                text.DOColor(eliminatedTextColor, 0.3f);

            if (rect != null)
                rect.DOScale(1.02f, 0.3f);
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void RevealAnswersCascade()
        {
            for (int i = 0; i < answerButtons.Count; i++)
            {
                GameObject button = answerButtons[i];
                float delay = cascadeStartDelay + (cascadeItemDelay * i);

                DOVirtual.DelayedCall(delay, () =>
                {
                    RevealButton(button);
                });
            }
        }

        private void RevealButton(GameObject button)
        {
            if (button == null) return;

            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
            RectTransform rect = button.GetComponent<RectTransform>();

            if (canvasGroup != null)
            {
                canvasGroup.DOFade(1f, revealDuration).SetEase(Ease.OutQuad);
            }

            if (rect != null)
            {
                Vector2 targetPos = rect.anchoredPosition;
                targetPos.x = 0;
                rect.DOAnchorPos(targetPos, revealDuration).SetEase(Ease.OutQuad);
            }
        }

        // ===========================
        // UTILITY
        // ===========================

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            ClearAnswers();
        }
    }
}
