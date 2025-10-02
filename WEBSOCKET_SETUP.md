# WebSocket Integration - Quick Setup Guide

## Option 1: NativeWebSocket (Recommended)

### Installation

1. **Add to Unity Package Manager:**
   ```
   Window → Package Manager → + → Add package from git URL
   ```
   Enter: `https://github.com/endel/NativeWebSocket.git#upm`

2. **Or add to manifest.json:**
   ```json
   {
     "dependencies": {
       "com.endel.nativewebsocket": "https://github.com/endel/NativeWebSocket.git#upm"
     }
   }
   ```

### Update NetworkManager.cs

**Replace lines 98-117 and 496-525 with:**

```csharp
using NativeWebSocket;

// In NetworkManager class:
private WebSocket webSocket;

public async void Connect(string url = null)
{
    if (isConnected)
    {
        Debug.LogWarning("Already connected to server");
        return;
    }

    string connectionUrl = string.IsNullOrEmpty(url) ? serverUrl : url;
    Debug.Log($"Connecting to server: {connectionUrl}");

    webSocket = new WebSocket(connectionUrl);

    webSocket.OnOpen += () =>
    {
        Debug.Log("WebSocket connected!");
        isConnected = true;
        pendingEvents.Enqueue(() => OnConnected?.Invoke());
    };

    webSocket.OnMessage += (bytes) =>
    {
        string message = System.Text.Encoding.UTF8.GetString(bytes);
        HandleMessage(message);
    };

    webSocket.OnError += (error) =>
    {
        Debug.LogError($"WebSocket error: {error}");
        pendingEvents.Enqueue(() => OnError?.Invoke(error));
    };

    webSocket.OnClose += (code) =>
    {
        Debug.Log($"WebSocket closed: {code}");
        isConnected = false;
        pendingEvents.Enqueue(() => OnDisconnected?.Invoke());
    };

    await webSocket.Connect();
}

public async void Disconnect()
{
    if (!isConnected || webSocket == null) return;

    Debug.Log("Disconnecting from server");
    await webSocket.Close();
    isConnected = false;
}

private async void SendMessage(string message)
{
    if (!isConnected || webSocket == null)
    {
        Debug.LogError("Cannot send message: Not connected");
        return;
    }

    if (webSocket.State == WebSocketState.Open)
    {
        await webSocket.SendText(message);
        Debug.Log($">> Sent: {message}");
    }
}

private void Update()
{
    // Dispatch WebSocket messages
    #if !UNITY_WEBGL || UNITY_EDITOR
    webSocket?.DispatchMessageQueue();
    #endif

    // Process pending events on main thread
    while (pendingEvents.Count > 0)
    {
        pendingEvents.Dequeue()?.Invoke();
    }
}
```

---

## Option 2: WebSocketSharp (Windows/Mac/Linux)

### Installation

1. **Download WebSocketSharp:**
   - Get from NuGet or GitHub: `https://github.com/sta/websocket-sharp`

2. **Add DLL to Unity:**
   - Place `websocket-sharp.dll` in `Assets/Plugins/`

### Update NetworkManager.cs

```csharp
using WebSocketSharp;

// In NetworkManager class:
private WebSocket webSocket;

public void Connect(string url = null)
{
    if (isConnected) return;

    string connectionUrl = string.IsNullOrEmpty(url) ? serverUrl : url;
    webSocket = new WebSocket(connectionUrl);

    webSocket.OnOpen += (sender, e) =>
    {
        isConnected = true;
        pendingEvents.Enqueue(() => OnConnected?.Invoke());
    };

    webSocket.OnMessage += (sender, e) =>
    {
        HandleMessage(e.Data);
    };

    webSocket.OnError += (sender, e) =>
    {
        pendingEvents.Enqueue(() => OnError?.Invoke(e.Message));
    };

    webSocket.OnClose += (sender, e) =>
    {
        isConnected = false;
        pendingEvents.Enqueue(() => OnDisconnected?.Invoke());
    };

    webSocket.Connect();
}

public void Disconnect()
{
    webSocket?.Close();
}

private void SendMessage(string message)
{
    if (!isConnected || webSocket == null) return;
    webSocket.Send(message);
}
```

---

## Testing Without Server

### Mock Testing (No Server Needed)

Keep the existing `SimulateConnection()` and manually trigger events:

```csharp
// In Unity Inspector or test script:
NetworkManager.Instance.Connect(); // Uses SimulateConnection

// Manually trigger events for testing:
NetworkManager.Instance.OnRoomJoined?.Invoke("{\"roomCode\":\"TEST\"}");
NetworkManager.Instance.OnRoundStarted?.Invoke(questionJson);
NetworkManager.Instance.OnAllAnswersSubmitted?.Invoke(answersJson);
```

### Local Server (Simple Node.js)

```javascript
// server.js
const WebSocket = require('ws');
const wss = new WebSocket.Server({ port: 3000 });

wss.on('connection', (ws) => {
    console.log('Client connected');

    ws.on('message', (message) => {
        console.log('Received:', message);

        // Echo back for testing
        ws.send(message);
    });

    ws.on('close', () => {
        console.log('Client disconnected');
    });
});

console.log('WebSocket server running on ws://localhost:3000');
```

Run with: `node server.js`

---

## Server Message Format

### From Client to Server:

```json
{
    "action": "create-room",
    "playerName": "Alice",
    "playerIcon": "icon1"
}

{
    "action": "join-room",
    "roomCode": "ABCD",
    "playerName": "Bob",
    "playerIcon": "icon2"
}

{
    "action": "submit-answer",
    "roomCode": "ABCD",
    "playerName": "Alice",
    "answer": "Pepperoni",
    "timestamp": 638123456789
}

{
    "action": "submit-elimination-vote",
    "roomCode": "ABCD",
    "playerName": "Alice",
    "vote": "Motor Oil"
}

{
    "action": "submit-final-vote",
    "roomCode": "ABCD",
    "playerName": "Alice",
    "vote": "Pepperoni"
}
```

### From Server to Client:

```json
{
    "type": "room-joined",
    "roomCode": "ABCD",
    "data": "{\"roomCode\":\"ABCD\",\"playerCount\":2}"
}

{
    "type": "round-started",
    "roomCode": "ABCD",
    "data": "{\"roundNumber\":1,\"question\":{...}}"
}

{
    "type": "all-answers-submitted",
    "roomCode": "ABCD",
    "data": "{\"answers\":[...]}"
}

{
    "type": "elimination-complete",
    "roomCode": "ABCD",
    "data": "{\"eliminatedAnswer\":\"Motor Oil\",\"tieOccurred\":false}"
}

{
    "type": "all-votes-submitted",
    "roomCode": "ABCD",
    "data": "{\"votes\":[{\"playerName\":\"Alice\",\"answer\":\"Pepperoni\"}]}"
}

{
    "type": "final-round-scores",
    "roomCode": "ABCD",
    "data": "{\"roundNumber\":1}"
}
```

---

## Quick Start Checklist

### 1. Choose WebSocket Library
- [ ] NativeWebSocket (WebGL compatible) ✅ Recommended
- [ ] WebSocketSharp (Desktop only)
- [ ] Unity WebSocket (Built-in, limited)

### 2. Update NetworkManager
- [ ] Replace placeholder WebSocketConnection class
- [ ] Update Connect() method with real WebSocket
- [ ] Update SendMessage() with await/Send
- [ ] Add DispatchMessageQueue to Update (for NativeWebSocket)

### 3. Test Connection
- [ ] Create simple echo server (Node.js example above)
- [ ] Update serverUrl in NetworkManager Inspector
- [ ] Press Play and call NetworkManager.Instance.Connect()
- [ ] Check Console for "WebSocket connected!" message

### 4. Test Events
- [ ] Send room join request
- [ ] Verify OnRoomJoined event fires
- [ ] Check GameManager.HandleRoomJoined called
- [ ] Verify room code displayed

### 5. Full Flow Test
- [ ] Start game (2 players minimum)
- [ ] Submit answers in QuestionScreen
- [ ] Vote in EliminationScreen
- [ ] Vote in VotingScreen
- [ ] View scores in ResultsScreen

---

## Troubleshooting

### Connection Issues

**"WebSocket connection failed"**
- Check server is running: `telnet localhost 3000`
- Verify URL format: `ws://localhost:3000` (not `http://`)
- Check firewall settings

**"Messages not receiving"**
- Add Debug.Log in HandleMessage to see raw JSON
- Verify JSON format matches expected structure
- Check DispatchMessageQueue is called in Update

**"Events not firing"**
- Ensure pendingEvents queue is processed in Update
- Check UnityEvent listeners are properly subscribed
- Verify OnConnected fires before other events

### Common Mistakes

1. **Forgetting DispatchMessageQueue:**
   ```csharp
   void Update() {
       webSocket?.DispatchMessageQueue(); // REQUIRED for NativeWebSocket
   }
   ```

2. **Wrong JSON format:**
   - Server must send: `{"type":"event-name","data":"json-string"}`
   - Not: `{"eventName":"data"}` ❌

3. **Not awaiting async calls:**
   ```csharp
   await webSocket.Connect();  // ✅
   webSocket.Connect();        // ❌ May not work
   ```

---

## Performance Tips

1. **Limit message size:**
   - Keep JSON compact
   - Don't send entire game state every update
   - Only send deltas/changes

2. **Batch updates:**
   - Collect multiple small updates
   - Send in single message
   - Reduces network overhead

3. **Compression (optional):**
   ```csharp
   // Client side
   byte[] compressed = Compress(jsonString);
   await webSocket.Send(compressed);

   // Server side
   string json = Decompress(bytes);
   ```

---

## Security Considerations

1. **Use WSS (WebSocket Secure) in production:**
   ```csharp
   serverUrl = "wss://your-server.com"; // Not ws://
   ```

2. **Validate all server messages:**
   ```csharp
   if (!IsValidMessage(message)) return;
   ```

3. **Sanitize user input:**
   ```csharp
   answer = SanitizeInput(answer);
   ```

4. **Rate limiting:**
   - Limit messages per second per client
   - Prevent spam/DOS attacks

---

## Next Steps After WebSocket Integration

1. **Build Lobby UI:**
   - Room code display
   - Player list
   - Ready/Start buttons

2. **Add Reconnection:**
   - Store room code + player ID
   - Auto-reconnect on disconnect
   - Resume game state

3. **Error Recovery:**
   - Handle vote timeouts
   - Deal with player disconnects
   - Sync state mismatches

4. **Production Server:**
   - Deploy to cloud (AWS/Azure/GCP)
   - Use managed WebSocket service
   - Add Redis for state management
   - Implement proper scaling

---

## Estimated Time

- **WebSocket Library Setup:** 10 minutes
- **Code Integration:** 20 minutes
- **Testing with Echo Server:** 15 minutes
- **First Full Game Test:** 30 minutes

**Total: ~1 hour to working WebSocket connection**

Then you just need the game server implementation! 🚀
