using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Data;
using RobotsGame.Managers;
using RobotsGame.Core;

namespace RobotsGame.UI
{
    /// <summary>
    /// Desktop-only overlay showing received answers with cascade animation.
    /// Based on unityspec.md QuestionScreen - Answers Overlay specifications.
    /// </summary>
    public class AnswersOverlay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject answerItemPrefab;
        [SerializeField] private Transform answerContainer;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float showDelay = 2f; // 2000ms after all answers received
        [SerializeField] private float cascadeDelay = 0.2f; // 200ms between items
        [SerializeField] private float itemRevealDuration = 0.5f;

        private List<GameObject> answerItems = new List<GameObject>();
        private bool isVisible = false;

        // ===========================
        // LIFECYCLE
        // ===========================
        private void Awake()
        {
            if (answerContainer == null)
                answerContainer = transform;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            if (headerText == null)
                headerText = GetComponentInChildren<TextMeshProUGUI>();

            // Start hidden
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        // ===========================
        // PUBLIC METHODS
        // ===========================

        /// <summary>
        /// Show the overlay with answers
        /// </summary>
        public void ShowOverlay(List<Answer> answers)
        {
            if (isVisible) return;

            // Clear existing items
            ClearAnswers();

            // Create answer items
            foreach (var answer in answers)
            {
                CreateAnswerItem(answer);
            }

            // Show with delay
            DOVirtual.DelayedCall(showDelay, () =>
            {
                gameObject.SetActive(true);
                isVisible = true;

                // Play swoosh sound
                if (AudioManager.TryGetInstance(out var audioManager))
                {
                    audioManager.PlayResponsesSwoosh();
                }

                // Fade in overlay background
                canvasGroup.DOFade(1f, 0.3f);

                // Cascade reveal items
                RevealItemsCascade();
            });
        }

        /// <summary>
        /// Hide the overlay
        /// </summary>
        public void HideOverlay()
        {
            if (!isVisible) return;

            canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                gameObject.SetActive(false);
                isVisible = false;
                ClearAnswers();
            });
        }

        /// <summary>
        /// Clear all answer items
        /// </summary>
        public void ClearAnswers()
        {
            foreach (var item in answerItems)
            {
                if (item != null)
                    Destroy(item);
            }

            answerItems.Clear();
        }

        // ===========================
        // ANSWER ITEM CREATION
        // ===========================

        private void CreateAnswerItem(Answer answer)
        {
            GameObject itemObj;

            if (answerItemPrefab != null)
            {
                itemObj = Instantiate(answerItemPrefab, answerContainer);
            }
            else
            {
                itemObj = CreateDefaultAnswerItem();
            }

            // Setup item data
            SetupAnswerItem(itemObj, answer);

            // Store reference
            answerItems.Add(itemObj);

            // Start hidden (for cascade animation)
            CanvasGroup itemCanvasGroup = itemObj.GetComponent<CanvasGroup>();
            if (itemCanvasGroup == null)
                itemCanvasGroup = itemObj.AddComponent<CanvasGroup>();

            itemCanvasGroup.alpha = 0f;

            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                Vector2 pos = itemRect.anchoredPosition;
                pos.x = -100f; // Start off to the left
                itemRect.anchoredPosition = pos;
            }
        }

        private GameObject CreateDefaultAnswerItem()
        {
            GameObject itemObj = new GameObject("AnswerItem");
            itemObj.transform.SetParent(answerContainer, false);

            // Add RectTransform
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 60); // Width auto, height 60

            // Add background image
            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(11f/255f, 52f/255f, 73f/255f, 0.65f);

            // Add layout group for text
            VerticalLayoutGroup layout = itemObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 10, 10);
            layout.spacing = 5;
            layout.childForceExpandHeight = false;

            // Player name text
            GameObject nameObj = new GameObject("PlayerName");
            nameObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.fontSize = 12;
            nameText.color = new Color(1f, 1f, 1f, 0.75f);
            nameText.fontStyle = FontStyles.Bold;
            nameText.textWrappingMode = TextWrappingModes.NoWrap;

            // Answer text
            GameObject answerObj = new GameObject("AnswerText");
            answerObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI answerText = answerObj.AddComponent<TextMeshProUGUI>();
            answerText.fontSize = 16;
            answerText.color = Color.white;
            answerText.textWrappingMode = TextWrappingModes.Normal;

            return itemObj;
        }

        private void SetupAnswerItem(GameObject itemObj, Answer answer)
        {
            TextMeshProUGUI[] texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 2)
            {
                // Player name
                texts[0].text = answer.PlayerName.ToUpper();

                // Answer text
                texts[1].text = answer.Text;
            }
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void RevealItemsCascade()
        {
            for (int i = 0; i < answerItems.Count; i++)
            {
                GameObject item = answerItems[i];
                if (item == null) continue;

                float delay = cascadeDelay * i;

                DOVirtual.DelayedCall(delay, () =>
                {
                    RevealItem(item);
                });
            }
        }

        private void RevealItem(GameObject item)
        {
            if (item == null) return;

            CanvasGroup itemCanvasGroup = item.GetComponent<CanvasGroup>();
            RectTransform itemRect = item.GetComponent<RectTransform>();

            if (itemCanvasGroup != null)
            {
                itemCanvasGroup.DOFade(1f, itemRevealDuration).SetEase(Ease.OutQuad);
            }

            if (itemRect != null)
            {
                // Slide in from left
                Vector2 targetPos = itemRect.anchoredPosition;
                targetPos.x = 0;
                itemRect.DOAnchorPos(targetPos, itemRevealDuration).SetEase(Ease.OutQuad);
            }
        }

        // ===========================
        // CLEANUP
        // ===========================
        private void OnDestroy()
        {
            ClearAnswers();
            DOTween.Kill(canvasGroup);
        }
    }
}
