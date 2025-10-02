# Multiplayer Testing Checklist

## 🧪 Complete Testing Guide for 2-8 Player Multiplayer

---

## Pre-Testing Setup

### Environment Setup
- [ ] WebSocket library installed (NativeWebSocket)
- [ ] Game server running locally or deployed
- [ ] Server URL configured in NetworkManager Inspector
- [ ] 2+ devices/instances ready for testing

### Build Configurations
- [ ] Development build with debug logs enabled
- [ ] Test on target platforms (WebGL, Desktop, Mobile)
- [ ] Network firewall allows connections on test port

---

## Phase 1: Connection Testing

### Basic Connection
- [ ] **Single client connects successfully**
  - Launch game
  - Call NetworkManager.Instance.Connect()
  - Verify "WebSocket connected!" in console
  - Check OnConnected event fires

- [ ] **Multiple clients can connect**
  - Launch 2+ game instances
  - Each connects to same server
  - No connection errors in logs

- [ ] **Connection recovery**
  - Disconnect client intentionally
  - Verify OnDisconnected event fires
  - Reconnect successfully
  - Resume game state

### Room Management
- [ ] **Host creates room**
  - Desktop client creates room
  - Receives unique room code
  - OnRoomJoined event fires with code

- [ ] **Client joins room**
  - Mobile client enters room code
  - Joins successfully
  - OnPlayerJoined event fires for host
  - OnPlayersUpdate shows all players

- [ ] **Invalid room code handling**
  - Try joining non-existent room
  - Error message displayed
  - No crash or hang

- [ ] **Room capacity**
  - Join maximum 8 players
  - 9th player gets error/rejection
  - No server crash

---

## Phase 2: Game Flow Testing

### Question Phase

#### Desktop (Host)
- [ ] **Round starts correctly**
  - OnRoundStarted event fires
  - Question displays on desktop
  - Timer starts at 60 seconds
  - Robot character displays

- [ ] **Answer submission tracking**
  - Submit answer as host
  - Player icon appears immediately
  - NetworkManager.SubmitAnswer called
  - OnPlayerAnswered event fires

- [ ] **Receive other player submissions**
  - Mobile player submits
  - OnPlayerAnswered fires for host
  - Player icon pops on desktop
  - SFX plays ('player_icon_pop')

- [ ] **All answers received**
  - Last player submits
  - OnAllAnswersSubmitted fires
  - Answers overlay appears (desktop)
  - Shows all player answers
  - Transitions after delay

#### Mobile (Client)
- [ ] **Question received from server**
  - OnRoundStarted event received
  - Question text displays
  - Timer synchronized with host
  - Mobile input field enabled

- [ ] **Answer validation**
  - Type correct answer → Shows decoy warning
  - Type duplicate → Shows duplicate warning
  - Type profanity → Shows profanity warning
  - Valid answer → Submit succeeds

- [ ] **Submit confirmation**
  - Tap Submit button
  - NetworkManager.SubmitFinalVote called
  - Input disabled
  - "Answer submitted" feedback

- [ ] **Wait for all players**
  - Shows "Waiting for others..." state
  - Receives OnAllAnswersSubmitted
  - Transitions to EliminationScreen

#### Edge Cases
- [ ] **Timer expiration**
  - Let timer run to 0
  - Auto-submits empty/current answer
  - Transition happens automatically
  - No crash or hang

- [ ] **Host disconnect during question**
  - Host leaves mid-round
  - Server handles gracefully
  - Game pauses or migrates host

---

### Elimination Phase

#### Desktop (Host)
- [ ] **Elimination screen loads**
  - All player answers displayed
  - Robot and correct answers included
  - Answers shuffled/randomized
  - Timer starts at 30 seconds

- [ ] **Vote selection**
  - Click answer to select
  - Visual highlight on selection
  - Can change selection before submit
  - Click again to submit vote

- [ ] **Vote tracking**
  - Submit elimination vote
  - GameManager.RecordEliminationVote called
  - NetworkManager.SubmitEliminationVote sent
  - Vote stored correctly

