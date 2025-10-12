using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Collections;

/// <summary>
/// Unity Netcode-based GameManager using NetworkVariables and NetworkLists
/// Follows Unity Netcode best practices for state synchronization
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class GameManager : NetworkBehaviour
{
    // Singleton pattern - only one GameManager exists
    public static GameManager Instance;

    // === CONFIGURATION ===
    [Header("Game Configuration")]
    public NetworkVariable<GameMode> gameMode = new NetworkVariable<GameMode>(GameMode.EightQuestions);

    // === GAME STATE === (Synced across network)
    public NetworkVariable<int> currentRound = new NetworkVariable<int>(0);
    public NetworkVariable<bool> isHalftimePlayed = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isBonusRoundPlayed = new NetworkVariable<bool>(false);
    public NetworkVariable<GameState> currentGameState = new NetworkVariable<GameState>(GameState.Loading);

    // === PLAYER DATA ===
    [Header("Player Data")]
    public NetworkList<NetworkedPlayerData> networkPlayers; // Replaces Dictionary

    // Temporary dictionaries for compatibility during transition (server-only)
    private Dictionary<string, string> currentRoundAnswers = new Dictionary<string, string>();
    private Dictionary<string, string> eliminationVotes = new Dictionary<string, string>();
    private Dictionary<string, string> votingVotes = new Dictionary<string, string>();
    private readonly Dictionary<string, string> bonusVotes = new Dictionary<string, string>();

    // === ROUND DATA ===
    [Header("Current Round Data")]
    public Question currentQuestion;
    public NetworkVariable<FixedString128Bytes> robotAnswer = new NetworkVariable<FixedString128Bytes>();
    public NetworkVariable<FixedString128Bytes> correctAnswer = new NetworkVariable<FixedString128Bytes>();
    public NetworkList<FixedString128Bytes> allAnswers; // For Elimination
    public NetworkList<FixedString128Bytes> remainingAnswers; // For Voting
    public NetworkVariable<FixedString128Bytes> eliminatedAnswer = new NetworkVariable<FixedString128Bytes>();

    // === BONUS ROUND DATA ===
    [Header("Bonus Round Data")]
    public BonusQuestion bonusQuestions;
    public NetworkVariable<int> currentBonusQuestion = new NetworkVariable<int>(0);

    // === TIMER STATE ===
    [Header("Timer Configuration")]
    public float questionTimer = 60f;
    public float eliminationTimer = 30f;
    public float votingTimer = 30f;
    public NetworkVariable<float> currentTimerValue = new NetworkVariable<float>(0f);
    public NetworkVariable<bool> timerActive = new NetworkVariable<bool>(false);

    // === QUESTION DATA ===
    [Header("Question Database")]
    public List<Question> standardQuestions = new List<Question>();
    public List<Question> playerQuestions = new List<Question>();
    public List<Question> pictureQuestions = new List<Question>();
    private int standardQuestionIndex = 0;
    private int playerQuestionIndex = 0;
    private int pictureQuestionIndex = 0;

    // === ENUMS ===
    public enum GameMode
    {
        EightQuestions,
        TwelveQuestions
    }

    public enum GameState
    {
        Loading,
        Landing,
        Lobby,
        IntroVideo,
        RoundArt,
        Question,
        Elimination,
        Voting,
        RoundResults,
        Halftime,
        BonusIntro,
        BonusQuestion,
        BonusResults,
        FinalResults,
        Credits
    }

    private NetworkObject _networkObject;

    void Awake()
    {
        // Initialize NetworkLists before NetworkObject spawns
        networkPlayers = new NetworkList<NetworkedPlayerData>();
        allAnswers = new NetworkList<FixedString128Bytes>();
        remainingAnswers = new NetworkList<FixedString128Bytes>();

        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Instance created");

            _networkObject = GetComponent<NetworkObject>();

            TrySpawnNetworkObject();

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            }
        }
        else
        {
            Debug.Log("[GameManager] Duplicate found and destroyed");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        }
    }

    private void HandleServerStarted()
    {
        TrySpawnNetworkObject();
    }

    private void TrySpawnNetworkObject()
    {
        if (_networkObject == null)
        {
            _networkObject = GetComponent<NetworkObject>();
        }

        if (_networkObject == null)
        {
            _networkObject = gameObject.AddComponent<NetworkObject>();
            Debug.LogWarning("[GameManager] NetworkObject component was missing and has been added at runtime.");
        }

        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (!_networkObject.IsSpawned)
        {
            _networkObject.Spawn();
            Debug.Log("[GameManager] NetworkObject spawned by host");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"[GameManager] NetworkSpawn - IsServer: {IsServer}, IsClient: {IsClient}");

        // Subscribe to NetworkVariable changes for client-side reactions
        if (IsClient && !IsServer)
        {
            currentGameState.OnValueChanged += OnGameStateChanged;
            currentRound.OnValueChanged += OnRoundChanged;
            timerActive.OnValueChanged += OnTimerActiveChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsClient && !IsServer)
        {
            currentGameState.OnValueChanged -= OnGameStateChanged;
            currentRound.OnValueChanged -= OnRoundChanged;
            timerActive.OnValueChanged -= OnTimerActiveChanged;
        }
    }

    void Start()
    {
        LoadQuestions();
    }

    void Update()
    {
        // Only server updates timer
        if (!IsServer) return;

        // Handle timer countdown
        if (timerActive.Value && currentTimerValue.Value > 0)
        {
            currentTimerValue.Value -= Time.deltaTime;

            if (currentTimerValue.Value <= 0)
            {
                currentTimerValue.Value = 0;
                timerActive.Value = false;
                OnTimerExpired();
            }
        }
    }

    // === NETWORK VARIABLE CALLBACKS ===

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($"[GameManager] State changed: {oldState} → {newState}");
    }

    private void OnRoundChanged(int oldRound, int newRound)
    {
        Debug.Log($"[GameManager] Round changed: {oldRound} → {newRound}");
    }

    private void OnTimerActiveChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[GameManager] Timer active: {oldValue} → {newValue}");
    }

    // === GAME FLOW METHODS ===

    public void StartGame(GameMode mode)
    {
        if (!IsServer) return;

        gameMode.Value = mode;
        currentRound.Value = 0;
        isHalftimePlayed.Value = false;
        isBonusRoundPlayed.Value = false;

        // Reset all player scores
        for (int i = 0; i < networkPlayers.Count; i++)
        {
            var player = networkPlayers[i];
            player.scorePercentage = 0;
            networkPlayers[i] = player;
        }

        LoadScene("IntroVideoScreen");
        currentGameState.Value = GameState.IntroVideo;
    }

    public void AdvanceToNextScreen()
    {
        if (!IsServer) return;

        switch (currentGameState.Value)
        {
            case GameState.Loading:
                // Check device type - mobile skips IntroVideo and Landing, goes directly to join flow
                if (DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile())
                {
                    LoadScene("JoinRoomScreen");
                    currentGameState.Value = GameState.Lobby;
                }
                else
                {
                    LoadScene("IntroVideoScreen");
                    currentGameState.Value = GameState.IntroVideo;
                }
                break;

            case GameState.IntroVideo:
                LoadScene("LandingScreen");
                currentGameState.Value = GameState.Landing;
                break;

            case GameState.Landing:
                LoadScene("LobbyScreen");
                currentGameState.Value = GameState.Lobby;
                break;

            case GameState.Lobby:
                Debug.Log("[GameManager] AdvanceToNextScreen - Lobby state detected, calling StartNextRound()");
                StartNextRound();
                break;

            case GameState.RoundArt:
                LoadQuestionScreen();
                break;

            case GameState.Question:
                LoadScene("EliminationScreen");
                currentGameState.Value = GameState.Elimination;
                PrepareEliminationPhase();
                StartTimer(eliminationTimer);
                break;

            case GameState.Elimination:
                ProcessEliminationVotes();
                LoadScene("VotingScreen");
                currentGameState.Value = GameState.Voting;
                PrepareVotingPhase();
                StartTimer(votingTimer);
                break;

            case GameState.Voting:
                LoadScene("ResultsScreen");
                currentGameState.Value = GameState.RoundResults;
                CalculateRoundScores();
                break;

            case GameState.RoundResults:
                CheckForSpecialScreens();
                break;

            case GameState.Halftime:
                LoadScene("BonusIntroScreen");
                currentGameState.Value = GameState.BonusIntro;
                break;

            case GameState.BonusIntro:
                Debug.Log("[GameManager] Advancing from BonusIntro to BonusQuestion");
                LoadScene("BonusQuestionScreen");
                currentGameState.Value = GameState.BonusQuestion;
                currentBonusQuestion.Value = 0;
                Debug.Log($"[GameManager] Reset currentBonusQuestion to 0, starting timer");
                StartTimer(votingTimer);
                break;

            case GameState.BonusQuestion:
                Debug.Log($"[GameManager] BonusQuestion AdvanceToNextScreen - currentBonusQuestion before increment: {currentBonusQuestion.Value}");
                ProcessBonusVotes();
                currentBonusQuestion.Value++;
                Debug.Log($"[GameManager] After increment: currentBonusQuestion = {currentBonusQuestion.Value}");

                int totalBonusQuestions = GetBonusQuestionCount();
                if (currentBonusQuestion.Value < totalBonusQuestions)
                {
                    Debug.Log($"[GameManager] Continuing to bonus question {currentBonusQuestion.Value}");
                    bonusVotes.Clear();
                    StartTimer(votingTimer);
                }
                else
                {
                    Debug.Log($"[GameManager] All {totalBonusQuestions} bonus questions complete, going to BonusResults");
                    isBonusRoundPlayed.Value = true;
                    LoadScene("BonusResultsScreen");
                    currentGameState.Value = GameState.BonusResults;
                }
                break;

            case GameState.BonusResults:
                StartNextRound();
                break;

            case GameState.FinalResults:
                LoadScene("CreditsScreen");
                currentGameState.Value = GameState.Credits;
                break;

            case GameState.Credits:
                LoadScene("LandingScreen");
                currentGameState.Value = GameState.Landing;
                break;
        }
    }

    void StartNextRound()
    {
        if (!IsServer) return;

        currentRound.Value++;

        Debug.Log($"[GameManager] StartNextRound - currentRound incremented to: {currentRound.Value}");

        int totalRounds = (gameMode.Value == GameMode.EightQuestions) ? 8 : 12;

        if (currentRound.Value > totalRounds)
        {
            LoadScene("FinalResults");
            currentGameState.Value = GameState.FinalResults;
            return;
        }

        Debug.Log($"[GameManager] Loading RoundArtScreen for round {currentRound.Value}");
        LoadScene("RoundArtScreen");
        currentGameState.Value = GameState.RoundArt;
    }

    void LoadQuestionScreen()
    {
        if (!IsServer) return;

        // Determine which question type for this round
        QuestionType questionType = GetQuestionTypeForRound(currentRound.Value);

        // Load appropriate question data
        switch (questionType)
        {
            case QuestionType.Standard:
                currentQuestion = GetStandardQuestion();
                correctAnswer.Value = currentQuestion.correctAnswer;
                robotAnswer.Value = currentQuestion.robotAnswer;
                LoadScene("QuestionScreen");
                currentGameState.Value = GameState.Question;
                StartTimer(questionTimer);
                break;

            case QuestionType.Player:
                Debug.Log("[GameManager] LoadQuestionScreen - Loading Player Question");
                currentQuestion = GetPlayerQuestion();
                correctAnswer.Value = currentQuestion.correctAnswer;
                robotAnswer.Value = currentQuestion.robotAnswer;
                LoadScene("PlayerQuestionVideoScreen");
                currentGameState.Value = GameState.Question;
                break;

            case QuestionType.Picture:
                currentQuestion = GetPictureQuestion();
                correctAnswer.Value = currentQuestion.correctAnswer;
                robotAnswer.Value = currentQuestion.robotAnswer;
                LoadScene("PictureQuestionScreen");
                currentGameState.Value = GameState.Question;
                StartTimer(questionTimer);
                break;
        }

        // Clear previous round data
        currentRoundAnswers.Clear();
        eliminationVotes.Clear();
        votingVotes.Clear();
        allAnswers.Clear();
        remainingAnswers.Clear();
        eliminatedAnswer.Value = "";
    }

    void CheckForSpecialScreens()
    {
        if (!IsServer) return;

        int halftimeRound = (gameMode.Value == GameMode.EightQuestions) ? 4 : 6;
        int totalRounds = (gameMode.Value == GameMode.EightQuestions) ? 8 : 12;

        // Check for Halftime
        if (currentRound.Value == halftimeRound && !isHalftimePlayed.Value)
        {
            LoadScene("HalftimeResultsScreen");
            currentGameState.Value = GameState.Halftime;
            isHalftimePlayed.Value = true;
            return;
        }

        // Check for Final Results
        if (currentRound.Value >= totalRounds)
        {
            LoadScene("FinalResults");
            currentGameState.Value = GameState.FinalResults;
            return;
        }

        // Otherwise, continue to next round
        StartNextRound();
    }

    // === QUESTION TYPE LOGIC ===

    public enum QuestionType
    {
        Standard,
        Player,
        Picture
    }

    QuestionType GetQuestionTypeForRound(int round)
    {
        // Picture question is always round 8 or 12
        int pictureRound = (gameMode.Value == GameMode.EightQuestions) ? 8 : 12;
        if (round == pictureRound)
        {
            return QuestionType.Picture;
        }

        // Player questions are rounds 3, 6, 9
        if (round == 3 || round == 6 || round == 9)
        {
            return QuestionType.Player;
        }

        // Everything else is standard
        return QuestionType.Standard;
    }

    public bool IsPlayerQuestion()
    {
        return GetQuestionTypeForRound(currentRound.Value) == QuestionType.Player;
    }

    public bool IsPictureQuestion()
    {
        return GetQuestionTypeForRound(currentRound.Value) == QuestionType.Picture;
    }

    // === PHASE PREPARATION ===

    void PrepareEliminationPhase()
    {
        if (!IsServer) return;

        allAnswers.Clear();

        // Add all player answers
        foreach (var answer in currentRoundAnswers.Values)
        {
            allAnswers.Add(answer);
        }

        // Add robot answer
        allAnswers.Add(robotAnswer.Value);

        // For Player Questions, also add the "correct" answer as a 2nd decoy
        if (IsPlayerQuestion())
        {
            allAnswers.Add(correctAnswer.Value);
        }

        // Shuffle the answers using Fisher-Yates
        ShuffleNetworkList(allAnswers);
    }

    void PrepareVotingPhase()
    {
        if (!IsServer) return;

        remainingAnswers.Clear();

        // Add all answers except the eliminated one
        for (int i = 0; i < allAnswers.Count; i++)
        {
            if (allAnswers[i].ToString() != eliminatedAnswer.Value.ToString())
            {
                remainingAnswers.Add(allAnswers[i]);
            }
        }
    }

    // === ANSWER SUBMISSION ===

    /// <summary>
    /// Get all existing answers for duplicate checking
    /// </summary>
    public List<string> GetAllExistingAnswers()
    {
        List<string> existingAnswers = new List<string>();

        // Add all player answers
        foreach (var answer in currentRoundAnswers.Values)
        {
            if (!string.IsNullOrEmpty(answer))
            {
                existingAnswers.Add(answer);
            }
        }

        // Add robot answer
        if (!string.IsNullOrEmpty(robotAnswer.Value.ToString()))
        {
            existingAnswers.Add(robotAnswer.Value.ToString());
        }

        // Add correct answer (not for player questions)
        if (!IsPlayerQuestion() && !string.IsNullOrEmpty(correctAnswer.Value.ToString()))
        {
            existingAnswers.Add(correctAnswer.Value.ToString());
        }

        return existingAnswers;
    }

    public void SubmitPlayerAnswer(string playerID, string answer)
    {
        if (!IsServer) return;

        if (!currentRoundAnswers.ContainsKey(playerID))
        {
            currentRoundAnswers.Add(playerID, answer);
        }

        // Check if all players have submitted
        if (currentRoundAnswers.Count >= networkPlayers.Count)
        {
            StopTimer();
            AdvanceToNextScreen();
        }
    }

    // === ELIMINATION VOTING ===

    public void SubmitEliminationVote(string playerID, string votedAnswer)
    {
        if (!IsServer) return;

        if (!eliminationVotes.ContainsKey(playerID))
        {
            eliminationVotes.Add(playerID, votedAnswer);
        }
        else
        {
            eliminationVotes[playerID] = votedAnswer;
        }

        // Check if all players have voted
        if (eliminationVotes.Count >= networkPlayers.Count)
        {
            ProcessEliminationVotes();
            StopTimer();
            AdvanceToNextScreen();
        }
    }

    void ProcessEliminationVotes()
    {
        if (!IsServer) return;

        // Skip if no votes or already processed
        if (eliminationVotes.Count == 0 || !string.IsNullOrEmpty(eliminatedAnswer.Value.ToString()))
        {
            return;
        }

        // Count votes for each answer
        Dictionary<string, int> voteCounts = new Dictionary<string, int>();

        foreach (var vote in eliminationVotes.Values)
        {
            if (!voteCounts.ContainsKey(vote))
            {
                voteCounts.Add(vote, 0);
            }
            voteCounts[vote]++;
        }

        // Find most voted answer
        string mostVoted = "";
        int maxVotes = 0;

        foreach (var kvp in voteCounts)
        {
            if (kvp.Value > maxVotes)
            {
                maxVotes = kvp.Value;
                mostVoted = kvp.Key;
            }
        }

        eliminatedAnswer.Value = mostVoted;

        // Award points for correct elimination
        bool isRobotEliminated = (eliminatedAnswer.Value.ToString() == robotAnswer.Value.ToString());
        bool isCorrectEliminated = (eliminatedAnswer.Value.ToString() == correctAnswer.Value.ToString() && !IsPlayerQuestion());
        bool isDecoyEliminated = (eliminatedAnswer.Value.ToString() == correctAnswer.Value.ToString() && IsPlayerQuestion()) || isRobotEliminated;

        int eliminationPoints = GetEliminationPoints();

        foreach (var kvp in eliminationVotes)
        {
            string playerID = kvp.Key;
            string vote = kvp.Value;

            if (IsPlayerQuestion())
            {
                // For player questions, both robot and "correct" are decoys
                if (isDecoyEliminated && vote == eliminatedAnswer.Value.ToString())
                {
                    AwardPoints(playerID, eliminationPoints);
                }
            }
            else
            {
                // Standard/Picture questions
                if (isRobotEliminated && vote == eliminatedAnswer.Value.ToString())
                {
                    AwardPoints(playerID, eliminationPoints);
                }
            }
        }
    }

    // === VOTING PHASE ===

    public void SubmitVotingVote(string playerID, string votedAnswer)
    {
        if (!IsServer) return;

        if (!votingVotes.ContainsKey(playerID))
        {
            votingVotes.Add(playerID, votedAnswer);
        }
        else
        {
            votingVotes[playerID] = votedAnswer;
        }

        // Check if all players have voted
        if (votingVotes.Count >= networkPlayers.Count)
        {
            StopTimer();
            AdvanceToNextScreen();
        }
    }

    void CalculateRoundScores()
    {
        if (!IsServer) return;

        int correctVotePoints = GetCorrectVotePoints();
        int robotVotePenalty = GetRobotVotePenalty();
        int voteReceivedPoints = GetVoteReceivedPoints();

        // Count votes received per answer
        Dictionary<string, int> votesReceived = new Dictionary<string, int>();
        foreach (var vote in votingVotes.Values)
        {
            if (!votesReceived.ContainsKey(vote))
            {
                votesReceived.Add(vote, 0);
            }
            votesReceived[vote]++;
        }

        // Award points for voting correctly/incorrectly
        foreach (var kvp in votingVotes)
        {
            string playerID = kvp.Key;
            string vote = kvp.Value;

            if (IsPlayerQuestion())
            {
                // Player questions have no "correct" answer
                // Penalty for voting robot or decoy
                if (vote == robotAnswer.Value.ToString() || vote == correctAnswer.Value.ToString())
                {
                    AwardPoints(playerID, robotVotePenalty);
                }
            }
            else
            {
                // Standard/Picture questions
                if (vote == correctAnswer.Value.ToString())
                {
                    AwardPoints(playerID, correctVotePoints);
                }
                else if (vote == robotAnswer.Value.ToString())
                {
                    AwardPoints(playerID, robotVotePenalty);
                }
            }
        }

        // Award points for receiving votes on your answer
        foreach (var kvp in currentRoundAnswers)
        {
            string playerID = kvp.Key;
            string playerAnswer = kvp.Value;

            if (votesReceived.ContainsKey(playerAnswer))
            {
                int votes = votesReceived[playerAnswer];
                AwardPoints(playerID, voteReceivedPoints * votes);
            }
        }
    }

    // === BONUS ROUND ===

    public void SubmitBonusVote(string playerID, string votedPlayerID)
    {
        if (!IsServer) return;

        Debug.Log($"[GameManager] SubmitBonusVote: player {playerID} voted for {votedPlayerID} (question {currentBonusQuestion.Value})");

        if (!bonusVotes.ContainsKey(playerID))
        {
            bonusVotes.Add(playerID, votedPlayerID);
        }
        else
        {
            bonusVotes[playerID] = votedPlayerID;
        }

        Debug.Log($"[GameManager] Bonus votes now: {bonusVotes.Count}/{networkPlayers.Count}");

        // Check if all players have voted
        if (bonusVotes.Count >= networkPlayers.Count)
        {
            Debug.Log("[GameManager] All players voted, advancing to next screen");
            StopTimer();
            AdvanceToNextScreen();
        }
    }

    void ProcessBonusVotes()
    {
        if (!IsServer) return;

        Debug.Log($"[GameManager] ProcessBonusVotes for question {currentBonusQuestion.Value}, total votes: {bonusVotes.Count}");

        // Count votes for each player
        Dictionary<string, int> voteCounts = new Dictionary<string, int>();

        foreach (var vote in bonusVotes.Values)
        {
            if (!voteCounts.ContainsKey(vote))
            {
                voteCounts.Add(vote, 0);
            }
            voteCounts[vote]++;
        }

        // Find max votes
        int maxVotes = 0;
        foreach (var count in voteCounts.Values)
        {
            if (count > maxVotes)
            {
                maxVotes = count;
            }
        }

        // Award points to all players with max votes
        int bonusPoints = (gameMode.Value == GameMode.EightQuestions) ? 6 : 4;

        Debug.Log($"[GameManager] Awarding {bonusPoints} points to players with {maxVotes} votes");

        foreach (var kvp in voteCounts)
        {
            if (kvp.Value == maxVotes)
            {
                AwardPoints(kvp.Key, bonusPoints);
            }
        }
    }

    // === SCORING HELPERS ===

    int GetEliminationPoints()
    {
        if (IsPictureQuestion())
        {
            return (gameMode.Value == GameMode.EightQuestions) ? 8 : 6;
        }
        return (gameMode.Value == GameMode.EightQuestions) ? 4 : 3;
    }

    public int GetCorrectVotePoints()
    {
        if (IsPictureQuestion())
        {
            return (gameMode.Value == GameMode.EightQuestions) ? 16 : 12;
        }
        return (gameMode.Value == GameMode.EightQuestions) ? 8 : 6;
    }

    public int GetRobotVotePenalty()
    {
        return (gameMode.Value == GameMode.EightQuestions) ? -8 : -6;
    }

    public int GetVoteReceivedPoints()
    {
        bool isDoubled = IsPlayerQuestion() || IsPictureQuestion();

        if (gameMode.Value == GameMode.EightQuestions)
        {
            return isDoubled ? 8 : 4;
        }
        else
        {
            return isDoubled ? 6 : 3;
        }
    }

    void AwardPoints(string playerID, int points)
    {
        if (!IsServer) return;

        for (int i = 0; i < networkPlayers.Count; i++)
        {
            if (networkPlayers[i].playerID.ToString() == playerID)
            {
                var player = networkPlayers[i];
                player.scorePercentage += points;
                networkPlayers[i] = player;
                Debug.Log($"[GameManager] Awarded {points} points to {player.playerName} (new score: {player.scorePercentage})");
                break;
            }
        }
    }

    // === TIMER MANAGEMENT ===

    public void StartTimer(float duration)
    {
        if (!IsServer) return;

        currentTimerValue.Value = duration;
        timerActive.Value = true;
    }

    void StopTimer()
    {
        if (!IsServer) return;

        timerActive.Value = false;
        currentTimerValue.Value = 0f;
    }

    void OnTimerExpired()
    {
        if (!IsServer) return;

        // Auto-advance when timer expires
        AdvanceToNextScreen();
    }

    public float GetTimeRemaining()
    {
        return currentTimerValue.Value;
    }

    public string GetTimerDisplay()
    {
        int seconds = Mathf.FloorToInt(currentTimerValue.Value);
        return seconds.ToString();
    }

    // === PLAYER MANAGEMENT ===

    public void AddPlayer(string playerID, string playerName, string iconName, ulong clientId = 0)
    {
        if (!IsServer) return;

        // Check if player already exists
        for (int i = 0; i < networkPlayers.Count; i++)
        {
            if (networkPlayers[i].playerID.ToString() == playerID)
            {
                Debug.LogWarning($"[GameManager] Player {playerID} already exists");
                return;
            }
        }

        NetworkedPlayerData newPlayer = new NetworkedPlayerData
        {
            playerID = playerID,
            playerName = playerName,
            iconName = iconName,
            scorePercentage = 0,
            isHost = (clientId == 0),
            deviceType = (clientId == 0) ? "desktop" : "mobile",
            clientId = clientId
        };

        networkPlayers.Add(newPlayer);
        Debug.Log($"[GameManager] Added player: {playerName} (ID: {playerID}, ClientID: {clientId})");
    }

    public void RemovePlayer(string playerID)
    {
        if (!IsServer) return;

        for (int i = 0; i < networkPlayers.Count; i++)
        {
            if (networkPlayers[i].playerID.ToString() == playerID)
            {
                networkPlayers.RemoveAt(i);
                Debug.Log($"[GameManager] Removed player: {playerID}");

                // Clean up votes and answers
                currentRoundAnswers.Remove(playerID);
                eliminationVotes.Remove(playerID);
                votingVotes.Remove(playerID);
                bonusVotes.Remove(playerID);

                break;
            }
        }
    }

    public PlayerData GetPlayer(string playerID)
    {
        for (int i = 0; i < networkPlayers.Count; i++)
        {
            if (networkPlayers[i].playerID.ToString() == playerID)
            {
                return networkPlayers[i].ToPlayerData();
            }
        }
        return null;
    }

    public List<PlayerData> GetAllPlayers()
    {
        List<PlayerData> result = new List<PlayerData>();
        for (int i = 0; i < networkPlayers.Count; i++)
        {
            result.Add(networkPlayers[i].ToPlayerData());
        }
        return result;
    }

    public IReadOnlyDictionary<string, string> BonusVotes => bonusVotes;

    public List<PlayerData> GetPlayersByRank()
    {
        List<PlayerData> allPlayers = GetAllPlayers();
        return allPlayers.OrderByDescending(p => p.scorePercentage).ToList();
    }

    // Compatibility property for old Dictionary access pattern
    public Dictionary<string, PlayerData> players
    {
        get
        {
            Dictionary<string, PlayerData> dict = new Dictionary<string, PlayerData>();
            for (int i = 0; i < networkPlayers.Count; i++)
            {
                var player = networkPlayers[i].ToPlayerData();
                dict[player.playerID] = player;
            }
            return dict;
        }
    }

    // === ANSWER FILTERING (for mobile display) ===

    public List<string> GetAnswersForPlayer(string playerID, List<string> answers)
    {
        // Filter out the player's own answer
        string playerAnswer = "";
        if (currentRoundAnswers.ContainsKey(playerID))
        {
            playerAnswer = currentRoundAnswers[playerID];
        }

        List<string> filtered = new List<string>();
        foreach (var answer in answers)
        {
            if (answer != playerAnswer)
            {
                filtered.Add(answer);
            }
        }

        return filtered;
    }

    public List<string> GetEliminationAnswersForMobile(string playerID)
    {
        List<string> allAnswersList = new List<string>();
        for (int i = 0; i < allAnswers.Count; i++)
        {
            allAnswersList.Add(allAnswers[i].ToString());
        }
        return GetAnswersForPlayer(playerID, allAnswersList);
    }

    public List<string> GetVotingAnswersForMobile(string playerID)
    {
        List<string> remainingAnswersList = new List<string>();
        for (int i = 0; i < remainingAnswers.Count; i++)
        {
            remainingAnswersList.Add(remainingAnswers[i].ToString());
        }
        return GetAnswersForPlayer(playerID, remainingAnswersList);
    }

    // === DATA LOADING ===

    void LoadQuestions()
    {
        Debug.Log("[GameManager] Ready to use questions");
        Debug.Log($"[GameManager] Standard Questions: {standardQuestions.Count}");
        Debug.Log($"[GameManager] Player Questions: {playerQuestions.Count}");
        Debug.Log($"[GameManager] Picture Questions: {pictureQuestions.Count}");
    }

    Question GetStandardQuestion()
    {
        if (standardQuestions.Count == 0)
        {
            Debug.LogError("[GameManager] No standard questions loaded!");
            return null;
        }

        Question question = standardQuestions[standardQuestionIndex];
        standardQuestionIndex++;

        if (standardQuestionIndex >= standardQuestions.Count)
        {
            standardQuestionIndex = 0;
        }

        return question;
    }

    Question GetPlayerQuestion()
    {
        if (playerQuestions.Count == 0)
        {
            Debug.LogError("[GameManager] No player questions loaded!");
            return null;
        }

        Question question = playerQuestions[playerQuestionIndex];
        playerQuestionIndex++;

        if (playerQuestionIndex >= playerQuestions.Count)
        {
            playerQuestionIndex = 0;
        }

        return question;
    }

    Question GetPictureQuestion()
    {
        if (pictureQuestions.Count == 0)
        {
            Debug.LogError("[GameManager] No picture questions loaded!");
            return null;
        }

        Question question = pictureQuestions[pictureQuestionIndex];
        pictureQuestionIndex++;

        if (pictureQuestionIndex >= pictureQuestions.Count)
        {
            pictureQuestionIndex = 0;
        }

        return question;
    }

    // === UTILITY ===

    void ShuffleNetworkList(NetworkList<FixedString128Bytes> list)
    {
        if (!IsServer) return;

        // Fisher-Yates shuffle
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void LoadScene(string sceneName)
    {
        // HOST AUTHORITY: Only host can initiate scene changes
        // Mobile clients will receive ChangeScene RPC via Unity Netcode and load scenes that way
        bool isHostCheck = RWMNetworkManager.Instance != null && RWMNetworkManager.Instance.isHost;
        bool isMobile = DeviceDetector.Instance != null && DeviceDetector.Instance.IsMobile();

        // If mobile client, ignore - wait for host's RPC
        if (isMobile && !isHostCheck)
        {
            Debug.LogWarning($"[GameManager] Mobile client attempted to load scene '{sceneName}' - ignoring. Waiting for host command.");
            return;
        }

        // Host loads scene locally
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }

        // Host broadcasts to all clients
        if (isHostCheck && RWMNetworkManager.Instance != null)
        {
            RWMNetworkManager.Instance.ChangeScene(sceneName);
            Debug.Log($"[GameManager] Host broadcasting scene change to clients: {sceneName}");
        }
    }

    // === PUBLIC GETTERS ===

    public Question GetCurrentQuestion()
    {
        return currentQuestion;
    }

    public int GetCurrentRound()
    {
        return currentRound.Value;
    }

    public string GetCorrectAnswer()
    {
        return correctAnswer.Value.ToString();
    }

    public string GetRobotAnswer()
    {
        return robotAnswer.Value.ToString();
    }

    public List<string> GetAllAnswers()
    {
        List<string> result = new List<string>();
        for (int i = 0; i < allAnswers.Count; i++)
        {
            result.Add(allAnswers[i].ToString());
        }
        return result;
    }

    public List<string> GetRemainingAnswers()
    {
        List<string> result = new List<string>();
        for (int i = 0; i < remainingAnswers.Count; i++)
        {
            result.Add(remainingAnswers[i].ToString());
        }
        return result;
    }

    public string GetEliminatedAnswer()
    {
        return eliminatedAnswer.Value.ToString();
    }

    public string GetCurrentBonusQuestion()
    {
        if (bonusQuestions != null && currentBonusQuestion.Value < bonusQuestions.miniQuestions.Count)
        {
            return bonusQuestions.miniQuestions[currentBonusQuestion.Value];
        }
        return "";
    }

    public int GetBonusQuestionCount()
    {
        if (bonusQuestions != null && bonusQuestions.miniQuestions != null)
        {
            return bonusQuestions.miniQuestions.Count;
        }
        return 0;
    }

    public Dictionary<string, int> GetVotingResults()
    {
        Dictionary<string, int> results = new Dictionary<string, int>();
        foreach (var vote in votingVotes.Values)
        {
            if (!results.ContainsKey(vote))
            {
                results.Add(vote, 0);
            }
            results[vote]++;
        }
        return results;
    }

    public Dictionary<string, string> GetVotingVotesByPlayer()
    {
        return new Dictionary<string, string>(votingVotes);
    }

    public Dictionary<string, int> GetEliminationResults()
    {
        Dictionary<string, int> results = new Dictionary<string, int>();
        foreach (var vote in eliminationVotes.Values)
        {
            if (!results.ContainsKey(vote))
            {
                results.Add(vote, 0);
            }
            results[vote]++;
        }
        return results;
    }
}

