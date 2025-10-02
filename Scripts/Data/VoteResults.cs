using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        [SerializeField] private List<string> voterNames = new List<string>();
        [SerializeField] private List<string> voterAnswers = new List<string>();

        private Dictionary<string, int> voteCountsDict;
        private Dictionary<string, string> playerVotesDict;

        public string EliminatedAnswer => eliminatedAnswer;
        public bool TieOccurred => tieOccurred;
        public int TotalVotesCast => totalVotesCast;
        public IReadOnlyDictionary<string, int> VoteCounts
        {
            get
            {
                EnsureVoteCountsDictionary();
                return new ReadOnlyDictionary<string, int>(voteCountsDict);
            }
        }
        public IReadOnlyDictionary<string, string> PlayerVotes
        {
            get
            {
                EnsurePlayerVotesDictionary();
                return new ReadOnlyDictionary<string, string>(playerVotesDict);
            }
        }

        public VoteResults()
        {
            eliminatedAnswer = null;
            tieOccurred = false;
            totalVotesCast = 0;
            voteCountsDict = new Dictionary<string, int>();
            playerVotesDict = new Dictionary<string, string>();
        }

        public void AddVote(string answerText)
        {
            EnsureVoteCountsDictionary();

            if (!voteCountsDict.ContainsKey(answerText))
                voteCountsDict[answerText] = 0;

            voteCountsDict[answerText]++;
            RecalculateTotalVotes();
            SerializeState();
        }

        public int GetVoteCount(string answerText)
        {
            EnsureVoteCountsDictionary();

            return voteCountsDict.ContainsKey(answerText) ? voteCountsDict[answerText] : 0;
        }

        public Dictionary<string, int> GetVoteCounts()
        {
            EnsureVoteCountsDictionary();

            return new Dictionary<string, int>(voteCountsDict);
        }

        public IReadOnlyDictionary<string, string> GetPlayerVotes()
        {
            EnsurePlayerVotesDictionary();
            return new ReadOnlyDictionary<string, string>(playerVotesDict);
        }

        public void RecordPlayerVote(string playerName, string answerText)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(answerText))
            {
                return;
            }

            EnsureVoteCountsDictionary();
            EnsurePlayerVotesDictionary();

            if (playerVotesDict.TryGetValue(playerName, out var previousAnswer))
            {
                if (voteCountsDict.ContainsKey(previousAnswer))
                {
                    voteCountsDict[previousAnswer]--;
                    if (voteCountsDict[previousAnswer] <= 0)
                    {
                        voteCountsDict.Remove(previousAnswer);
                    }
                }
            }

            playerVotesDict[playerName] = answerText;

            if (!voteCountsDict.ContainsKey(answerText))
            {
                voteCountsDict[answerText] = 0;
            }

            voteCountsDict[answerText]++;
            RecalculateTotalVotes();
            SerializeState();
        }

        public void ApplyVoteCounts(Dictionary<string, int> counts)
        {
            EnsureVoteCountsDictionary();
            voteCountsDict.Clear();

            if (counts != null)
            {
                foreach (var kvp in counts)
                {
                    if (string.IsNullOrEmpty(kvp.Key))
                        continue;

                    voteCountsDict[kvp.Key] = kvp.Value;
                }
            }

            RecalculateTotalVotes();
            SerializeState();
        }

        public void ApplyPlayerVotes(Dictionary<string, string> votes)
        {
            EnsureVoteCountsDictionary();
            EnsurePlayerVotesDictionary();

            voteCountsDict.Clear();
            playerVotesDict.Clear();

            if (votes != null)
            {
                foreach (var kvp in votes)
                {
                    if (string.IsNullOrEmpty(kvp.Key) || string.IsNullOrEmpty(kvp.Value))
                        continue;

                    playerVotesDict[kvp.Key] = kvp.Value;

                    if (!voteCountsDict.ContainsKey(kvp.Value))
                    {
                        voteCountsDict[kvp.Value] = 0;
                    }

                    voteCountsDict[kvp.Value]++;
                }
            }

            RecalculateTotalVotes();
            SerializeState();
        }

        public void SetOutcome(string eliminatedAnswer, bool tieOccurred)
        {
            this.eliminatedAnswer = eliminatedAnswer;
            this.tieOccurred = tieOccurred;
        }

        public void SetTotalVotes(int totalVotes)
        {
            totalVotesCast = Mathf.Max(0, totalVotes);
        }

        public void RecalculateTotalsFromCounts()
        {
            RecalculateTotalVotes();
            SerializeState();
        }

        public void CalculateElimination()
        {
            EnsureVoteCountsDictionary();

            if (voteCountsDict == null || voteCountsDict.Count == 0)
            {
                tieOccurred = true;
                eliminatedAnswer = null;
                totalVotesCast = 0;
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

            RecalculateTotalVotes();
            SerializeState();
        }

        private void EnsureVoteCountsDictionary()
        {
            if (voteCountsDict != null)
            {
                return;
            }

            voteCountsDict = new Dictionary<string, int>();

            if (voteAnswers != null && voteAnswers.Count > 0)
            {
                for (int i = 0; i < voteAnswers.Count && i < voteCounts.Count; i++)
                {
                    string answer = voteAnswers[i];
                    if (string.IsNullOrEmpty(answer))
                        continue;

                    voteCountsDict[answer] = voteCounts[i];
                }
            }

            RecalculateTotalVotes();
        }

        private void EnsurePlayerVotesDictionary()
        {
            if (playerVotesDict != null)
            {
                return;
            }

            playerVotesDict = new Dictionary<string, string>();

            if (voterNames != null && voterNames.Count > 0 && voterNames.Count == voterAnswers.Count)
            {
                for (int i = 0; i < voterNames.Count; i++)
                {
                    string name = voterNames[i];
                    string answer = voterAnswers[i];

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(answer))
                        continue;

                    playerVotesDict[name] = answer;
                }

                EnsureVoteCountsDictionary();
                voteCountsDict.Clear();

                foreach (var kvp in playerVotesDict)
                {
                    if (!voteCountsDict.ContainsKey(kvp.Value))
                    {
                        voteCountsDict[kvp.Value] = 0;
                    }

                    voteCountsDict[kvp.Value]++;
                }

                RecalculateTotalVotes();
                SerializeState();
            }
        }

        private void SerializeState()
        {
            voteAnswers.Clear();
            voteCounts.Clear();
            voterNames.Clear();
            voterAnswers.Clear();

            if (voteCountsDict != null)
            {
                foreach (var kvp in voteCountsDict)
                {
                    voteAnswers.Add(kvp.Key);
                    voteCounts.Add(kvp.Value);
                }
            }

            if (playerVotesDict != null)
            {
                foreach (var kvp in playerVotesDict)
                {
                    voterNames.Add(kvp.Key);
                    voterAnswers.Add(kvp.Value);
                }
            }
        }

        private void RecalculateTotalVotes()
        {
            if (voteCountsDict == null)
            {
                totalVotesCast = 0;
                return;
            }

            int total = 0;
            foreach (var kvp in voteCountsDict)
            {
                total += kvp.Value;
            }

            totalVotesCast = total;
        }
    }
}