- [ ] **Receive votes**
  - OnEliminationVoteCast fires for each vote
  - (Optional) Visual feedback per vote

- [ ] **Elimination result**
  - OnEliminationComplete fires
  - Shows "ROBOT IDENTIFIED" or "TIE VOTE"
  - Eliminated answer highlighted
  - Result panel displays 4 seconds
  - Transitions to VotingScreen

#### Mobile (Client)
- [ ] **Answer list received**
  - All answers display on mobile
  - Player's own answer highlighted/disabled
  - Can select other answers
  - Timer synchronized

- [ ] **Vote submission**
  - Tap answer to select (orange highlight)
  - Tap "ELIMINATE THIS ANSWER" button
  - Vote sent to server
  - Button disabled

- [ ] **Result display**
  - OnEliminationComplete received
  - Shows result: Robot eliminated or Tie
  - Brief hold before transition
  - Transitions to VotingScreen

#### Edge Cases
- [ ] **Tie vote scenario**
  - Multiple answers with equal votes
  - Shows "TIE VOTE - NO ELIMINATION"
  - No answer removed
  - Proceeds to voting with all answers

- [ ] **No votes cast**
  - All players timeout
  - Server handles gracefully
  - Random/first answer eliminated or tie

- [ ] **Single voter**
  - Only one player votes
  - That answer is eliminated
  - No crash

---

### Voting Phase

#### Desktop (Host)
- [ ] **Voting screen loads**
  - Eliminated answer NOT shown
  - Remaining answers displayed
  - Timer starts at 30 seconds
  - Can select answer

- [ ] **Vote for correct answer**
  - Select correct answer
  - Submit vote
  - GameManager.RecordFinalVote called
  - NetworkManager.SubmitFinalVote sent

- [ ] **All votes received**
  - OnAllVotesSubmitted fires
  - GameManager.HandleAllVotesSubmitted
  - votingResults populated
  - CalculateRoundScores called (host)
  - Correct answer highlighted
  - Hold 4 seconds
  - Transition to ResultsScreen

#### Mobile (Client)
- [ ] **Voting screen synchronized**
  - Same answers as desktop (minus eliminated)
  - Timer in sync
  - Can select and submit

- [ ] **Vote submission**
  - Select answer
  - Tap "SUBMIT VOTE" button
  - Vote sent successfully
  - Button disabled

- [ ] **Correct answer reveal**
  - OnAllVotesSubmitted received
  - Correct answer highlighted green
  - Shows vote counts per answer
  - Transitions after host

#### Edge Cases
- [ ] **Vote for eliminated answer**
  - Should not be possible (filtered out)
  - If somehow sent, server rejects

- [ ] **Player votes for robot**
  - Voting completes normally
  - Player gets penalty points (-8 or -6)
  - Shown in results as "fooled"

- [ ] **All players vote correctly**
  - All get correct answer points
  - No fooled players
  - Leaderboard updates

---

### Results Phase

#### Desktop (Host)
- [ ] **Results screen loads**
  - If multiplayer: Waits for OnFinalRoundScores
  - Timeout fallback after 10 seconds
  - If timeout: CalculateLocalScores

- [ ] **Panel 1: Correct Answer**
  - Shows true answer
  - Lists players who got it right
  - Displays points earned (+8 or +6)
  - Player icons cascade animate

- [ ] **Panel 2: Robot Decoy**
  - Shows robot answer
  - "Not Fooled" section: players who avoided robot
  - "Fooled" section: players who voted for robot
  - Points shown: +4/+3 (identified) or -8/-6 (fooled)

- [ ] **Panel 3: Player Answers**
  - All player answers listed
  - Vote count per answer shown
  - Points for votes received displayed

- [ ] **Panel 4: Standings**
  - All players ranked by total score
  - Scores updated from round
  - Leader highlighted
  - "Next Round" button appears

- [ ] **Next round transition**
  - Click "Next Round" button
  - Host triggers RequestNextRound
  - Returns to QuestionScreen
  - New question loads

