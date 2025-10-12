# Netcode Implementation Review (Verification Pass)

## ✅ Items Verified
- `RWMNetworkManager` now enforces its dependencies with `[RequireComponent]`, caches the authored `NetworkManager`/`UnityTransport`, and spawns its `NetworkObject` in `OnServerStarted`, so host/client RPC invocations now pass the `IsSpawned` guards. 【F:Assets/Scripts/network_manager_script.cs†L11-L86】【F:Assets/Scripts/network_manager_script.cs†L90-L123】
- `GameManager` carries a `NetworkObject`, subscribes to `NetworkManager.OnServerStarted`, and attempts to spawn itself under server authority, allowing its `NetworkVariables`/`NetworkLists` to replicate. 【F:Assets/Scripts/game_manager_script.cs†L13-L121】

## ⚠️ Outstanding Issues / Best-Practice Gaps
1. **Fallback `NetworkManager` is still a blank runtime construct**  
   When a scene omits the authored networking rig, `CoreSystemsBootstrapper` fabricates a new GameObject and simply adds `NetworkManager`, `NetworkObject`, and `UnityTransport` components with all default configuration. That object has no registered prefabs, connection approval rules, or transport settings, so any host started from the fallback cannot spawn gameplay state. Prefer instantiating a preconfigured prefab or halting with an explicit error so the scene can be fixed. 【F:Assets/Scripts/core_systems_bootstrapper.cs†L47-L87】

2. **Scene flow still bypasses `NetworkSceneManager`**  
   Both `GameManager.LoadScene` (and all call sites such as `AdvanceToNextScreen`) and `RWMNetworkManager.ChangeSceneClientRpc` call directly into `SceneManager.LoadScene`, which only loads the scene locally and relies on RPC mirroring to other clients. This skips Netcode's scene system, so late joiners will not be synchronized and rollback/host migration scenarios break. Switch to `NetworkManager.Singleton.SceneManager.LoadScene` (or additive equivalents) so the server authoritatively manages scene state. 【F:Assets/Scripts/game_manager_script.cs†L216-L399】【F:Assets/Scripts/network_manager_script.cs†L328-L356】

3. **Round payloads remain server-only fields**  
   Gameplay data such as `currentQuestion`, `standardQuestions`, and the answer dictionaries never enter a `NetworkVariable`/`NetworkList`. Client UI scripts (e.g., `QuestionScreenScript.DisplayQuestion`, `PictureQuestionScript`) read them through `GameManager.Instance`, but only the host ever assigns those fields. As a result, clients that connect or reload scenes see `null` questions. Promote the question/answer payloads to networked containers or distribute them via RPC before clients rely on them. 【F:Assets/Scripts/game_manager_script.cs†L29-L205】【F:Assets/Scripts/game_manager_script.cs†L400-L474】【F:Assets/Scripts/question_screen_script.cs†L162-L210】【F:Assets/Scripts/picture_question_script.cs†L90-L126】

4. **Host transport still binds to loopback**  
   `StartHost` hardcodes `unityTransport.SetConnectionData("127.0.0.1", port);`, so the server only listens on loopback. Remote/mobile clients cannot join unless they run on the same machine. Expose the listen address (and relay data if applicable) so the host binds to an accessible endpoint (e.g., `0.0.0.0`). 【F:Assets/Scripts/network_manager_script.cs†L141-L179】

## 📌 Recommendations
- Replace the fallback singleton creation with a preconfigured prefab (or fail loudly) so runtime sessions always inherit the intended `NetworkManager` configuration.
- Route every scene transition through `NetworkSceneManager` to keep the host authoritative and late joiners synchronized.
- Synchronize question/answer payloads via `NetworkVariable<FixedString128Bytes>`/`NetworkList` or explicit RPC payloads before client UI logic reads them.
- Expose transport configuration (listen address/relay) instead of hard-coding loopback so external devices can connect.
