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
        public bool IsMultiplayer => true; // Game is always multiplayer (2-8 players)
        public bool IsHost => NetworkManager.Instance?.IsHost ?? false;
        public int CurrentRound => currentRound;
        public Question CurrentQuestion => currentQuestion;
        public List<Player> Players => players;
        public List<Answer> CurrentAnswers => currentAnswers;
        public int TotalRounds => (int)gameMode;
        public string RoomCode => roomCode;
        public IReadOnlyList<RoundScore> CurrentRoundScores => currentRoundScores;
        public VoteResults EliminationResults => eliminationResults;
        public VoteResults VotingResults => votingResults;

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

            if (eliminationResults == null)
            {
                eliminationResults = new VoteResults();
            }

            if (votingResults == null)
            {
                votingResults = new VoteResults();
            }
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
            if (networkEventsSubscribed)
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
            // Ensure no stale vote data leaks into the new round
            ClearVoteTracking();

            currentRound++;
            currentQuestion = question;
            currentQuestion?.ClearEliminatedAnswers();
            currentAnswers.Clear();
            currentRoundScores.Clear();
            eliminationResults = new VoteResults();
            votingResults = new VoteResults();

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

        private VoteResults BuildVoteResultsFromTrackedVotes(Dictionary<string, string> trackedVotes)
        {
            var results = new VoteResults();

            if (trackedVotes != null)
            {
                foreach (var kvp in trackedVotes)
                {
                    results.RecordPlayerVote(kvp.Key, kvp.Value);
                }
            }

            results.CalculateElimination();
            return results;
        }

        // ===========================
        // SCORING
        // ===========================
        public void CalculateRoundScores(VoteResults eliminationVotes = null, VoteResults votingVotes = null)
        {
            var scoring = GameConstants.Scoring.GetScoring(gameMode);
            this.eliminationResults = eliminationVotes ?? BuildVoteResultsFromTrackedVotes(playerEliminationVotes);
            this.votingResults = votingVotes ?? BuildVoteResultsFromTrackedVotes(playerFinalVotes);

            if (this.eliminationResults == null)
            {
                this.eliminationResults = new VoteResults();
            }

            if (this.votingResults == null)
            {
                this.votingResults = new VoteResults();
            }

            if (currentRoundScores.Count == 0)
            {
                foreach (var player in players)
                {
                    currentRoundScores.Add(new RoundScore(player.PlayerName, player.Icon));
                }
            }

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
            if (data == null)
            {
                data = new EliminationCompleteData();
            }

            // Populate VoteResults from tracked votes
            this.eliminationResults = BuildVoteResultsFromTrackedVotes(playerEliminationVotes);

            if (data.voteCounts != null && data.voteCounts.Length > 0)
            {
                this.eliminationResults.ApplyVoteCounts(ConvertVoteCounts(data.voteCounts));
            }

            if (data.votes != null && data.votes.Length > 0)
            {
                var playerVotes = ConvertVotesToDictionary(data.votes);
                if (playerVotes.Count > 0)
                {
                    this.eliminationResults.ApplyPlayerVotes(playerVotes);
                    SyncTrackedVotesDictionary(this.eliminationResults, playerEliminationVotes);
                }
            }
            else
            {
                SyncTrackedVotesDictionary(this.eliminationResults, playerEliminationVotes);
            }

            if (data.totalVotesCast > 0)
            {
                this.eliminationResults.SetTotalVotes(data.totalVotesCast);
            }

            if (!string.IsNullOrEmpty(data.eliminatedAnswer))
            {
                this.eliminationResults.SetOutcome(data.eliminatedAnswer, false);
                currentQuestion.AddEliminatedAnswer(data.eliminatedAnswer);
                Debug.Log($"Answer eliminated: {data.eliminatedAnswer}");
            }
            else if (data.tieOccurred)
            {
                this.eliminationResults.SetOutcome(null, true);
                Debug.Log("Elimination tie - no answer eliminated");
            }
            else
            {
                this.eliminationResults.CalculateElimination();
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
            if (data == null)
            {
                data = new VotingResultsData();
            }

            // Populate VoteResults from tracked final votes
            this.votingResults = BuildVoteResultsFromTrackedVotes(playerFinalVotes);

            if (data.voteCounts != null && data.voteCounts.Length > 0)
            {
                this.votingResults.ApplyVoteCounts(ConvertVoteCounts(data.voteCounts));
            }

            if (data.votes != null && data.votes.Length > 0)
            {
                var playerVotes = ConvertVotesToDictionary(data.votes);
                if (playerVotes.Count > 0)
                {
                    this.votingResults.ApplyPlayerVotes(playerVotes);
                    SyncTrackedVotesDictionary(this.votingResults, playerFinalVotes);
                }
            }
            else
            {
                SyncTrackedVotesDictionary(this.votingResults, playerFinalVotes);
            }

            if (data.totalVotesCast > 0)
            {
                this.votingResults.SetTotalVotes(data.totalVotesCast);
            }

            if (!string.IsNullOrEmpty(data.correctAnswer))
            {
                this.votingResults.SetOutcome(data.correctAnswer, false);
            }
            else
            {
                this.votingResults.CalculateElimination();
            }

            // Calculate round scores now that we have all votes
            if (IsHost)
            {
                CalculateRoundScores(this.eliminationResults, this.votingResults);
                Debug.Log($"Round {currentRound} scores calculated (Host)");

                // Broadcast updated player scores to all clients
                NetworkManager.Instance?.BroadcastPlayerScores(players, currentRound);
            }

            Debug.Log($"All votes submitted: {playerFinalVotes.Count} votes");
        }

        private void HandleFinalRoundScores(string jsonData)
        {
            var data = JsonUtility.FromJson<RoundScoresData>(jsonData);
            if (data == null)
            {
                Debug.LogWarning("HandleFinalRoundScores: Received null data from server");
                return;
            }

            if (data.roundNumber > 0)
            {
                currentRound = data.roundNumber;
            }

            if (data.scores != null && data.scores.Length > 0)
            {
                currentRoundScores.Clear();

                var scoring = GameConstants.Scoring.GetScoring(gameMode);
                foreach (var scoreEntry in data.scores)
                {
                    if (scoreEntry == null || string.IsNullOrEmpty(scoreEntry.playerName))
                        continue;

                    var player = GetPlayer(scoreEntry.playerName);
                    string icon = player?.Icon ?? scoreEntry.icon ?? string.Empty;
                    var roundScore = new RoundScore(scoreEntry.playerName, icon);

                    int votesCount = scoreEntry.votesReceivedCount;
                    if (votesCount == 0 && scoreEntry.votesReceivedPoints != 0)
                    {
                        int perVote = scoring.vote;
                        if (perVote != 0)
                        {
                            votesCount = scoreEntry.votesReceivedPoints / perVote;
                        }
                    }

                    roundScore.ApplyServerBreakdown(
                        scoreEntry.correctAnswerPoints,
                        scoreEntry.robotIdentifiedPoints,
                        votesCount,
                        scoreEntry.votesReceivedPoints,
                        scoreEntry.fooledPoints,
                        scoreEntry.total
                    );

                    currentRoundScores.Add(roundScore);
                }
            }

            if (data.standings != null && data.standings.Length > 0)
            {
                foreach (var standing in data.standings)
                {
                    if (standing == null || string.IsNullOrEmpty(standing.playerName))
                        continue;

                    var player = GetPlayer(standing.playerName);
                    if (player != null)
                    {
                        player.SetScore(standing.totalScore);
                    }
                }
            }

            if (data.eliminationResults != null)
            {
                var results = BuildVoteResultsFromPayload(data.eliminationResults);
                if (results != null)
                {
                    eliminationResults = results;
                    SyncTrackedVotesDictionary(eliminationResults, playerEliminationVotes);
                }
            }

            if (data.votingResults != null)
            {
                var results = BuildVoteResultsFromPayload(data.votingResults);
                if (results != null)
                {
                    votingResults = results;
                    SyncTrackedVotesDictionary(votingResults, playerFinalVotes);
                }
            }

            if (data.playerAnswers != null && data.playerAnswers.Length > 0)
            {
                currentAnswers.Clear();
                foreach (var answerData in data.playerAnswers)
                {
                    if (answerData == null || string.IsNullOrEmpty(answerData.text))
                        continue;

                    var answerType = answerData.type == "robot" ? GameConstants.AnswerType.Robot :
                                     answerData.type == "correct" ? GameConstants.AnswerType.Correct :
                                     GameConstants.AnswerType.Player;

                    currentAnswers.Add(new Answer(answerData.text, answerType, answerData.playerName));
                }
            }

            Debug.Log($"Round {currentRound} scores updated from server");
        }

        // ===========================
        // UTILITY
        // ===========================
        private Dictionary<string, int> ConvertVoteCounts(VoteCountData[] voteCountsData)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();

            if (voteCountsData == null)
            {
                return counts;
            }

            foreach (var entry in voteCountsData)
            {
                if (entry == null || string.IsNullOrEmpty(entry.answer))
                    continue;

                counts[entry.answer] = entry.count;
            }

            return counts;
        }

        private Dictionary<string, string> ConvertVotesToDictionary(VoteData[] votes)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            if (votes == null)
            {
                return results;
            }

            foreach (var vote in votes)
            {
                if (vote == null || string.IsNullOrEmpty(vote.playerName) || string.IsNullOrEmpty(vote.answer))
                    continue;

                results[vote.playerName] = vote.answer;
            }

            return results;
        }

        private VoteResults BuildVoteResultsFromPayload(VoteResultsPayload payload)
        {
            if (payload == null)
            {
                return null;
            }

            var results = new VoteResults();

            var playerVotes = ConvertVotesToDictionary(payload.votes);
            if (playerVotes.Count > 0)
            {
                results.ApplyPlayerVotes(playerVotes);
            }
            else if (payload.voteCounts != null && payload.voteCounts.Length > 0)
            {
                results.ApplyVoteCounts(ConvertVoteCounts(payload.voteCounts));
            }

            if (payload.totalVotesCast > 0)
            {
                results.SetTotalVotes(payload.totalVotesCast);
            }
            else
            {
                results.RecalculateTotalsFromCounts();
            }

            if (!string.IsNullOrEmpty(payload.eliminatedAnswer) || payload.tieOccurred)
            {
                results.SetOutcome(payload.eliminatedAnswer, payload.tieOccurred);
            }
            else
            {
                results.CalculateElimination();
            }

            return results;
        }

        private void SyncTrackedVotesDictionary(VoteResults results, Dictionary<string, string> target)
        {
            if (results == null || target == null)
            {
                return;
            }

            target.Clear();
            foreach (var kvp in results.GetPlayerVotes())
            {
                target[kvp.Key] = kvp.Value;
            }
        }

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
            public VoteCountData[] voteCounts;
            public int totalVotesCast;
            public VoteData[] votes;
        }

        [System.Serializable]
        private class VotingResultsData
        {
            public VoteData[] votes;
            public VoteCountData[] voteCounts;
            public string correctAnswer;
            public string robotAnswer;
            public int totalVotesCast;
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
            public RoundScoreEntryData[] scores;
            public PlayerStandingData[] standings;
            public VoteResultsPayload eliminationResults;
            public VoteResultsPayload votingResults;
            public AnswerData[] playerAnswers;
        }

        [System.Serializable]
        private class RoundScoreEntryData
        {
            public string playerName;
            public int total;
            public int correctAnswerPoints;
            public int robotIdentifiedPoints;
            public int votesReceivedPoints;
            public int votesReceivedCount;
            public int fooledPoints;
            public string icon;
        }

        [System.Serializable]
        private class PlayerStandingData
        {
            public string playerName;
            public int totalScore;
            public int placement;
        }

        [System.Serializable]
        private class VoteResultsPayload
        {
            public VoteCountData[] voteCounts;
            public VoteData[] votes;
            public string eliminatedAnswer;
            public bool tieOccurred;
            public int totalVotesCast;
        }

        [System.Serializable]
        private class VoteCountData
        {
            public string answer;
            public int count;
        }
    }
}