#### Mobile (Client)
- [ ] **Results synchronized**
  - OnFinalRoundScores received
  - Same results as desktop
  - Panels display sequentially
  - Auto-advances after host

- [ ] **Score updates**
  - Player's score increases/decreases
  - Leaderboard position changes
  - Visual feedback on score change

#### Edge Cases
- [ ] **OnFinalRoundScores timeout**
  - 10 seconds pass without event
  - CalculateLocalScores triggered
  - Panels show anyway
  - Warning logged but no crash

- [ ] **Tie in standings**
  - Multiple players same score
  - Both shown at same rank
  - Sorted by name or join order

---

## Phase 3: Multi-Round Testing

### Full 8-Round Game
- [ ] **Round progression**
  - Complete all 8 rounds
  - Each round transitions correctly
  - Scores accumulate properly
  - No memory leaks

- [ ] **Picture questions**
  - Round with imageUrl loads
  - Image displays correctly
  - Points are halved (correct/robot)
  - Voting works same as text

- [ ] **Vote clearing**
  - Votes reset between rounds
  - playerEliminationVotes cleared
  - playerFinalVotes cleared
  - No vote carryover

- [ ] **Final results**
  - After round 8/12: Show FinalResultsScreen
  - Winner crowned
  - Full game stats displayed
  - Option to play again

### Scoring Verification

#### 8-Round Game
- [ ] Correct answer: +8 points
- [ ] Robot identified: +4 points
- [ ] Vote received: +4 points per vote
- [ ] Fooled by robot: -8 points

#### 12-Round Game
- [ ] Correct answer: +6 points
- [ ] Robot identified: +3 points
- [ ] Vote received: +3 points per vote
- [ ] Fooled by robot: -6 points

#### Picture Round (Double Points)
- [ ] Correct answer: 2x normal points
- [ ] Robot identified: 1x normal (half of 2x)
- [ ] Votes/fooled: Same as normal

### Score Calculation Accuracy
- [ ] Player who gets correct + votes → Highest score
- [ ] Player fooled by robot → Negative points applied
- [ ] Player who identifies robot → Bonus points
- [ ] Totals match manual calculation

---

## Phase 4: Error & Edge Case Testing

### Network Errors
- [ ] **Server goes down mid-game**
  - Clients detect disconnect
  - OnDisconnected fires
  - Error message shown
  - Option to reconnect

- [ ] **High latency**
  - Simulate 500ms+ delay
  - Timers still sync correctly
  - Votes still register
  - No duplicate submissions

- [ ] **Packet loss**
  - Some messages dropped
  - Clients recover gracefully
  - Timeout fallbacks trigger
  - No permanent desyncs

### Player Behavior
- [ ] **Player disconnects mid-round**
  - Other players continue
  - Disconnected player marked inactive
  - Auto-submit empty/timeout for them
  - Game proceeds normally

- [ ] **Host disconnects**
  - Server migrates host to another player
  - OR game pauses until host returns
  - OR game ends gracefully

- [ ] **Player rejoins**
  - Reconnects to same room
  - Resumes at current phase
  - Sees up-to-date state
  - Can continue playing

### Data Validation
- [ ] **Invalid JSON from server**
  - Malformed message received
  - Error logged but no crash
  - Event ignored or default action

- [ ] **Missing required fields**
  - Server sends incomplete data
  - Null checks prevent crashes
  - Default values used

- [ ] **Type mismatches**
  - String instead of int, etc.
  - JsonUtility handles gracefully
  - Or try-catch prevents crash

### UI/UX Edge Cases
- [ ] **Rapid button clicking**
  - Click submit multiple times fast
  - Only one vote registers
  - hasVoted flag prevents duplicates

- [ ] **Screen orientation change** (mobile)
  - Rotate device mid-game
  - UI adjusts responsively
  - No data loss
  - Game continues

- [ ] **App backgrounding** (mobile)
  - Send app to background
  - WebSocket stays connected (or reconnects)
  - Return to app
  - Game state preserved

---

