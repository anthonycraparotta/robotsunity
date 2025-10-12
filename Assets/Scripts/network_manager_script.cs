using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Unity Netcode-based NetworkManager for RWM multiplayer
/// Desktop acts as Host (Server + Client), mobile devices connect as Clients
/// </summary>
public class RWMNetworkManager : NetworkBehaviour
{
    public static RWMNetworkManager Instance;

    [Header("Network Settings")]
    public string roomCode = "";
    public bool isHost = false;
    public ushort port = 7777;

    [Header("Connection Status")]
    public bool isConnected = false;
    public string playerId = "";

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

    private NetworkManager networkManager;

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
        // Get or add Unity's NetworkManager component
        networkManager = GetComponent<NetworkManager>();
        if (networkManager == null)
        {
            networkManager = gameObject.AddComponent<NetworkManager>();
        }

        // Setup Unity Transport
        var transport = GetComponent<UnityTransport>();
        if (transport == null)
        {
            transport = gameObject.AddComponent<UnityTransport>();
            networkManager.NetworkConfig.NetworkTransport = transport;
        }

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

        // Register connection callbacks
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        networkManager.OnServerStarted += OnServerStarted;
    }

    // === CONNECTION CALLBACKS ===

    private void OnServerStarted()
    {
        Debug.Log("[NetworkManager] Server started successfully");
        isConnected = true;
        isHost = true;
        OnRoomCreated?.Invoke(roomCode);
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetworkManager] Client connected: {clientId}");

        if (NetworkManager.Singleton.IsClient && clientId == NetworkManager.Singleton.LocalClientId)
        {
            isConnected = true;
            OnRoomJoined?.Invoke(roomCode);
        }

        OnPlayerJoined?.Invoke(clientId.ToString());
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NetworkManager] Client disconnected: {clientId}");

        if (NetworkManager.Singleton.IsClient && clientId == NetworkManager.Singleton.LocalClientId)
        {
            isConnected = false;
            OnConnectionError?.Invoke();
        }

        OnPlayerLeft?.Invoke(clientId.ToString());
    }

    // === HOST METHODS ===

    public void StartHost()
    {
        isHost = true;

        // Generate room code
        GenerateRoomCode();

        // Start as Host (Server + Client)
        var transport = networkManager.GetComponent<UnityTransport>();
        transport.SetConnectionData("127.0.0.1", port);

        bool success = networkManager.StartHost();

        if (success)
        {
            Debug.Log($"[NetworkManager] Host started with room code: {roomCode}");
        }
        else
        {
            Debug.LogError("[NetworkManager] Failed to start host");
            OnConnectionError?.Invoke();
        }
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

    public bool JoinGame(string code, string hostIP = "127.0.0.1")
    {
        isHost = false;
        roomCode = code.ToUpper();

        // Set connection data to host's IP
        var transport = networkManager.GetComponent<UnityTransport>();
        transport.SetConnectionData(hostIP, port);

        // Start as Client
        bool success = networkManager.StartClient();

        if (success)
        {
            Debug.Log($"[NetworkManager] Attempting to join room: {roomCode} at {hostIP}:{port}");
        }
        else
        {
            Debug.LogError("[NetworkManager] Failed to start client");
            OnConnectionError?.Invoke();
        }

        return success;
    }

    // === PLAYER MANAGEMENT ===

    public void AddPlayer(string playerName, string iconName)
    {
        if (!IsSpawned) return;

        AddPlayerServerRpc(playerId, playerName, iconName);
        Debug.Log($"[NetworkManager] Adding player: {playerName}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerServerRpc(string playerIdParam, string playerName, string iconName, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        // Add to GameManager on server
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayer(playerIdParam, playerName, iconName, clientId);
        }

        // Broadcast to all clients
        AddPlayerClientRpc(playerIdParam, playerName, iconName);
    }

    [ClientRpc]
    private void AddPlayerClientRpc(string playerIdParam, string playerName, string iconName)
    {
        OnPlayerAdded?.Invoke(playerIdParam, playerName, iconName);

        // Add to GameManager on all clients
        if (GameManager.Instance != null && !GameManager.Instance.players.ContainsKey(playerIdParam))
        {
            GameManager.Instance.AddPlayer(playerIdParam, playerName, iconName);
        }
    }

    public void RemovePlayer(string playerIdToRemove)
    {
        if (!IsSpawned) return;

        RemovePlayerServerRpc(playerIdToRemove);
        Debug.Log($"[NetworkManager] Removing player: {playerIdToRemove}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerServerRpc(string playerIdToRemove)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RemovePlayer(playerIdToRemove);
        }

        RemovePlayerClientRpc(playerIdToRemove);
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(string playerIdToRemove)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RemovePlayer(playerIdToRemove);
        }
    }

    // === GAME STATE SYNC ===

    public void SyncGameState(GameManager.GameState state)
    {
        if (!isHost || !IsSpawned) return;

        SyncGameStateClientRpc(state.ToString());
    }

    [ClientRpc]
    private void SyncGameStateClientRpc(string gameState)
    {
        if (isHost) return; // Host already has the state

        OnGameStateChanged?.Invoke(gameState);

        if (GameManager.Instance != null && !string.IsNullOrEmpty(gameState))
        {
            GameManager.GameState state = (GameManager.GameState)Enum.Parse(typeof(GameManager.GameState), gameState);
            GameManager.Instance.currentGameState = state;
        }
    }

    public void SyncTimer(float timerValue, bool isActive)
    {
        if (!isHost || !IsSpawned) return;

        SyncTimerClientRpc(timerValue, isActive);
    }

    [ClientRpc]
    private void SyncTimerClientRpc(float timerValue, bool isActive)
    {
        if (isHost) return; // Host already has the timer

        OnTimerSync?.Invoke(timerValue, isActive);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentTimerValue = timerValue;
            GameManager.Instance.timerActive = isActive;
        }
    }

    public void SyncCurrentRound(int round)
    {
        if (!isHost || !IsSpawned) return;

        SyncCurrentRoundClientRpc(round);
    }

    [ClientRpc]
    private void SyncCurrentRoundClientRpc(int round)
    {
        if (isHost) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentRound = round;
        }
    }

    // === ANSWER SUBMISSION ===

    public void SubmitAnswer(string answer)
    {
        if (!IsSpawned) return;

        SubmitAnswerServerRpc(playerId, answer);
        Debug.Log($"[NetworkManager] Submitting answer: {answer}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitAnswerServerRpc(string playerIdParam, string answer)
    {
        OnAnswerSubmitted?.Invoke(playerIdParam, answer);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubmitPlayerAnswer(playerIdParam, answer);
        }

        // Broadcast to all clients
        SubmitAnswerClientRpc(playerIdParam, answer);
    }

    [ClientRpc]
    private void SubmitAnswerClientRpc(string playerIdParam, string answer)
    {
        if (isHost) return; // Host already processed

        OnAnswerSubmitted?.Invoke(playerIdParam, answer);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubmitPlayerAnswer(playerIdParam, answer);
        }
    }

    // === VOTING ===

    public void SubmitEliminationVote(string votedAnswer)
    {
        if (!IsSpawned) return;

        SubmitEliminationVoteServerRpc(playerId, votedAnswer);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitEliminationVoteServerRpc(string playerIdParam, string votedAnswer)
    {
        OnVoteSubmitted?.Invoke(playerIdParam, votedAnswer);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubmitEliminationVote(playerIdParam, votedAnswer);
        }

        SubmitEliminationVoteClientRpc(playerIdParam, votedAnswer);
    }

    [ClientRpc]
    private void SubmitEliminationVoteClientRpc(string playerIdParam, string votedAnswer)
    {
        if (isHost) return;

        OnVoteSubmitted?.Invoke(playerIdParam, votedAnswer);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubmitEliminationVote(playerIdParam, votedAnswer);
        }
    }

    public void SubmitVotingVote(string votedAnswer)
    {
        if (!IsSpawned) return;

        SubmitVotingVoteServerRpc(playerId, votedAnswer);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitVotingVoteServerRpc(string playerIdParam, string votedAnswer)
    {
        OnVoteSubmitted?.Invoke(playerIdParam, votedAnswer);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubmitVotingVote(playerIdParam, votedAnswer);
        }

        SubmitVotingVoteClientRpc(playerIdParam, votedAnswer);
    }

    [ClientRpc]
    private void SubmitVotingVoteClientRpc(string playerIdParam, string votedAnswer)
    {
        if (isHost) return;

        OnVoteSubmitted?.Invoke(playerIdParam, votedAnswer);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubmitVotingVote(playerIdParam, votedAnswer);
        }
    }

    public void SubmitBonusVote(string votedPlayerID)
    {
        if (!IsSpawned) return;

        SubmitBonusVoteServerRpc(playerId, votedPlayerID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitBonusVoteServerRpc(string playerIdParam, string votedPlayerID)
    {
        OnVoteSubmitted?.Invoke(playerIdParam, votedPlayerID);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubmitBonusVote(playerIdParam, votedPlayerID);
        }

        SubmitBonusVoteClientRpc(playerIdParam, votedPlayerID);
    }

    [ClientRpc]
    private void SubmitBonusVoteClientRpc(string playerIdParam, string votedPlayerID)
    {
        if (isHost) return;

        OnVoteSubmitted?.Invoke(playerIdParam, votedPlayerID);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubmitBonusVote(playerIdParam, votedPlayerID);
        }
    }

    // === SCORE UPDATES ===

    public void UpdateScore(string playerIdToUpdate, int newScore)
    {
        if (!isHost || !IsSpawned) return;

        UpdateScoreClientRpc(playerIdToUpdate, newScore);
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(string playerIdToUpdate, int newScore)
    {
        OnScoreUpdated?.Invoke(playerIdToUpdate, newScore);

        if (GameManager.Instance != null && GameManager.Instance.players.ContainsKey(playerIdToUpdate))
        {
            GameManager.Instance.players[playerIdToUpdate].scorePercentage = newScore;
        }
    }

    // === SCENE TRANSITIONS ===

    public void ChangeScene(string sceneName)
    {
        if (!isHost || !IsSpawned) return;

        ChangeSceneClientRpc(sceneName);
    }

    [ClientRpc]
    private void ChangeSceneClientRpc(string sceneName)
    {
        OnSceneChanged?.Invoke(sceneName);

        if (!isHost) // Clients follow host's scene changes
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    // === ROOM CODE ACCESS ===

    public string GetRoomCode()
    {
        return roomCode;
    }

    // === CLEANUP ===

    void OnApplicationQuit()
    {
        if (networkManager != null && networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }

    void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            networkManager.OnServerStarted -= OnServerStarted;

            if (networkManager.IsListening)
            {
                networkManager.Shutdown();
            }
        }
    }
}
