using UnityEngine;

namespace RobotsGame.Core
{
    /// <summary>
    /// Central repository for all game constants including scoring, timing, and validation thresholds.
    /// Based on unityspec.md GLOBAL SPECIFICATIONS section.
    /// </summary>
    public static class GameConstants
    {
        // ===========================
        // GAME MODES
        // ===========================
        public enum GameMode
        {
            EightRound = 8,
            TwelveRound = 12
        }

        // ===========================
        // SCORING
        // ===========================
        public static class Scoring
        {
            // 8-round game scoring
            public const int EightRound_CorrectAnswer = 8;
            public const int EightRound_RobotIdentified = 4;
            public const int EightRound_VoteReceived = 4;
            public const int EightRound_FooledByRobot = -8;

            // 12-round game scoring
            public const int TwelveRound_CorrectAnswer = 6;
            public const int TwelveRound_RobotIdentified = 3;
            public const int TwelveRound_VoteReceived = 3;
            public const int TwelveRound_FooledByRobot = -6;

            /// <summary>
            /// Get scoring values based on game mode
            /// </summary>
            public static (int correct, int robotId, int vote, int fooled) GetScoring(GameMode mode)
            {
                return mode switch
                {
                    GameMode.EightRound => (EightRound_CorrectAnswer, EightRound_RobotIdentified,
                                           EightRound_VoteReceived, EightRound_FooledByRobot),
                    GameMode.TwelveRound => (TwelveRound_CorrectAnswer, TwelveRound_RobotIdentified,
                                            TwelveRound_VoteReceived, TwelveRound_FooledByRobot),
                    _ => (0, 0, 0, 0)
                };
            }
        }

        // ===========================
        // TIME LIMITS (seconds)
        // ===========================
        public static class TimeLimits
        {
            public const float QuestionPhase = 60f;
            public const float EliminationPhase = 30f;
            public const float VotingPhase = 30f;
        }

        // ===========================
        // TIMER VISUAL THRESHOLDS
        // ===========================
        public static class TimerThresholds
        {
            public const float WarningThreshold = 0.25f; // 25% time remaining (yellow)
            public const float CriticalThreshold = 0.10f; // 10% time remaining (red, shaking)
            public const float FinalTenSeconds = 12f; // Audio cue at 12 seconds
            public const float TimeWarningAudio = 18f; // Voice over at 18 seconds
        }

        // ===========================
        // ANSWER VALIDATION THRESHOLDS
        // ===========================
        public static class Validation
        {
            public const float CorrectAnswerSimilarity = 0.85f; // 85% similarity
            public const int CorrectAnswerMaxDifference = 1; // Within 1 character difference

            public const float DuplicateAnswerSimilarity = 0.90f; // 90% similarity
            public const int DuplicateAnswerMaxDifference = 1; // Within 1 character difference

            public const int MinimumAnswerLength = 1;
        }

        // ===========================
        // PLAYER LIMITS
        // ===========================
        public static class PlayerLimits
        {
            public const int MinPlayers = 1;
            public const int MaxPlayers = 20;
            public const int TotalIconsAvailable = 20; // icon1 through icon20
        }

        // ===========================
        // TIMING & DELAYS
        // ===========================
        public static class Delays
        {
            public const float FadeTransitionDuration = 1f; // 1000ms
            public const float EarlySubmissionDelay = 2f; // 2-3 second delay when all submit early
            public const float MobileFollowDelay = 0.5f; // 500ms delay for mobile following desktop
            public const float PanelTransitionDelay = 3f; // 3-4 seconds between result panels
            public const float FlashWarningDuration = 2f; // 2000ms flash animation
            public const float QuestionIntroDelay = 1f; // 1 second after QuestionScreen loads
            public const float TimeWarningDelay = 0.5f; // 500ms delay for time warning VO
        }

        // ===========================
        // UI CONSTANTS
        // ===========================
        public static class UI
        {
            // Button effects
            public const float ButtonHoverScale = 1.05f;
            public const float ButtonPressScale = 0.95f;
            public const float ButtonTransitionDuration = 0.3f;