## Phase 5: Performance Testing

### Stress Tests
- [ ] **8 players simultaneous submission**
  - All 8 submit at same time
  - Server handles load
  - All submissions registered
  - No lag or timeout

- [ ] **Rapid phase transitions**
  - Complete phases very quickly
  - Timers at minimum
  - No race conditions
  - Clean transitions

- [ ] **Long game session**
  - Play full 12-round game
  - Monitor memory usage
  - No memory leaks
  - Framerate stable

### Memory & Resources
- [ ] **Monitor heap size**
  - Use Unity Profiler
  - Check for memory growth
  - Verify garbage collection
  - No permanent allocations

- [ ] **Network bandwidth**
  - Measure bytes sent/received
  - Typical: <10KB per message
  - No excessive traffic
  - Efficient JSON

- [ ] **CPU usage**
  - Game runs at 60 FPS
  - Network thread not blocking
  - UI responsive
  - No frame drops

---

## Phase 6: Platform-Specific Testing

### WebGL Build
- [ ] WebSocket connects in browser
- [ ] No CORS issues
- [ ] All events fire correctly
- [ ] Performance acceptable
- [ ] Works in Chrome, Firefox, Safari

### Desktop Build (Windows/Mac/Linux)
- [ ] NativeWebSocket works
- [ ] Firewall doesn't block connection
- [ ] Can host or join
- [ ] Alt-Tab doesn't break connection

### Mobile Build (iOS/Android)
- [ ] WebSocket works on cellular
- [ ] Works on WiFi
- [ ] Background/foreground transitions OK
- [ ] Touch controls responsive
- [ ] No unexpected disconnects

### Cross-Platform
- [ ] Desktop host + mobile clients
- [ ] WebGL host + desktop clients
- [ ] All combinations work
- [ ] Same behavior across platforms

---

## Testing Tools & Utilities

### Debug Helpers

**Add to GameManager for testing:**
```csharp
[ContextMenu("Force Next Round")]
public void DEBUG_ForceNextRound()
{
    NetworkManager.Instance.RequestNextRound();
}

[ContextMenu("Simulate All Votes")]
public void DEBUG_SimulateVotes()
{
    foreach (var player in Players)
    {
        RecordFinalVote(player.PlayerName, CurrentQuestion.CorrectAnswer);
    }
}
```

**Add to NetworkManager:**
```csharp
[ContextMenu("Trigger Test Event")]
public void DEBUG_TriggerEvent(string eventName, string jsonData)
{
    var net = NetworkManager.Instance;
    net.GetType().GetField(eventName)?.GetValue(net)
        ?.GetType().GetMethod("Invoke")?.Invoke(null, new object[] { jsonData });
}
```

### Test Server (Node.js)

```javascript
// test-server.js - Echo server with logging
const WebSocket = require('ws');
const wss = new WebSocket.Server({ port: 3000 });

const rooms = new Map();
const clients = new Map();

wss.on('connection', (ws) => {
    console.log('✓ Client connected');

    ws.on('message', (msg) => {
        const data = JSON.parse(msg);
        console.log('→ Received:', data.action, data);

        // Echo back with type wrapper
        const response = {
            type: data.action + '-response',
            roomCode: data.roomCode,
            data: JSON.stringify(data)
        };

        ws.send(JSON.stringify(response));
        console.log('← Sent:', response.type);
    });

    ws.on('close', () => {
        console.log('✗ Client disconnected');
    });
});

console.log('Test server running on ws://localhost:3000');
```

### Automated Testing Script

