using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobotsGame.Core;
using RobotsGame.Data;
using RobotsGame.Network;

namespace RobotsGame.Managers
{
    /// <summary>
    /// Central game state manager. Singleton pattern.
    /// Manages game flow, rounds, players, and multiplayer synchronization.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // ===========================
        // GAME STATE
        // ===========================
        [Header("Game Configuration")]
        [SerializeField] private GameConstants.GameMode gameMode = GameConstants.GameMode.EightRound;
        [SerializeField] private bool isDesktopMode = true; // Set based on screen width or build target

        [Header("Current Game State")]
        [SerializeField] private int currentRound = 0;
        [SerializeField] private Question currentQuestion;
        [SerializeField] private List<Player> players = new List<Player>();
        [SerializeField] private List<Answer> currentAnswers = new List<Answer>();
        [SerializeField] private List<RoundScore> currentRoundScores = new List<RoundScore>();

        [Header("Multiplayer State")]
        [SerializeField] private bool isMultiplayer = true;
        [SerializeField] private string roomCode;
        [SerializeField] private Dictionary<string, string> playerEliminationVotes = new Dictionary<string, string>();
        [SerializeField] private Dictionary<string, string> playerFinalVotes = new Dictionary<string, string>();
        [SerializeField] private VoteResults eliminationResults;
        [SerializeField] private VoteResults votingResults;

        // ===========================
        // PROPERTIES
        // ===========================
        public GameConstants.GameMode GameMode => gameMode;
        public bool IsDesktopMode => isDesktopMode;
        public bool IsMultiplayer => isMultiplayer;
        public bool IsHost => NetworkManager.Instance?.IsHost ?? false;
        public int CurrentRound => currentRound;
        public Question CurrentQuestion => currentQuestion;
        public List<Player> Players => players;
        public List<Answer> CurrentAnswers => currentAnswers;
        public List<RoundScore> CurrentRoundScores => currentRoundScores;
        public VoteResults EliminationResults => eliminationResults;
        public VoteResults VotingResults => votingResults;
        public int TotalRounds => (int)gameMode;
        public string RoomCode => roomCode;

