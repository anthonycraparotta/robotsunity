using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;
    
    [Header("Debug Settings")]
    public bool debugModeEnabled = false;
    public KeyCode debugMenuKey = KeyCode.F1;
    public bool showDebugGUI = false;
    
    [Header("Quick Actions")]
    public KeyCode skipToRoundKey = KeyCode.F2;
    public KeyCode addTestPlayersKey = KeyCode.F3;
    public KeyCode simulateAnswersKey = KeyCode.F4;
    public KeyCode toggleTimerKey = KeyCode.F5;
    public KeyCode add100PointsKey = KeyCode.F6;
    
    [Header("Test Players")]
    public int numberOfTestPlayers = 4;
    public List<string> testPlayerNames = new List<string> 
    { 
        "TestPlayer1", "TestPlayer2", "TestPlayer3", "TestPlayer4",
        "TestPlayer5", "TestPlayer6", "TestPlayer7", "TestPlayer8"
    };
    
    private Rect debugWindowRect = new Rect(20, 20, 300, 400);
    private Vector2 scrollPosition;
    private int targetRound = 1;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (!debugModeEnabled) return;
        
        // Toggle debug menu
        if (Input.GetKeyDown(debugMenuKey))
        {
            showDebugGUI = !showDebugGUI;
        }
        
        // Quick actions
        if (Input.GetKeyDown(addTestPlayersKey))
        {
            AddTestPlayers();
        }
        
        if (Input.GetKeyDown(simulateAnswersKey))
        {
            SimulateAllAnswers();
        }
        
        if (Input.GetKeyDown(toggleTimerKey))
        {
            ToggleTimer();
        }
        
        if (Input.GetKeyDown(add100PointsKey))
        {
            AddPointsToAllPlayers(100);
        }
    }
    
    void OnGUI()
    {
        if (!debugModeEnabled || !showDebugGUI) return;
        
        debugWindowRect = GUI.Window(0, debugWindowRect, DebugWindow, "Debug Tools");
    }
    
    void DebugWindow(int windowID)
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("=== GAME STATE ===");
        GUILayout.Label("Current Round: " + GameManager.Instance.currentRound);
        GUILayout.Label("Game Mode: " + GameManager.Instance.gameMode);
        GUILayout.Label("Players: " + GameManager.Instance.players.Count);
        GUILayout.Label("Timer: " + GameManager.Instance.GetTimerDisplay());
        
        GUILayout.Space(10);
        
        // === SCENE JUMPING ===
        GUILayout.Label("=== SCENE NAVIGATION ===");
        
        if (GUILayout.Button("Landing Screen"))
        {
            SceneManager.LoadScene("LandingScreen");
        }
        
        if (GUILayout.Button("Lobby Screen"))
        {
            SceneManager.LoadScene("LobbyScreen");
        }
        
        if (GUILayout.Button("Question Screen"))
        {
            SceneManager.LoadScene("QuestionScreen");
        }
        
        if (GUILayout.Button("Elimination Screen"))
        {
            SceneManager.LoadScene("EliminationScreen");
        }
        
        if (GUILayout.Button("Voting Screen"))
        {
            SceneManager.LoadScene("VotingScreen");
        }
        
        if (GUILayout.Button("Results Screen"))
        {
            SceneManager.LoadScene("ResultsScreen");
        }
        
        if (GUILayout.Button("Final Results"))
        {
            SceneManager.LoadScene("FinalResults");
        }
        
        GUILayout.Space(10);
        
        // === ROUND CONTROL ===
        GUILayout.Label("=== ROUND CONTROL ===");
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Skip to Round:");
        targetRound = (int)GUILayout.HorizontalSlider(targetRound, 1, 12);
        GUILayout.Label(targetRound.ToString());
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Skip to Round " + targetRound))
        {
            SkipToRound(targetRound);
        }
        
        if (GUILayout.Button("Next Round"))
        {
            GameManager.Instance.currentRound++;
            GameManager.Instance.AdvanceToNextScreen();
        }
        
        GUILayout.Space(10);
        
        // === PLAYER MANAGEMENT ===
        GUILayout.Label("=== PLAYER MANAGEMENT ===");
        
        if (GUILayout.Button("Add " + numberOfTestPlayers + " Test Players"))
        {
            AddTestPlayers();
        }
        
        if (GUILayout.Button("Clear All Players"))
        {
            ClearAllPlayers();
        }
        
        GUILayout.Space(10);
        
        // === SIMULATION ===
        GUILayout.Label("=== SIMULATION ===");
        
        if (GUILayout.Button("Simulate All Answers"))
        {
            SimulateAllAnswers();
        }
        
        if (GUILayout.Button("Simulate Random Votes"))
        {
            SimulateRandomVotes();
        }
        
        if (GUILayout.Button("Auto-Complete Round"))
        {
            AutoCompleteRound();
        }
        
        GUILayout.Space(10);
        
        // === SCORING ===
        GUILayout.Label("=== SCORING ===");
        
        if (GUILayout.Button("Add 100% to All Players"))
        {
            AddPointsToAllPlayers(100);
        }
        
        if (GUILayout.Button("Reset All Scores"))
        {
            ResetAllScores();
        }
        
        if (GUILayout.Button("Randomize Scores"))
        {
            RandomizeScores();
        }
        
        GUILayout.Space(10);
        
        // === TIMER ===
        GUILayout.Label("=== TIMER ===");
        
        if (GUILayout.Button("Pause/Resume Timer"))
        {
            ToggleTimer();
        }
        
        if (GUILayout.Button("Set Timer to 5 seconds"))
        {
            GameManager.Instance.currentTimerValue = 5f;
        }
        
        if (GUILayout.Button("Skip Timer"))
        {
            GameManager.Instance.currentTimerValue = 0f;
        }
        
        GUILayout.Space(10);
        
        // === GAME MODE ===
        GUILayout.Label("=== GAME MODE ===");
        
        if (GUILayout.Button("Switch to 8Q Mode"))
        {
            GameManager.Instance.gameMode = GameManager.GameMode.EightQuestions;
        }
        
        if (GUILayout.Button("Switch to 12Q Mode"))
        {
            GameManager.Instance.gameMode = GameManager.GameMode.TwelveQuestions;
        }
        
        GUILayout.EndScrollView();
        
        GUI.DragWindow();
    }
    
    // === DEBUG FUNCTIONS ===
    
    public void AddTestPlayers()
    {
        // Find next available test player index
        int startIndex = 0;
        while (GameManager.Instance.players.ContainsKey("test_player_" + startIndex))
        {
            startIndex++;
        }

        for (int i = 0; i < numberOfTestPlayers; i++)
        {
            int playerIndex = startIndex + i;

            if (playerIndex >= testPlayerNames.Count)
            {
                Debug.LogWarning("Reached maximum test players (" + testPlayerNames.Count + ")");
                break;
            }

            string playerID = "test_player_" + playerIndex;
            string playerName = testPlayerNames[playerIndex];
            string iconName = "player icon (" + ((playerIndex % 20) + 1) + ")";

            GameManager.Instance.AddPlayer(playerID, playerName, iconName);
        }

        Debug.Log("Added " + numberOfTestPlayers + " test players (starting from index " + startIndex + ")");
    }
    
    public void ClearAllPlayers()
    {
        GameManager.Instance.players.Clear();
        Debug.Log("Cleared all players");
    }
    
    public void SimulateAllAnswers()
    {
        string[] sampleAnswers = new string[]
        {
            "This is a test answer",
            "Another sample response",
            "Testing the answer system",
            "Random answer here",
            "Simulated player response",
            "Debug answer text",
            "Example answer content",
            "Test submission"
        };
        
        int index = 0;
        foreach (var player in GameManager.Instance.players)
        {
            string answer = sampleAnswers[index % sampleAnswers.Length];
            GameManager.Instance.SubmitPlayerAnswer(player.Key, answer);
            index++;
        }
        
        Debug.Log("Simulated answers for all players");
    }
    
    public void SimulateRandomVotes()
    {
        List<string> answers = GameManager.Instance.GetAllAnswers();
        if (answers.Count == 0)
        {
            Debug.LogWarning("No answers available to vote on");
            return;
        }
        
        foreach (var player in GameManager.Instance.players)
        {
            string randomAnswer = answers[Random.Range(0, answers.Count)];
            
            // Simulate elimination or voting depending on current screen
            if (SceneManager.GetActiveScene().name == "EliminationScreen")
            {
                GameManager.Instance.SubmitEliminationVote(player.Key, randomAnswer);
            }
            else if (SceneManager.GetActiveScene().name == "VotingScreen")
            {
                GameManager.Instance.SubmitVotingVote(player.Key, randomAnswer);
            }
        }
        
        Debug.Log("Simulated random votes for all players");
    }
    
    public void AutoCompleteRound()
    {
        StartCoroutine(AutoCompleteRoundCoroutine());
    }

    System.Collections.IEnumerator AutoCompleteRoundCoroutine()
    {
        // Simulate entire round automatically
        SimulateAllAnswers();
        yield return new WaitForSeconds(0.5f);

        GameManager.Instance.AdvanceToNextScreen();
        yield return new WaitForSeconds(0.5f);

        SimulateRandomVotes();
        yield return new WaitForSeconds(0.5f);

        GameManager.Instance.AdvanceToNextScreen();
        yield return new WaitForSeconds(0.5f);

        SimulateRandomVotes();

        Debug.Log("Auto-completed round");
    }
    
    public void SkipToRound(int round)
    {
        GameManager.Instance.currentRound = round - 1;
        GameManager.Instance.AdvanceToNextScreen();
        Debug.Log("Skipped to round " + round);
    }
    
    public void AddPointsToAllPlayers(int points)
    {
        foreach (var player in GameManager.Instance.players)
        {
            player.Value.scorePercentage += points;
        }
        Debug.Log("Added " + points + "% to all players");
    }
    
    public void ResetAllScores()
    {
        foreach (var player in GameManager.Instance.players)
        {
            player.Value.scorePercentage = 0;
        }
        Debug.Log("Reset all player scores to 0%");
    }
    
    public void RandomizeScores()
    {
        foreach (var player in GameManager.Instance.players)
        {
            player.Value.scorePercentage = Random.Range(-50, 150);
        }
        Debug.Log("Randomized all player scores");
    }
    
    public void ToggleTimer()
    {
        GameManager.Instance.timerActive = !GameManager.Instance.timerActive;
        Debug.Log("Timer " + (GameManager.Instance.timerActive ? "resumed" : "paused"));
    }
    
    // === COROUTINE HELPER ===
    
    IEnumerator<WaitForSeconds> yield(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    
    // === LOGGING ===
    
    public void LogGameState()
    {
        Debug.Log("=== GAME STATE ===");
        Debug.Log("Round: " + GameManager.Instance.currentRound);
        Debug.Log("Mode: " + GameManager.Instance.gameMode);
        Debug.Log("Players: " + GameManager.Instance.players.Count);
        Debug.Log("Timer: " + GameManager.Instance.GetTimeRemaining() + "s");
        Debug.Log("Current Scene: " + SceneManager.GetActiveScene().name);
    }
}
