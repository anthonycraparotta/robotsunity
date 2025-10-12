# Netcode Implementation Review (Follow-up)

## ✅ Items Verified
- `RWMNetworkManager` now requires and caches the `NetworkManager`, `NetworkObject`, and `UnityTransport` components instead of creating new ones at runtime, and it spawns itself on `OnServerStarted` so the RPC guards on `IsSpawned` now succeed.
- `GameManager` has a `NetworkObject`, subscribes to `NetworkManager.OnServerStarted`, and attempts to spawn itself under host authority, resolving the original issue where its `NetworkVariables` never replicated.

## ⚠️ Outstanding Issues / Best-Practice Gaps
1. **Fallback `NetworkManager` is created without configuration**  
   `CoreSystemsBootstrapper` still manufactures a bare `NetworkManager`/`UnityTransport` pair when the scene copy is missing. That runtime-created singleton has none of the configured prefabs, connection approval settings, lobby handlers, etc., so any session that relies on the fallback will fail to spawn gameplay objects. Best practice is to load a pre-configured prefab (via `NetworkManager.Singleton.NetworkConfig.NetworkPrefabs`) or, preferably, log a fatal error and halt boot so the author-supplied setup is fixed rather than silently replaced.

2. **Scene flow bypasses Netcode's scene system**  
   `GameManager.LoadScene` still uses `SceneManager.LoadScene` locally and then fires a `ClientRpc` to ask everyone else to do the same. This skips Netcode's built-in `NetworkSceneManager`, so late joiners won't be synchronized (they miss the historical scene events), host migration is impossible, and additive network scene support is lost. Switching to `NetworkManager.Singleton.SceneManager.LoadScene` (or additive equivalents) keeps scene state authoritative and automatically handles clients that connect mid-game.

3. **Key gameplay data stays server-only**  
   `GameManager.currentQuestion` and several other round payloads remain plain fields. Client UIs (for example `QuestionScreenScript.DisplayQuestion`) call into `GameManager.Instance.GetCurrentQuestion()`, but those fields never synchronize—only the host ever assigns them. Either move the data into `NetworkVariables`/`NetworkLists` or send structured RPC payloads so that clients receive the current question, answer set, and metadata before rendering screens.

4. **Host transport binds to loopback**  
   `RWMNetworkManager.StartHost` hardcodes `unityTransport.SetConnectionData("127.0.0.1", port)`. This prevents remote devices from connecting because the server only listens on the loopback interface. Use `UnityTransport.SetConnectionData(ServerAddress, port, listenAddress: "0.0.0.0")` (or expose the listen address) so LAN/Internet clients can discover the host.

## 📌 Recommendations
- Replace the fallback singleton creation with a reference to a preconfigured bootstrap prefab, or throw an explicit error prompting designers to include the real networking rig in their scenes.
- Adopt `NetworkSceneManager` for all scene loads (`LoadScene`, `ChangeScene`) so scene transitions are replicated, trackable, and recoverable for late joins.
- Promote question/answer round payloads to netcode-friendly containers (`NetworkVariable<FixedString>` / `NetworkList<FixedString>`), or push them through server-to-client RPCs, before client UI scripts depend on them.
- Expose transport configuration (listen address, port, relay allocation if applicable) instead of hard-coding loopback so mobile/remote clients can connect.

## 🔍 Current Audit Findings

### ✅ Fixes Confirmed
- `RWMNetworkManager.StartHost` now binds the Unity Transport to `0.0.0.0`, allowing remote peers instead of restricting the host to loopback only. 【F:Assets/Scripts/network_manager_script.cs†L151-L160】
- `CoreSystemsBootstrapper` aborts play mode if the preconfigured `RWMNetworkManager` rig is missing, preventing the old runtime-instantiated singleton that shipped with no NetworkConfig. 【F:Assets/Scripts/core_systems_bootstrapper.cs†L40-L105】
- `GameManager.LoadScene` delegates to `NetworkManager.SceneManager`, so authoritative scene changes replicate through Netcode rather than using the local `SceneManager`. 【F:Assets/Scripts/game_manager_script.cs†L1258-L1280】

### ⚠️ Outstanding Netcode Regressions *(re-verified 2025-10-13 after fix claim)*
Despite the renewed claim that all regressions were fixed, the latest re-audit confirms the following issues are still present in the live scripts:
1. **Client-side score patches still bypass Netcode containers.** `RWMNetworkManager.UpdateScoreClientRpc` writes directly into `GameManager.Instance.players[...]`, but that accessor rebuilds a fresh dictionary on every call, so the assignment never touches the authoritative `NetworkList`. Clients that rely on the event receive no persistent score change. 【F:Assets/Scripts/network_manager_script.cs†L429-L444】【F:Assets/Scripts/game_manager_script.cs†L1117-L1129】
2. **UI resets ignore server authority.** The Credits screen resets `currentRound`, `isHalftimePlayed`, and `isBonusRoundPlayed` by assigning to the `NetworkVariable` wrappers and tries to clear the ephemeral `players` dictionary. None of those calls execute through the server, so nothing actually synchronizes to clients. 【F:Assets/Scripts/credits_screen_script.cs†L108-L123】【F:Assets/Scripts/game_manager_script.cs†L24-L47】【F:Assets/Scripts/game_manager_script.cs†L1117-L1129】
3. **Developer debug tools still mutate desynced copies.** `DebugManager.ClearAllPlayers` and related helpers operate on the same transient dictionary and set `correctAnswer`/`robotAnswer` as if they were strings, contradicting the new `NetworkVariable<FixedString>` fields. These utilities no longer behave as intended and encourage patterns that skip RPC/NetworkVariable writes. 【F:Assets/Scripts/debug_manager_script.cs†L309-L377】【F:Assets/Scripts/game_manager_script.cs†L41-L47】【F:Assets/Scripts/game_manager_script.cs†L479-L480】
4. **Scene jumps outside Netcode remain.** `PlayerQuestionVideoScript.AdvanceToNextScreen` still calls `SceneTransitionManager` (which falls back to `SceneManager.LoadScene`), so the host loads the Question scene locally without Netcode scene events when the player question video ends. This recreates the original desync for late joiners. 【F:Assets/Scripts/player_question_video_script.cs†L127-L140】【F:Assets/Scripts/scene_transition_manager.cs†L94-L133】

### ▶️ Next Steps
- Route all score, timer, and state resets through server-side `GameManager` helpers that touch the underlying `NetworkList`/`NetworkVariable` instances instead of modifying the compatibility dictionary.
- Replace any remaining direct `SceneManager`/`SceneTransitionManager` calls in gameplay flows with `GameManager.LoadScene` (or explicit `NetworkSceneManager` calls) so scene history remains authoritative.
- Update debug utilities to issue RPCs or invoke `GameManager` server methods; otherwise keep them editor-only to avoid misleading runtime workflows.
