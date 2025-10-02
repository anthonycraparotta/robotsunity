using System.Collections;
using UnityEngine;
using RobotsGame.Managers;

namespace RobotsGame.Network
{
    /// <summary>
    /// Handles host/client synchronization delays and timing.
    /// Desktop (host) controls timing, mobile (clients) follow with delays.
    /// Based on unityspec.md timing specifications.
    /// </summary>
    public class MultiplayerSync : MonoBehaviour
    {
        public static MultiplayerSync Instance { get; private set; }

        // Timing constants from screenspec.md
        private const float HOST_SYNC_DELAY_MS = 500f;
        private const float MOBILE_FOLLOW_DELAY_MS = 800f;

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

        /// <summary>
        /// Executes an action with host synchronization delay.
        /// Host: Waits HOST_SYNC_DELAY_MS before executing
        /// Client: Executes immediately (waits for server event)
        /// </summary>
        public void ExecuteWithHostDelay(System.Action action, float additionalDelayMs = 0)
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.IsHost)
            {
                StartCoroutine(DelayedExecution(action, HOST_SYNC_DELAY_MS + additionalDelayMs));
            }
            else
            {
                // Clients wait for server events, no local delay
                action?.Invoke();
            }
        }

        /// <summary>
        /// Executes an action with mobile follow delay.
        /// Host: Executes immediately
        /// Client: Waits MOBILE_FOLLOW_DELAY_MS before executing
        /// </summary>
        public void ExecuteWithClientDelay(System.Action action)
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.IsHost)
            {
                action?.Invoke();
            }
            else
            {
                StartCoroutine(DelayedExecution(action, MOBILE_FOLLOW_DELAY_MS));
            }
        }

        /// <summary>
        /// Schedules phase transition for host.
        /// Calculates max(timing, HOST_SYNC_DELAY_MS) and notifies server.
        /// </summary>
        public void ScheduleHostTransition(System.Action transitionAction, float calculatedTimingMs)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsHost) return;

            float delay = Mathf.Max(calculatedTimingMs, HOST_SYNC_DELAY_MS);
            StartCoroutine(DelayedExecution(transitionAction, delay));
        }

        /// <summary>
        /// Schedules round transition with bonus panel delay.
        /// Desktop: 1200ms follow delay
        /// Mobile: MOBILE_FOLLOW_DELAY_MS after host
        /// </summary>
        public void ScheduleRoundTransition(System.Action action)
        {
            if (GameManager.Instance.IsDesktopMode)
            {
                StartCoroutine(DelayedExecution(action, 1200f));
            }
            else
            {
                StartCoroutine(DelayedExecution(action, MOBILE_FOLLOW_DELAY_MS));
            }
        }

        /// <summary>
        /// Waits for all players to submit with timeout.
        /// Returns true if all submitted, false if timeout.
        /// </summary>
        public IEnumerator WaitForAllPlayers(System.Func<bool> checkCondition, float timeoutSeconds, System.Action onComplete, System.Action onTimeout = null)
        {
            float elapsed = 0f;

            while (elapsed < timeoutSeconds)
            {
                if (checkCondition())
                {
                    onComplete?.Invoke();
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Timeout reached
            onTimeout?.Invoke();
        }

        /// <summary>
        /// Notifies server of phase transition (host only).
        /// </summary>
        public void NotifyTransition(string transitionType)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsHost) return;

            var net = NetworkManager.Instance;
            if (net == null) return;

            switch (transitionType)
            {
                case "elimination":
                    net.RequestTransitionToElimination();
                    break;
                case "voting":
                    net.RequestTransitionToVoting();
                    break;
                case "next-round":
                    net.RequestNextRound();
                    break;
                case "elimination-timeout":
                    net.NotifyEliminationTimeExpired();
                    break;
                case "voting-timeout":
                    net.NotifyVotingTimeExpired();
                    break;
            }
        }

        /// <summary>
        /// Generic delayed execution coroutine.
        /// </summary>
        private IEnumerator DelayedExecution(System.Action action, float delayMs)
        {
            yield return new WaitForSeconds(delayMs / 1000f);
            action?.Invoke();
        }

        /// <summary>
        /// Checks if all players have submitted answers.
        /// </summary>
        public bool AllPlayersSubmitted()
        {
            var game = GameManager.Instance;
            int expectedCount = game.Players.Count;
            int actualCount = game.CurrentAnswers.Count - 2; // Exclude robot and correct answers

            return actualCount >= expectedCount;
        }

        /// <summary>
        /// Gets the appropriate delay for current device.
        /// </summary>
        public float GetDeviceDelay()
        {
            if (GameManager.Instance == null) return HOST_SYNC_DELAY_MS;
            return GameManager.Instance.IsHost ? HOST_SYNC_DELAY_MS : MOBILE_FOLLOW_DELAY_MS;
        }

        /// <summary>
        /// Synchronizes screen transition across all clients.
        /// Host: Schedules transition and notifies server
        /// Client: Waits for server event
        /// </summary>
        public void SyncScreenTransition(string screenName, System.Action transitionAction, float hostTimingMs = 0)
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.IsHost)
            {
                // Host: Calculate delay and notify server
                float delay = Mathf.Max(hostTimingMs, HOST_SYNC_DELAY_MS);
                StartCoroutine(HostTransition(screenName, transitionAction, delay));
            }
            else
            {
                // Client: Execute immediately (will be triggered by server event)
                transitionAction?.Invoke();
            }
        }

        private IEnumerator HostTransition(string screenName, System.Action action, float delay)
        {
            yield return new WaitForSeconds(delay / 1000f);

            // Notify server first
            NotifyTransition(screenName);

            // Then execute locally
            action?.Invoke();
        }
    }
}
