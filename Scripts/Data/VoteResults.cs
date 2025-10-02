using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobotsGame.Data
{
    /// <summary>
    /// Represents voting results for elimination or voting phase.
    /// Based on unityspec.md DATA STRUCTURES section.
    /// </summary>
    [Serializable]
    public class VoteResults
    {
        [SerializeField] private string eliminatedAnswer;
        [SerializeField] private bool tieOccurred;
        [SerializeField] private int totalVotesCast;

        // Dictionary stored as two parallel lists for serialization
        [SerializeField] private List<string> voteAnswers = new List<string>();
        [SerializeField] private List<int> voteCounts = new List<int>();

        private Dictionary<string, int> voteCountsDict;

        public string EliminatedAnswer => eliminatedAnswer;
        public bool TieOccurred => tieOccurred;
        public int TotalVotesCast => totalVotesCast;

        public VoteResults()
        {
            eliminatedAnswer = null;
            tieOccurred = false;
            totalVotesCast = 0;
            voteCountsDict = new Dictionary<string, int>();
        }

        public void AddVote(string answerText)
        {
            if (voteCountsDict == null)
                voteCountsDict = new Dictionary<string, int>();

            if (!voteCountsDict.ContainsKey(answerText))
                voteCountsDict[answerText] = 0;

            voteCountsDict[answerText]++;
            totalVotesCast++;
        }

        public int GetVoteCount(string answerText)
        {
            if (voteCountsDict == null)
                BuildDictionaryFromLists();

            return voteCountsDict.ContainsKey(answerText) ? voteCountsDict[answerText] : 0;
        }

        public Dictionary<string, int> GetVoteCounts()
        {
            if (voteCountsDict == null || voteCountsDict.Count == 0)
                BuildDictionaryFromLists();

            return new Dictionary<string, int>(voteCountsDict);
        }

        public void CalculateElimination()
        {
            if (voteCountsDict == null || voteCountsDict.Count == 0)
            {
                tieOccurred = true;
                eliminatedAnswer = null;
                return;
            }

            int maxVotes = 0;
            List<string> topAnswers = new List<string>();

            foreach (var kvp in voteCountsDict)
            {
                if (kvp.Value > maxVotes)
                {
                    maxVotes = kvp.Value;
                    topAnswers.Clear();
                    topAnswers.Add(kvp.Key);
                }
                else if (kvp.Value == maxVotes)
                {
                    topAnswers.Add(kvp.Key);
                }
            }

            if (topAnswers.Count > 1)
            {
                tieOccurred = true;
                eliminatedAnswer = null;
            }
            else
            {
                tieOccurred = false;
                eliminatedAnswer = topAnswers.Count > 0 ? topAnswers[0] : null;
            }

            // Prepare for serialization
            SerializeDictionary();
        }

        private void SerializeDictionary()
        {
            voteAnswers.Clear();
            voteCounts.Clear();

            if (voteCountsDict != null)
            {
                foreach (var kvp in voteCountsDict)
                {
                    voteAnswers.Add(kvp.Key);
                    voteCounts.Add(kvp.Value);
                }
            }
        }

        private void BuildDictionaryFromLists()
        {
            voteCountsDict = new Dictionary<string, int>();

            for (int i = 0; i < voteAnswers.Count && i < voteCounts.Count; i++)
            {
                voteCountsDict[voteAnswers[i]] = voteCounts[i];
            }
        }
    }
}