// === NETWORK-SERIALIZABLE DATA STRUCTURES ===

/// <summary>
/// Network-serializable player data for NetworkList
/// Uses INetworkSerializable for efficient synchronization
/// </summary>
public struct NetworkedPlayerData : INetworkSerializable
{
    public FixedString64Bytes playerID;
    public FixedString64Bytes playerName;
    public FixedString64Bytes iconName;
    public int scorePercentage;
    public bool isHost;
    public FixedString32Bytes deviceType;
    public ulong clientId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref iconName);
        serializer.SerializeValue(ref scorePercentage);
        serializer.SerializeValue(ref isHost);
        serializer.SerializeValue(ref deviceType);
        serializer.SerializeValue(ref clientId);
    }

    public PlayerData ToPlayerData()
    {
        return new PlayerData
        {
            playerID = playerID.ToString(),
            playerName = playerName.ToString(),
            iconName = iconName.ToString(),
            scorePercentage = scorePercentage,
            isHost = isHost,
            deviceType = deviceType.ToString(),
            clientId = clientId
        };
    }
}

// === LEGACY DATA STRUCTURES (for compatibility) ===

[System.Serializable]
public class PlayerData
{
    public string playerID;
    public string playerName;
    public string iconName;
    public int scorePercentage;
    public bool isHost;
    public string deviceType;
    public ulong clientId;
}

[System.Serializable]
public class Question
{
    public string questionText;
    public string correctAnswer;
    public string robotAnswer;
    public string robotAnecdote;
    public string questionType;
    public string imageURL;
}

[System.Serializable]
public class BonusQuestion
{
    public List<string> miniQuestions = new List<string>();
}

[System.Serializable]
public class QuestionDatabase
{
    public List<Question> standardQuestions;
    public List<Question> playerQuestions;
    public Question pictureQuestion8Q;
    public Question pictureQuestion12Q;
    public BonusQuestion bonusQuestions;
}