            // Text scaling
            public const float MinFontSize = 10f;
            public const float TextPaddingBuffer = 30f;

            // Responsive breakpoint
            public const int MobileMaxWidth = 768;
            public const int DesktopMinWidth = 769;

            // Viewport base dimensions
            public const float DesktopBaseWidth = 1920f;
            public const float DesktopBaseHeight = 1080f;
            public const float MobileBaseWidth = 375f;
            public const float MobileSafeAreaHeight = 0.9f; // 90vh

            // Z-index / Canvas sort order
            public const int LayerBackground = 0;
            public const int LayerForeground = 1;
            public const int LayerUI = 2;
            public const int LayerOverlay = 3;
            public const int LayerPanel = 4;
            public const int LayerModal = 5;
            public const int LayerFadeTransition = 9999;
            public const int LayerFadeTransitionTop = 10000;

            // Touch target minimum (mobile)
            public const float MinTouchTargetSize = 44f;
        }

        // ===========================
        // COLORS
        // ===========================
        public static class Colors
        {
            public static readonly Color PrimaryYellow = HexToColor("#ffd82f");
            public static readonly Color Cream = HexToColor("#fffbbc");
            public static readonly Color DarkBlue = HexToColor("#030231");
            public static readonly Color DarkPurple = HexToColor("#26174f");
            public static readonly Color BrightGreen = HexToColor("#11ffce");
            public static readonly Color Teal = HexToColor("#0e8f9f");
            public static readonly Color BrightRed = HexToColor("#fe1d4a");
            public static readonly Color DarkRed = HexToColor("#9a0a30");
            public static readonly Color Grey = HexToColor("#7c7a61");
            public static readonly Color Black = Color.black;

            private static Color HexToColor(string hex)
            {
                if (ColorUtility.TryParseHtmlString(hex, out Color color))
                    return color;
                return Color.magenta; // Fallback for invalid hex
            }
        }

        // ===========================
        // FALLBACK TEXT
        // ===========================
        public static class FallbackText
        {
            public const string NoAnswer = "No answer provided";
            public const string NoTesters = "[No Testers!]";
            public const string TieVote = "TIE VOTE!";
            public const string RobotIdentified = "ROBOT IDENTIFIED";
        }

        // ===========================
        // WARNING MESSAGES
        // ===========================
        public static class Warnings
        {
            public const string DuplicateResponse = "DUPLICATE RESPONSE";
            public const string CorrectAnswer = "CORRECT ANSWER, ENTER A DECOY";
            public const string InappropriateContent = "INAPPROPRIATE CONTENT";
        }

        // ===========================
        // AUDIO CLIP NAMES
        // ===========================
        public static class Audio
        {
            // Sound Effects
            public const string SFX_ButtonPress = "button_press";
            public const string SFX_RobotSlideOut = "robot_slide_out";
            public const string SFX_ResponsesSwoosh = "responses_swoosh";
            public const string SFX_TimerFinal10Sec = "timer_final_10sec";
            public const string SFX_InputAccept = "input_accept";
            public const string SFX_PlayerIconPop = "player_icon_pop";
            public const string SFX_Failure = "failure";
            public const string SFX_Success = "success";

            // Voice Overs (Desktop Only)
            public const string VO_LandingRules = "landingrules";
            public const string VO_QuestionIntro = "question intro";
            public const string VO_QuestionNudge = "question nudge";
            public const string VO_TimeWarning = "time warning";
            public const string VO_RobotAnswerGone = "robotanswergone";
            public const string VO_NoRobotAnswerGone = "norobotanswergone";
        }

        // ===========================
        // ANSWER TYPES
        // ===========================
        public enum AnswerType
        {
            Correct,
            Robot,
            Player
        }

        // ===========================
        // QUESTION TYPES
        // ===========================
        public enum QuestionType
        {
            Text,
            Picture
        }
    }
}
