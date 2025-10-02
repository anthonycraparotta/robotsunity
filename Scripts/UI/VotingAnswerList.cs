using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Core;
using RobotsGame.Data;
using RobotsGame.Managers;

namespace RobotsGame.UI
{
    /// <summary>
    /// Manages the answer list for voting on correct answer.
    /// Similar to EliminationAnswerList but votes FOR correct instead of eliminating.
    /// Based on unityspec.md VotingScreen specifications.
    /// </summary>
    public class VotingAnswerList : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject answerButtonPrefab;
        [SerializeField] private Transform answerContainer;
        [SerializeField] private Sprite checkIconSprite;

        [Header("Settings")]
        [SerializeField] private bool isMobile = false;
        [SerializeField] private float revealDelay = 0.5f;
        [SerializeField] private float revealDuration = 0.5f;

        [Header("Colors")]
        [SerializeField] private Color normalBgColor;
        [SerializeField] private Color selectedBgColor;
        [SerializeField] private Color correctBgColor;
        [SerializeField] private Color textColor;
        [SerializeField] private Color correctTextColor;

        private List<GameObject> answerButtons = new List<GameObject>();
        private Dictionary<string, GameObject> answerButtonMap = new Dictionary<string, GameObject>();
        private string selectedAnswer = null;
        private string ownAnswer = null;
        private bool hasVoted = false;
        private bool votingComplete = false;

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
            selectedBgColor = GameConstants.Colors.BrightGreen;
            correctBgColor = GameConstants.Colors.BrightRed;
            textColor = GameConstants.Colors.DarkBlue;
            correctTextColor = GameConstants.Colors.Cream;
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Populate answer list (all shown immediately, no cascade)
        /// </summary>
        public void SetupAnswers(List<Answer> answers, string playerOwnAnswer, bool filterOwnAnswer = false)
        {
            ClearAnswers();

            ownAnswer = playerOwnAnswer;

            // Filter
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

            // Reveal all at once (after delay)
            RevealAllAnswers();
        }

        /// <summary>
        /// Highlight correct answer after voting
        /// </summary>
        public void HighlightCorrectAnswer(string correctAnswer, bool playerGotItRight = false)
        {
            votingComplete = true;

            if (answerButtonMap.TryGetValue(correctAnswer, out GameObject buttonObj))
            {
                HighlightButton(buttonObj);
            }

            // Play success sound if player voted correctly (mobile only)
            if (isMobile && playerGotItRight)
            {
                AudioManager.Instance.PlaySuccess();
            }

            // Grey out all other answers
            foreach (var kvp in answerButtonMap)
            {
                if (kvp.Key != correctAnswer)
                {
                    GreyOutButton(kvp.Value);
                }
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
                    if (bg != null)
                        bg.color = GameConstants.Colors.Grey;
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

            // Start hidden for reveal
            CanvasGroup canvasGroup = buttonObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = buttonObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }

        private GameObject CreateDefaultAnswerButton()
        {
            GameObject buttonObj = new GameObject("AnswerButton");
            buttonObj.transform.SetParent(answerContainer, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 80);

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
            text.color = textColor;
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

                // Dynamic font size based on answer count (same as Elimination)
                float baseFontSize = GetDynamicFontSize(answerButtons.Count + 1);
                text.fontSize = baseFontSize;
            }

            // Setup button click
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                bool isOwnAnswer = !isMobile && answer.Text == ownAnswer;

                if (isOwnAnswer)
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
            // Same as EliminationAnswerList
            switch (answerCount)
            {
                case 1: return 88f;
                case 2: return 77f * 1.3f;
                case 3: return 66f * 1.2f;
                case 4: return 57f;
                case 5: return 51f;
                case 6: return 44f;
                case 7: return 40f;
                case 8: return 35f;
                default: return 31f;
            }
        }

        // ===========================
        // INTERACTION
        // ===========================

        private void OnAnswerButtonClicked(string answerText)
        {
            if (hasVoted || votingComplete) return;

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
            if (bg != null)
                bg.DOColor(normalBgColor, 0.2f);
        }

        private void SetButtonSelectedState(GameObject buttonObj)
        {
            Image bg = buttonObj.GetComponent<Image>();
            if (bg != null)
                bg.DOColor(selectedBgColor, 0.2f);
        }

        private void HighlightButton(GameObject buttonObj)
        {
            Image bg = buttonObj.GetComponent<Image>();
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            RectTransform rect = buttonObj.GetComponent<RectTransform>();

            if (bg != null)
                bg.DOColor(correctBgColor, 0.3f);

            if (text != null)
            {
                text.DOColor(correctTextColor, 0.3f);

                // Append label
                text.text += " <size=50%><i><b>(TRUE ANSWER)</b></i></size>";
            }

            if (rect != null)
                rect.DOScale(1.02f, 0.3f);

            // Add check icon (if sprite available)
            if (checkIconSprite != null)
            {
                GameObject iconObj = new GameObject("CheckIcon");
                iconObj.transform.SetParent(buttonObj.transform, false);

                Image icon = iconObj.AddComponent<Image>();
                icon.sprite = checkIconSprite;

                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(40, 40);
                iconRect.anchorMin = new Vector2(1, 0.5f);
                iconRect.anchorMax = new Vector2(1, 0.5f);
                iconRect.pivot = new Vector2(1, 0.5f);
                iconRect.anchoredPosition = new Vector2(-20, 0);
            }
        }

        private void GreyOutButton(GameObject buttonObj)
        {
            Image bg = buttonObj.GetComponent<Image>();
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            CanvasGroup cg = buttonObj.GetComponent<CanvasGroup>();

            if (bg != null)
                bg.color = new Color(0.97f, 0.98f, 0.98f, 0.7f);

            if (text != null)
                text.color = new Color(0.4f, 0.4f, 0.4f);

            if (cg != null)
                cg.alpha = 0.8f;
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void RevealAllAnswers()
        {
            DOVirtual.DelayedCall(revealDelay, () =>
            {
                foreach (var button in answerButtons)
                {
                    RevealButton(button);
                }
            });
        }

        private void RevealButton(GameObject button)
        {
            if (button == null) return;

            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();

            if (canvasGroup != null)
            {
                canvasGroup.DOFade(1f, revealDuration).SetEase(Ease.OutQuad);
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
