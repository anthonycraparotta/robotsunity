using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

public class RWMNetworkManager : NetworkBehaviour
{
    public static RWMNetworkManager Instance;
    
    [Header("Network Settings")]
    public string roomCode = "";
    public bool isHost = false;
    
    // Network Variables for syncing
    private NetworkVariable<FixedString128Bytes> networkRoomCode = new NetworkVariable<FixedString128Bytes>();
    private NetworkVariable<int> networkCurrentRound = new NetworkVariable<int>();
    private NetworkVariable<float> networkTimerValue = new NetworkVariable<float>();
    private NetworkVariable<bool> networkTimerActive = new NetworkVariable<bool>();
    
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
    
    void Start()
    {
        // Subscribe to network events only if NetworkManager exists
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            Debug.LogWarning("NetworkManager.Singleton is null - networking features disabled. Add a NetworkManager GameObject to enable multiplayer.");
        }
    }
    
    // === HOST METHODS ===
    
    public void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Cannot start host - NetworkManager.Singleton is null");
            return;
        }

        isHost = true;
        NetworkManager.Singleton.StartHost();

        // Generate room code
        GenerateRoomCode();

        Debug.Log("Host started with room code: " + roomCode);
    }
    
    void GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Text.StringBuilder code = new System.Text.StringBuilder();
        
        for (int i = 0; i < 4; i++)
        {
            code.Append(chars[Random.Range(0, chars.Length)]);
        }
        
        roomCode = code.ToString();
        
        if (IsServer)
        {
            networkRoomCode.Value = roomCode;
        }
    }
    
    // === CLIENT METHODS ===
    
    public void JoinGame(string code)
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Cannot join game - NetworkManager.Singleton is null");
            return;
        }

        isHost = false;
        roomCode = code;

        // In a real implementation, this would connect to a relay/matchmaking service
        // For local testing, just start as client
        NetworkManager.Singleton.StartClient();

        Debug.Log("Attempting to join room: " + code);
    }
    
    // === PLAYER MANAGEMENT ===
    
    void OnClientConnected(ulong clientId)
    {
        Debug.Log("Client connected: " + clientId);
        
        if (IsServer)
        {
            // Server side: Track the new player
            // Send current game state to new player
            SyncGameStateToClient(clientId);
        }
    }
    
    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("Client disconnected: " + clientId);
        
        if (IsServer)
        {
            // Remove player from game
            RemovePlayerFromGame(clientId);
        }
    }
    
    void RemovePlayerFromGame(ulong clientId)
    {
        // Find and remove player with this clientId
        string playerIdToRemove = "";

        foreach (var player in GameManager.Instance.players)
        {
            if (player.Value.clientId == clientId)
            {
                playerIdToRemove = player.Key;
                break;
            }
        }

        if (!string.IsNullOrEmpty(playerIdToRemove))
        {
            GameManager.Instance.RemovePlayer(playerIdToRemove);
            Debug.Log($"Removed player {playerIdToRemove} due to disconnect (clientId: {clientId})");
        }
    }
    
    // === GAME STATE SYNC ===
    
    void SyncGameStateToClient(ulong clientId)
    {
        // Send current round, timer, scores to newly connected client
        // This ensures late joiners get up to speed
    }
    
    public void SyncTimerToClients()
    {
        if (!IsServer) return;
        
        networkTimerValue.Value = GameManager.Instance.currentTimerValue;
        networkTimerActive.Value = GameManager.Instance.timerActive;
    }
    
    void Update()
    {
        // When the networking stack isn't running we shouldn't push/pull state,
        // otherwise local single-player sessions get overwritten with default values.
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            return;
        }

        if (IsServer)
        {
            // Host continuously syncs timer
            SyncTimerToClients();

            // Sync round number
            networkCurrentRound.Value = GameManager.Instance.currentRound;
        }
        else if (IsClient)
        {
            // Clients update their local GameManager from network
            GameManager.Instance.currentTimerValue = networkTimerValue.Value;
            GameManager.Instance.timerActive = networkTimerActive.Value;
            GameManager.Instance.currentRound = networkCurrentRound.Value;
        }
    }
    
    // === PLAYER DATA SYNC ===
    
    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(string playerID, string playerName, string iconName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Add player on server with clientId
        GameManager.Instance.AddPlayer(playerID, playerName, iconName, clientId);

        // Broadcast to all clients
        AddPlayerClientRpc(playerID, playerName, iconName, clientId);
    }

    [ClientRpc]
    void AddPlayerClientRpc(string playerID, string playerName, string iconName, ulong clientId)
    {
        // Add player on all clients
        if (!IsServer) // Don't duplicate on server
        {
            GameManager.Instance.AddPlayer(playerID, playerName, iconName, clientId);
        }
    }
    
    // === ANSWER SUBMISSION ===
    
    [ServerRpc(RequireOwnership = false)]
    public void SubmitAnswerServerRpc(string playerID, string answer)
    {
        // Process answer on server
        GameManager.Instance.SubmitPlayerAnswer(playerID, answer);
        
        // Broadcast to all clients for UI updates
        UpdateAnswerSubmittedClientRpc(playerID);
    }
    
    [ClientRpc]
    void UpdateAnswerSubmittedClientRpc(string playerID)
    {
        // Update UI to show player has submitted
        Debug.Log("Player " + playerID + " submitted their answer");
    }
    
    // === ELIMINATION VOTING ===
    
    [ServerRpc(RequireOwnership = false)]
    public void SubmitEliminationVoteServerRpc(string playerID, string votedAnswer)
    {
        // Process vote on server
        GameManager.Instance.SubmitEliminationVote(playerID, votedAnswer);
    }
    
    // === VOTING ===
    
    [ServerRpc(RequireOwnership = false)]
    public void SubmitVotingVoteServerRpc(string playerID, string votedAnswer)
    {
        // Process vote on server
        GameManager.Instance.SubmitVotingVote(playerID, votedAnswer);
    }
    
    // === BONUS ROUND ===
    
    [ServerRpc(RequireOwnership = false)]
    public void SubmitBonusVoteServerRpc(string playerID, string votedPlayerID)
    {
        // Process bonus vote on server
        GameManager.Instance.SubmitBonusVote(playerID, votedPlayerID);
    }
    
    // === SCORE UPDATES ===
    
    [ClientRpc]
    public void UpdateScoresClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // Tell all clients to refresh score displays
        Debug.Log("Scores updated");
    }
    
    // === SCENE TRANSITIONS ===
    
    [ClientRpc]
    public void ChangeSceneClientRpc(string sceneName)
    {
        // All clients change scene together
        if (!IsServer) // Server changes scene separately
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
    
    // === ROOM CODE ACCESS ===
    
    public string GetRoomCode()
    {
        if (IsServer)
        {
            return roomCode;
        }
        else
        {
            return networkRoomCode.Value.ToString();
        }
    }
    
    // === CLEANUP ===

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
