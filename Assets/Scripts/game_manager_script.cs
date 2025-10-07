using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // Singleton pattern - only one GameManager exists
    public static GameManager Instance;
    
    // === CONFIGURATION ===
    [Header("Game Configuration")]
    public GameMode gameMode = GameMode.EightQuestions;
    
    // === GAME STATE ===
    public int currentRound = 0;
    public bool isHalftimePlayed = false;
    public bool isBonusRoundPlayed = false;
    
    // === PLAYER DATA ===
    [Header("Player Data")]
    public Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();
    public Dictionary<string, string> currentRoundAnswers = new Dictionary<string, string>();
    public Dictionary<string, string> eliminationVotes = new Dictionary<string, string>();
    public Dictionary<string, string> votingVotes = new Dictionary<string, string>();
    
    // === ROUND DATA ===
    [Header("Current Round Data")]
    public Question currentQuestion;
    public string robotAnswer = "";
    public string correctAnswer = "";
    public List<string> allAnswers = new List<string>(); // For Elimination
    public List<string> remainingAnswers = new List<string>(); // For Voting
    public string eliminatedAnswer = "";
    
    // === BONUS ROUND DATA ===
    [Header("Bonus Round Data")]
    public BonusQuestion bonusQuestions;
    public int currentBonusQuestion = 0;
    public Dictionary<string, string> bonusVotes = new Dictionary<string, string>(); // playerID -> votedPlayerID
    
    // === TIMER STATE ===
    [Header("Timer Configuration")]
    public float questionTimer = 60f;
    public float eliminationTimer = 30f;
    public float votingTimer = 30f;
    public float currentTimerValue = 0f;
    public bool timerActive = false;
    
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
    
    public GameState currentGameState = GameState.Loading;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager created");
        }
        else
        {
            Debug.Log("Duplicate GameManager found and destroyed");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadQuestions();
    }
    
    void Update()
    {
        // Handle timer countdown
        if (timerActive && currentTimerValue > 0)
        {
            currentTimerValue -= Time.deltaTime;
            
            if (currentTimerValue <= 0)
            {
                currentTimerValue = 0;
                timerActive = false;
                OnTimerExpired();
            }
        }
    }
    
    // === GAME FLOW METHODS ===
    
    public void StartGame(GameMode mode)
    {
        gameMode = mode;
        currentRound = 0;
        isHalftimePlayed = false;
        isBonusRoundPlayed = false;
        
        // Reset all player scores
        foreach (var player in players.Values)
        {
            player.scorePercentage = 0;
        }
        
        LoadScene("IntroVideoScreen");
        currentGameState = GameState.IntroVideo;
    }
    
    public void AdvanceToNextScreen()
    {
        switch (currentGameState)
        {
            case GameState.Loading:
                LoadScene("IntroVideoScreen");
                currentGameState = GameState.IntroVideo;
                break;

            case GameState.IntroVideo:
                LoadScene("LandingScreen");
                currentGameState = GameState.Landing;
                break;

            case GameState.Landing:
                LoadScene("LobbyScreen");
                currentGameState = GameState.Lobby;
                break;

            case GameState.Lobby:
                Debug.Log("AdvanceToNextScreen - Lobby state detected, calling StartNextRound()");
                StartNextRound();
                break;
                
            case GameState.RoundArt:
                LoadQuestionScreen();
                break;
                
            case GameState.Question:
                LoadScene("EliminationScreen");
                currentGameState = GameState.Elimination;
                PrepareEliminationPhase();
                StartTimer(eliminationTimer);
                break;
                
            case GameState.Elimination:
                ProcessEliminationVotes();
                LoadScene("VotingScreen");
                currentGameState = GameState.Voting;
                PrepareVotingPhase();
                StartTimer(votingTimer);
                break;
                
            case GameState.Voting:
                LoadScene("ResultsScreen");
                currentGameState = GameState.RoundResults;
                CalculateRoundScores();
                break;
                
            case GameState.RoundResults:
                CheckForSpecialScreens();
                break;
                
            case GameState.Halftime:
                LoadScene("BonusIntroScreen");
                currentGameState = GameState.BonusIntro;
                break;
                
            case GameState.BonusIntro:
                LoadScene("BonusQuestionScreen");
                currentGameState = GameState.BonusQuestion;
                currentBonusQuestion = 0;
                StartTimer(votingTimer);
                break;
                
            case GameState.BonusQuestion:
                ProcessBonusVotes();
                currentBonusQuestion++;
                
                if (currentBonusQuestion < 4)
                {
                    // Next bonus question
                    bonusVotes.Clear();
                    StartTimer(votingTimer);
                }
                else
                {
                    // Bonus round complete
                    isBonusRoundPlayed = true;
                    LoadScene("BonusResultsScreen");
                    currentGameState = GameState.BonusResults;
                }
                break;
                
            case GameState.BonusResults:
                StartNextRound();
                break;
                
            case GameState.FinalResults:
                LoadScene("CreditsScreen");
                currentGameState = GameState.Credits;
                break;
                
            case GameState.Credits:
                // Return to landing or restart
                LoadScene("LandingScreen");
                currentGameState = GameState.Landing;
                break;
        }
    }
    
    void StartNextRound()
    {
        currentRound++;

        Debug.Log("StartNextRound called - currentRound incremented to: " + currentRound);

        int totalRounds = (gameMode == GameMode.EightQuestions) ? 8 : 12;

        if (currentRound > totalRounds)
        {
            LoadScene("FinalResults");
            currentGameState = GameState.FinalResults;
            return;
        }

        Debug.Log("Loading RoundArtScreen for round " + currentRound);
        Debug.Log("About to load scene - currentRound is: " + currentRound + ", GameManager instance ID: " + GetInstanceID());
        LoadScene("RoundArtScreen");
        currentGameState = GameState.RoundArt;
        Debug.Log("After setting RoundArt state - currentRound is: " + currentRound);
    }
    
    void LoadQuestionScreen()
    {
        // Determine which question type for this round
        QuestionType questionType = GetQuestionTypeForRound(currentRound);

        // Load appropriate question data
        switch (questionType)
        {
            case QuestionType.Standard:
                currentQuestion = GetStandardQuestion();
                correctAnswer = currentQuestion.correctAnswer;
                robotAnswer = currentQuestion.robotAnswer;
                LoadScene("QuestionScreen");
                currentGameState = GameState.Question;
                StartTimer(questionTimer);
                break;

            case QuestionType.Player:
                // Player questions: Load question data first, then show intro video
                Debug.Log("LoadQuestionScreen - Loading Player Question");
                currentQuestion = GetPlayerQuestion();
                Debug.Log($"LoadQuestionScreen - currentQuestion after GetPlayerQuestion: {currentQuestion?.questionText}");
                correctAnswer = currentQuestion.correctAnswer;
                robotAnswer = currentQuestion.robotAnswer;
                Debug.Log($"LoadQuestionScreen - Set correctAnswer: '{correctAnswer}', robotAnswer: '{robotAnswer}'");
                LoadScene("PlayerQuestionVideoScreen");
                currentGameState = GameState.Question;
                // Timer will start after video finishes
                break;

            case QuestionType.Picture:
                currentQuestion = GetPictureQuestion();
                correctAnswer = currentQuestion.correctAnswer;
                robotAnswer = currentQuestion.robotAnswer;
                LoadScene("PictureQuestionScreen");
                currentGameState = GameState.Question;
                StartTimer(questionTimer);
                break;
        }

        // Clear previous round data at start of new question
        currentRoundAnswers.Clear();
        eliminationVotes.Clear();
        votingVotes.Clear();
        allAnswers.Clear();
        remainingAnswers.Clear();
        eliminatedAnswer = "";
    }
    
    void CheckForSpecialScreens()
    {
        int halftimeRound = (gameMode == GameMode.EightQuestions) ? 4 : 6;
        int totalRounds = (gameMode == GameMode.EightQuestions) ? 8 : 12;
        
        // Check for Halftime
        if (currentRound == halftimeRound && !isHalftimePlayed)
        {
            LoadScene("HalftimeResultsScreen");
            currentGameState = GameState.Halftime;
            isHalftimePlayed = true;
            return;
        }
        
        // Check for Final Results
        if (currentRound >= totalRounds)
        {
            LoadScene("FinalResults");
            currentGameState = GameState.FinalResults;
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
        int pictureRound = (gameMode == GameMode.EightQuestions) ? 8 : 12;
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
        return GetQuestionTypeForRound(currentRound) == QuestionType.Player;
    }
    
    public bool IsPictureQuestion()
    {
        return GetQuestionTypeForRound(currentRound) == QuestionType.Picture;
    }
    
    // === PHASE PREPARATION ===
    
    void PrepareEliminationPhase()
    {
        allAnswers.Clear();
        
        // Add all player answers
        foreach (var answer in currentRoundAnswers.Values)
        {
            allAnswers.Add(answer);
        }
        
        // Add robot answer
        allAnswers.Add(robotAnswer);
        
        // For Player Questions, also add the "correct" answer as a 2nd decoy
        if (IsPlayerQuestion())
        {
            allAnswers.Add(correctAnswer);
        }
        
        // Shuffle the answers
        ShuffleList(allAnswers);
    }
    
    void PrepareVotingPhase()
    {
        remainingAnswers.Clear();
        
        // Add all answers except the eliminated one
        foreach (var answer in allAnswers)
        {
            if (answer != eliminatedAnswer)
            {
                remainingAnswers.Add(answer);
            }
        }
    }
    
    // === ANSWER SUBMISSION ===

    /// <summary>
    /// Get all existing answers for duplicate checking (includes robot answer and correct answer)
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
        if (!string.IsNullOrEmpty(robotAnswer))
        {
            existingAnswers.Add(robotAnswer);
        }

        // Add correct answer (not for player questions, but for standard/picture)
        if (!IsPlayerQuestion() && !string.IsNullOrEmpty(correctAnswer))
        {
            existingAnswers.Add(correctAnswer);
        }

        return existingAnswers;
    }

    public void SubmitPlayerAnswer(string playerID, string answer)
    {
        if (!currentRoundAnswers.ContainsKey(playerID))
        {
            currentRoundAnswers.Add(playerID, answer);
        }

        // Check if all players have submitted
        if (currentRoundAnswers.Count >= players.Count)
        {
            StopTimer();
            AdvanceToNextScreen();
        }
    }
    
    // === ELIMINATION VOTING ===
    
    public void SubmitEliminationVote(string playerID, string votedAnswer)
    {
        if (!eliminationVotes.ContainsKey(playerID))
        {
            eliminationVotes.Add(playerID, votedAnswer);
        }
        else
        {
            eliminationVotes[playerID] = votedAnswer;
        }
        
        // Check if all players have voted
        if (eliminationVotes.Count >= players.Count)
        {
            ProcessEliminationVotes();
            StopTimer();
            AdvanceToNextScreen();
        }
    }
    
    void ProcessEliminationVotes()
    {
        // Skip if no votes or already processed
        if (eliminationVotes.Count == 0 || !string.IsNullOrEmpty(eliminatedAnswer))
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

        eliminatedAnswer = mostVoted;
        
        // Award points for correct elimination
        bool isRobotEliminated = (eliminatedAnswer == robotAnswer);
        bool isCorrectEliminated = (eliminatedAnswer == correctAnswer && !IsPlayerQuestion());
        bool isDecoyEliminated = (eliminatedAnswer == correctAnswer && IsPlayerQuestion()) || isRobotEliminated;
        
        int eliminationPoints = GetEliminationPoints();
        
        foreach (var kvp in eliminationVotes)
        {
            string playerID = kvp.Key;
            string vote = kvp.Value;
            
            if (IsPlayerQuestion())
            {
                // For player questions, both robot and "correct" are decoys
                if (isDecoyEliminated && vote == eliminatedAnswer)
                {
                    AwardPoints(playerID, eliminationPoints);
                }
            }
            else
            {
                // Standard/Picture questions
                if (isRobotEliminated && vote == eliminatedAnswer)
                {
                    AwardPoints(playerID, eliminationPoints);
                }
                // Eliminating correct answer = 0 points (no penalty, but no reward)
            }
        }
    }
    
    // === VOTING PHASE ===
    
    public void SubmitVotingVote(string playerID, string votedAnswer)
    {
        if (!votingVotes.ContainsKey(playerID))
        {
            votingVotes.Add(playerID, votedAnswer);
        }
        else
        {
            votingVotes[playerID] = votedAnswer;
        }
        
        // Check if all players have voted
        if (votingVotes.Count >= players.Count)
        {
            StopTimer();
            AdvanceToNextScreen();
        }
    }
    
    void CalculateRoundScores()
    {
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
                if (vote == robotAnswer || vote == correctAnswer)
                {
                    AwardPoints(playerID, robotVotePenalty); // Negative points
                }
                // No points for voting for any player answer (they're all valid opinions)
            }
            else
            {
                // Standard/Picture questions
                if (vote == correctAnswer)
                {
                    AwardPoints(playerID, correctVotePoints);
                }
                else if (vote == robotAnswer)
                {
                    AwardPoints(playerID, robotVotePenalty); // Negative points
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
        if (!bonusVotes.ContainsKey(playerID))
        {
            bonusVotes.Add(playerID, votedPlayerID);
        }
        else
        {
            bonusVotes[playerID] = votedPlayerID;
        }
        
        // Check if all players have voted
        if (bonusVotes.Count >= players.Count)
        {
            StopTimer();
            AdvanceToNextScreen();
        }
    }
    
    void ProcessBonusVotes()
    {
        Debug.Log($"ProcessBonusVotes called for question {currentBonusQuestion}, total votes: {bonusVotes.Count}");

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

        // Award points to all players with max votes (handles ties)
        int bonusPoints = (gameMode == GameMode.EightQuestions) ? 6 : 4;

        Debug.Log($"Awarding {bonusPoints} points to players with {maxVotes} votes");

        foreach (var kvp in voteCounts)
        {
            if (kvp.Value == maxVotes)
            {
                PlayerData player = GetPlayer(kvp.Key);
                Debug.Log($"Awarding {bonusPoints} points to player {player?.playerName} (ID: {kvp.Key})");
                AwardPoints(kvp.Key, bonusPoints);
            }
        }
    }
    
    // === SCORING HELPERS ===
    
    int GetEliminationPoints()
    {
        if (IsPictureQuestion())
        {
            return (gameMode == GameMode.EightQuestions) ? 8 : 6;
        }
        return (gameMode == GameMode.EightQuestions) ? 4 : 3;
    }
    
    public int GetCorrectVotePoints()
    {
        if (IsPictureQuestion())
        {
            return (gameMode == GameMode.EightQuestions) ? 16 : 12;
        }
        return (gameMode == GameMode.EightQuestions) ? 8 : 6;
    }

    public int GetRobotVotePenalty()
    {
        // Penalty is NOT doubled for picture questions
        return (gameMode == GameMode.EightQuestions) ? -8 : -6;
    }

    public int GetVoteReceivedPoints()
    {
        bool isDoubled = IsPlayerQuestion() || IsPictureQuestion();

        if (gameMode == GameMode.EightQuestions)
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
        if (players.ContainsKey(playerID))
        {
            players[playerID].scorePercentage += points;
        }
    }
    
    // === TIMER MANAGEMENT ===

    public void StartTimer(float duration)
    {
        currentTimerValue = duration;
        timerActive = true;
    }
    
    void StopTimer()
    {
        timerActive = false;
        currentTimerValue = 0f;
    }
    
    void OnTimerExpired()
    {
        // Auto-advance when timer expires
        AdvanceToNextScreen();
    }
    
    public float GetTimeRemaining()
    {
        return currentTimerValue;
    }
    
    public string GetTimerDisplay()
    {
        int seconds = Mathf.FloorToInt(currentTimerValue);
        return seconds.ToString();
    }
    
    // === PLAYER MANAGEMENT ===
    
    public void AddPlayer(string playerID, string playerName, string iconName, ulong clientId = 0)
    {
        if (!players.ContainsKey(playerID))
        {
            players.Add(playerID, new PlayerData
            {
                playerID = playerID,
                playerName = playerName,
                iconName = iconName,
                scorePercentage = 0,
                isHost = false,
                deviceType = "mobile",
                clientId = clientId
            });
        }
    }
    
    public void RemovePlayer(string playerID)
    {
        if (players.ContainsKey(playerID))
        {
            players.Remove(playerID);
        }
    }
    
    public PlayerData GetPlayer(string playerID)
    {
        if (players.ContainsKey(playerID))
        {
            return players[playerID];
        }
        return null;
    }
    
    public List<PlayerData> GetAllPlayers()
    {
        return players.Values.ToList();
    }
    
    public List<PlayerData> GetPlayersByRank()
    {
        return players.Values.OrderByDescending(p => p.scorePercentage).ToList();
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
        return GetAnswersForPlayer(playerID, allAnswers);
    }
    
    public List<string> GetVotingAnswersForMobile(string playerID)
    {
        return GetAnswersForPlayer(playerID, remainingAnswers);
    }
    
    // === DATA LOADING ===
    
    void LoadQuestions()
    {
        // Questions are now loaded by QuestionLoader.cs
        // This method is called after QuestionLoader populates the lists
        Debug.Log("GameManager ready to use questions");
        Debug.Log("Standard Questions: " + standardQuestions.Count);
        Debug.Log("Player Questions: " + playerQuestions.Count);
        Debug.Log("Picture Questions: " + pictureQuestions.Count);
    }
    
    Question GetStandardQuestion()
    {
        if (standardQuestions.Count == 0)
        {
            Debug.LogError("No standard questions loaded!");
            return null;
        }
        
        // Get next question and increment index
        Question question = standardQuestions[standardQuestionIndex];
        standardQuestionIndex++;
        
        // Loop back if we run out (shouldn't happen in normal gameplay)
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
            Debug.LogError("No player questions loaded!");
            return null;
        }

        Debug.Log($"GetPlayerQuestion - playerQuestions.Count: {playerQuestions.Count}, playerQuestionIndex: {playerQuestionIndex}");

        // Get next question and increment index
        Question question = playerQuestions[playerQuestionIndex];

        Debug.Log($"GetPlayerQuestion - Retrieved question: '{question?.questionText}'");

        playerQuestionIndex++;

        // Loop back if we run out
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
            Debug.LogError("No picture questions loaded!");
            return null;
        }
        
        // Get next question and increment index
        Question question = pictureQuestions[pictureQuestionIndex];
        pictureQuestionIndex++;
        
        // Loop back if we run out
        if (pictureQuestionIndex >= pictureQuestions.Count)
        {
            pictureQuestionIndex = 0;
        }
        
        return question;
    }
    
    // === UTILITY ===
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    void LoadScene(string sceneName)
    {
        // Use SceneTransitionManager if available for smooth transitions
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(sceneName);
        }
        else
        {
            // Fallback to direct loading
            SceneManager.LoadScene(sceneName);
        }
    }
    
    // === PUBLIC GETTERS ===
    
    public Question GetCurrentQuestion()
    {
        return currentQuestion;
    }
    
    public int GetCurrentRound()
    {
        return currentRound;
    }
    
    public string GetCorrectAnswer()
    {
        return correctAnswer;
    }
    
    public string GetRobotAnswer()
    {
        return robotAnswer;
    }
    
    public List<string> GetAllAnswers()
    {
        return allAnswers;
    }
    
    public List<string> GetRemainingAnswers()
    {
        return remainingAnswers;
    }
    
    public string GetEliminatedAnswer()
    {
        return eliminatedAnswer;
    }
    
    public string GetCurrentBonusQuestion()
    {
        if (bonusQuestions != null && currentBonusQuestion < bonusQuestions.miniQuestions.Count)
        {
            return bonusQuestions.miniQuestions[currentBonusQuestion];
        }
        return "";
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

// === DATA STRUCTURES ===

[System.Serializable]
public class PlayerData
{
    public string playerID;
    public string playerName;
    public string iconName;
    public int scorePercentage;
    public bool isHost;
    public string deviceType; // "desktop" or "mobile"
    public ulong clientId; // Network client ID for tracking connections
}

[System.Serializable]
public class Question
{
    public string questionText;
    public string correctAnswer;
    public string robotAnswer;
    public string robotAnecdote;
    public string questionType; // "standard", "player", or "picture"
    public string imageURL; // For picture questions
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