        // ===========================
        // LIFECYCLE
        // ===========================
        private bool networkEventsSubscribed = false;
        private Coroutine networkSubscriptionCoroutine;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Determine desktop vs mobile mode based on screen width
            isDesktopMode = Screen.width > GameConstants.UI.MobileMaxWidth;
        }

        private void Start()
        {
            InitializeNetworkSubscriptions();
        }

        private void OnEnable()
        {
            // If Start has not been called yet, OnEnable might be invoked first.
            InitializeNetworkSubscriptions();
        }

        private void OnDisable()
        {
            if (!isMultiplayer)
            {
                return;
            }

            if (networkSubscriptionCoroutine != null)
            {
                StopCoroutine(networkSubscriptionCoroutine);
                networkSubscriptionCoroutine = null;
            }

            if (networkEventsSubscribed)
            {
                UnsubscribeFromNetworkEvents();
                networkEventsSubscribed = false;
            }
        }

        private void OnDestroy()
        {
            if (networkEventsSubscribed)
            {
                UnsubscribeFromNetworkEvents();
                networkEventsSubscribed = false;
            }
        }

        private void InitializeNetworkSubscriptions()
        {
            if (!isMultiplayer || networkEventsSubscribed)
            {
                return;
            }

            if (networkSubscriptionCoroutine == null)
            {
                networkSubscriptionCoroutine = StartCoroutine(WaitForNetworkManagerAndSubscribe());
            }
        }

        private System.Collections.IEnumerator WaitForNetworkManagerAndSubscribe()
        {
            while (NetworkManager.Instance == null)
            {
                yield return null;
            }

            SubscribeToNetworkEvents();
            networkEventsSubscribed = true;
            networkSubscriptionCoroutine = null;
        }

        /// <summary>
        /// Subscribe to network events for multiplayer synchronization.
        /// </summary>
        private void SubscribeToNetworkEvents()
        {
            var net = NetworkManager.Instance;
            if (net == null)
            {
                return;
            }

            net.OnRoomJoined.AddListener(HandleRoomJoined);
            net.OnPlayersUpdate.AddListener(HandlePlayersUpdate);
            net.OnRoundStarted.AddListener(HandleRoundStarted);
            net.OnPlayerAnswered.AddListener(HandlePlayerAnswered);
            net.OnAllAnswersSubmitted.AddListener(HandleAllAnswersSubmitted);
            net.OnEliminationVoteCast.AddListener(HandleEliminationVoteCast);
            net.OnEliminationComplete.AddListener(HandleEliminationComplete);
            net.OnFinalVoteCast.AddListener(HandleFinalVoteCast);
            net.OnAllVotesSubmitted.AddListener(HandleAllVotesSubmitted);
            net.OnFinalRoundScores.AddListener(HandleFinalRoundScores);
        }

        /// <summary>
        /// Unsubscribe from network events.
        /// </summary>
        private void UnsubscribeFromNetworkEvents()
        {
            var net = NetworkManager.Instance;
            if (net == null) return;

            net.OnRoomJoined.RemoveListener(HandleRoomJoined);
            net.OnPlayersUpdate.RemoveListener(HandlePlayersUpdate);
            net.OnRoundStarted.RemoveListener(HandleRoundStarted);
            net.OnPlayerAnswered.RemoveListener(HandlePlayerAnswered);
            net.OnAllAnswersSubmitted.RemoveListener(HandleAllAnswersSubmitted);
            net.OnEliminationVoteCast.RemoveListener(HandleEliminationVoteCast);
            net.OnEliminationComplete.RemoveListener(HandleEliminationComplete);
            net.OnFinalVoteCast.RemoveListener(HandleFinalVoteCast);
            net.OnAllVotesSubmitted.RemoveListener(HandleAllVotesSubmitted);
            net.OnFinalRoundScores.RemoveListener(HandleFinalRoundScores);
        }

        // ===========================
        // GAME SETUP
        // ===========================
        public void SetGameMode(GameConstants.GameMode mode)
        {
            gameMode = mode;
        }

        public void AddPlayer(Player player)
        {
            if (players.Count < GameConstants.PlayerLimits.MaxPlayers)
            {
                players.Add(player);
            }
            else
            {
                Debug.LogWarning($"Cannot add player {player.PlayerName}. Max players ({GameConstants.PlayerLimits.MaxPlayers}) reached.");
            }
        }

        public void RemovePlayer(string playerName)
        {
            players.RemoveAll(p => p.PlayerName == playerName);
        }

        public Player GetPlayer(string playerName)
        {
            return players.Find(p => p.PlayerName == playerName);
        }

        // ===========================
        // ROUND MANAGEMENT
        // ===========================
        public void StartNewGame()
        {
            currentRound = 0;
            foreach (var player in players)
            {
                player.ResetScore();
            }
            currentAnswers.Clear();
            currentRoundScores.Clear();
            currentQuestion = null;
            ClearVoteTracking();
        }

        public void StartNextRound(Question question)
        {
            currentRound++;
            currentQuestion = question;
            currentAnswers.Clear();
            currentRoundScores.Clear();
            ClearVoteTracking();

            // Initialize round scores for all players
            foreach (var player in players)
            {
                currentRoundScores.Add(new RoundScore(player.PlayerName, player.Icon));
            }

            Debug.Log($"Starting Round {currentRound}/{TotalRounds}: {question.QuestionText}");
        }

        public bool IsLastRound()
        {
            return currentRound >= TotalRounds;
        }

        // ===========================
        // ANSWER MANAGEMENT
        // ===========================
        public void SubmitAnswer(Answer answer)
        {
            // Check for duplicates (shouldn't happen due to client validation, but safety check)
            bool isDuplicate = false;
            foreach (var existing in currentAnswers)
            {
                if (AnswerValidator.IsDuplicate(answer.Text, existing.Text))
                {
                    isDuplicate = true;
                    Debug.LogWarning($"Duplicate answer rejected: {answer.Text}");
                    break;
                }
            }

            if (!isDuplicate)
            {
                currentAnswers.Add(answer);
                Debug.Log($"Answer submitted by {answer.PlayerName}: {answer.Text}");
            }
        }

        public void AddRobotAndCorrectAnswers()
        {
            if (currentQuestion == null) return;

            // Add correct answer
            currentAnswers.Add(new Answer(
                currentQuestion.CorrectAnswer,
                GameConstants.AnswerType.Correct,
                "System"
            ));

            // Add robot answer
            currentAnswers.Add(new Answer(
                currentQuestion.RobotAnswer,
                GameConstants.AnswerType.Robot,
                "Robot"
            ));
        }

        public List<Answer> GetShuffledAnswers()
        {
            List<Answer> shuffled = new List<Answer>(currentAnswers);

            // Fisher-Yates shuffle
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                Answer temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }

            return shuffled;
        }

        // ===========================
        // VOTE TRACKING (MULTIPLAYER)
        // ===========================

        /// <summary>
        /// Records a player's elimination vote.
        /// </summary>
        public void RecordEliminationVote(string playerName, string votedAnswer)
        {
            playerEliminationVotes[playerName] = votedAnswer;
            Debug.Log($"{playerName} voted to eliminate: {votedAnswer}");
        }

        /// <summary>
        /// Records a player's final vote.
        /// </summary>
        public void RecordFinalVote(string playerName, string votedAnswer)
        {
            playerFinalVotes[playerName] = votedAnswer;
            Debug.Log($"{playerName} voted for: {votedAnswer}");
        }

        /// <summary>
        /// Gets who a specific player voted for in elimination.
        /// </summary>
        public string GetPlayerEliminationVote(string playerName)
        {
            return playerEliminationVotes.ContainsKey(playerName) ? playerEliminationVotes[playerName] : null;
        }

        /// <summary>
        /// Gets who a specific player voted for in final voting.
        /// </summary>
        public string GetPlayerFinalVote(string playerName)
        {
            return playerFinalVotes.ContainsKey(playerName) ? playerFinalVotes[playerName] : null;
        }

        /// <summary>
        /// Clears vote tracking for new round.
        /// </summary>
        public void ClearVoteTracking()
        {
            playerEliminationVotes.Clear();
            playerFinalVotes.Clear();
        }

        // ===========================
        // SCORING
        // ===========================
        public void CalculateRoundScores(VoteResults eliminationVotes, VoteResults votingVotes)
        {
            // Validate vote results
            if (eliminationVotes == null || votingVotes == null)
            {
                Debug.LogError("Cannot calculate scores: Vote results are null");
                return;
            }

            var scoring = GameConstants.Scoring.GetScoring(gameMode);
            this.eliminationResults = eliminationVotes;
            this.votingResults = votingVotes;

            foreach (var roundScore in currentRoundScores)
            {
                var player = GetPlayer(roundScore.PlayerName);
                if (player == null) continue;

                // Check if player got correct answer
                var playerAnswer = currentAnswers.Find(a => a.PlayerName == roundScore.PlayerName);
                if (playerAnswer != null && AnswerValidator.IsCorrectAnswer(playerAnswer.Text, currentQuestion.CorrectAnswer))
                {
                    roundScore.SetCorrectAnswer(scoring.correct);
                }

                // Check if player identified robot (voted for robot in elimination)
                string playerElimVote = GetPlayerEliminationVote(roundScore.PlayerName);
                if (playerElimVote != null && playerElimVote == currentQuestion.RobotAnswer)
                {
                    int points = currentQuestion.IsPictureQuestion() ? scoring.robotIdentified / 2 : scoring.robotIdentified;
                    roundScore.SetRobotIdentified(points);
                }

                // Count votes received in voting phase
                if (playerAnswer != null && votingVotes != null)
                {
                    int votes = votingVotes.GetVoteCount(playerAnswer.Text);
                    for (int i = 0; i < votes; i++)
                    {
                        roundScore.AddVoteReceived(scoring.vote);
                    }
                }

                // Check if fooled by robot (voted for robot answer in voting phase)
                string playerFinalVote = GetPlayerFinalVote(roundScore.PlayerName);
                if (playerFinalVote != null && playerFinalVote == currentQuestion.RobotAnswer)
                {
                    roundScore.SetFooled(scoring.fooled); // Already negative
                }

                // Apply score to player
                player.AddScore(roundScore.Total);
            }
        }

        public List<Player> GetRankedPlayers()
        {
            List<Player> ranked = new List<Player>(players);
            ranked.Sort((a, b) => b.Score.CompareTo(a.Score)); // Descending order
            return ranked;
        }

        public Player GetWinner()
        {
            var ranked = GetRankedPlayers();
            return ranked.Count > 0 ? ranked[0] : null;
        }

        public Player GetLastPlace()
        {
            var ranked = GetRankedPlayers();
            return ranked.Count > 0 ? ranked[ranked.Count - 1] : null;
        }

        // ===========================
        // NETWORK EVENT HANDLERS
        // ===========================

        private void HandleRoomJoined(string jsonData)
        {
            var data = JsonUtility.FromJson<RoomJoinedData>(jsonData);
            roomCode = data.roomCode;
            Debug.Log($"Joined room: {roomCode}");
        }

        private void HandlePlayersUpdate(string jsonData)
        {
            var data = JsonUtility.FromJson<PlayersUpdateData>(jsonData);

            // Update players list from server
            players.Clear();
            foreach (var playerData in data.players)
            {
                players.Add(new Player(playerData.name, playerData.icon, playerData.score));
            }

            Debug.Log($"Players updated: {players.Count} players");
        }

        private void HandleRoundStarted(string jsonData)
        {
            var data = JsonUtility.FromJson<RoundStartData>(jsonData);

            var question = new Question(
                data.question.id,
                data.question.text,
                data.question.correctAnswer,
                data.question.robotAnswer,
                data.question.type == "picture" ? GameConstants.QuestionType.Picture : GameConstants.QuestionType.Text,
                data.roundNumber,
                data.question.imageUrl
            );

            StartNextRound(question);
        }

        private void HandlePlayerAnswered(string jsonData)
        {
            var data = JsonUtility.FromJson<PlayerAnsweredData>(jsonData);
            Debug.Log($"Player {data.playerName} submitted answer");
        }

        private void HandleAllAnswersSubmitted(string jsonData)
        {
            var data = JsonUtility.FromJson<AllAnswersData>(jsonData);

            // Clear and rebuild answers from server
            currentAnswers.Clear();
            foreach (var answerData in data.answers)
            {
                var answerType = answerData.type == "robot" ? GameConstants.AnswerType.Robot :
                                answerData.type == "correct" ? GameConstants.AnswerType.Correct :
                                GameConstants.AnswerType.Player;

                currentAnswers.Add(new Answer(answerData.text, answerType, answerData.playerName));
            }

            Debug.Log($"All answers submitted: {currentAnswers.Count} answers");
        }

        private void HandleEliminationVoteCast(string jsonData)
        {
            var data = JsonUtility.FromJson<VoteCastData>(jsonData);
            RecordEliminationVote(data.playerName, data.vote);
        }

        private void HandleEliminationComplete(string jsonData)
        {
            var data = JsonUtility.FromJson<EliminationCompleteData>(jsonData);

            // Populate VoteResults from tracked votes
            this.eliminationResults = new VoteResults();
            foreach (var kvp in playerEliminationVotes)
            {
                this.eliminationResults.AddVote(kvp.Value);
            }
            this.eliminationResults.CalculateElimination();

            if (!string.IsNullOrEmpty(data.eliminatedAnswer))
            {
                currentQuestion.AddEliminatedAnswer(data.eliminatedAnswer);
                Debug.Log($"Answer eliminated: {data.eliminatedAnswer}");
            }
            else if (data.tieOccurred)
            {
                Debug.Log("Elimination tie - no answer eliminated");
            }
        }

        private void HandleFinalVoteCast(string jsonData)
        {
            var data = JsonUtility.FromJson<VoteCastData>(jsonData);
            RecordFinalVote(data.playerName, data.vote);
        }

        private void HandleAllVotesSubmitted(string jsonData)
        {
            var data = JsonUtility.FromJson<VotingResultsData>(jsonData);

            // Populate VoteResults from tracked final votes
            this.votingResults = new VoteResults();
            foreach (var kvp in playerFinalVotes)
            {
                this.votingResults.AddVote(kvp.Value);
            }
            this.votingResults.CalculateElimination(); // Calculate vote counts

            // Calculate round scores now that we have all votes
            if (IsHost)
            {
                CalculateRoundScores(this.eliminationResults, this.votingResults);
                Debug.Log($"Round {currentRound} scores calculated (Host)");
            }

            Debug.Log($"All votes submitted: {playerFinalVotes.Count} votes");
        }

        private void HandleFinalRoundScores(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogWarning("HandleFinalRoundScores received empty payload.");
                return;
            }

            var data = JsonUtility.FromJson<RoundScoresData>(jsonData);
            if (data == null)
            {
                Debug.LogWarning("Failed to parse RoundScoresData from server payload.");
                return;
            }

            if (data.roundNumber > 0)
            {
                currentRound = data.roundNumber;
            }

            UpdateRoundScoresFromServer(data);
            SyncPlayersWithServerData(data);

            if (data.eliminationResults != null)
            {
                eliminationResults = BuildVoteResultsFromServer(data.eliminationResults, playerEliminationVotes, "elimination");
            }

            if (data.votingResults != null)
            {
                votingResults = BuildVoteResultsFromServer(data.votingResults, playerFinalVotes, "voting");
            }

            Debug.Log($"Round {currentRound} scores updated from server (Host: {IsHost})");

            if (!IsHost && data.scores != null && data.scores.Length > 0)
            {
                bool totalsMatch = true;
                foreach (var scoreData in data.scores)
                {
                    string playerName = !string.IsNullOrEmpty(scoreData.playerName) ? scoreData.playerName : scoreData.name;
                    if (string.IsNullOrEmpty(playerName))
                    {
                        continue;
                    }

                    var player = GetPlayer(playerName);
                    int expectedTotal = ResolveScoreTotal(scoreData);
                    if (player == null || player.Score != expectedTotal)
                    {
                        totalsMatch = false;
                        break;
                    }
                }

                Debug.Assert(totalsMatch, "Non-host did not receive synchronized round totals from server.");
                if (!totalsMatch)
                {
                    Debug.LogWarning("Non-host mismatch detected for round totals after server sync.");
                }
            }
        }

        private void UpdateRoundScoresFromServer(RoundScoresData data)
        {
            if (data.scores == null || data.scores.Length == 0)
            {
                return;
            }

            currentRoundScores.Clear();
            var scoring = GameConstants.Scoring.GetScoring(gameMode);

            foreach (var scoreEntry in data.scores)
            {
                if (scoreEntry == null)
                {
                    continue;
                }

                string playerName = !string.IsNullOrEmpty(scoreEntry.playerName) ? scoreEntry.playerName : scoreEntry.name;
                if (string.IsNullOrEmpty(playerName))
                {
                    continue;
                }

                string icon = !string.IsNullOrEmpty(scoreEntry.icon) ? scoreEntry.icon : GetPlayer(playerName)?.Icon;
                var roundScore = new RoundScore(playerName, icon ?? string.Empty);

                int correctPoints = scoreEntry.correctAnswer != 0 ? scoreEntry.correctAnswer : scoreEntry.correctAnswerPoints;
                int robotPoints = scoreEntry.robotIdentified != 0 ? scoreEntry.robotIdentified : scoreEntry.robotIdentifiedPoints;

                int votesPoints = scoreEntry.votesReceivedPoints != 0 ? scoreEntry.votesReceivedPoints : scoreEntry.votesReceived;
                int votesCount = scoreEntry.votesReceivedCount;
                if (votesCount <= 0 && votesPoints != 0 && scoring.vote > 0)
                {
                    votesCount = Mathf.Max(0, votesPoints / scoring.vote);
                }

                int fooledTotal = 0;
                if (scoreEntry.fooled != 0)
                {
                    fooledTotal = scoreEntry.fooled;
                }
                else if (scoreEntry.fooledPoints != 0)
                {
                    fooledTotal = scoreEntry.fooledPoints;
                }
                else if (scoreEntry.fooledPenalty != 0)
                {
                    fooledTotal = scoreEntry.fooledPenalty;
                }

                int totalPoints = ResolveScoreTotal(scoreEntry);

                roundScore.ApplyServerBreakdown(correctPoints, robotPoints, votesCount, votesPoints, fooledTotal, totalPoints);
                currentRoundScores.Add(roundScore);
            }
        }

        private int ResolveScoreTotal(PlayerRoundScoreData data)
        {
            if (data == null)
            {
                return 0;
            }

            if (data.total != 0)
            {
                return data.total;
            }

            if (data.totalScore != 0)
            {
                return data.totalScore;
            }

            return data.score;
        }

        private void SyncPlayersWithServerData(RoundScoresData data)
        {
            bool hasScoreData = data.scores != null && data.scores.Length > 0;
            bool hasStandings = data.standings != null && data.standings.Length > 0;

            if (!hasScoreData && !hasStandings)
            {
                return;
            }

            Dictionary<string, PlayerRoundScoreData> scoreLookup = new Dictionary<string, PlayerRoundScoreData>();
            if (hasScoreData)
            {
                foreach (var scoreEntry in data.scores)
                {
                    if (scoreEntry == null)
                    {
                        continue;
                    }

                    string name = !string.IsNullOrEmpty(scoreEntry.playerName) ? scoreEntry.playerName : scoreEntry.name;
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    scoreLookup[name] = scoreEntry;
                }
            }

            if (hasStandings)
            {
                foreach (var standing in data.standings)
                {
                    if (standing == null)
                    {
                        continue;
                    }

                    string name = !string.IsNullOrEmpty(standing.playerName) ? standing.playerName : standing.name;
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    var player = GetPlayer(name);
                    if (player == null)
                    {
                        string icon = !string.IsNullOrEmpty(standing.icon) ? standing.icon :
                                      (scoreLookup.TryGetValue(name, out var scoreEntry) && !string.IsNullOrEmpty(scoreEntry.icon) ? scoreEntry.icon : "icon1");
                        player = new Player(name, icon);
                        players.Add(player);
                    }

                    int score = standing.totalScore != 0 ? standing.totalScore :
                                standing.score != 0 ? standing.score :
                                (scoreLookup.TryGetValue(name, out var lookupScore) ? ResolveScoreTotal(lookupScore) : player.Score);

                    player.SetScore(score);
                }
            }
            else
            {
                foreach (var kvp in scoreLookup)
                {
                    var player = GetPlayer(kvp.Key);
                    if (player == null)
                    {
                        string icon = !string.IsNullOrEmpty(kvp.Value.icon) ? kvp.Value.icon : "icon1";
                        player = new Player(kvp.Key, icon);
                        players.Add(player);
                    }

                    player.SetScore(ResolveScoreTotal(kvp.Value));
                }
            }
        }

        private VoteResults BuildVoteResultsFromServer(VoteBreakdownData breakdown, Dictionary<string, string> voteTracker, string label)
        {
            if (breakdown == null)
            {
                return null;
            }

            if (breakdown.votes != null && breakdown.votes.Length > 0 && voteTracker != null)
            {
                voteTracker.Clear();
                foreach (var vote in breakdown.votes)
                {
                    if (vote == null || string.IsNullOrEmpty(vote.playerName) || string.IsNullOrEmpty(vote.answer))
                    {
                        continue;
                    }

                    voteTracker[vote.playerName] = vote.answer;
                }
            }

            var results = new VoteResults();

            bool votesAdded = false;
            if (breakdown.voteCounts != null && breakdown.voteCounts.Length > 0)
            {
                foreach (var voteCount in breakdown.voteCounts)
                {
                    if (voteCount == null || string.IsNullOrEmpty(voteCount.answer) || voteCount.count <= 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < voteCount.count; i++)
                    {
                        results.AddVote(voteCount.answer);
                    }

                    votesAdded = true;
                }
            }

            if (!votesAdded)
            {
                if (voteTracker != null && voteTracker.Count > 0)
                {
                    foreach (var vote in voteTracker.Values)
                    {
                        if (string.IsNullOrEmpty(vote))
                        {
                            continue;
                        }

                        results.AddVote(vote);
                        votesAdded = true;
                    }
                }
                else if (breakdown.votes != null && breakdown.votes.Length > 0)
                {
                    foreach (var vote in breakdown.votes)
                    {
                        if (vote == null || string.IsNullOrEmpty(vote.answer))
                        {
                            continue;
                        }

                        results.AddVote(vote.answer);
                        votesAdded = true;
                    }
                }
            }

            results.CalculateElimination();

            if (label == "elimination" && !string.IsNullOrEmpty(breakdown.eliminatedAnswer) && !breakdown.tieOccurred && results.EliminatedAnswer != breakdown.eliminatedAnswer)
            {
                Debug.LogWarning($"Server elimination results disagreed with local calculation. Server: {breakdown.eliminatedAnswer}, Local: {results.EliminatedAnswer}");
            }

            return results;
        }

        // ===========================
        // UTILITY
        // ===========================
        public void ResetGame()
        {
            currentRound = 0;
            currentQuestion = null;
            currentAnswers.Clear();
            currentRoundScores.Clear();
            players.Clear();
            ClearVoteTracking();
        }

        // ===========================
        // NETWORK DATA STRUCTURES
        // ===========================

        [System.Serializable]
        private class RoomJoinedData
        {
            public string roomCode;
        }

        [System.Serializable]
        private class PlayersUpdateData
        {
            public PlayerData[] players;
        }

        [System.Serializable]
        private class PlayerData
        {
            public string name;
            public string icon;
            public int score;
        }

        [System.Serializable]
        private class RoundStartData
        {
            public int roundNumber;
            public QuestionData question;
        }

        [System.Serializable]
        private class QuestionData
        {
            public string id;
            public string text;
            public string correctAnswer;
            public string robotAnswer;
            public string type;
            public string imageUrl;
        }

        [System.Serializable]
        private class PlayerAnsweredData
        {
            public string playerName;
        }

        [System.Serializable]
        private class AllAnswersData
        {
            public AnswerData[] answers;
        }

        [System.Serializable]
        private class AnswerData
        {
            public string text;
            public string type;
            public string playerName;
        }

        [System.Serializable]
        private class VoteCastData
        {
            public string playerName;
            public string vote;
        }

        [System.Serializable]
        private class EliminationCompleteData
        {
            public string eliminatedAnswer;
            public bool tieOccurred;
        }

        [System.Serializable]
        private class VotingResultsData
        {
            public VoteData[] votes;
        }

        [System.Serializable]
        private class VoteData
        {
            public string playerName;
            public string answer;
        }

        [System.Serializable]
        private class RoundScoresData
        {
            public int roundNumber;
            public PlayerRoundScoreData[] scores;
            public PlayerStandingData[] standings;
            public VoteBreakdownData eliminationResults;
            public VoteBreakdownData votingResults;
        }

        [System.Serializable]
        private class PlayerRoundScoreData
        {
            public string playerName;
            public string name;
            public string icon;
            public int total;
            public int totalScore;
            public int score;
            public int correctAnswer;
            public int correctAnswerPoints;
            public int robotIdentified;
            public int robotIdentifiedPoints;
            public int votesReceived;
            public int votesReceivedPoints;
            public int votesReceivedCount;
            public int fooled;
            public int fooledPoints;
            public int fooledPenalty;
        }

        [System.Serializable]
        private class PlayerStandingData
        {
            public string playerName;
            public string name;
            public string icon;
            public int totalScore;
            public int score;
            public int placement;
        }

        [System.Serializable]
        private class VoteBreakdownData
        {
            public VoteData[] votes;
            public VoteCountData[] voteCounts;
            public string eliminatedAnswer;
            public bool tieOccurred;
        }

        [System.Serializable]
        private class VoteCountData
        {
            public string answer;
            public int count;
        }
    }
}
