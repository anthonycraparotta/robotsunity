using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using RobotsGame.Data;
using NativeWebSocket;

namespace RobotsGame.Network
{
    /// <summary>
    /// Manages network communication with game server via WebSocket.
    /// Handles host/client synchronization and real-time game events.
    /// Based on unityspec.md NETWORK EVENTS section.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Connection Settings")]
        [SerializeField] private string serverUrl = "ws://localhost:3000";
        [SerializeField] private bool isHost = false;
        [SerializeField] private float hostSyncDelayMs = 500f;
        [SerializeField] private float mobileFollowDelayMs = 800f;

        [Header("Room Settings")]
        [SerializeField] private string roomCode;
        [SerializeField] private string playerName;
        [SerializeField] private string playerIcon;

        // Network Events
        [System.Serializable] public class NetworkEvent : UnityEvent { }
        [System.Serializable] public class NetworkEventWithData : UnityEvent<string> { }

        [Header("Connection Events")]
        public NetworkEvent OnConnected;
        public NetworkEvent OnDisconnected;
        public NetworkEventWithData OnError;

        [Header("Room Events")]
        public NetworkEventWithData OnRoomJoined;
        public NetworkEventWithData OnPlayerJoined;
        public NetworkEventWithData OnPlayerLeft;
        public NetworkEventWithData OnPlayersUpdate;
        public NetworkEventWithData OnRoomStateUpdate;

        [Header("Game Phase Events")]
        public NetworkEventWithData OnRoundStarted;
        public NetworkEventWithData OnPlayerAnswered;
        public NetworkEventWithData OnAllAnswersSubmitted;
        public NetworkEventWithData OnEliminationVoteCast;
        public NetworkEventWithData OnEliminationComplete;
        public NetworkEventWithData OnStartVotingPhase;
        public NetworkEventWithData OnFinalVoteCast;
        public NetworkEventWithData OnAllVotesSubmitted;
        public NetworkEventWithData OnFinalRoundScores;

        [Header("Bonus Round Events")]
        public NetworkEventWithData OnBonusRoundStarted;
        public NetworkEventWithData OnBonusPlayerVoted;
        public NetworkEventWithData OnBonusVotesComplete;

        // WebSocket connection (NativeWebSocket)
        private WebSocket webSocket;
        private bool isConnected = false;
        private List<Player> connectedPlayers = new List<Player>();

        // Pending events queue
        private Queue<Action> pendingEvents = new Queue<Action>();

        public bool IsHost => isHost;
        public bool IsConnected => isConnected;
        public string RoomCode => roomCode;
        public List<Player> ConnectedPlayers => new List<Player>(connectedPlayers);
        public string LocalPlayerName => playerName;
        public string LocalPlayerIcon => playerIcon;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            webSocket?.DispatchMessageQueue();
#endif

