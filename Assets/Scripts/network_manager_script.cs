using UnityEngine;
using System;
using System.Collections.Generic;
using NativeWebSocket;

/// <summary>
/// WebSocket-based NetworkManager for RWM multiplayer
/// Connects to Node.js server for room management and game state sync
/// </summary>
public class RWMNetworkManager : MonoBehaviour
{
    public static RWMNetworkManager Instance;

    [Header("Network Settings")]
    public string serverUrl = "ws://localhost:3000"; // Change to your server URL
    public string roomCode = "";
    public bool isHost = false;

    [Header("Connection Status")]
    public bool isConnected = false;
    public string playerId = "";

    private WebSocket websocket;

    // Event delegates for network messages
    public event Action<string> OnRoomCreated;
    public event Action<string> OnRoomJoined;
    public event Action<string> OnPlayerJoined;
    public event Action<string> OnPlayerLeft;
    public event Action<string, string, string> OnPlayerAdded; // playerID, playerName, iconName
    public event Action<string> OnGameStateChanged;
    public event Action<string, string> OnAnswerSubmitted; // playerID, answer
    public event Action<string, string> OnVoteSubmitted; // playerID, votedTarget
    public event Action<string, int> OnScoreUpdated; // playerID, newScore
    public event Action<string> OnSceneChanged; // sceneName
    public event Action<float, bool> OnTimerSync; // timerValue, isActive
    public event Action OnConnectionError;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[NetworkManager] Instance created");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load or generate player ID
        if (PlayerPrefs.HasKey("PlayerID"))
        {
            playerId = PlayerPrefs.GetString("PlayerID");
        }
        else
        {
            playerId = "player_" + UnityEngine.Random.Range(100000000, 999999999);
            PlayerPrefs.SetString("PlayerID", playerId);
        }

