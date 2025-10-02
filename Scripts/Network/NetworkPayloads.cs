using System;
using System.Collections.Generic;
using UnityEngine;
using RobotsGame.Data;

namespace RobotsGame.Network
{
    /// <summary>
    /// Network payload data structures for socket communication.
    /// Based on screenspec.md SOCKET EVENTS section.
    /// </summary>

    // ===========================
    // ROOM STATE PAYLOADS
    // ===========================

    /// <summary>
    /// Complete room state snapshot for synchronization.
    /// </summary>
    [Serializable]
    public class RoomState
    {
        public string roomCode;
        public string hostPlayerName;
        public List<PlayerData> players;
        public int currentRound;
        public int totalRounds;
        public string gamePhase; // "lobby", "question", "elimination", "voting", "results", "halftime", "bonus", "final"
        public QuestionData currentQuestion;
        public long phaseStartTime; // Unix timestamp
        public int phaseTimerDuration; // Seconds
    }

    /// <summary>
    /// Player data for network transmission.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public string name;
        public string icon;
        public int score;
        public bool isHost;
        public bool isConnected;
        public bool hasAnswered;
        public bool hasVoted;
    }

    /// <summary>
    /// Question data for network transmission.
    /// </summary>
    [Serializable]
    public class QuestionData
    {
        public int questionNumber;
        public string questionText;
        public string questionImagePath; // For picture questions
        public bool isPictureQuestion;
        public List<string> answerChoices; // Mixed pool of answers for elimination/voting
        public string correctAnswer; // Not sent to clients until results
        public string robotAnswer; // Not sent to clients until results
    }

    // ===========================
    // ROUND FLOW PAYLOADS
    // ===========================

    /// <summary>
    /// Round start notification payload.
    /// </summary>
    [Serializable]
    public class RoundStartData
    {
        public int roundNumber;
        public int totalRounds;
        public QuestionData question;
        public int questionTimerDuration; // Seconds
        public bool isPictureQuestion;
    }

    /// <summary>
    /// Answer submission payload.
    /// </summary>
    [Serializable]
    public class AnswerSubmitData
    {
        public string playerName;
        public string answer;
        public long timestamp;
    }

    /// <summary>
    /// All answers submitted notification.
    /// </summary>
    [Serializable]
    public class AllAnswersSubmittedData
    {
        public List<string> answerChoices; // Shuffled pool including correct, robot, player answers
        public int eliminationTimerDuration; // Seconds
    }

    // ===========================
    // ELIMINATION PAYLOADS
    // ===========================

    /// <summary>
    /// Elimination vote submission payload.
    /// </summary>
    [Serializable]
    public class EliminationVoteData
    {
        public string playerName;
        public string votedAnswer;
        public long timestamp;
    }

    /// <summary>
    /// Elimination complete notification.
    /// </summary>
    [Serializable]
    public class EliminationCompleteData
    {
        public string eliminatedAnswer; // Answer with most votes
        public bool wasRobotEliminated; // True if robot was correctly identified
        public Dictionary<string, int> voteCounts; // Answer -> vote count
        public List<string> remainingAnswers; // Answers moving to voting phase
    }

    // ===========================
    // VOTING PAYLOADS
    // ===========================

    /// <summary>
    /// Start voting phase notification.
    /// </summary>
    [Serializable]
    public class StartVotingPhaseData
    {
        public List<string> votingChoices; // Remaining answers after elimination
        public int votingTimerDuration; // Seconds
    }

    /// <summary>
    /// Final vote submission payload.
    /// </summary>
    [Serializable]
    public class FinalVoteData
    {
        public string playerName;
        public string votedAnswer;
        public long timestamp;
    }

    /// <summary>
    /// All votes submitted notification.
    /// </summary>
    [Serializable]
    public class AllVotesSubmittedData
    {
        public Dictionary<string, int> voteCounts; // Answer -> vote count
        public string correctAnswer;
        public string robotAnswer;
    }

    // ===========================
    // RESULTS PAYLOADS
    // ===========================

    /// <summary>
    /// Round results with score changes.
    /// </summary>
    [Serializable]
    public class RoundResultsData
    {
        public int roundNumber;
        public string correctAnswer;
        public string robotAnswer;
        public Dictionary<string, int> voteCounts; // Answer -> vote count
        public List<PlayerRoundScore> playerScores;
        public bool isPictureQuestion;
    }

    /// <summary>
    /// Individual player round score breakdown.
    /// </summary>
    [Serializable]
    public class PlayerRoundScore
    {
        public string playerName;
        public string playerIcon;
        public int previousScore;
        public int scoreChange;
        public int newScore;
        public int placement; // Current placement after this round
        public bool answeredCorrectly;
        public bool identifiedRobot;
        public int votesReceived; // Votes for their answer
        public bool fooledByRobot; // Voted for robot answer
        public string scoringBreakdown; // "+8 correct, +4 robot ID, +8 votes (2)"
    }

    // ===========================
    // HALFTIME PAYLOADS
    // ===========================

    /// <summary>
    /// Halftime results notification.
    /// </summary>
    [Serializable]
    public class HalftimeData
    {
        public int halftimeRound; // 4 or 8
        public int totalRounds;
        public List<PlayerStanding> standings;
        public int nextRoundNumber; // Bonus round introduction
    }

    /// <summary>
    /// Player standing for halftime/final results.
    /// </summary>
    [Serializable]
    public class PlayerStanding
    {
        public string playerName;
        public string playerIcon;
        public int score;
        public int placement;
        public bool isTied;
    }

    // ===========================
    // BONUS ROUND PAYLOADS
    // ===========================

    /// <summary>
    /// Bonus round question data.
    /// </summary>
    [Serializable]
    public class BonusQuestionData
    {
        public int questionNumber; // 1, 2, or 3
        public string questionText;
        public List<BonusAnswerChoice> answerChoices; // Player name + answer pairs
        public int votingTimerDuration; // 30 seconds
    }

    /// <summary>
    /// Bonus round answer choice with player attribution.
    /// </summary>
    [Serializable]
    public class BonusAnswerChoice
    {
        public string playerName;
        public string playerIcon;
        public string answerText;
    }

    /// <summary>
    /// Bonus vote submission payload.
    /// </summary>
    [Serializable]
    public class BonusVoteData
    {
        public string playerName; // Voter
        public string votedPlayer; // Player voted for
        public int questionNumber;
        public long timestamp;
    }

    /// <summary>
    /// Bonus round results notification.
    /// </summary>
    [Serializable]
    public class BonusResultsData
    {
        public int questionNumber;
        public Dictionary<string, int> voteCounts; // PlayerName -> vote count
        public List<PlayerBonusScore> playerScores;
    }

    /// <summary>
    /// Individual player bonus round score.
    /// </summary>
    [Serializable]
    public class PlayerBonusScore
    {
        public string playerName;
        public string playerIcon;
        public int votesReceived;
        public int scoreChange; // Based on votes received
        public int newScore;
    }

    // ===========================
    // GAME COMPLETION PAYLOADS
    // ===========================

    /// <summary>
    /// Final game results notification.
    /// </summary>
    [Serializable]
    public class FinalScoresData
    {
        public List<PlayerStanding> finalStandings;
        public PlayerStanding winner;
        public PlayerStanding loser; // Last place
        public int totalRounds;
    }

    // ===========================
    // SPECTATE PAYLOADS
    // ===========================

    /// <summary>
    /// Spectate room joined notification (mobile preview).
    /// </summary>
    [Serializable]
    public class SpectateRoomData
    {
        public string roomCode;
        public int playerCount;
        public int maxPlayers; // 20
        public List<string> playerIcons; // Preview of taken icons
        public bool isGameInProgress;
    }

    /// <summary>
    /// Spectate update notification (live player count).
    /// </summary>
    [Serializable]
    public class SpectateUpdateData
    {
        public string roomCode;
        public int playerCount;
        public List<string> playerIcons;
    }

    // ===========================
    // ERROR PAYLOADS
    // ===========================

    /// <summary>
    /// Error notification payload.
    /// </summary>
    [Serializable]
    public class ErrorData
    {
        public string errorCode; // "ROOM_FULL", "DUPLICATE_NAME", "INVALID_ROOM", etc.
        public string errorMessage; // Human-readable error
        public string details; // Additional context
    }

    // ===========================
    // PLAYER JOIN/LEAVE PAYLOADS
    // ===========================

    /// <summary>
    /// Player joined notification.
    /// </summary>
    [Serializable]
    public class PlayerJoinedData
    {
        public string playerName;
        public string playerIcon;
        public int totalPlayerCount;
        public RoomState roomState; // Full room snapshot
    }

    /// <summary>
    /// Player left notification.
    /// </summary>
    [Serializable]
    public class PlayerLeftData
    {
        public string playerName;
        public int totalPlayerCount;
        public bool wasHost; // True if host disconnected
    }

    // ===========================
    // UTILITY METHODS
    // ===========================

    /// <summary>
    /// Helper methods for payload serialization/deserialization.
    /// </summary>
    public static class PayloadHelper
    {
        /// <summary>
        /// Parse JSON payload to specific type.
        /// </summary>
        public static T ParsePayload<T>(string json)
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse payload: {e.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Serialize payload to JSON.
        /// </summary>
        public static string SerializePayload<T>(T payload)
        {
            try
            {
                return JsonUtility.ToJson(payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize payload: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Convert Player list to PlayerData list for network transmission.
        /// </summary>
        public static List<PlayerData> ToPlayerDataList(List<Player> players)
        {
            List<PlayerData> data = new List<PlayerData>();
            foreach (Player player in players)
            {
                data.Add(new PlayerData
                {
                    name = player.PlayerName,
                    icon = player.Icon,
                    score = player.Score,
                    isHost = false, // Set by caller
                    isConnected = true,
                    hasAnswered = false, // Set by caller based on game phase
                    hasVoted = false
                });
            }
            return data;
        }

        /// <summary>
        /// Convert PlayerStanding list to PlayerData list.
        /// </summary>
        public static List<PlayerData> StandingsToPlayerData(List<PlayerStanding> standings)
        {
            List<PlayerData> data = new List<PlayerData>();
            foreach (PlayerStanding standing in standings)
            {
                data.Add(new PlayerData
                {
                    name = standing.playerName,
                    icon = standing.playerIcon,
                    score = standing.score,
                    isHost = false,
                    isConnected = true,
                    hasAnswered = false,
                    hasVoted = false
                });
            }
            return data;
        }
    }
}
