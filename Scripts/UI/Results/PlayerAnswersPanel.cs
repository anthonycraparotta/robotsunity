using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Data;
using RobotsGame.Core;

namespace RobotsGame.UI.Results
{
    /// <summary>
    /// Panel 3: Shows all player answers with vote counts.
    /// Based on unityspec.md ResultsScreen Panel 3 specifications.
    /// </summary>
    public class PlayerAnswersPanel : ResultPanelBase
    {
        [Header("UI References")]
        [SerializeField] private Transform answerRowContainer;
        [SerializeField] private GameObject answerRowPrefab;
        [SerializeField] private TextMeshProUGUI footerText;

        [Header("Settings")]
        [SerializeField] private float rowCascadeDelay = 0.2f;
        [SerializeField] private float footerDelay = 0.5f;

        private List<GameObject> answerRows = new List<GameObject>();

        // ===========================
        // PUBLIC METHODS
        // ===========================

        public void ShowPanel(List<Answer> answers, Dictionary<string, int> voteCounts, int pointsPerVote, System.Action onComplete = null)
        {
            ClearRows();

            // Create answer rows
            foreach (var answer in answers)
            {
                if (answer.Type == GameConstants.AnswerType.Player)
                {
                    int votes = voteCounts.ContainsKey(answer.Text) ? voteCounts[answer.Text] : 0;
                    GameObject row = CreateAnswerRow(answer, votes);
                    answerRows.Add(row);
                }
            }

            // Setup footer
            if (footerText != null)
                footerText.text = $"Each vote you receive is worth +{pointsPerVote}";

            Show(onComplete);
            AnimateContent();
        }

        // ===========================
        // ROW CREATION
        // ===========================

        private GameObject CreateAnswerRow(Answer answer, int voteCount)
        {
            GameObject row;

            if (answerRowPrefab != null)
            {
                row = Instantiate(answerRowPrefab, answerRowContainer);
            }
            else
            {
                row = CreateDefaultRow();
            }

            // Set data
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = answer.Text;
                texts[1].text = $"{voteCount} vote{(voteCount != 1 ? "s" : "")}";
            }

            // Start hidden
            row.transform.localScale = Vector3.zero;
            CanvasGroup cg = row.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = row.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            return row;
        }

        private GameObject CreateDefaultRow()
        {
            GameObject row = new GameObject("AnswerRow");
            row.transform.SetParent(answerRowContainer, false);

            RectTransform rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 60);

            UnityEngine.UI.Image bg = row.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            UnityEngine.UI.HorizontalLayoutGroup layout = row.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.childForceExpandWidth = false;

            // Answer text
            GameObject answerObj = new GameObject("Answer");
            answerObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI answerText = answerObj.AddComponent<TextMeshProUGUI>();
            answerText.fontSize = 18;
            answerText.color = Color.white;

            // Vote count
            GameObject voteObj = new GameObject("Votes");
            voteObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI voteText = voteObj.AddComponent<TextMeshProUGUI>();
            voteText.fontSize = 16;
            voteText.color = GameConstants.Colors.PrimaryYellow;

            return row;
        }

        private void ClearRows()
        {
            foreach (var row in answerRows)
            {
                if (row != null)
                    Destroy(row);
            }
            answerRows.Clear();
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void AnimateContent()
        {
            // Cascade rows
            for (int i = 0; i < answerRows.Count; i++)
            {
                GameObject row = answerRows[i];
                float delay = fadeInDuration + (i * rowCascadeDelay);

                DOVirtual.DelayedCall(delay, () =>
                {
                    AnimateRowIn(row);
                });
            }

            // Footer after rows
            float footerTiming = fadeInDuration + (answerRows.Count * rowCascadeDelay) + footerDelay;
            if (footerText != null)
            {
                footerText.gameObject.SetActive(false);
                DOVirtual.DelayedCall(footerTiming, () =>
                {
                    footerText.gameObject.SetActive(true);
                    footerText.GetComponent<CanvasGroup>()?.DOFade(1f, 0.5f);
                });
            }
        }

        private void AnimateRowIn(GameObject row)
        {
            if (row == null) return;

            row.transform.DOScale(1f, 0.4f).SetEase(Ease.OutQuad);
            row.GetComponent<CanvasGroup>()?.DOFade(1f, 0.4f);
        }

        // ===========================
        // CLEANUP
        // ===========================
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearRows();
        }
    }
}