        Debug.Log($"[NetworkManager] Player ID: {playerId}");
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        // Dispatch WebSocket messages on main thread (not needed for WebGL)
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
        #endif
    }

    // === CONNECTION METHODS ===

    public async void Connect()
    {
        if (isConnected)
        {
            Debug.LogWarning("[NetworkManager] Already connected");
            return;
        }

        Debug.Log($"[NetworkManager] Connecting to {serverUrl}");

        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("[NetworkManager] WebSocket connected!");
            isConnected = true;
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"[NetworkManager] WebSocket error: {e}");
            isConnected = false;
            OnConnectionError?.Invoke();
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log($"[NetworkManager] WebSocket closed: {e}");
            isConnected = false;
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };

        await websocket.Connect();
    }

    public async void Disconnect()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            Debug.Log("[NetworkManager] Disconnecting...");
            await websocket.Close();
            isConnected = false;
        }
    }

    // === HOST METHODS ===

    public void StartHost()
    {
        if (!isConnected)
        {
            Debug.LogError("[NetworkManager] Cannot start host - not connected to server");
            Connect();
            return;
        }

        isHost = true;

        // Generate room code
        GenerateRoomCode();

        // Send create room request to server
        var message = new NetworkMessage
        {
            type = "create_room",
            roomCode = roomCode,
            playerId = playerId
        };

        SendMessage(message);
        Debug.Log($"[NetworkManager] Host started with room code: {roomCode}");
    }

    void GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Text.StringBuilder code = new System.Text.StringBuilder();

        for (int i = 0; i < 5; i++)
        {
            code.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
        }

        roomCode = code.ToString();
    }

    // === CLIENT METHODS ===

    public bool JoinGame(string code)
    {
        if (!isConnected)
        {
            Debug.LogError("[NetworkManager] Cannot join game - not connected to server");
            Connect();
            return false;
        }

        isHost = false;
        roomCode = code.ToUpper();

        // Send join room request to server
        var message = new NetworkMessage
        {
            type = "join_room",
            roomCode = roomCode,
            playerId = playerId
        };

        SendMessage(message);
        Debug.Log($"[NetworkManager] Attempting to join room: {roomCode}");
        return true;
    }

    // === PLAYER MANAGEMENT ===

    public void AddPlayer(string playerName, string iconName)
    {
        var message = new NetworkMessage
        {
            type = "add_player",
            roomCode = roomCode,
            playerId = playerId,
            playerName = playerName,
            iconName = iconName
        };

        SendMessage(message);
        Debug.Log($"[NetworkManager] Adding player: {playerName}");
    }

    public void RemovePlayer(string playerIdToRemove)
    {
        var message = new NetworkMessage
        {
            type = "remove_player",
            roomCode = roomCode,
            playerId = playerIdToRemove
        };

        SendMessage(message);
        Debug.Log($"[NetworkManager] Removing player: {playerIdToRemove}");
    }

    // === GAME STATE SYNC ===

    public void SyncGameState(GameManager.GameState state)
    {
        if (!isHost) return;

        var message = new NetworkMessage
        {
            type = "game_state",
            roomCode = roomCode,
            gameState = state.ToString()
        };

        SendMessage(message);
    }

    public void SyncTimer(float timerValue, bool isActive)
    {
        if (!isHost) return;

        var message = new NetworkMessage
        {
            type = "timer_sync",
            roomCode = roomCode,
            timerValue = timerValue,
            timerActive = isActive
        };

        SendMessage(message);
    }

    public void SyncCurrentRound(int round)
    {
        if (!isHost) return;

        var message = new NetworkMessage
        {
            type = "round_sync",
            roomCode = roomCode,
            currentRound = round
        };

        SendMessage(message);
    }

    // === ANSWER SUBMISSION ===

    public void SubmitAnswer(string answer)
    {
        var message = new NetworkMessage
        {
            type = "submit_answer",
            roomCode = roomCode,
            playerId = playerId,
            answer = answer
        };

        SendMessage(message);
        Debug.Log($"[NetworkManager] Submitting answer: {answer}");
    }

    // === VOTING ===

    public void SubmitEliminationVote(string votedAnswer)
    {
        var message = new NetworkMessage
        {
            type = "elimination_vote",
            roomCode = roomCode,
            playerId = playerId,
            votedAnswer = votedAnswer
        };

        SendMessage(message);
    }

    public void SubmitVotingVote(string votedAnswer)
    {
        var message = new NetworkMessage
        {
            type = "voting_vote",
            roomCode = roomCode,
            playerId = playerId,
            votedAnswer = votedAnswer
        };

        SendMessage(message);
    }

    public void SubmitBonusVote(string votedPlayerID)
    {
        var message = new NetworkMessage
        {
            type = "bonus_vote",
            roomCode = roomCode,
            playerId = playerId,
            votedPlayerID = votedPlayerID
        };

        SendMessage(message);
    }

    // === SCORE UPDATES ===

    public void UpdateScore(string playerIdToUpdate, int newScore)
    {
        if (!isHost) return;

        var message = new NetworkMessage
        {
            type = "score_update",
            roomCode = roomCode,
            playerId = playerIdToUpdate,
            score = newScore
        };

        SendMessage(message);
    }

    // === SCENE TRANSITIONS ===

    public void ChangeScene(string sceneName)
    {
        if (!isHost) return;

        var message = new NetworkMessage
        {
            type = "scene_change",
            roomCode = roomCode,
            sceneName = sceneName
        };

        SendMessage(message);
    }

    // === MESSAGE HANDLING ===

    void HandleMessage(string messageJson)
    {
        try
        {
            NetworkMessage message = JsonUtility.FromJson<NetworkMessage>(messageJson);

            Debug.Log($"[NetworkManager] Received: {message.type}");

            switch (message.type)
            {
                case "room_created":
                    OnRoomCreated?.Invoke(message.roomCode);
                    break;

                case "room_joined":
                    OnRoomJoined?.Invoke(message.roomCode);
                    break;

                case "player_joined":
                    OnPlayerJoined?.Invoke(message.playerId);
                    break;

                case "player_left":
                    OnPlayerLeft?.Invoke(message.playerId);
                    break;

                case "player_added":
                    OnPlayerAdded?.Invoke(message.playerId, message.playerName, message.iconName);
                    // Add to GameManager
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddPlayer(message.playerId, message.playerName, message.iconName);
                    }
                    break;

                case "game_state":
                    OnGameStateChanged?.Invoke(message.gameState);
                    // Update GameManager
                    if (GameManager.Instance != null && !string.IsNullOrEmpty(message.gameState))
                    {
                        GameManager.GameState state = (GameManager.GameState)Enum.Parse(typeof(GameManager.GameState), message.gameState);
                        GameManager.Instance.currentGameState = state;
                    }
                    break;

                case "answer_submitted":
                    OnAnswerSubmitted?.Invoke(message.playerId, message.answer);
                    // Process in GameManager
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SubmitPlayerAnswer(message.playerId, message.answer);
                    }
                    break;

                case "elimination_vote":
                    OnVoteSubmitted?.Invoke(message.playerId, message.votedAnswer);
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SubmitEliminationVote(message.playerId, message.votedAnswer);
                    }
                    break;

                case "voting_vote":
                    OnVoteSubmitted?.Invoke(message.playerId, message.votedAnswer);
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SubmitVotingVote(message.playerId, message.votedAnswer);
                    }
                    break;

                case "bonus_vote":
                    OnVoteSubmitted?.Invoke(message.playerId, message.votedPlayerID);
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SubmitBonusVote(message.playerId, message.votedPlayerID);
                    }
                    break;

                case "score_update":
                    OnScoreUpdated?.Invoke(message.playerId, message.score);
                    if (GameManager.Instance != null && GameManager.Instance.players.ContainsKey(message.playerId))
                    {
                        GameManager.Instance.players[message.playerId].scorePercentage = message.score;
                    }
                    break;

                case "scene_change":
                    OnSceneChanged?.Invoke(message.sceneName);
                    if (!isHost) // Clients follow host's scene changes
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(message.sceneName);
                    }
                    break;

                case "timer_sync":
                    OnTimerSync?.Invoke(message.timerValue, message.timerActive);
                    if (GameManager.Instance != null && !isHost) // Only clients update from sync
                    {
                        GameManager.Instance.currentTimerValue = message.timerValue;
                        GameManager.Instance.timerActive = message.timerActive;
                    }
                    break;

                case "round_sync":
                    if (GameManager.Instance != null && !isHost)
                    {
                        GameManager.Instance.currentRound = message.currentRound;
                    }
                    break;

                case "error":
                    Debug.LogError($"[NetworkManager] Server error: {message.error}");
                    break;

                default:
                    Debug.LogWarning($"[NetworkManager] Unknown message type: {message.type}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkManager] Error parsing message: {e.Message}\nMessage: {messageJson}");
        }
    }

    async void SendMessage(NetworkMessage message)
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            Debug.LogError("[NetworkManager] Cannot send message - not connected");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(message);
            await websocket.SendText(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkManager] Error sending message: {e.Message}");
        }
    }

    // === ROOM CODE ACCESS ===

    public string GetRoomCode()
    {
        return roomCode;
    }

    // === CLEANUP ===

    async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    void OnDestroy()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.Close();
        }
    }
}

/// <summary>
/// Network message structure for WebSocket communication
/// Must match server-side message format
/// </summary>
[Serializable]
public class NetworkMessage
{
    public string type;
    public string roomCode;
    public string playerId;
    public string playerName;
    public string iconName;
    public string gameState;
    public string answer;
    public string votedAnswer;
    public string votedPlayerID;
    public int score;
    public string sceneName;
    public float timerValue;
    public bool timerActive;
    public int currentRound;
    public string error;
}
