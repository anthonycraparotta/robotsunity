using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using RobotsGame.Core;
using RobotsGame.Data;

namespace RobotsGame.UI.Results
{
    /// <summary>
    /// Panel 4: Shows ranked standings by score.
    /// Based on unityspec.md ResultsScreen Panel 4 specifications.
    /// </summary>
    public class StandingsPanel : ResultPanelBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private Transform standingRowContainer;
        [SerializeField] private GameObject standingRowPrefab;

        [Header("Settings")]
        [SerializeField] private float rowCascadeDelay = 0.2f;
        [SerializeField] private float firstPlaceHighlightDelay = 2f;
        [SerializeField] private float lastPlaceHighlightDelay = 4f;

        private List<GameObject> standingRows = new List<GameObject>();

        // ===========================
        // PUBLIC METHODS
        // ===========================

        public void ShowPanel(List<Player> rankedPlayers, System.Action onComplete = null)
        {
            ClearRows();

            if (headerText != null)
                headerText.text = "ROUND STANDINGS";

            // Create standing rows
            for (int i = 0; i < rankedPlayers.Count; i++)
            {
                GameObject row = CreateStandingRow(rankedPlayers[i], i + 1);
                standingRows.Add(row);
            }

            Show(onComplete);
            AnimateContent(rankedPlayers.Count);
        }

        // ===========================
        // ROW CREATION
        // ===========================

        private GameObject CreateStandingRow(Player player, int placement)
        {
            GameObject row;

            if (standingRowPrefab != null)
            {
                row = Instantiate(standingRowPrefab, standingRowContainer);
            }
            else
            {
                row = CreateDefaultRow();
            }

            // Set data
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                texts[0].text = GetPlacementText(placement);
                texts[1].text = player.PlayerName;
                texts[2].text = player.Score.ToString();
                texts[2].color = player.Score >= 0 ? GameConstants.Colors.BrightGreen : GameConstants.Colors.BrightRed;
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
            GameObject row = new GameObject("StandingRow");
            row.transform.SetParent(standingRowContainer, false);

            RectTransform rect = row.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 70);

            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.55f);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.spacing = 15;
            layout.childForceExpandWidth = false;

            // Placement
            GameObject placementObj = new GameObject("Placement");
            placementObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI placementText = placementObj.AddComponent<TextMeshProUGUI>();
            placementText.fontSize = 20;
            placementText.fontStyle = FontStyles.Bold;
            placementText.color = GameConstants.Colors.BrightGreen;

            // Player name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.fontSize = 18;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = GameConstants.Colors.Cream;

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.fontSize = 18;
            scoreText.fontStyle = FontStyles.Bold;

            return row;
        }

        private string GetPlacementText(int placement)
        {
            string suffix = "th";
            if (placement == 1) suffix = "st";
            else if (placement == 2) suffix = "nd";
            else if (placement == 3) suffix = "rd";

            return $"{placement}{suffix}";
        }

        private void ClearRows()
        {
            foreach (var row in standingRows)
            {
                if (row != null)
                    Destroy(row);
            }
            standingRows.Clear();
        }

        // ===========================
        // ANIMATION
        // ===========================

        private void AnimateContent(int playerCount)
        {
            // Cascade rows
            for (int i = 0; i < standingRows.Count; i++)
            {
                GameObject row = standingRows[i];
                float delay = fadeInDuration + (i * rowCascadeDelay);

                DOVirtual.DelayedCall(delay, () =>
                {
                    AnimateRowIn(row);
                });
            }

            // Highlight first place
            if (standingRows.Count > 0)
            {
                DOVirtual.DelayedCall(fadeInDuration + (playerCount * rowCascadeDelay) + firstPlaceHighlightDelay, () =>
                {
                    HighlightRow(standingRows[0], GameConstants.Colors.PrimaryYellow);
                });
            }

            // Highlight last place
            if (standingRows.Count > 1)
            {
                DOVirtual.DelayedCall(fadeInDuration + (playerCount * rowCascadeDelay) + lastPlaceHighlightDelay, () =>
                {
                    HighlightRow(standingRows[standingRows.Count - 1], GameConstants.Colors.BrightRed);
                });
            }
        }

        private void AnimateRowIn(GameObject row)
        {
            if (row == null) return;

            row.transform.DOScale(1f, 0.4f).SetEase(Ease.OutQuad);
            row.GetComponent<CanvasGroup>()?.DOFade(1f, 0.4f);
        }

        private void HighlightRow(GameObject row, Color glowColor)
        {
            if (row == null) return;

            Image bg = row.GetComponent<Image>();
            if (bg != null)
            {
                Outline outline = row.GetComponent<Outline>();
                if (outline == null)
                    outline = row.AddComponent<Outline>();

                outline.effectColor = glowColor;
                outline.effectDistance = new Vector2(5, 5);
                outline.DOFade(1f, 0.3f);
            }
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
