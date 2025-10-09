using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles player authentication, ID management, and mapping between network clients and game players.
/// Persists player data across scenes.
/// </summary>
public class PlayerAuthSystem : NetworkBehaviour
{
    public static PlayerAuthSystem Instance;
    
    [Header("Local Player Info")]
    public string localPlayerID = "";
    public string localPlayerName = "";
    public string localPlayerIcon = "";
    
    [Header("Network Mapping")]
    // Maps network client ID to game player ID
    private NetworkVariable<NetworkPlayerData> networkPlayerData = new NetworkVariable<NetworkPlayerData>();
    
    // Client-side storage
    private Dictionary<ulong, string> clientToPlayerMap = new Dictionary<ulong, string>();
    
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
        // Generate or load persistent player ID
        InitializeLocalPlayer();
    }
    
    // === LOCAL PLAYER INITIALIZATION ===
    
    void InitializeLocalPlayer()
    {
        // Check if player ID is saved in PlayerPrefs
        if (PlayerPrefs.HasKey("PlayerID"))
        {
            localPlayerID = PlayerPrefs.GetString("PlayerID");
            Debug.Log("Loaded existing player ID: " + localPlayerID);
        }
        else
        {
            // Generate new player ID
            localPlayerID = GeneratePlayerID();
            PlayerPrefs.SetString("PlayerID", localPlayerID);
            PlayerPrefs.Save();
            Debug.Log("Generated new player ID: " + localPlayerID);
        }
    }
    
    string GeneratePlayerID()
    {
        // Use device unique identifier combined with timestamp for uniqueness
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        string timestamp = System.DateTime.UtcNow.Ticks.ToString();
        
        // Create a hash to make it shorter and more readable
        int hash = (deviceID + timestamp).GetHashCode();
        return "player_" + Mathf.Abs(hash).ToString();
    }
    
    // === PLAYER REGISTRATION ===
    
    public void RegisterPlayer(string playerName, string playerIcon)
    {
        localPlayerName = playerName;
        localPlayerIcon = playerIcon;

        // Save to PlayerPrefs for persistence
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetString("PlayerIcon", playerIcon);
        PlayerPrefs.Save();
        
        Debug.Log($"Player registered: {playerName} ({localPlayerID})");

        // If connected to network, send to server
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            RegisterPlayerServerRpc(localPlayerID, playerName, playerIcon);
        }
        else if (GameManager.Instance != null)
        {
            // Offline fallback so local sessions still work
            GameManager.Instance.AddPlayer(localPlayerID, playerName, playerIcon);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RegisterPlayerServerRpc(string playerID, string playerName, string playerIcon, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Map network client ID to game player ID
        if (!clientToPlayerMap.ContainsKey(clientId))
        {
            clientToPlayerMap.Add(clientId, playerID);
        }

        // Add player to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayer(playerID, playerName, playerIcon, clientId);
        }

        // Broadcast to all clients
        SyncPlayerToClientsClientRpc(playerID, playerName, playerIcon, clientId);

        Debug.Log($"Server registered player: {playerName} (Client: {clientId}, Player: {playerID})");
    }

    [ClientRpc]
    void SyncPlayerToClientsClientRpc(string playerID, string playerName, string playerIcon, ulong clientId)
    {
        // Update local GameManager on all clients
        if (GameManager.Instance != null && !IsServer)
        {
            GameManager.Instance.AddPlayer(playerID, playerName, playerIcon, clientId);
        }
    }
    
    // === PLAYER ID RETRIEVAL ===
    
    public string GetLocalPlayerID()
    {
        return localPlayerID;
    }
    
    public string GetLocalPlayerName()
    {
        return localPlayerName;
    }
    
    public string GetLocalPlayerIcon()
    {
        return localPlayerIcon;
    }
    
    public string GetPlayerIDFromClientID(ulong clientId)
    {
        if (clientToPlayerMap.ContainsKey(clientId))
        {
            return clientToPlayerMap[clientId];
        }
        
        Debug.LogWarning($"No player ID found for client {clientId}");
        return "";
    }
    
    // === NETWORK DISCONNECT HANDLING ===
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    
    void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        
        // Get player ID for this client
        string playerID = GetPlayerIDFromClientID(clientId);
        
        if (!string.IsNullOrEmpty(playerID))
        {
            // Remove from game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RemovePlayer(playerID);
            }
            
            // Remove from mapping
            clientToPlayerMap.Remove(clientId);
            
            Debug.Log($"Player {playerID} disconnected (Client: {clientId})");
        }
    }
    
    // === UTILITY ===
    
    public bool IsPlayerRegistered()
    {
        return !string.IsNullOrEmpty(localPlayerName) && !string.IsNullOrEmpty(localPlayerIcon);
    }
    
    public void ClearPlayerData()
    {
        localPlayerID = "";
        localPlayerName = "";
        localPlayerIcon = "";
        
        PlayerPrefs.DeleteKey("PlayerID");
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("PlayerIcon");
        PlayerPrefs.Save();
    }
}

// === NETWORK DATA STRUCTURE ===

[System.Serializable]
public struct NetworkPlayerData : INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes playerID;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerIcon;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerIcon);
    }
}
