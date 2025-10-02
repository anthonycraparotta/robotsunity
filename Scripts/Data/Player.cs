using System;
using UnityEngine;

namespace RobotsGame.Data
{
    /// <summary>
    /// Represents a player in the game.
    /// Based on unityspec.md DATA STRUCTURES section.
    /// </summary>
    [Serializable]
    public class Player
    {
        [SerializeField] private string playerName;
        [SerializeField] private string icon; // icon1 through icon20
        [SerializeField] private int score;
        [SerializeField] private bool isDesktop;

        public string PlayerName => playerName;
        public string Icon => icon;
        public int Score => score;
        public bool IsDesktop => isDesktop;

        public Player(string playerName, string icon, bool isDesktop = false)
        {
            this.playerName = playerName;
            this.icon = icon;
            this.score = 0;
            this.isDesktop = isDesktop;
        }

        public void AddScore(int points)
        {
            score += points;
        }

        public void SetScore(int newScore)
        {
            score = newScore;
        }

        public void ResetScore()
        {
            score = 0;
        }
    }
}