```csharp
// AutoTest.cs - Attach to test GameObject
public class MultiplayerAutoTest : MonoBehaviour
{
    private IEnumerator TestFullRound()
    {
        // Connect
        NetworkManager.Instance.Connect();
        yield return new WaitForSeconds(1);

        // Create room
        NetworkManager.Instance.CreateRoom("TestPlayer", "icon1");
        yield return new WaitForSeconds(1);

        // Submit answer
        var question = new Question("q1", "Test?", "Answer", "Robot",
            GameConstants.QuestionType.Text, 1);
        GameManager.Instance.StartNextRound(question);
        yield return new WaitForSeconds(1);

        NetworkManager.Instance.SubmitAnswer("TestAnswer");
        yield return new WaitForSeconds(1);

        // Verify
        Assert.AreEqual(1, GameManager.Instance.CurrentAnswers.Count);
        Debug.Log("✓ Test passed!");
    }

    [ContextMenu("Run Auto Test")]
    public void RunTest()
    {
        StartCoroutine(TestFullRound());
    }
}
```

---

## Success Criteria

### Minimum Viable Multiplayer (MVP)
- [x] 2 players can connect
- [x] Complete one full round
- [x] Scores calculate correctly
- [x] No crashes or hangs
- [x] Basic error handling

### Production Ready
- [ ] 8 players simultaneously
- [ ] Full 8 or 12 round game
- [ ] Handle disconnects gracefully
- [ ] Reconnection works
- [ ] Performance: 60 FPS, <100ms latency
- [ ] All edge cases handled
- [ ] Comprehensive error logging

---

## Regression Testing (After Changes)

### Critical Paths
After any code changes, re-test:
- [ ] Connection → Room Join → Round Start
- [ ] Answer Submit → Elimination → Voting → Results
- [ ] Score calculation accuracy
- [ ] Timer synchronization
- [ ] Event subscriptions/cleanup

### Quick Smoke Test (5 minutes)
1. Connect 2 clients
2. Create/join room
3. Complete one round
4. Verify scores
5. Check for errors in log

### Full Regression (30 minutes)
Run all Phase 1-4 tests with 2-4 players

---

## Bug Reporting Template

```markdown
## Bug Report

**Title:** [Brief description]

**Severity:** Critical / High / Medium / Low

**Platform:** Desktop / Mobile / WebGL

**Players:** [Number of players when bug occurred]

**Steps to Reproduce:**
1.
2.
3.

**Expected Result:**
[What should happen]

**Actual Result:**
[What actually happened]

**Console Logs:**
```
[Paste relevant logs]
```

**Screenshots/Video:**
[Attach if applicable]

**Additional Context:**
- Game mode: 8 / 12 rounds
- Round number: X
- Phase: Question / Elimination / Voting / Results
```

---

## Performance Benchmarks

### Target Metrics
- **Connection Time:** <1 second
- **Message Latency:** <100ms
- **Memory Usage:** <200MB
- **CPU Usage:** <30%
- **Frame Rate:** 60 FPS stable
- **Network Traffic:** <50KB per round

### Monitor These
- Unity Profiler → CPU Usage
- Unity Profiler → Memory
- Network tab → Message sizes
- Console → Event timing logs

---

## Final Checklist

Before declaring multiplayer "complete":

### Functionality
- [ ] All screens support multiplayer
- [ ] All game phases work correctly
- [ ] Scoring is accurate
- [ ] Timers synchronize
- [ ] Vote tracking works
- [ ] Elimination logic correct

### Reliability
- [ ] No crashes in normal play
- [ ] Handles network errors
- [ ] Timeout fallbacks work
- [ ] Null checks prevent crashes
- [ ] Memory is properly managed

### Usability
- [ ] Smooth experience for players
- [ ] Clear feedback on actions
- [ ] Error messages helpful
- [ ] No confusing states
- [ ] Responsive UI

### Performance
- [ ] 60 FPS maintained
- [ ] Low network usage
- [ ] Quick phase transitions
- [ ] No lag spikes
- [ ] Works on target devices

---

## Testing Timeline Estimate

- **Basic Connection:** 30 minutes
- **Single Round Flow:** 1 hour
- **Multi-Round Game:** 1 hour
- **Edge Cases:** 2 hours
- **Performance Tests:** 1 hour
- **Platform Testing:** 2 hours
- **Bug Fixes:** 2-4 hours

**Total: ~10-12 hours of testing**

Once all checkboxes are complete and all bugs fixed, multiplayer is production-ready! 🎉
