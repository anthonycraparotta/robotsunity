using UnityEngine;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Unity Netcode-based NetworkManager for RWM multiplayer
/// Desktop acts as Host (Server + Client), mobile devices connect as Clients
/// </summary>
[RequireComponent(typeof(NetworkManager), typeof(NetworkObject), typeof(UnityTransport))]
public class RWMNetworkManager : NetworkBehaviour
{
    public static RWMNetworkManager Instance;

    [Header("Network Settings")]
    public string roomCode = "";
    public bool isHost = false;
    public ushort port = 7777;

    [Tooltip("IP address clients should use to connect when this device is hosting. Leave blank to use the transport's configured address.")]
    public string hostAddress = "";

    [Tooltip("Network interface address the host should bind to. Defaults to all interfaces.")]
    public string listenAddress = "0.0.0.0";

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
    private NetworkObject networkObject;
    private UnityTransport unityTransport;
    private bool componentsValidated;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[NetworkManager] Instance created");

            EnsureNetworkingComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!EnsureNetworkingComponents())
        {
            Debug.LogError("[NetworkManager] Missing required networking components.");
            enabled = false;
            return;
        }

        networkManager.NetworkConfig.NetworkTransport = unityTransport;

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
        if (!networkObject.IsSpawned)
        {
            networkObject.Spawn();
        }
        OnRoomCreated?.Invoke(roomCode);

        var gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null)
        {
            var gmNetworkObject = gameManager.GetComponent<NetworkObject>();

            if (gmNetworkObject != null && !gmNetworkObject.IsSpawned)
            {
                gmNetworkObject.Spawn();
                Debug.Log("[NetworkManager] Spawned GameManager NetworkObject on host start");
            }
        }
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
        // Bind to the specified listen address so remote connections are accepted.
        string advertisedAddress = hostAddress;

        if (string.IsNullOrWhiteSpace(advertisedAddress))
        {
            advertisedAddress = unityTransport.ConnectionData.Address;

            if (string.IsNullOrWhiteSpace(advertisedAddress))
            {
                advertisedAddress = "127.0.0.1";
            }
        }

        string bindAddress = string.IsNullOrWhiteSpace(listenAddress) ? "0.0.0.0" : listenAddress;

        unityTransport.SetConnectionData(advertisedAddress, port, bindAddress);

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
        unityTransport.SetConnectionData(hostIP, port);

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

    // REMOVED: Redundant sync methods - GameManager NetworkVariables handle replication automatically
    // NetworkVariables in GameManager already sync from server to clients automatically.
    // These manual RPC sync methods were duplicating that functionality and trying to
    // write to server-owned NetworkVariables from clients, which violates Netcode authority.
    //
    // If UI needs to react to state changes, use NetworkVariable.OnValueChanged callbacks
    // in GameManager instead of these RPCs.

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

    /// <summary>
    /// DEPRECATED: Use GameManager.SetPlayerScore() instead
    /// This method tried to update scores via RPC but wrote to a transient dictionary copy.
    /// GameManager's NetworkList<NetworkedPlayerData> handles replication automatically.
    /// </summary>
    [System.Obsolete("Use GameManager.SetPlayerScore() - NetworkList handles replication")]
    public void UpdateScore(string playerIdToUpdate, int newScore)
    {
        Debug.LogWarning("[RWMNetworkManager] UpdateScore is deprecated. Use GameManager.SetPlayerScore() instead.");

        if (!isHost || !IsSpawned) return;

        // Forward to proper server-authoritative method
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerScore(playerIdToUpdate, newScore);
        }
    }

    // === SCENE TRANSITIONS ===
    // Note: Scene transitions now handled by NetworkManager.SceneManager
    // OnSceneChanged event still available for custom logic

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

    private bool EnsureNetworkingComponents()
    {
        networkManager = GetComponent<NetworkManager>();
        networkObject = GetComponent<NetworkObject>();
        unityTransport = GetComponent<UnityTransport>();

        if (componentsValidated)
        {
            return networkManager != null && networkObject != null && unityTransport != null;
        }

        if (networkManager == null)
        {
            Debug.LogError("[NetworkManager] NetworkManager component is missing. Please ensure the Managers object in LoadingScreen includes a configured NetworkManager component.");
            return false;
        }

        if (networkObject == null)
        {
            networkObject = gameObject.AddComponent<NetworkObject>();
            Debug.LogWarning("[NetworkManager] NetworkObject component was missing and has been added at runtime. Update the Managers object in LoadingScreen to include it to avoid runtime creation.");
        }

        if (unityTransport == null)
        {
            unityTransport = gameObject.AddComponent<UnityTransport>();
            Debug.LogWarning("[NetworkManager] UnityTransport component was missing and has been added at runtime with default settings. Configure the transport on the Managers object in LoadingScreen so remote clients can connect.");
        }

        componentsValidated = true;
        return true;
    }
}