            // Process pending events on main thread
            while (pendingEvents.Count > 0)
            {
                pendingEvents.Dequeue()?.Invoke();
            }
        }

        /// <summary>
        /// Connects to game server.
        /// </summary>
        public async void Connect(string url = null)
        {
            if (isConnected ||
                (webSocket != null &&
                (webSocket.State == WebSocketState.Connecting || webSocket.State == WebSocketState.Open)))
            {
                Debug.LogWarning("Already connected to server");
                return;
            }

            string connectionUrl = string.IsNullOrEmpty(url) ? serverUrl : url;
            Debug.Log($"Connecting to server: {connectionUrl}");

            CleanupWebSocket();

            webSocket = new WebSocket(connectionUrl);
            webSocket.OnOpen += HandleWebSocketOpen;
            webSocket.OnMessage += HandleWebSocketMessage;
            webSocket.OnError += HandleWebSocketError;
            webSocket.OnClose += HandleWebSocketClose;

            try
            {
                await webSocket.Connect();
            }
            catch (Exception e)
            {
                HandleWebSocketError($"Failed to connect: {e.Message}");
                CleanupWebSocket();
            }
        }

        /// <summary>
        /// Disconnects from game server.
        /// </summary>
        public async void Disconnect()
        {
            if (webSocket == null && !isConnected)
            {
                return;
            }

            Debug.Log("Disconnecting from server");

            if (webSocket != null)
            {
                try
                {
                    await webSocket.Close();
                }
                catch (Exception e)
                {
                    HandleWebSocketError($"Error during disconnect: {e.Message}");
                }
            }
            else if (isConnected)
            {
                pendingEvents.Enqueue(() =>
                {
                    isConnected = false;
                    OnDisconnected?.Invoke();
                });
            }
        }

        /// <summary>
        /// Creates a new game room as host.
        /// </summary>
        public void CreateRoom(string playerName, string playerIcon)
        {
            if (!isConnected)
            {
                Debug.LogError("Cannot create room: Not connected to server");
                return;
            }

            isHost = true;
            this.playerName = playerName;
            this.playerIcon = playerIcon;

            var data = new
            {
                action = "create-room",
                playerName = playerName,
                playerIcon = playerIcon
            };

            SendMessage(JsonUtility.ToJson(data));
            Debug.Log("Creating room as host");
        }

        /// <summary>
        /// Joins an existing game room as client.
        /// </summary>
        public void JoinRoom(string roomCode, string playerName, string playerIcon)
        {
            if (!isConnected)
            {
                Debug.LogError("Cannot join room: Not connected to server");
                return;
            }

            isHost = false;
            this.roomCode = roomCode;
            this.playerName = playerName;
            this.playerIcon = playerIcon;

            var data = new
            {
                action = "join-room",
                roomCode = roomCode,
                playerName = playerName,
                playerIcon = playerIcon
            };

            SendMessage(JsonUtility.ToJson(data));
            Debug.Log($"Joining room: {roomCode}");
        }

        /// <summary>
        /// Starts the game (host only).
        /// </summary>
        public void StartGame(int roundCount)
        {
            if (!isHost)
            {
                Debug.LogError("Only host can start game");
                return;
            }

            var data = new
            {
                action = "start-game",
                roomCode = roomCode,
                roundCount = roundCount
            };

            SendMessage(JsonUtility.ToJson(data));
            Debug.Log($"Starting {roundCount}-round game");
        }

        /// <summary>
        /// Submits player answer for current question.
        /// </summary>
        public void SubmitAnswer(string answerText)
        {
            var data = new
            {
                action = "submit-answer",
                roomCode = roomCode,
                playerName = playerName,
                answer = answerText,
                timestamp = DateTime.UtcNow.Ticks
            };

            SendMessage(JsonUtility.ToJson(data));
            Debug.Log($"Submitting answer: {answerText}");
        }

        /// <summary>
        /// Casts elimination vote.
        /// </summary>
        public void SubmitEliminationVote(string selectedAnswer)
        {
            var data = new
            {
                action = "submit-elimination-vote",
                roomCode = roomCode,
                playerName = playerName,
                vote = selectedAnswer
            };

            SendMessage(JsonUtility.ToJson(data));
            Debug.Log($"Elimination vote: {selectedAnswer}");
        }

        /// <summary>
        /// Notifies server that elimination time expired (host only).
        /// </summary>
        public void NotifyEliminationTimeExpired()
        {
            if (!isHost) return;

            var data = new
            {
                action = "elimination-time-expired",
                roomCode = roomCode
            };

            SendMessage(JsonUtility.ToJson(data));
        }

        /// <summary>
        /// Casts final vote for correct answer.
        /// </summary>
        public void SubmitFinalVote(string selectedAnswer)
        {
            var data = new
            {
                action = "submit-final-vote",
                roomCode = roomCode,
                playerName = playerName,
                vote = selectedAnswer
            };

            SendMessage(JsonUtility.ToJson(data));
            Debug.Log($"Final vote: {selectedAnswer}");
        }

        /// <summary>
        /// Notifies server that voting time expired (host only).
        /// </summary>
        public void NotifyVotingTimeExpired()
        {
            if (!isHost) return;

            var data = new
            {
                action = "final-voting-time-expired",
                roomCode = roomCode
            };

            SendMessage(JsonUtility.ToJson(data));
        }

        /// <summary>
        /// Requests transition to next round (host only).
        /// </summary>
        public void RequestNextRound()
        {
            if (!isHost) return;

            var data = new
            {
                action = "request-next-round",
                roomCode = roomCode
            };

            SendMessage(JsonUtility.ToJson(data));
        }

        /// <summary>
        /// Requests transition to elimination screen (host only).
        /// </summary>
        public void RequestTransitionToElimination()
        {
            if (!isHost) return;

            var data = new
            {
                action = "request-transition-to-elimination",
                roomCode = roomCode
            };

            SendMessage(JsonUtility.ToJson(data));
        }

        /// <summary>
        /// Requests transition to voting phase (host only).
        /// </summary>
        public void RequestTransitionToVoting()
        {
            if (!isHost) return;

            var data = new
            {
                action = "start-voting-phase",
                roomCode = roomCode
            };

            SendMessage(JsonUtility.ToJson(data));
        }

        /// <summary>
        /// Sends raw message to server.
        /// </summary>
        private async void SendMessage(string message)
        {
            if (!isConnected)
            {
                Debug.LogError("Cannot send message: Not connected");
                return;
            }

            if (webSocket == null || webSocket.State != WebSocketState.Open)
            {
                Debug.LogError("Cannot send message: WebSocket not open");
                return;
            }

            try
            {
                await webSocket.SendText(message);
            }
            catch (Exception e)
            {
                HandleWebSocketError($"Send failed: {e.Message}");
            }
        }

        /// <summary>
        /// Handles incoming WebSocket messages.
        /// </summary>
        private void HandleMessage(string jsonMessage)
        {
            Debug.Log($"<< Received: {jsonMessage}");

            try
            {
                var message = JsonUtility.FromJson<NetworkMessage>(jsonMessage);

                // Queue event to be processed on main thread
                pendingEvents.Enqueue(() => ProcessMessage(message));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing message: {e.Message}");
            }
        }

        /// <summary>
        /// Processes network messages on main thread.
        /// </summary>
        private void ProcessMessage(NetworkMessage message)
        {
            switch (message.type)
            {
                case "room-joined":
                    roomCode = message.roomCode;
                    OnRoomJoined?.Invoke(message.data);
                    break;

                case "player-joined":
                    OnPlayerJoined?.Invoke(message.data);
                    break;

                case "player-left":
                    OnPlayerLeft?.Invoke(message.data);
                    break;

                case "players-update":
                    OnPlayersUpdate?.Invoke(message.data);
                    break;

                case "room-state-update":
                    OnRoomStateUpdate?.Invoke(message.data);
                    break;

                case "round-started":
                    OnRoundStarted?.Invoke(message.data);
                    break;

                case "player-answered":
                    OnPlayerAnswered?.Invoke(message.data);
                    break;

                case "all-answers-submitted":
                    OnAllAnswersSubmitted?.Invoke(message.data);
                    break;

                case "elimination-vote-cast":
                    OnEliminationVoteCast?.Invoke(message.data);
                    break;

                case "elimination-complete":
                    OnEliminationComplete?.Invoke(message.data);
                    break;

                case "start-voting-phase":
                    OnStartVotingPhase?.Invoke(message.data);
                    break;

                case "final-vote-cast":
                    OnFinalVoteCast?.Invoke(message.data);
                    break;

                case "all-votes-submitted":
                    OnAllVotesSubmitted?.Invoke(message.data);
                    break;

                case "final-round-scores":
                    OnFinalRoundScores?.Invoke(message.data);
                    break;

                case "bonus-round-started":
                    OnBonusRoundStarted?.Invoke(message.data);
                    break;

                case "bonus-player-voted":
                    OnBonusPlayerVoted?.Invoke(message.data);
                    break;

                case "bonus-votes-complete":
                    OnBonusVotesComplete?.Invoke(message.data);
                    break;

                case "error":
                    OnError?.Invoke(message.data);
                    break;

                default:
                    Debug.LogWarning($"Unknown message type: {message.type}");
                    break;
            }
        }

        /// <summary>
        /// Network message structure.
        /// </summary>
        [Serializable]
        private class NetworkMessage
        {
            public string type;
            public string roomCode;
            public string data;
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void HandleWebSocketOpen()
        {
            pendingEvents.Enqueue(() =>
            {
                isConnected = true;
                OnConnected?.Invoke();
            });
        }

        private void HandleWebSocketMessage(byte[] data)
        {
            var message = Encoding.UTF8.GetString(data);
            HandleMessage(message);
        }

        private void HandleWebSocketError(string error)
        {
            pendingEvents.Enqueue(() =>
            {
                Debug.LogError($"WebSocket error: {error}");
                OnError?.Invoke(error);
            });
        }

        private void HandleWebSocketClose(WebSocketCloseCode closeCode)
        {
            pendingEvents.Enqueue(() =>
            {
                bool wasConnected = isConnected;
                isConnected = false;
                CleanupWebSocket();

                if (wasConnected)
                {
                    OnDisconnected?.Invoke();
                }
            });
        }

        private void CleanupWebSocket()
        {
            if (webSocket == null)
            {
                return;
            }

            webSocket.OnOpen -= HandleWebSocketOpen;
            webSocket.OnMessage -= HandleWebSocketMessage;
            webSocket.OnError -= HandleWebSocketError;
            webSocket.OnClose -= HandleWebSocketClose;
            webSocket = null;
        }
    }
}
