# Game Screen Specifications

This document provides comprehensive visual, timing, and behavioral specifications for recreating each game screen in another coding language.

---

## GLOBAL SPECIFICATIONS

### GAME CONSTANTS:

**Game Modes**:
- 8-round game mode
- 12-round game mode

**Scoring (8-round game)**:
- Correct answer: +8 points
- Robot identified (elimination): +4 points
- Vote received (per vote): +4 points
- Fooled by robot (voting for robot answer): -8 points

**Scoring (12-round game)**:
- Correct answer: +6 points
- Robot identified (elimination): +3 points
- Vote received (per vote): +3 points
- Fooled by robot (voting for robot answer): -6 points

**Time Limits**:
- Question phase: 60 seconds
- Elimination phase: 30 seconds
- Voting phase: 30 seconds

**Timer Visual Thresholds**:
- Normal state: Full time to 30 seconds remaining
- Warning state: 25% time remaining (yellow)
- Critical state: 10% time remaining (red, shaking)

**Answer Validation Thresholds**:
- Correct answer detection: 85% similarity OR within 1 character difference
- Duplicate answer detection: 90% similarity OR within 1 character difference

**Player Limits**:
- Minimum players: 1
- Maximum players: 20 available icons

---

### GAME FLOW:

```
START
  ↓
[LandingPage] (Desktop: Video + Start Button | Mobile: Black Screen)
  ↓
  → Click Start/Enter/Space →
  ↓
[Join Room / Lobby] (Not specified in this document)
  ↓
  → Host starts game →
  ↓
╔═══════════════════════════════════════════════════════════╗
║ ROUND LOOP (8 or 12 times)                                ║
║                                                            ║
║  [QuestionScreen] (60 seconds)                            ║
║    Players submit creative answers                        ║
║    Desktop: Shows robot character, card flip animation    ║
║    Mobile: Text input with submit button                  ║
║    ↓                                                       ║
║    All answers submitted OR timer expires                 ║
║    ↓                                                       ║
║  [EliminationScreen] (30 seconds)                         ║
║    Players vote which answer is the robot                 ║
║    Desktop: Answer list with vote selection               ║
║    Mobile: Answer buttons with eliminate button           ║
║    ↓                                                       ║
║    All votes submitted OR timer expires                   ║
║    ↓                                                       ║
║    → Calculate elimination (most voted answer removed)    ║
║    → Show result: "ROBOT IDENTIFIED" or "TIE VOTE"        ║
║    ↓                                                       ║
║  [VotingScreen] (30 seconds)                              ║
║    Players vote for correct answer from remaining         ║
║    Desktop: Answer list with vote selection               ║
║    Mobile: Answer buttons with submit button              ║
║    ↓                                                       ║
║    All votes submitted OR timer expires                   ║
║    ↓                                                       ║
║    → Highlight true answer                                ║
║    ↓                                                       ║
║  [ResultsScreen]                                          ║
║    4 sequential panels:                                   ║
║      Panel 1: Who got correct answer + points             ║
║      Panel 2: Who identified robot vs who was fooled      ║
║      Panel 3: Player answers with vote counts             ║
║      Panel 4: Current standings (ranked by score)         ║
║    Desktop: Click "Next Round" to continue                ║
║    Mobile: Auto-advances after host                       ║
║    ↓                                                       ║
║  (Loop continues to next round)                           ║
║                                                            ║
╚═══════════════════════════════════════════════════════════╝
  ↓
  All rounds complete
  ↓
[FinalResults]
  Desktop: Winner + Last Place spotlights, New Game/Credits buttons
  Mobile: Player placement, score, Share Results/Games buttons
  ↓
  Click New Game → Return to start
  END
```

**Special Flow Notes**:
- Desktop acts as "host" - controls timing and transitions
- Mobile devices follow desktop's lead with slight delays
- All transitions include fade-to-black effects (500ms-1000ms)
- Timer pause when all players complete action early

---

### SHARED STYLING:

**Font Families**:
- Primary UI: SINK (all buttons, headers, body text)
- Question text: BILLY (question cards, large display text)
- Small labels: SIGNAL (metadata, timestamps, small text)
- Special headers: HAVOKS (room codes, emphasis text)
- Timer display: HALF AWAKE (countdown numbers)
- Sans-serif fallback for all fonts

**Color System**:
- **Primary Yellow** `#ffd82f` - Buttons, highlights, scores, emphasis
- **Cream/Off-white** `#fffbbc` - General text, backgrounds, labels
- **Dark Blue** `#030231` - Text on light backgrounds, primary content
- **Dark Purple** `#26174f` - Panel backgrounds, headers, badges
- **Bright Green** `#11ffce` - Success states, correct answers, winner highlights
- **Teal** `#0e8f9f` - Secondary success, borders, icons
- **Bright Red** `#fe1d4a` - Elimination, wrong answers, errors, warnings
- **Dark Red** `#9a0a30` - Gradients, blocked states
- **Grey (disabled)** `#7c7a61` - Disabled buttons, unselected states
- **Black** `#000000` - Backgrounds, overlays

**Standard Button Pattern**:
- Background: PNG image asset (no-repeat, contain, centered)
- No border
- Background color: transparent
- Cursor: pointer (enabled) or not-allowed (disabled)
- Drop shadow: 0.21vw 0.37vh 0.42vw rgba(0, 0, 0, 0.5)
- Hover: scale(1.05)
- Active/Press: scale(0.95) on mobile
- Transition: transform 0.3s

**Standard Spacing Patterns**:
- Desktop margins: 0.78vw to 2.60vw
- Desktop padding: 1.04vw to 2.08vw
- Mobile margins: 2.67vw to 8.00vw
- Mobile padding: 4.00vw to 5.33vw
- Gap between items: 1.04vw (desktop), 2.67vw (mobile)

**Viewport Units**:
- Desktop base: 1920px width = 100vw, 1080px height = 100vh
- Mobile base: 375px width = 100vw, safe area = 90vh (accounts for browser UI)
- Desktop conversion: 1px = 0.052vw
- Mobile conversion: 1px = 0.267vw

**Border Radius Standards**:
- Small elements: 0.52vw
- Medium elements: 1.04vw
- Large panels: 1.56vw
- Mobile elements: 2.67vw to 5.34vw

**Z-Index Layering**:
- Layer 0: Background images
- Layer 1-2: Foreground elements, robot characters, UI elements
- Layer 3-4: Overlays, answer panels, popups
- Layer 5: Modal windows
- Layer 9999-10000: Fade transition overlays

---

### DATA STRUCTURES:

**Question Format**:
```
Question {
  id: unique identifier (text)
  question: question text (text)
  correctAnswer: the true answer (text)
  robotAnswer: AI-generated decoy (text)
  type: "text" or "picture"
  round: round number 1-12 (integer)
  imageUrl: image URL (text, only for picture questions)
}
```

**Player Answer Format**:
```
Answer {
  text: answer content (text)
  type: "correct" | "robot" | "player"
  playerName: who submitted it (text)
}
```

**Player Format**:
```
Player {
  name: display name (text)
  icon: icon key "icon1" through "icon20" (text)
  score: cumulative points (integer)
}
```

**Round Score Tracking**:
```
RoundScore {
  playerName: player identifier (text)
  total: round total points (integer, can be negative)
  correctAnswer: points from correct answer, 0 or 8/6 (integer)
  robotIdentified: points from elimination, 0 or 4/3 (integer)
  votesReceived: count of votes received (integer)
  fooled: penalty points, 0 or -8/-6 (integer)
  icon: player icon key (text)
}
```

**Vote Results Format**:
```
VoteResults {
  eliminatedAnswer: answer text or null if tie (text or null)
  tieOccurred: true if tied vote (boolean)
  voteCounts: map of answer to vote count (dictionary)
  totalVotesCast: number of votes (integer)
}
```

---

### RESPONSIVE BEHAVIOR:

**Desktop (≥769px width)**:
- Full 100vh height
- Complex multi-panel animations
- All player answers visible (including own)
- Keyboard navigation (Enter/Space keys)
- Hover effects on buttons
- Larger text and spacing
- Shows robot character animations
- Acts as game host (controls timing)

**Mobile (≤768px width)**:
- 90vh height (safe area for browser UI)
- Simplified layouts, minimal animations
- Own answer filtered from voting lists
- Touch-only interaction
- Press feedback (scale 0.95)
- Smaller text with auto-scaling
- Static backgrounds
- Follows host timing with delays

**Element Scaling Rules**:
- Text: Dynamically scales based on length to fit container
  - Minimum font size: 10px
  - Padding buffer: 30px
  - Scale factor: containerWidth / measuredTextWidth
- Icons: Fixed sizes converted to viewport units
  - Large: 140px (7.28vw desktop)
  - Medium: 70px (3.64vw desktop)
  - Small: 60px (3.12vw desktop)
- Panels: Scale to viewport with max-width constraints
- Buttons: Fixed aspect ratios, scale with viewport

**Minimum Dimensions**:
- Mobile minimum width: 320px
- Desktop minimum width: 1024px
- Text minimum size: 10px
- Icon minimum size: 40px
- Button minimum touch target: 44px × 44px (mobile)

**Maximum Dimensions**:
- Desktop max width: No limit (scales with vw)
- Panel max width: 70vw (varies by screen)
- Text max size: Based on container and question length
- Video max width: 63vw

---

### EDGE CASES:

**Long Answer Text**:
- Desktop: Dynamic font sizing (2.29vw down to 1.61vw based on answer count)
- Mobile: Auto-scales using dynamic text algorithm (minimum 10px)
- Extremely long answers: Truncate with ellipsis after maximum length (implementation-defined)

**Very Short Answers**:
- Minimum 1 character required for submission
- Empty string treated as no answer submitted

**No Answer Submitted**:
- System inserts fallback: "No answer provided"
- Fallback cannot be voted for (disabled in UI)
- Does not count toward duplicate detection

**Empty States**:
- No players voted correctly: Show "[No Testers!]" fallback text (yellow)
- No votes received on player answer: Show "0 votes"
- Tie vote (elimination): Show "TIE VOTE!" message, no answer removed

**Single Player Game**:
- All screens function normally
- Shows only winner spotlight on FinalResults
- Voting still occurs (player votes among robot/correct/own answer)

**Maximum Players (20)**:
- Icon grid wraps to multiple rows
- Answer list scrollable if needed (implementation-defined)
- All 20 icons available: icon1 through icon20

**Timer Expiration**:
- Question phase: Submit current answer or "No answer provided"
- Elimination phase: Submit current selection or no vote
- Voting phase: Submit current selection or no vote
- No penalty for missing votes (just no points earned)

**All Players Submit Early**:
- Timer stops
- Immediate transition to next phase
- 2-3 second delay for visual feedback

**Network Delays**:
- Desktop waits for all answers before proceeding
- Mobile follows desktop transitions with 500ms delay
- State updates broadcast to all clients

**Duplicate Answer Detection**:
- Mobile: Show flash warning "DUPLICATE RESPONSE", clear field, restore input
- Desktop: Real-time validation, prevent submission
- Duration: 2000ms flash animation

**Correct Answer Attempted**:
- Mobile: Show flash warning "CORRECT ANSWER, ENTER A DECOY", restore original text
- Desktop: Real-time validation, prevent submission
- Duration: 2000ms flash animation

**Profanity Detection**:
- Mobile: Show flash warning "INAPPROPRIATE CONTENT", clear field
- Desktop: Real-time validation, prevent submission
- Duration: 2000ms flash animation
- Uses substring matching on normalized text

---

### AUDIO CUES:

**Sound Effects**:
- **button_press**: All button clicks (desktop and mobile)
- **robot_slide_out**: Robot character appears on QuestionScreen
- **responses_swoosh**: Answers overlay appears on QuestionScreen
- **timer_final_10sec**: Timer reaches 12 seconds (plays once)
- **input_accept**: Answer accepted on mobile
- **player_icon_pop**: Player icon appears when answer submitted (desktop)
- **failure**: Duplicate or profanity warning triggered
- **success**: Player voted correctly (mobile only, VotingScreen)

**Voice Overs (Desktop Only)**:
- **landingrules**: Rules video audio on LandingPage
- **question intro**: Plays 1 second after QuestionScreen loads
- **question nudge**: Plays at 30 seconds remaining (if not submitted)
- **time warning**: Plays at 18 seconds remaining (500ms delay)
- **robotanswergone**: Plays after elimination complete
- **norobotanswergone**: Plays after tie vote result

**Audio Behavior**:
- All audio clears when transitioning screens
- SFX play immediately on action
- VO plays with specified delays
- Timer sounds play once at threshold (not repeating)
- Mobile success sound conditional (only if correct vote)

---

## SCREEN: LandingPage

### ASSETS USED:
- **Background**: `landingbg.png` (from `assets/mainscreenbg/`)
- **Fonts**:
  - SINK (sans-serif fallback) - `assets/fonts/SINK.otf`
- **Images**:
  - `startgamebutton.png` (from `assets/ui/`)
- **Video**:
  - `rulesvideo.mp4` (from `/assets/video/`)

### LAYOUT SPECIFICATIONS:

**Desktop View:**
- **Container**: Fixed position, fills 100vw x 100vh, overflow hidden
  - Background: `landingbg.png` stretched to 100% x 100%
  - Position: center
  - Background color: #000 (black)
  - Font family: SINK, sans-serif

- **Fade Overlay (Initial)**:
  - Type: Div overlay
  - Position: Fixed, top 0, left 0
  - Size: 100% x 100%
  - Color: black
  - Opacity: 1 initially, fades to 0
  - Transition: opacity 1s ease-in-out
  - Z-index: 9999
  - Pointer events: none

- **Video Container**:
  - Type: Video element with container div
  - Position: Relative, centered
  - Width: 74.2% of viewport
  - Height: auto
  - Margin top: -4.37vh
  - Margin bottom: 7.41vh
  - Video attributes: loop, playsInline, muted initially (unmuted after user interaction)
  - Max width: clamp(50.4vw, 59.06vw, 63vw)
  - Border radius: 0.78vw
  - Object fit: contain
  - Z-index: 2

- **Start Game Button**:
  - Type: Button with background image
  - Position: Absolute, bottom 5%
  - Background: `startgamebutton.png` (contain, no-repeat, centered)
  - Size: 16.6vw x 9.84vh
  - Border: none
  - Background color: transparent
  - Cursor: pointer
  - Filter: drop-shadow(0.21vw 0.37vh 0.42vw rgba(0, 0, 0, 0.5))
  - Transition: transform 0.3s
  - Hover: scale(1.05)
  - Normal: scale(1)

- **Fade to Black Overlay (Exit)**:
  - Type: Div overlay
  - Position: Fixed, top 0, left 0
  - Size: 100% x 100%
  - Color: black
  - Opacity: 0 initially, fades to 1
  - Transition: opacity 1s ease-in-out
  - Z-index: 10000
  - Pointer events: auto when active, none otherwise

**Mobile View:**
- **Container**: Fixed position, 100vw x 90vh
  - Background: black (#000)
  - Display: flex, centered
  - Content: Empty black screen (no elements displayed)

### TIMING/ANIMATIONS:

**Desktop:**
- **Initial Fade In**:
  - Delay: 100ms after mount
  - Duration: 1s
  - Effect: Opacity 0 � 1

- **Video Autoplay**:
  - Timing: Immediately on mount
  - Behavior: Video plays automatically with audio

- **Button Click to Fade Out**:
  - Trigger: Button click
  - Delay before transition: 1000ms (1 second)
  - Fade duration: 1s

**Mobile:**
- No animations (static black screen)

### STATE VARIABLES:

**Input:**
- None (entry point)

**Internal:**
- `fadeIn` (boolean) - Controls initial fade-in overlay
- `fadeOut` (boolean) - Controls exit fade-out overlay

**Output:**
- Calls `onGoToJoinRoom()` when transitioning to next screen

### USER INTERACTIONS:

**Desktop:**
- **Click Targets**:
  - Start Game Button � Plays SFX 'button_press', stops any VO, triggers fade out and transition
- **Keyboard**:
  - Enter key � Same as button click
  - Space key � Same as button click

**Mobile:**
- **Click Targets**:
  - Start Game Button � Immediately calls `onGoToJoinRoom()` (no fade transition)

### GAME LOGIC:

**Entry Conditions:**
- First screen shown when app loads
- Both desktop and mobile see this screen

**Exit Conditions:**
- Button click (desktop with 1s fade delay)
- Button click (mobile immediate)
- Keyboard press (Enter or Space on desktop)

**Special Rules:**
- Desktop shows rules video with fade transitions
- Mobile shows blank black screen (to avoid confusion with lobby)
- Video loops continuously
- Audio manager clears any playing VO on button press

### EXAMPLE DATA:
- Video source: `/assets/video/rulesvideo.mp4`
- Button text: "START GAME" (hidden with visibility:hidden, using image)

---

## SCREEN: QuestionScreen

### ASSETS USED:
- **Background**:
  - Desktop: Round-specific backgrounds (`Q1BG.png` through `Q12BG.png` from `assets/mainscreenbg/`)
  - Mobile: `mobilecontrollerbg.png`, `questionmobile.png`, `picqmobile.png` (from `assets/mobilebg/`)
- **Fonts**:
  - SINK (main UI) - `assets/fonts/SINK.otf`
  - BILLY (question text) - `assets/fonts/BILLY.otf`
  - SIGNAL (small text) - `assets/fonts/SIGNAL.ttf`
- **Images**:
  - `submitanswerbutton.png` (from `assets/ui/`)
  - Round-specific foregrounds: `Q1FG.png` through `Q12FG.png` (from `assets/mainscreenbg/`)
  - Round-specific blink foregrounds: `Q1FG-blink.png` through `Q12FG-blink.png`
  - Player icons (various, from `components/PlayerIcon`)
- **Animations**:
  - Lottie timer animation: `TimerAnim.json` (from `animations/timer anim/`)

### LAYOUT SPECIFICATIONS:

**Desktop View:**

- **Container**: Fixed position, 100vw x 100vh, overflow hidden
  - Background: Round-specific background image (100vw x 100vh)
  - Position: bottom center
  - Font family: SINK, sans-serif

- **Foreground Overlay (Robot Character)**:
  - Type: Absolutely positioned div with background image
  - Position: Round-specific (see foregroundTopSettings in code)
    - Example Round 1: top 8% (3% + 5% adjustment), left -15%, width 100vw, height 100vh
    - Size: Round-specific (e.g., R1: 77%, R2: 73%, R8: 154%)
  - Background: Round-specific foreground PNG
  - Animation: slideInFromLeft 1000ms ease-in-out forwards
  - Initial transform: translateX(-100%)
  - Z-index: 1
  - Pointer events: none

- **Blink Foreground Overlay**:
  - Same positioning as regular foreground
  - Only visible during blink state (every 5 seconds for 200ms)
  - Z-index: 2
  - Blink timing: Every 5000ms, duration 200ms

- **Question Panel (Text Questions)**:
  - Type: Card with flip animation
  - Position: Absolute, top 5vh, left 5vw
  - Size: 75vw x 26.25vh
  - Background: #fffbbc (off-white/cream)
  - Border radius: varies
  - Z-index: 2

  - **Front Face (Placeholder)**:
    - Text: "WRITE A CREATIVE ANSWER TO THIS QUESTION"
    - Font: SINK, 3.13vw, uppercase
    - Color: #26174f (dark purple)
    - Fade in: 1.2s delay
    - Fade out: 3.8s delay

  - **Back Face (Question)**:
    - Flip animation: 4.2s delay, 0.6s duration
    - Text reveal: 4.8s delay
    - Font: SINK, 3.13vw, uppercase
    - Color: #030231 (dark blue)
    - Letter spacing: 0.10vw
    - Text align: left

- **Question Panel (Picture Questions)**:
  - Position: Absolute, top 4.63vh, left 45%
  - Transform: translateX(-50%)
  - Z-index: 2
  - Content: PictureQuestionScreen component

- **Player Icons Response Zone**:
  - Position: Absolute
    - Text questions: top 50%, left 45%
    - Picture questions: top 40%, left 75%
  - Transform: translate(-50%, -50%)
  - Size:
    - Text: 41.67vw x 37.04vh
    - Picture: 15.63vw x 37.04vh
  - Display: flex, wrap, centered
  - Gap:
    - Text: 1.04vw
    - Picture: 0.52vw
  - Z-index: 2

  - **Player Icon**:
    - Size: 140px (text) or 70px (picture)
    - Background: rgba(255, 255, 255, 0.9)
    - Border radius: 50%
    - Padding: 0.26vw (text) or 0.16vw (picture)
    - Margin bottom: 0.42vw
    - Border: 0.16vw solid #0e8f9f
    - Box shadow: 0 0.21vw 0.78vw rgba(0,0,0,0.3)
    - Animation: playerAppear 0.5s ease-in-out

  - **Player Name Label**:
    - Font: SIGNAL, 1.20vw, bold
    - Color: #fffbbc
    - Background: rgba(0, 0, 0, 0.7)
    - Padding: 0.21vw 0.42vw
    - Border radius: 0.78vw
    - Min width: 4.17vw

- **Answers Overlay (Desktop Only)**:
  - Position: Absolute
    - Text: top -28vh, left -20vw (relative to player icons)
    - Picture: top -18vh, left -25vw
  - Size:
    - Text: 35vw
    - Picture: 32vw
  - Background: rgba(4, 22, 37, 0.85)
  - Border: 0.21vw solid rgba(255, 255, 255, 0.25)
  - Border radius: 1.25vw
  - Padding: 1.56vw
  - Box shadow: 0 0.78vw 2.60vw rgba(0, 0, 0, 0.45)
  - Backdrop filter: blur(0.52vw)
  - Z-index: 3

  - **Header**:
    - Text: "Answers Received"
    - Font: SIGNAL, 1.56vw, bold (700)
    - Color: #E9F7FF
    - Letter spacing: 0.10vw
    - Text transform: uppercase
    - Margin bottom: 0.83vw

  - **Answer Item**:
    - Background: rgba(11, 52, 73, 0.65)
    - Border radius: 0.73vw
    - Padding: 0.73vw 0.94vw
    - Border: 0.10vw solid rgba(255, 255, 255, 0.12)
    - Gap between items: 0.73vw
    - Reveal animation: Cascade every 200ms per item
    - Transform: translateX(-5vw) � translateX(0)
    - Opacity: 0 � 1
    - Transition: 0.5s ease

    - Player name: SIGNAL, 0.94vw, uppercase, rgba(255, 255, 255, 0.75)
    - Answer text: 1.25vw, #FFFFFF

- **Timer (Lottie Animation)**:
  - Position: Absolute, bottom 0, right 0
  - Size: 14.58vw (calculated from window.innerWidth)
  - Variant: default
  - Initial time: 60 seconds
  - Animation: Scale in from 0 to 1
  - Delay: 1.2s
  - Transition: all 0.5s cubic-bezier(0.4, 0, 0.2, 1)
  - Z-index: 2

**Mobile View:**

- **Container**: Fixed position, 100vw x 90vh
  - Background: `questionmobile.png` or `picqmobile.png`
  - Background size: 100vw 90vh (forced exact)
  - Background position: center center
  - Font family: SINK, sans-serif

- **Content Wrapper**:
  - Position: Relative, min-height 90vh
  - Padding: 5.33vw
  - Display: flex, column, centered
  - Gap: 4vh
  - Z-index: 2

- **Question Display**:
  - Width: 80%
  - Max width: 90.67vw
  - Min height: 40vh (45vh for picture questions)
  - Padding: 2.67vw
  - Margin top: 0vh (text) or -5vh (picture)

  - **Text Question**:
    - Font: BILLY, dynamic size (9.60vw to 4.27vw based on length)
    - Color: #ffd82f (yellow)
    - Text transform: uppercase
    - Letter spacing: 0.27vw
    - Line height: 1.3
    - Text align: center
    - Typewriter animation: 50ms delay per character
    - Fade in: opacity 0 � 1 over 0.2s

- **Input Field**:
  - Width: 80%
  - Max width: 90.67vw
  - Padding: 2.93vw 4.00vw
  - Font: SINK, 4.80vw
  - Border: 0.53vw solid #fffbbc (normal) or #fe1d4a (warning)
  - Border radius: 1.33vw
  - Background:
    - Normal: rgba(255, 251, 188, 0.9)
    - Submitted: rgba(3, 2, 49, 0.25)
    - Warning flash: #fe1d4a
  - Color:
    - Normal: #030231
    - Flash: #ffffff
  - Opacity: Always 1
  - Transition: all 0.3s ease
  - Animation (flash): borderPulse 0.5s ease-in-out 4 times
  - Disabled when: hasSubmitted OR timeLeft === 0
  - Read-only when: flashing warning message

  - **Placeholder states**:
    - Default: "Type your answer here..."
    - Submitted: "Answer submitted!"
    - Time up: "Time's up!"
    - Decoy warning: "Submit a decoy answer"
    - Duplicate warning: "Try a different answer"
    - Profanity warning: "Enter appropriate content"

- **Warning Messages** (Mobile flash messages):
  - **Decoy Flash**:
    - Message: "CORRECT ANSWER, ENTER A DECOY"
    - Duration: 2000ms
    - Background: #fe1d4a
    - Text color: #ffffff
    - Restores original text after flash

  - **Duplicate Flash**:
    - Message: "DUPLICATE RESPONSE"
    - Duration: 2000ms
    - Background: #fe1d4a
    - Sound: 'failure' SFX
    - Clears field after flash

  - **Profanity Flash**:
    - Message: "INAPPROPRIATE CONTENT"
    - Duration: 2000ms
    - Background: #fe1d4a
    - Sound: 'failure' SFX
    - Clears field after flash

- **Submit Button**:
  - Background: `submitanswerbutton.png`
  - Width: 80%
  - Max width: 90.67vw
  - Height: 7.88vh
  - Opacity: 0.5 (disabled) or 1 (enabled)
  - Cursor: not-allowed (disabled) or pointer (enabled)
  - Disabled when: no text OR submitted OR time === 0 OR warnings active
  - Sound on success: 'input_accept' (mobile) or 'player_icon_pop' (desktop)

### TIMING/ANIMATIONS:

**Desktop:**
- **Fade from black**: 0-1000ms, removes black overlay
- **Robot slideout**: 100ms delay, 1000ms duration, slide from left
- **Timer scale in**: 1200ms delay, 500ms duration, scale 0�1
- **Question card animations**:
  - Slide in: 0-800ms
  - Placeholder fade in: 1200ms
  - Placeholder fade out: 3800ms
  - Card flip: 4200ms (600ms duration)
  - Question text reveal: 4800ms
- **Player icon pop**: Immediate when answer received, 500ms animation
- **Answers overlay**:
  - Appears: 2000ms after all answers received
  - Swoosh SFX plays when appearing
  - Items reveal: Cascade 1000ms + (200ms � index)
- **Timer urgency levels**:
  - 30s remaining: urgencyLevel = 1
  - 18s remaining: urgencyLevel = 2, plays 'time' VO after 500ms delay
  - 12s remaining: urgencyLevel = 3, plays 'timer_final_10sec' SFX once
- **Blink animation**:
  - Starts after blink image loads
  - Interval: Every 5000ms
  - Duration: 200ms per blink

**Mobile:**
- **Question fade in**: 1000ms delay, 200ms duration
- **Input flash animations**: 2000ms duration for warnings
- **No card flip** (instant question display)

**Voice Overs:**
- **Question intro**: 1s delay after screen loads (desktop only)
- **Nudge VO**: At 30s remaining (desktop only, if not submitted)

### STATE VARIABLES:

**Input:**
- `round` (number) - Current round (1-12)
- `questionData` (object) - Contains question, correctAnswer, robotAnswer, type
- `players` (array) - List of all players
- `gameMode` (string) - "8" or "12"

**Internal:**
- `playerAnswer` (string) - Player's typed answer
- `hasSubmitted` (boolean) - Whether player has submitted
- `playersWithAnswers` (array) - Names of players who submitted
- `allAnswersReceived` (boolean) - All players submitted
- `showAnswersOverlay` (boolean) - Desktop overlay visible
- `showDecoyWarning` (boolean) - Player tried to submit correct answer
- `showDuplicateWarning` (boolean) - Player tried to submit duplicate
- `showProfanityWarning` (boolean) - Player submitted banned word
- `submittedAnswers` (array) - Cached answers for duplicate detection
- `overlayAnswers` (array) - Answers to show in overlay
- `overlayRevealedIndices` (array) - Which overlay items to show
- `timeLeft` (number) - Seconds remaining
- `urgencyLevel` (number) - 0=normal, 1=urgent, 2=critical, 3=final
- `isBlinking` (boolean) - Robot blink state
- `fadeFromBlack` (boolean) - Entry transition
- `questionVisible` (boolean) - Question revealed
- `isQuestionCardFlipped` (boolean) - Desktop card flip state
- `timerVisible` (boolean) - Timer animation state

**Output:**
- Calls `onSubmit(playerAnswer, allAnswers)` when all answers received

### USER INTERACTIONS:

**Desktop:**
- **Input field**: Type answer (auto-checks for correct/duplicate/profanity)
- **Keyboard**: Enter key submits answer
- **Validation**: Real-time checking while typing

**Mobile:**
- **Input field**: Type answer with on-screen keyboard
- **Submit button**: Touch to submit
- **Flash warnings**: Visual feedback for invalid submissions

**Both:**
- **Answer submission**:
  1. Checks if answer matches correct answer (Levenshtein distance d 1 or 85% similarity) � Shows decoy warning
  2. Checks against submitted answers (within 1 letter or 90% similarity) � Shows duplicate warning
  3. Checks against robot answer (within 1 letter or 90% similarity) � Shows duplicate warning
  4. Checks for banned words (substring match) � Shows profanity warning
  5. If all checks pass � Submits and adds to playersWithAnswers
- **Auto-submit**: When timer expires, submits current answer or NO_ANSWER_FALLBACK

### GAME LOGIC:

**Entry Conditions:**
- Triggered after previous round's results screen
- Shows round-specific background and robot

**Exit Conditions:**
- All players submit answers � 2s delay � Shows answers overlay � 3s delay � Transitions to EliminationScreen
- Timer expires � Auto-submits � Same as above
- Desktop host triggers 'request-transition-to-elimination' event

**Validation:**
- **Correct Answer Detection**:
  - Normalized comparison (lowercase, no punctuation, normalized whitespace)
  - Exact match OR within 1 letter OR 85% similarity
- **Duplicate Detection**:
  - Checks against all previously submitted answers
  - Robot answer always treated as submitted
  - Within 1 letter OR 90% similarity
- **Profanity Filter**:
  - Normalized text (lowercase, no special chars, no spaces)
  - Substring match against banned words list

**Special Rules:**
- Question data includes: question text, correctAnswer, robotAnswer, type ("text" or "picture")
- Seeded answers (robot + correct) prevent submission from start
- Desktop shows player icons as they submit
- Mobile doesn't show own answer in duplicate checking
- Timer pauses when all answers received
- Desktop sync delay: 3000ms, mobile follow delay: 500ms

### EXAMPLE DATA:
```javascript
questionData: {
  id: "q1_round1",
  question: "What's the best pizza topping?",
  correctAnswer: "Pepperoni",
  robotAnswer: "Motor Oil",
  type: "text",
  round: 1
}

players: [
  { name: "Alice", icon: "icon1" },
  { name: "Bob", icon: "icon2" }
]

allAnswers: [
  { text: "Pepperoni", type: "correct", playerName: "System" },
  { text: "Motor Oil", type: "robot", playerName: "Robot" },
  { text: "Pineapple", type: "player", playerName: "Alice" },
  { text: "Mushrooms", type: "player", playerName: "Bob" }
]
```

---

## SCREEN: EliminationScreen

### ASSETS USED:
- **Background**:
  - Desktop: `eliminatevotebg.png` (from `assets/mainscreenbg/`)
  - Mobile: `namecodeiconmobile.png` (from `assets/mobilebg/`)
- **Fonts**:
  - SINK (main UI) - `assets/fonts/SINK.otf`
  - SIGNAL (small text) - `assets/fonts/SIGNAL.ttf`
- **Images**:
  - `eliminatehero.png` (from `assets/ui/`)
  - `robotidentno-slideout.png` (from `assets/ui/`)
  - Player icons
- **Animations**:
  - Lottie timer: `TimerAnim.json` with altcolor variant

### LAYOUT SPECIFICATIONS:

**Desktop View:**

- **Container**: Fixed position, 100vw x 100vh
  - Background: `eliminatevotebg.png` (100vw x 100vh)
  - Position: center center
  - Font family: SINK, sans-serif

- **Hero Image**:
  - Position: Absolute, top -5%, right 0%
  - Size: 80% x 80%
  - Background: `eliminatehero.png`
  - Background size: contain
  - Background position: top right
  - Z-index: 2

- **Timer (Lottie)**:
  - Position: Absolute, bottom 0, right 0
  - Size: 14.58vw
  - Variant: altcolor
  - Initial time: 30 seconds
  - Scale animation: 0 � 1 over 0.5s after 1s delay
  - Z-index: 2

- **Answer List Container**:
  - Position: Absolute, top 5%, left 5%
  - Size: 55% x 90%
  - Display: flex, column, centered vertically
  - Justify content: center
  - Align items: flex-start
  - Padding: 1.04vw
  - Z-index: 2

- **Answer Item** (dynamic sizing based on count):
  - Font: SINK, bold, uppercase
  - Color: #030231 (normal) or #ffd82f (eliminated)
  - Letter spacing: 0.10vw
  - Border: none
  - Background: transparent (normal) or gradient(#9a0a30, #fe1d4a) (eliminated)
  - Border radius: 0.63vw
  - Text align: left
  - Margin: dynamic (0.39vw to 0.16vw based on count)
  - Padding: dynamic (1.30vw to 0.78vw based on count)
  - Font size: dynamic
    - 1 answer: 4.59vw
    - 2 answers: 4.02vw � 1.3 (30% larger)
    - 3 answers: 3.44vw � 1.2 (20% larger)
    - 4 answers: 2.98vw
    - 5 answers: 2.64vw
    - 6 answers: 2.29vw
    - 7 answers: 2.07vw
    - 8 answers: 1.84vw
    - 9+ answers: 1.61vw
  - Reveal animation: translateX(-5vw) � translateX(0)
  - Opacity: 0 � 1
  - Transition: 0.5s ease
  - Stagger delay: 1000ms + (200ms � index)
  - Cursor: pointer (clickable) or not-allowed (own answer/voted)

- **Eliminated Highlight**:
  - Red gradient background: linear-gradient(45deg, #9a0a30, #fe1d4a)
  - Yellow text: #ffd82f
  - Label appended: "(ELIMINATED)" in 0.4em size, #fffbbc color
  - Hold duration: 4000ms (desktop)
  - Slight scale: 1.02
  - Box shadow: 0 0.21vw 0.42vw rgba(214, 48, 49, 0.3)

- **Robot Ident Result** (after elimination):
  - Position: Absolute, left 5%, top 30%
  - Width: 60%
  - Z-index: 4

  - **Headline Panel**:
    - Background: #26174f
    - Padding: 0.8vw 2vw
    - Margin: -2vw left, 1.5vw bottom
    - Display: inline-block
    - Text: "ROBOT IDENTIFIED"
    - Font: SINK, 5.2vw, bold
    - Gradient text: linear-gradient(90deg, #ffd82f, #fe1d4a)
    - Letter spacing: 0.15vw
    - Text transform: uppercase

  - **Subheadline**:
    - Text: "Eliminated Response:"
    - Font: SINK, 1.8vw
    - Color: #fffbbc
    - Margin bottom: 0.8vw
    - Letter spacing: 0.05vw

  - **Eliminated Answer Text**:
    - Font: Havoks, 3.5vw, bold
    - Color: #26174f
    - Text transform: uppercase
    - Letter spacing: 0.08vw
    - Line height: 1.2
    - Margin bottom: 2vw
    - Wrapped in quotes

  - **Footer**:
    - Text: "[YOU WILL NOW CHOOSE FROM THE REMAINING RESPONSES]"
    - Font: SINK, 1.5vw, italic
    - Color: #fffbbc
    - Letter spacing: 0.05vw

  - **Animation**:
    - Slide up from translateY(50px) � translateY(0)
    - Opacity 0 � 1
    - Duration: 0.8s cubic-bezier
    - Delay: Robot ident shows first, text follows after 500ms

- **Tie Result**:
  - Shows `robotidentno-slideout.png` image instead
  - Position: Absolute, top 50%, left 5.21vw
  - Size: 52.08vw x 55.56vh
  - Margin top: -27.78vh (centers vertically)
  - Slide animation: translateY(150%) � translateY(0)
  - Duration: 0.8s cubic-bezier
  - Z-index: 3

- **Instruction Text** (bottom left):
  - Position: Absolute, left 3%, bottom 3%
  - Font: SIGNAL, clamp(16px, 1.46vw, 24px)
  - Color: #fffbbc
  - Letter spacing: 0.05vw
  - Line height: 1.4
  - Text: "Vote for which answer you think is the robot"
  - Pointer events: none
  - Z-index: 2

**Mobile View:**

- **Container**: Fixed position, 100vw x 90vh
  - Background: `namecodeiconmobile.png` (100vw x 90vh)
  - Font family: SINK, sans-serif

- **Content Wrapper**:
  - Position: Relative, min-height 90vh
  - Padding: 5.33vw
  - Display: flex, column, centered
  - Z-index: 2

- **Header**:
  - Text: "ELIMINATE THE ROBOT"
  - Font: SINK, 6.40vw
  - Color: #fe1d4a (red)
  - Margin: 0 0 8.00vw 0
  - Hidden when voting results visible

- **Voting Results Display** (after vote):

  **Tie Result:**
  - Width: 80vw (10% narrower)
  - Position: Absolute, centered (left 50%, top 50%, transform translate(-50%, -50%))
  - Padding: 5.33vw
  - Display: flex, column, centered

  - **Title**: "TIE VOTE!"
    - Font: SINK, 24.00vw (3� larger)
    - Color: #ffd82f (bright yellow)
    - Margin: 0 0 4.00vw 0

  - **Subtitle**: "No answer eliminated due to tie vote!"
    - Font: SINK, 6.13vw
    - Color: #fffbbc
    - Margin: 0 0 4.00vw 0

  - **Footer**: "Moving to final voting round..."
    - Font: SINK, 5.33vw (25% larger), italic
    - Color: #fffbbc

  **Normal Elimination:**
  - Same 80vw width and centering
  - **Title**: "Eliminated Answer:"
    - Font: SINK, 18.13vw
    - Color: #ffd82f
    - Margin: 0 0 5.33vw 0

  - **Answer Text**: Uses dynamic sizing utility
    - Base font: 60px SINK
    - Wrapped in quotes
    - Color: #fffbbc
    - Width: 90% (of 80vw container)
    - Auto-scales to fit

- **Answer List** (before vote):
  - Width: 90% (1% narrower for mobile fit)
  - Max width: 93.87vw
  - Margin bottom: 5.33vw
  - Filters out player's own answer on mobile

  - **Answer Button**:
    - Padding: 4.00vw
    - Margin: 2.67vw 0
    - Background: Dynamic based on state
      - Own answer: #fffbbc (blocked, desktop only)
      - Selected: #fe1d4a (red)
      - After vote: #fe1d4a (selected) or #7c7a61 (others greyed)
      - Normal: #fffbbc (off-white)
      - Eliminated highlight: #fe1d4a
    - Color: Dynamic
      - Own answer: #030231 (desktop only)
      - Selected: #ffd82f (yellow)
      - After vote: #ffd82f (selected) or #030231 (others)
      - Normal: #030231
      - Eliminated: #ffd82f
    - Border: Dynamic
      - Own answer: 0.53vw solid #0e8f9f
      - Eliminated: 0.80vw solid #ffd82f
      - Selected: 0.53vw solid #ffd82f
      - Normal: 0.53vw solid #fffbbc
    - Border radius: 2.67vw (doubled roundness)
    - Font: Uses getDynamicTextStyle utility (base 16px SIGNAL)
    - Cursor: not-allowed (voted/own) or pointer
    - Transition: all 0.3s ease
    - Eliminated transform: scale(1.02)
    - Eliminated shadow: 0 1.07vw 2.13vw rgba(214, 48, 49, 0.3)

- **Eliminate Button**:
  - Width: 71% (5% narrower for mobile fit)
  - Max width: 93.87vw
  - Padding: 4.00vw 8.00vw
  - Font: SINK, 6.67vw (25% larger text), bold
  - Letter spacing: 0.53vw
  - Border: none
  - Border radius: 5.34vw (doubled roundness)
  - Background: Dynamic gradient
    - Voted: linear-gradient(to right, #fe1d4a, #9a0a30)
    - Time up: #ccc
    - Selected: #fe1d4a
    - None selected: #ccc
  - Color: Dynamic
    - Voted: #ffd82f
    - Time up: #030231
    - Selected: #ffd82f
    - None selected: #030231
  - Cursor: not-allowed (disabled) or pointer
  - Margin bottom: 5.33vw
  - States:
    - Disabled: !selectedAnswer OR hasVoted OR timeLeft === 0
  - Text states:
    - Voted: "CALCULATING VOTES"
    - Time up: "VOTING CLOSED"
    - Normal: "ELIMINATE THIS ANSWER"

### TIMING/ANIMATIONS:

**Desktop:**
- **Fade from black**: 0-500ms
- **Timer scale in**: 1000ms delay, 500ms duration
- **Answer reveals**: Staggered cascade
  - First: 1000ms delay
  - Each subsequent: +200ms
  - Animation: translateX(-5vw) � 0, opacity 0 � 1
  - Duration: 500ms ease

**Elimination Flow:**
- **Red highlight hold**: 4000ms (desktop) or 2000ms (mobile)
- **Fade out answers**: 500ms after highlight
- **Robot ident appears**: 50ms after fade
- **Robot ident slides in**: 0ms delay
- **Results text reveal**: 500ms after slide
- **Results hold**: 5000ms
- **Fade to black**: 500ms duration
- **Slide out**: 100ms after fade to black (desktop)
- **Host transition**: max(calculated timing, 500ms HOST_SYNC_DELAY)
- **Mobile follow**: 500ms MOBILE_FOLLOW_DELAY

**Tie Flow:**
- **Initial delay**: 3000ms
- **Fade out answers**: 500ms
- **Robot ident no image**: 50ms after fade, slides in
- **Results text**: 500ms after slide
- **Results hold**: 5000ms
- **Slide out**: 600ms
- **Host transition**: max(calculated, 500ms)

### STATE VARIABLES:

**Input:**
- `eliminatedAnswer` (string or null) - Previously eliminated answer
- `questionData` (object) - Current question data
- `allPlayerAnswers` (array) - All submitted answers
- `playerAnswer` (string) - Current player's answer
- `players` (array) - All players

**Internal:**
- `selectedAnswer` (string) - Player's selected answer to eliminate
- `hasVoted` (boolean) - Player has voted
- `votingResults` (object or null) - Results from server
- `eliminatedAnswer` (string or null) - Answer that was eliminated
- `showRedHighlight` (boolean) - Show red highlight on eliminated
- `visibleAnswers` (array) - Indices of revealed answers
- `showResultsAnimation` (boolean) - Results panel visible
- `showRobotIdent` (boolean) - Robot ident graphic visible
- `robotIdentAnimateIn` (boolean) - Trigger slide animation
- `showResultsText` (boolean) - Text overlay visible
- `fadeOutAnswers` (boolean) - Fade out answer list
- `slideOutScreen` (boolean) - Desktop slide transition
- `fadeFromBlack` (boolean) - Entry transition
- `fadeToBlack` (boolean) - Exit transition
- `timerScale` (number) - Timer scale value (0 or 1)
- `timeLeft` (number) - Seconds remaining

**Output:**
- Calls `onEliminate(eliminatedAnswer)` when transitioning to voting

### USER INTERACTIONS:

**Desktop:**
- **Answer click**: Select answer (changes to red #fe1d4a)
- **Cannot click**: Own answer (blocked) or fallback answer
- **Cursor states**: not-allowed for blocked, pointer for selectable

**Mobile:**
- **Answer tap**: Select answer (changes to red)
- **Eliminate button**: Submit vote
- **Own answer**: Automatically filtered from list (not shown)

**Both:**
- **Auto-vote**: Timer expires � submits selected answer if any
- **Validation**: Cannot select own answer or NO_ANSWER_FALLBACK

### GAME LOGIC:

**Entry Conditions:**
- Previous screen sent all answers
- Desktop schedules with MOBILE_FOLLOW_DELAY (500ms)
- Creates answer list from allPlayerAnswers, shuffled

**Exit Conditions:**
- All votes submitted � Server calculates results � Triggers elimination-complete event
- Desktop schedules voting phase transition with delays
- Mobile follows scheduled room state update

**Validation:**
- Cannot vote for own answer
- Cannot vote for fallback answer ("I didn't answer in time")
- One vote per player

**Special Rules:**
- **Answer Creation**: Takes allPlayerAnswers (includes player, robot, correct)
  - Filters valid answers (non-null, non-empty)
  - Shuffles randomly
  - Desktop shows all answers
  - Mobile filters out player's own answer
- **Elimination Logic** (server-side):
  - Most voted answer eliminated (tie = no elimination)
  - Sends elimination-complete event with eliminatedAnswer
- **Desktop Synchronization**:
  - HOST_SYNC_DELAY_MS: 500ms
  - MOBILE_FOLLOW_DELAY_MS: 500ms
  - Schedules phase transition with forceMobileTransition
- **Timer**: 30 seconds total
  - Host notifies server when expired
  - Auto-submits selected answer if timer expires

### EXAMPLE DATA:
```javascript
allPlayerAnswers: [
  { text: "Pepperoni", type: "correct", playerName: "System" },
  { text: "Motor Oil", type: "robot", playerName: "Robot" },
  { text: "Pineapple", type: "player", playerName: "Alice" },
  { text: "Mushrooms", type: "player", playerName: "Bob" }
]

// After voting
eliminationResults: {
  eliminatedAnswer: "Motor Oil",
  tieOccurred: false,
  voteCounts: {
    "Motor Oil": 2,
    "Pineapple": 1
  },
  totalVotesCast: 3
}
```

---

## SCREEN: VotingScreen

### ASSETS USED:
- **Background**:
  - Desktop: `eliminatevotebg.png` (from `assets/mainscreenbg/`)
  - Mobile: `namecodeiconmobile.png` (from `assets/mobilebg/`)
- **Fonts**:
  - SINK (main UI) - `assets/fonts/SINK.otf`
  - SIGNAL (small text) - `assets/fonts/SIGNAL.ttf`
- **Images**:
  - `votinghero.png` (from `assets/ui/`)
  - `checkicon.png` (from `assets/ui/`) - shown next to correct answer
  - Player icons
- **Animations**:
  - Lottie timer: `TimerAnim.json` with altcolor variant

### LAYOUT SPECIFICATIONS:

**Desktop View:**

- **Container**: Fixed position, 100vw x 100vh
  - Background: `eliminatevotebg.png` (100vw x 100vh)
  - Position: center center
  - Font family: SINK, sans-serif

- **Hero Image**:
  - Position: Absolute, top -5vh, right 0vw
  - Size: 80vw x 80vh
  - Background: `votinghero.png`
  - Background size: contain
  - Background position: top right
  - Z-index: 2

- **Timer (Lottie)**:
  - Position: Absolute, bottom 0, right 0
  - Size: 14.58vw
  - Variant: altcolor
  - Initial time: 30 seconds
  - Scale animation: 0 � 1 over 0.5s after 1s delay
  - Z-index: 2

- **Answer List Container**:
  - Position: Absolute, top 5%, left 5%
  - Size: 55% x 90%
  - Display: flex, column, centered
  - Justify content: center
  - Align items: flex-start
  - Padding: 1.04vw
  - Z-index: 2

- **Answer Item** (dynamic sizing, same as Elimination):
  - Font: SINK, bold, uppercase
  - Letter spacing: 0.10vw
  - Border: none
  - Background: transparent
  - Color: #030231 (dark blue, always)
  - Border radius: 0.63vw
  - Text align: left
  - Margin: dynamic (same as Elimination)
  - Padding: dynamic (same as Elimination)
  - Font size: dynamic (same as Elimination)
  - Reveal animation: translateY(1.04vw) � translateY(0)
  - Opacity: 0 � 1
  - Transition: 0.5s ease
  - Stagger delay: 500ms + (500ms � index)
  - Display: All answers shown immediately (no cascade)
  - Cursor: pointer (selectable) or not-allowed (own/voted)

- **True Answer Highlight** (after voting complete):
  - Background: linear-gradient(45deg, #0e8f9f, #11ffce) (green gradient)
  - Color: #030231 (dark blue, maintained)
  - Label appended: "(TRUE ANSWER)" in 0.5em size, italic, bold
  - Opacity: 0.9 for label

- **Instruction Text** (bottom left):
  - Text: "Answer this ordinary multiple-choice trivia question"
  - Font: SIGNAL, clamp(16px, 1.46vw, 24px)
  - Color: #fffbbc
  - Letter spacing: 0.05vw
  - Line height: 1.4
  - Position: Absolute, left 3%, bottom 3%
  - Pointer events: none
  - Z-index: 2

**Mobile View:**

- **Container**: Fixed position, 100vw x 90vh
  - Background: `namecodeiconmobile.png` (100vw x 90vh)
  - Font family: SINK, sans-serif

- **Content Wrapper**:
  - Position: Relative, min-height 90vh
  - Padding: 5.33vw
  - Display: flex, column, centered
  - Z-index: 2

- **Header**:
  - Text: "CHOOSE THE RIGHT ANSWER"
  - Font: SINK, 6.40vw
  - Color: #11ffce (green)
  - Margin: 0 0 8.00vw 0

- **Answer List**:
  - Width: 90%
  - Max width: 93.87vw
  - Margin bottom: 5.33vw
  - Filters out player's own answer on mobile

  - **Answer Button** (before voting complete):
    - Padding: 4.00vw
    - Margin: 2.67vw 0
    - Background: Dynamic
      - Own answer: #9a0a30 (blocked, desktop only)
      - Selected: #11ffce (green)
      - After vote: #11ffce (selected) or #7c7a61 (others greyed)
      - Normal: #fffbbc (off-white)
    - Color: #030231 (always dark blue)
    - Border: Dynamic
      - Own answer: 0.53vw solid #9a0a30 (desktop only)
      - Selected: 0.53vw solid #ffd82f (yellow)
      - Normal: 0.53vw solid #fffbbc
    - Border radius: 2.67vw
    - Font: Uses getDynamicTextStyle (base 16px SIGNAL)
    - Cursor: not-allowed (own/voted) or pointer
    - Transition: all 0.2s
    - Opacity: 1 (always)

  - **Answer Button** (after voting complete):
    - Background:
      - True answer: #fe1d4a (red)
      - Others: rgba(248, 249, 250, 0.7)
    - Color:
      - True answer: #fffbbc (white)
      - Others: #666
    - Border:
      - True answer: 0.80vw solid #fe1d4a (red, thicker)
      - Others: 0.53vw solid #ddd
    - Border radius: 2.67vw
    - Opacity: 0.8
    - Cursor: default
    - Transform: scale(1.02) for true answer
    - Box shadow: True answer gets 0 1.07vw 2.13vw rgba(214, 48, 49, 0.3)

    - **Check Icon** (true answer only):
      - Image: `checkicon.png`
      - Margin left: 2.67vw
      - Size: 5.33vw x 5.33vw
      - Vertical align: middle

- **Submit Button**:
  - Width: 71%
  - Max width: 93.87vw
  - Padding: 4.00vw 8.00vw
  - Font: SINK, 6.67vw, bold
  - Letter spacing: 0.53vw
  - Border: none
  - Border radius: 5.34vw
  - Background: Dynamic gradient
    - Voted/Complete: linear-gradient(to right, #11ffce, #0e8f9f)
    - Time up: #ccc
    - Selected: #11ffce
    - None: #ccc
  - Color: #030231 (always dark blue)
  - Cursor: not-allowed (disabled) or pointer
  - Margin bottom: 5.33vw
  - States:
    - Disabled: !selectedAnswer OR hasVoted OR timeLeft === 0 OR votingComplete
  - Text states:
    - Voted/Complete: "VOTE CAST!"
    - Time up: "VOTING CLOSED"
    - Normal: "SUBMIT FINAL ANSWER"

### TIMING/ANIMATIONS:

**Desktop:**
- **Fade from black**: 0-500ms
- **Timer scale in**: 1000ms delay, 500ms duration
- **Answer reveals**: All shown immediately after 500ms delay (no cascade)

**Voting Complete Flow:**
- **Highlight delay**: 500ms after votes submitted
- **True answer highlight**: Appears instantly
- **Desktop hold**: 4000ms after highlight
- **Mobile advance**: 4000ms total
- **Desktop fade to black**: 500ms before transition
- **Host transition**: max(desktop timing, HOST_SYNC_DELAY_MS)
- **Mobile follow**: MOBILE_FOLLOW_DELAY_MS after host

**Sound Effects:**
- **Mobile only**: Plays 'success' SFX if player selected correct answer (when highlight appears)

### STATE VARIABLES:

**Input:**
- `eliminatedAnswer` (string or null) - Previously eliminated answer
- `questionData` (object) - Current question with correctAnswer
- `allPlayerAnswers` (array) - Remaining answers after elimination
- `playerAnswer` (string) - Current player's answer
- `players` (array) - All players

**Internal:**
- `selectedAnswer` (string) - Player's final vote
- `hasVoted` (boolean) - Player has submitted vote
- `votingComplete` (boolean) - All votes received
- `remainingAnswers` (array) - Answers after filtering eliminated
- `visibleAnswers` (array) - Indices of revealed answers
- `showTrueAnswerHighlight` (boolean) - Highlight correct answer
- `fadeToBlack` (boolean) - Exit transition
- `fadeFromBlack` (boolean) - Entry transition
- `timerScale` (number) - Timer animation (0 or 1)
- `timeLeft` (number) - Seconds remaining

**Output:**
- Calls `onSubmit(selectedAnswer, votingResults)` when transitioning

### USER INTERACTIONS:

**Desktop:**
- **Answer click**: Select answer (no visual change in this screen)
- **Cannot click**: Own answer (blocked) or fallback
- **Display host**: Cannot interact (spectator mode)

**Mobile:**
- **Answer tap**: Select answer (changes to green #11ffce)
- **Submit button**: Cast final vote
- **Own answer**: Filtered from list (not shown)
- **After vote**: Shows check icon next to correct answer

**Both:**
- **Auto-vote**: Timer expires � submits selected answer if any
- **Validation**: Cannot vote for own answer or fallback

### GAME LOGIC:

**Entry Conditions:**
- Received eliminatedAnswer from previous screen
- Creates remaining answers by filtering out eliminated answer
- Shuffles remaining answers randomly

**Exit Conditions:**
- All votes submitted � Server sends all-votes-submitted event
- Desktop schedules results phase transition
- Mobile follows scheduled update

**Validation:**
- Cannot vote for own answer
- Cannot vote for fallback answer
- One vote per player

**Special Rules:**
- **Answer Creation**:
  - Takes allPlayerAnswers
  - Filters out eliminatedAnswer (if not null/tie)
  - Shuffles randomly
  - Mobile filters out player's own answer
- **Voting Results** (server-side):
  - Tracks which players voted for correct answer
  - Calculates scoring based on votes
  - Sends all-votes-submitted with results
- **Desktop Synchronization**:
  - HOST_SYNC_DELAY_MS: 500ms
  - MOBILE_FOLLOW_DELAY_MS: 500ms
  - Schedules results transition
- **Mobile SFX**: Only plays if player voted correctly
- **Timer**: 30 seconds
  - Host notifies server when expired
  - Auto-submits selected if any

### EXAMPLE DATA:
```javascript
remainingAnswers: [
  { text: "Pepperoni", type: "correct", playerName: "System" },
  { text: "Pineapple", type: "player", playerName: "Alice" },
  { text: "Mushrooms", type: "player", playerName: "Bob" }
  // Motor Oil was eliminated
]

questionData: {
  correctAnswer: "Pepperoni",
  // ... other fields
}

votingResults: {
  voteCounts: {
    "Pepperoni": 2,
    "Pineapple": 1
  },
  correctAnswer: "Pepperoni"
}
```

---

## SCREEN: ResultsScreen

### ASSETS USED:
- **Background**:
  - Desktop: `resultsbg.png` (from `assets/mainscreenbg/`)
  - Mobile: `resultsmobile.png` (from `assets/mobilebg/`)
- **Fonts**:
  - SINK (main UI) - `assets/fonts/SINK.otf`
  - SIGNAL (small text) - `assets/fonts/SIGNAL.ttf`
  - Havoks (eliminated answer) - `assets/fonts/HAVOKS.otf`
- **Images**:
  - Round-specific results banners: `round1results.png` through `round12results.png` (from `assets/ui/`)
  - `nextroundbutton.png` or `resultsbutton.png` (from `assets/ui/`)
  - `buttonslideout.png` (from `assets/ui/`)
  - Player icons
- **Animations**: None (CSS animations only)

### LAYOUT SPECIFICATIONS:

**Desktop View:**

- **Container**: Fixed position, 100vw x 100vh
  - Background: `resultsbg.png` (100% x 100%)
  - Position: center
  - Font family: SINK, sans-serif
  - Overflow: hidden

- **Round Results Banner**:
  - Position: Absolute, top 2.78vh, left -0.44vw (2% left nudge)
  - Size: 25.00vw x 33.33vh
  - Background: Round-specific PNG (e.g., `round1results.png`)
  - Background size: contain
  - Background position: center
  - Transform: rotate(-8deg)
  - Z-index: 5
  - Opacity: 1

- **Panel System** (4 sequential panels):
  All panels centered at top 50%, left 50%, transform translate(-50%, -50%) scale(1.1)

  **Panel 1 - Right Answer** (0.7s - 5.7s):
  - Border: 0.42vw solid #11ffce (green)
  - Border radius: 1.56vw
  - Padding: 4.17vh 5.21vw
  - Backdrop filter: blur(18px)
  - Background: rgba(3, 2, 49, 0) (transparent)
  - Box shadow: 0 0 2.08vw rgba(17, 255, 206, 0.35)
  - Opacity: 0 � 1 (fade in 0.5s)
  - Z-index: Based on panel visibility

  - **Headline Panel**:
    - Background: #26174f
    - Padding: 1.2vh 2vw
    - Border radius: 0.52vw
    - Margin bottom: 0.5vh
    - Text: "TRUE ANSWER"
    - Font: SINK, 2.25vw, bold
    - Color: #ffd82f
    - Text align: center
    - Fade in: 0 � 1 over 0.5s

  - **Answer Text**:
    - Font: SINK, 4.5vw (50% larger), bold
    - Color: #030231 (dark blue)
    - Text align: center
    - Letter spacing: 0.2vw
    - Line height: 0.9
    - Fade in: 0 � 1 over 0.5s

  - **Player Icons** (who got it right):
    - Display: flex, gap 1.04vw, wrap, centered
    - Max width: 60vw
    - Min height: 80px
    - Icon size: 80px
    - Cascade animation: scale(0) � scale(1.15) � scale(0.95) � scale(1)
    - Duration: 0.5s cubic-bezier(0.34, 1.56, 0.64, 1)
    - Stagger: 300ms per icon
    - Fade in: 0.5s after answer text
    - Fallback text: "[No Testers!]" (2vw, bold, #ffd82f)

  - **Score Circle**:
    - Size: 12vw diameter
    - Background: #ffd82f (yellow circle)
    - Border radius: 50%
    - Display: flex, centered
    - Fade in: 1s after icons
    - Score text: 6vw, bold, #26174f
    - Format: "+8%" or "+6%" (depending on game mode)

  **Panel 2 - Robot Decoy** (5.7s - 10.7s):
  - Border: 0.42vw solid #fe1d4a (red)
  - Border radius: 1.56vw
  - Padding: 4.69vh 5.73vw
  - Backdrop filter: blur(18px)
  - Background: rgba(3, 2, 49, 0)
  - Box shadow: 0 0 2.08vw rgba(254, 29, 74, 0.35)
  - Opacity: 0 � 1 (fade in 0.5s)

  - **Headline Panel**:
    - Same style as Panel 1
    - Text: "ROBOT DECOY ANSWER"
    - Color: #ffd82f

  - **Decoy Answer Text**:
    - Font: SINK, 4.5vw, bold
    - Color: #fe1d4a (red)
    - Letter spacing: 0.2vw
    - Line height: 0.9
    - Slide in: translateY(1.85vh) � translateY(0)
    - Fade: 0 � 1 over 0.5s

  - **Not Fooled Section**:
    - Display: flex, aligned left
    - Gap: 1.56vw
    - Padding left: 10vw
    - Label: "NOT FOOLED (+X%):" (2vw, #fffbbc, bold)
    - Icons: 70px, cascade animation (300ms stagger)
    - Fallback: "[No Testers!]" (1.8vw, bold, #ffd82f)
    - Slide in: translateY(1.85vh) � 0, 0.5s after decoy text
    - Percentage: Dynamic from getNotFooledPercentage() (half points)

  - **Fooled Section**:
    - Same layout as Not Fooled
    - Label: "FOOLED (-X%):" (2vw, #fffbbc, bold)
    - Icons: 70px, cascade animation
    - Slide in: 0.5s after Not Fooled
    - Penalty: Dynamic from getFooledPenaltyPercentage() (8% or 6%)

  **Panel 3 - Player Answers** (10.7s - 15.7s):
  - Position: Centered (50%, 50%, translate -50%, -50%)
  - Width: 70vw
  - Display: flex, column, centered
  - Gap: 1.04vw
  - Opacity: 0 � 1

  - **Player Answer Row**:
    - Display: flex, space-between
    - Width: 80% (20% shorter)
    - Padding: 0.78vw 1.30vw
    - Background: rgba(0, 0, 0, 0.5)
    - Border: 0.10vw solid #ffd82f
    - Border radius: 0.52vw
    - Slide in: translateY(1.85vh) � 0
    - Fade: 0 � 1
    - Duration: 0.5s ease-out

    - Left side:
      - Player icon: 60px
      - Answer text: SIGNAL, 1.26vw (30% smaller)
      - Color: #fffbbc (or #26174f if matches correct answer)

    - Right side:
      - Vote count: 1vw, bold, #ffd82f
      - Format: "X vote(s)"
      - Voter icons: 40px, cascade (300ms stagger)

  - **Footer Panel**:
    - Background: #26174f
    - Padding: 1.2vh 2vw
    - Border radius: 0.52vw
    - Font: SINK, 1.5vw
    - Color: #fffbbc
    - Text: "Each vote you receive is worth +X%" (4% or 3%)
    - Margin top: 2.78vh
    - Slide in: 500ms after last answer

  **Panel 4 - Standings** (15.7s - 20.7s+):
  - Position: top 40%, left 50%
  - Transform: translate(-50%, -50%)
  - Width: 60vw
  - Display: flex, column, centered
  - Gap: 2.22vh
  - Opacity: 0 � 1

  - **Header Panel**:
    - Background: #26174f
    - Padding: 1.2vh 2.6vw
    - Border radius: 0.52vw
    - Font: SINK, 2.1vw, bold
    - Color: #ffd82f
    - Letter spacing: 0.1vw
    - Text: "ROUND STANDINGS"
    - Fade in: 0 � 1 over 0.5s

  - **Standing Row**:
    - Display: flex, space-between
    - Padding: 0.78vw 1.56vw
    - Background: rgba(0, 0, 0, 0.55)
    - Border: 0.10vw solid #11ffce
    - Border radius: 0.52vw
    - Slide in: translateY(1.85vh) � 0, 200ms stagger
    - Fade: 0 � 1
    - Transition: 0.4s ease-out

    - Left side:
      - Placement: SINK, 1.5vw, bold, #11ffce, min-width 4vw
      - Format: "1st", "2nd", "3rd", "4th", etc.
      - Icon: 55px
      - Name: SINK, 1.4vw, bold, #fffbbc

    - Right side:
      - Label: "SCORE" (0.83vw, #fffbbc, opacity 0.65)
      - Score: SINK, 1.4vw, bold
      - Color: #11ffce (positive) or #fe1d4a (negative)
      - Format: "+X%" or "-X%"

    - **First place highlight** (after 2s):
      - Box shadow: 0 0 0 0.26vw #ffd82f
      - Transition: 0.3s ease-in-out

    - **Last place highlight** (after 4s):
      - Box shadow: 0 0 0 0.26vw #fe1d4a
      - Can stack with first (both glow if same player)

- **Next Round Button** (after all panels):
  - Position: Fixed, bottom 2.78vh
  - Right: Slides from -35vw to 2.60vw
  - Transition: 0.8s ease-in-out
  - Z-index: 1000

  - **Slideout Background**:
    - Position: Absolute, left 0, top 50%, translateY(-50%)
    - Size: 31.25vw x 20.83vh
    - Background: `buttonslideout.png`
    - Background size: contain, no-repeat
    - Background position: center left
    - Z-index: 1

  - **Button**:
    - Background: `nextroundbutton.png` or `resultsbutton.png`
    - Size: 12.50vw x 8.89vh
    - Background size: contain, no-repeat, centered
    - Border: none
    - Background color: transparent
    - Cursor: pointer
    - Margin left: 4.17vw
    - Filter: drop-shadow(0 0.37vh 0.74vh rgba(0, 0, 0, 0.3))
    - Hover: scale(1.05)
    - Z-index: 2

**Mobile View:**

- **Container**: Fixed position, 100vw x 90vh
  - Background: `resultsmobile.png` (100vw x 90vh)
  - Font family: SINK, sans-serif
  - Display: flex, column, centered

- **Animation Steps** (staggered reveals):
  - Step 0: Everything hidden
  - Step 1 (700ms): Player name and placement fade in
  - Step 2 (1200ms): Total score fades in
  - Step 3 (1700ms): Humans fooled count fades in

- **Player Name**:
  - Font: SINK, 12vw, bold
  - Color: #fffbbc
  - Text transform: uppercase
  - Margin bottom: 1.33vw
  - Text align: center
  - Slide in: translateY(1.85vh) � 0
  - Fade: 0 � 1 over 0.5s

- **Placement Text**:
  - Font: SINK, 7.2vw, bold
  - Color: #11ffce
  - Text align: center
  - Margin top: -2vh
  - Slide in: translateY(1.85vh) � 0
  - Fade: 0 � 1 over 0.5s (0.1s delay)
  - Format: "1st place of X players", "2nd place of X players", etc.

- **Total Score**:
  - Font: SINK, 16.8vw, bold
  - Color: #11ffce (positive) or #fe1d4a (negative)
  - Text align: center
  - Text shadow: 0.8vw 0.8vw 1.6vw rgba(0,0,0,0.8)
  - Margin bottom: 0
  - Slide in: translateY(1.85vh) � 0
  - Fade: 0 � 1 over 0.5s (0.1s delay)
  - Format: "+X%" or "-X%"

- **Status Statement**:
  - Font: SINK, 4.8vw
  - Color: #fffbbc
  - Text align: center
  - Margin bottom: 5.33vw
  - Padding: 0 8vw
  - Messages:
    - 1st place: "[You were the MOST HUMAN]"
    - Last place: "[You were the MOST ROBOT-LIKE]"
    - 2nd place: "[You were the 2nd MOST HUMAN]"
    - 3rd place: "[You were the 3rd MOST HUMAN]"
    - Other: "[You placed #X out of Y]"

- **Humans Fooled Section**:
  - Display: flex, column, centered
  - Gap: 0.93vh
  - Slide in: translateY(1.85vh) � 0
  - Fade: 0 � 1 over 0.5s (0.2s delay)

  - Label: "Total Humans Fooled:"
    - Font: SINK, 7.2vw
    - Color: #fffbbc
    - Text align: center

  - Count:
    - Font: SINK, 14.4vw, bold
    - Color: #ffd82f
    - Format: Number (not percentage)
    - Source: votesReceived from server OR calculated locally

### TIMING/ANIMATIONS:

**Desktop Panel Sequence:**
- Initial delay: 700ms
- Each panel duration: 5000ms (5 seconds)
- Fade duration: 500ms (between panels)

**Panel 1 Timeline:**
- 0ms: Panel appears, headline fades in
- 500ms: Player icons fade in (cascade)
- 1000ms: Score circle fades in
- 5000ms: Panel fades out (500ms)

**Panel 2 Timeline:**
- 5000ms: Panel appears, headline and decoy fade in
- 500ms: Not Fooled section slides in
- 1000ms: Fooled section slides in
- 5000ms: Panel fades out (500ms)

**Panel 3 Timeline:**
- 10000ms: Panel appears
- 200ms + (300ms � index): Each answer row slides in
- After all answers + 500ms: Footer text slides in
- 5000ms: Panel fades out (500ms)

**Panel 4 Timeline:**
- 15000ms: Panel appears, header fades in
- 200ms + (200ms � index): Each standing row slides in
- +2000ms: First place highlight activates
- +4000ms: Last place highlight activates
- +500ms: Animations complete, button slides in (800ms slide)

**Mobile Timeline:**
- 500ms: Fade from black complete
- 700ms: Player name and placement (step 1)
- 1200ms: Total score (step 2)
- 1700ms: Humans fooled (step 3)
- 100ms + (totalBars � 200ms) + 500ms: Animations complete

**Keyboard Controls (Desktop):**
- Enter or Space: Triggers next round (when button visible)
- Plays 'button_press' SFX

### STATE VARIABLES:

**Input:**
- `round` (number) - Current round (1-12)
- `maxRounds` (number) - Total rounds (8 or 12)
- `playerAnswer` (string) - Player's submitted answer
- `eliminationChoice` (string) - Player's elimination vote
- `finalChoice` (string) - Player's final vote
- `questionData` (object) - Current question data
- `allPlayerScores` (array) - All player scores with icons
- `roundResultsData` (object) - Complete results from server

**Internal:**
- `roundResults` (object) - Stored results data
- `allPlayersRoundScores` (object) - Player scores keyed by name
- `fadeToBlack` (boolean) - Exit transition
- `fadeFromBlack` (boolean) - Entry transition (desktop)
- `currentPanel` (number) - 0=none, 1-4=panel number
- `panelAnimations` (object) - Flags for each animation element
- `buttonSlideIn` (boolean) - Button animation trigger
- `animationsComplete` (boolean) - All panels done
- `highlightFirstStanding` (boolean) - Highlight 1st place
- `highlightLastStanding` (boolean) - Highlight last place
- `mobileAnimationStep` (number) - 0-3 for mobile reveals

**Output:**
- Calls `onNextRound(roundPoints)` when button clicked

### USER INTERACTIONS:

**Desktop:**
- **Next Round Button**: Click to advance
- **Keyboard**: Enter or Space to advance
- **Sound**: 'button_press' SFX on click

**Mobile:**
- No user interaction (auto-advances after host)
- Special: Round 6 listens for bonus-transition-ready event

**Both:**
- **Fade to Black**: 500ms (desktop) or 600ms (mobile) before transition

### GAME LOGIC:

**Entry Conditions:**
- Receives roundResultsData from server after voting complete
- Contains: playerScores, placements, votingResults, eliminationResults, playerAnswers

**Exit Conditions:**
- Button click (desktop) or auto-advance (mobile)
- Fade to black transition
- Calls onNextRound with player's round points

**Special Rules - Round 6 (Bonus Round):**
- Desktop host: Calls notifyBonusTransitionReady with 1200ms followDelayMs
- Mobile: Listens for bonus-transition-ready event
- Mobile delays transition by max(followDelayMs, 600ms)

**Scoring Calculations:**
- **Correct Answer**: fullPoints (8% for 8-game, 6% for 12-game)
- **Robot Identified**: halfPoints (4% for 8-game, 3% for 12-game)
- **Fooled Penalty**: -8% (8-game) or -6% (12-game)
- **Votes Received**: +halfPoints per vote (4% or 3%)

**Panel 1 Data:**
- playersWhoVotedCorrect: Filters votingResults for correctAnswer
- Shows icons + scoreChange (correctAnswer points)

**Panel 2 Data:**
- playersNotFooled: Filters eliminationResults for robotAnswer
- playersFooled: Filters votingResults for robotAnswer
- Shows icons with no score (just participation)

**Panel 3 Data:**
- getPlayerAnswersWithVotes: Filters player answers (excludes correct + robot)
- Shows answer text, vote count, voter icons

**Panel 4 Data:**
- Uses placements from server OR sorts allPlayerScores locally
- Shows placement, icon, name, score
- Highlights first (gold glow) and last (red glow)

### EXAMPLE DATA:
```javascript
roundResultsData: {
  playerScores: {
    "Alice": {
      total: 12,
      correctAnswer: 8,
      robotIdentified: 4,
      votesReceived: 2,
      fooled: 0,
      icon: "icon1"
    },
    "Bob": {
      total: -4,
      correctAnswer: 0,
      robotIdentified: 0,
      votesReceived: 0,
      fooled: -8,
      icon: "icon2"
    }
  },
  placements: [
    { name: "Alice", placement: 1 },
    { name: "Bob", placement: 2 }
  ],
  votingResults: {
    "Alice": "Pepperoni",
    "Bob": "Motor Oil"
  },
  eliminationResults: {
    "Alice": "Motor Oil",
    "Bob": "Pineapple"
  },
  playerAnswers: {
    "Alice": "Pineapple",
    "Bob": "Mushrooms"
  },
  currentQuestion: {
    question: "Best pizza topping?",
    correctAnswer: "Pepperoni",
    robotAnswer: "Motor Oil"
  }
}
```

---

## SCREEN: FinalResults

### ASSETS USED:
- **Background**:
  - Desktop: `finalresultsbg.png` (from `assets/mainscreenbg/`)
  - Mobile: `finalresultsmobile.png` (from `assets/mobilebg/`)
- **Fonts**:
  - SINK (main UI) - `assets/fonts/SINK.otf`
  - SIGNAL (small text) - `assets/fonts/SIGNAL.ttf`
- **Images**:
  - `endresultshero.png` (from `assets/ui/`)
  - `newgamebutton.png` (from `assets/ui/`)
  - `creditsbutton.png` (from `assets/ui/`)
  - `gamesbutton.png` (from `assets/ui/`)
  - `shareresultsbutton.png` (from `assets/ui/`)
  - `buttonslideoutlong.png` (from `assets/ui/`)
  - Player icons
- **Components**:
  - ShareResultsPopup (modal overlay)
  - AnimatedScore (count-up animation)

### LAYOUT SPECIFICATIONS:

**Desktop View:**

- **Container**: Fixed position, 100vw x 100vh
  - Background: `finalresultsbg.png` (100% x 100%)
  - Position: center
  - Font family: SINK, sans-serif
  - Display: flex, centered
  - Overflow: hidden

- **Hero Image**:
  - Position: Absolute, top 38% (10% higher), left 0.08vw (2% left nudge)
  - Transform: translateY(-50%)
  - Size: 80% x 60% (2� original size)
  - Background: `endresultshero.png`
  - Background size: contain
  - Background position: center left
  - Z-index: 1
  - Animation: fadeIn 0.5s cubic-bezier(0.4, 0, 0.2, 1)

- **Results Container**:
  - Position: Absolute, top 7%, left 28%
  - Size: 44% x 86%
  - Display: flex, column, centered
  - Z-index: 2

  - **Spotlight Cards Container**:
    - Width: 100%
    - Height: 100%
    - Display: flex, column, centered
    - Gap: 1.56vw
    - Padding: 1.56vw 2.08vw

  - **Spotlight Card** (Winner and Last Place):
    - Width: 100%
    - Max width: 52.08vw
    - Padding: 1.56vw 2.08vw
    - Background: rgba(5, 24, 49, 0.65)
    - Backdrop filter: blur(0.52vw)
    - Border radius: 1.04vw
    - Display: flex, space-between
    - Slide in: translateY(-1.56vw) � 0
    - Opacity: 0 � 1
    - Duration: 0.6s ease-out
    - Stagger: 100ms per card

    **Winner Card:**
    - Border: 0.21vw solid #11ffce
    - Box shadow: 0 0.73vw 1.46vw rgba(17, 255, 206, 0.35)
    - Transform: scale(1.04) (4% larger)

    **Last Place Card:**
    - Border: 0.21vw solid #fe1d4a
    - Box shadow: 0 0.73vw 1.46vw rgba(254, 29, 74, 0.35)
    - Transform: scale(1)

    - Left section:
      - **Icon Wrapper**:
        - Size: 5.21vw (winner) or 4.69vw (last)
        - Border radius: 50%
        - Border: 0.21vw solid (matching card color)
        - Box shadow: matches card
        - Background: rgba(255, 251, 188, 0.12)
        - Icon size: 60px (winner) or 52px (last)

      - **Badge Label**:
        - Font: SINK, 0.83vw, bold
        - Color: #fffbbc
        - Opacity: 0.7
        - Letter spacing: 0.32vw
        - Text: "WINNER" or "LAST PLACE"

      - **Player Name**:
        - Font: SINK, 2.29vw (winner) or 1.98vw (last), bold
        - Color: #11ffce (winner) or #fe1d4a (last)
        - Text transform: uppercase
        - Text shadow: 0 0.21vw 0.52vw rgba(0, 0, 0, 0.45)

      - **Subtitle**:
        - Font: SINK, 1.15vw (winner) or 1.04vw (last), bold
        - Color: #fffbbc
        - Opacity: 0.9
        - Text: "MOST HUMAN" or "ROBOT IDENTIFIED"

    - Right section:
      - **Rank Label**:
        - Font: SINK, 0.94vw
        - Color: #fffbbc
        - Opacity: 0.75
        - Letter spacing: 0.26vw
        - Text: "#1" or "#X"

      - **Score Label**:
        - Font: SINK, 0.83vw
        - Color: #fffbbc
        - Opacity: 0.65
        - Letter spacing: 0.26vw
        - Text: "SCORE"

      - **Score Value**:
        - Font: SINK, 3.13vw (winner) or 2.81vw (last), bold
        - Color: matches card (#11ffce or #fe1d4a)
        - Text shadow: 0 0.21vw 0.52vw rgba(0, 0, 0, 0.45)
        - Format: "X%" (uses AnimatedScore component)
        - Animation: Count up from 0

- **Action Buttons** (after animations):
  - Position: Fixed, bottom 1.56vw
  - Right: Slides from -36.46vw to 2.60vw
  - Transition: 0.8s ease-in-out
  - Z-index: 1000

  - **Slideout Background**:
    - Position: Absolute, left 0, top 50%, translateY(-50%)
    - Size: 35.94vw x 5.39vw (15% larger)
    - Background: `buttonslideoutlong.png`
    - Background size: contain, no-repeat
    - Background position: center left
    - Z-index: 1

  - **Credits Button** (left):
    - Background: `creditsbutton.png`
    - Size: 5.99vw x 2.40vw (15% larger)
    - Margin left: 1.98vw
    - Filter: drop-shadow(0 0.21vw 0.42vw rgba(0, 0, 0, 0.3))
    - Hover: scale(1.05)
    - Z-index: 2

  - **New Test Button** (right):
    - Background: `newgamebutton.png`
    - Size: 5.99vw x 2.40vw (15% larger)
    - Margin left: 1.51vw
    - Filter: drop-shadow(0 0.21vw 0.42vw rgba(0, 0, 0, 0.3))
    - Hover: scale(1.05)
    - Z-index: 2

**Mobile View:**

- **Container**: Fixed position, 100vw x 90vh
  - Background: `finalresultsmobile.png` (100vw x 90vh, force fit)
  - Font family: SINK, sans-serif
  - Display: flex, column, centered
  - Overflow: hidden
  - Padding: 0 (removed to ensure full coverage)

- **Player Info Section**:
  - Position: Centered
  - Margin top: 25.33vw (moved up 4vw total)
  - Padding: 0 5.33vw (horizontal only)
  - Display: flex, column, centered

  - **Player Icon**:
    - Size: 19.2vw x 19.2vw (10% smaller)
    - Border radius: 50%
    - Background: rgba(255, 251, 188, 0.95)
    - Padding: 2.4vw (10% smaller)
    - Margin bottom: 2.67vw
    - Border: 0.8vw solid #ffd82f
    - Icon size: 24vw

  - **Player Name**:
    - Font: SINK, 8.53vw, bold
    - Color: #fffbbc
    - Text transform: uppercase
    - Margin bottom: 1.33vw
    - Text align: center

  - **Placement**:
    - Font: SINK, 7.2vw, bold
    - Color: #11ffce
    - Text align: center
    - Margin top: -2vh
    - Format: "1st place of X players", etc.

  - **Total Score**:
    - Font: SINK, 23.04vw (10% smaller), bold
    - Color: #ffd82f
    - Text shadow: 0.8vw 0.8vw 1.6vw rgba(0,0,0,0.8)
    - Margin bottom: 0
    - Text align: center
    - Format: "+X%" or "-X%" (uses pointsToPercentage utility)

  - **Status Statement**:
    - Font: SINK, 4.8vw
    - Color: #fffbbc
    - Text align: center
    - Margin bottom: 5.33vw
    - Padding: 0 8vw
    - Messages:
      - 1st: "[You were the MOST HUMAN]"
      - Last: "[You were the MOST ROBOT-LIKE]"
      - 2nd: "[You were the 2nd MOST HUMAN]"
      - 3rd: "[You were the 3rd MOST HUMAN]"
      - Other: "[You placed #X out of Y]"

- **Action Buttons**:
  - Position: Absolute, bottom 35.2vw (lowered 8vw), left 50%
  - Transform: translateX(-50%)
  - Width: 70%
  - Max width: 74.67vw (increased)
  - Display: flex, column (stacked vertically)
  - Gap: 2.67vw (20px closer)
  - Align: centered

  - **Share Results Button** (top):
    - Background: `shareresultsbutton.png`
    - Width: 125% (25% bigger)
    - Height: 20vw
    - Background size: contain, no-repeat, centered
    - Border: none
    - Cursor: pointer
    - Filter: drop-shadow(0 1.07vw 2.13vw rgba(0,0,0,0.3))
    - Touch feedback: scale(1.05) on press

  - **Games Button** (bottom):
    - Background: `gamesbutton.png`
    - Width: 125% (25% bigger)
    - Height: 20vw
    - Background size: contain, no-repeat, centered
    - Border: none
    - Cursor: pointer (currently disabled until launch)
    - Filter: drop-shadow(0 1.07vw 2.13vw rgba(0,0,0,0.3))
    - Touch feedback: scale(1.05) on press

- **ShareResultsPopup** (modal):
  - Triggered by Share Results button
  - Overlays entire screen
  - Props: playerName, playerIcon, totalScore, rank
  - Separate component (not detailed here)

### TIMING/ANIMATIONS:

**Desktop:**
- **Fade from black**: 0-500ms
- **Hero fade in**: Immediate, 500ms duration
- **Bar animations**: Start 800ms after mount
  - First bar: Immediate
  - Second bar: +200ms
  - Duration: 600ms each
- **Animations complete**: (totalBars � 200ms) + 500ms
- **Button slide in**: 100ms after complete, 800ms duration

**Mobile:**
- **Fade from black**: 0-500ms
- **Bar animations**: Start 600ms after fade complete
  - Each bar: +200ms stagger
  - Duration: 600ms per bar
- **Animations complete**: 100ms + (totalBars � 200ms) + 500ms
- **Red blinking effect** (last place): Starts 1s after mount, blinks every 500ms

**Keyboard Controls (Desktop):**
- Enter or Space: Triggers onPlayAgain when button visible
- Plays 'button_press' SFX

### STATE VARIABLES:

**Input:**
- `totalScore` (number) - Final cumulative score
- `allPlayerScores` (array) - All players with scores and icons
- `currentPlayerName` (string) - Name of current player

**Internal:**
- `visibleBars` (array) - Indices of bars to show
- `animationsComplete` (boolean) - All animations done
- `buttonSlideIn` (boolean) - Button animation trigger
- `redBlinking` (boolean) - Mobile last place blink state
- `showSharePopup` (boolean) - Share popup visible
- `fadeFromBlack` (boolean) - Entry transition

**Output:**
- Calls `onPlayAgain()` when New Test clicked
- Calls `onCredits()` when Credits clicked

### USER INTERACTIONS:

**Desktop:**
- **New Test Button**: Click to restart game
- **Credits Button**: Click to view credits
- **Keyboard**: Enter or Space triggers New Test

**Mobile:**
- **Share Results Button**: Opens share popup (not implemented)
- **Games Button**: Opens games site (currently disabled)

**Both:**
- No exit transition (stays on final results)

### GAME LOGIC:

**Entry Conditions:**
- Reached after final round's ResultsScreen
- Receives final cumulative scores for all players

**Exit Conditions:**
- Desktop: Button click returns to game start
- Mobile: Stays on screen (no navigation implemented)

**Validation:**
- Checks if allPlayerScores is empty � Shows warning message

**Special Rules:**
- **Spotlight Players**:
  - If 1 player: Shows winner only
  - If 2+ players: Shows winner (first) and last place
- **Sorting**: Highest score = winner, lowest = robot identified
- **Score Display**: Uses pointsToPercentage utility to format
- **Mobile Status**:
  - Calculates player's rank from sortedPlayers
  - Shows appropriate status message based on placement

**Scoring Summary:**
- Total score is cumulative from all rounds
- Displayed as percentage (not raw points)
- Can be negative

### EXAMPLE DATA:
```javascript
allPlayerScores: [
  {
    name: "Alice",
    score: 45,
    icon: "icon1"
  },
  {
    name: "Bob",
    score: -12,
    icon: "icon2"
  },
  {
    name: "Charlie",
    score: 28,
    icon: "icon3"
  }
]

// Sorted (winner to last):
// 1. Alice: 45% (WINNER - MOST HUMAN)
// 2. Charlie: 28%
// 3. Bob: -12% (LAST PLACE - ROBOT IDENTIFIED)

// Mobile display for Bob:
currentPlayerName: "Bob"
totalScore: -12
rank: 3
statusMessage: "[You were the MOST ROBOT-LIKE]"
```

---

## COMMON UTILITIES & CONSTANTS

### Constants:
```javascript
// NO_ANSWER_FALLBACK - Used when player doesn't submit answer in time
export const NO_ANSWER_FALLBACK = 'No answer provided';
```

**Usage**: When timer expires and player hasn't submitted, use this string as their answer.

---

### Scoring Functions:
```javascript
/**
 * Get the base points for a regular round
 * @param {string} gameMode - '8' or '12'
 * @returns {number} Base points value
 */
export const getBasePoints = (gameMode) => {
  return gameMode === '12' ? 6 : 8;
};

/**
 * Get the half points (for votes received and robot identification)
 * @param {string} gameMode - '8' or '12'
 * @returns {number} Half points value
 */
export const getHalfPoints = (gameMode) => {
  return gameMode === '12' ? 3 : 4;
};

/**
 * Get the points for a picture/double round
 * @param {string} gameMode - '8' or '12'
 * @returns {number} Double points value
 */
export const getDoublePoints = (gameMode) => {
  return gameMode === '12' ? 12 : 16;
};

/**
 * Get the points for a bonus calibration question
 * @param {string} gameMode - '8' or '12'
 * @returns {number} Bonus question points value
 */
export const getBonusQuestionPoints = (gameMode) => {
  return gameMode === '12' ? 3 : 4;
};

/**
 * Convert points to percentage string
 * @param {number} points - Total points
 * @returns {string} Percentage string with % sign
 */
export const pointsToPercentage = (points) => {
  return `${Math.round(points)}%`;
};

/**
 * Format score display
 * @param {number} points - Total points
 * @returns {string} Formatted score string
 */
export const formatScore = (points) => {
  return `${Math.round(points)} (${pointsToPercentage(points)})`;
};
```

**Scoring Rules Summary**:
- **8-round game**:
  - Correct answer: +8%
  - Robot identified: +4%
  - Each vote received: +4%
  - Fooled by robot: -8%

- **12-round game**:
  - Correct answer: +6%
  - Robot identified: +3%
  - Each vote received: +3%
  - Fooled by robot: -6%

---

### Dynamic Font Sizing:

**Purpose**: Automatically scales text to fit within specified width constraints

**Algorithm**:
1. Measure text width at base font size
2. Account for 30px padding buffer (safety margin for borders/padding)
3. If text fits within effective max width, use base font size
4. Otherwise, calculate scale factor: `effectiveMaxWidth / textWidth`
5. Apply scale factor to font size (round down to nearest integer)
6. Ensure result is not below minimum font size (10px default)

**Parameters**:
- Text to measure
- Maximum width in pixels
- Base font size in pixels
- Font family (default: SIGNAL, sans-serif)
- Minimum font size (default: 10px)

**Returns**: Optimal font size in pixels with smooth resize transition (0.3s ease)

---

### String Comparison Algorithm (for answer validation):

**Levenshtein Distance** (edit distance between strings):
```javascript
function levenshteinDistance(str1, str2) {
  const len1 = str1.length;
  const len2 = str2.length;
  const matrix = Array(len2 + 1).fill(null).map(() => Array(len1 + 1).fill(0));

  // Initialize first row and column
  for (let i = 0; i <= len1; i++) matrix[0][i] = i;
  for (let j = 0; j <= len2; j++) matrix[j][0] = j;

  // Fill matrix
  for (let j = 1; j <= len2; j++) {
    for (let i = 1; i <= len1; i++) {
      const cost = str1[i - 1] === str2[j - 1] ? 0 : 1;
      matrix[j][i] = Math.min(
        matrix[j][i - 1] + 1,     // Deletion
        matrix[j - 1][i] + 1,     // Insertion
        matrix[j - 1][i - 1] + cost  // Substitution
      );
    }
  }

  return matrix[len2][len1];
}
```

**Similarity Percentage**:
```javascript
function calculateSimilarity(str1, str2) {
  const maxLen = Math.max(str1.length, str2.length);
  if (maxLen === 0) return 1.0; // Both empty strings

  const distance = levenshteinDistance(str1, str2);
  return 1.0 - (distance / maxLen); // Returns 0.0 to 1.0
}
```

**Text Normalization** (before comparison):
```javascript
function normalizeText(text) {
  return text
    .toLowerCase()                          // Lowercase
    .replace(/[^\w\s]/g, '')               // Remove punctuation
    .replace(/\s+/g, ' ')                  // Normalize whitespace
    .trim();                               // Remove leading/trailing spaces
}
```

**Validation Logic** (QuestionScreen):
```javascript
// Check if answer matches correct answer
const normalizedAnswer = normalizeText(playerAnswer);
const normalizedCorrect = normalizeText(correctAnswer);
const distance = levenshteinDistance(normalizedAnswer, normalizedCorrect);
const similarity = calculateSimilarity(normalizedAnswer, normalizedCorrect);

// Triggers decoy warning if:
// - Exact match OR
// - Within 1 letter (distance ≤ 1) OR
// - 85%+ similarity
if (normalizedAnswer === normalizedCorrect ||
    distance <= 1 ||
    similarity >= 0.85) {
  showDecoyWarning();
  return;
}

// Check for duplicates (against all submitted answers)
for (const submittedAnswer of allAnswers) {
  const normalizedSubmitted = normalizeText(submittedAnswer);
  const dupDistance = levenshteinDistance(normalizedAnswer, normalizedSubmitted);
  const dupSimilarity = calculateSimilarity(normalizedAnswer, normalizedSubmitted);

  // Triggers duplicate warning if:
  // - Within 1 letter (distance ≤ 1) OR
  // - 90%+ similarity
  if (dupDistance <= 1 || dupSimilarity >= 0.90) {
    showDuplicateWarning();
    return;
  }
}
```

**Thresholds Summary**:
- **Correct answer detection**: Distance ≤ 1 OR Similarity ≥ 85%
- **Duplicate detection**: Distance ≤ 1 OR Similarity ≥ 90%
- **"Within one letter"** = Levenshtein distance ≤ 1

---

### Profanity Filter:

**Normalization for Profanity Check** (more aggressive than answer comparison):
```javascript
function normalizeProfanityCheck(text) {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9]/g, '')  // Remove ALL non-alphanumeric
    .replace(/\s+/g, '');       // Remove spaces
}
```

**Banned Words List** (commonly blocked terms):
```javascript
const bannedWords = [
  'fuck', 'shit', 'ass', 'bitch', 'damn', 'hell',
  'bastard', 'crap', 'piss', 'dick', 'cock', 'pussy',
  'slut', 'whore', 'fag', 'retard', 'nigger', 'nigga',
  // Add comprehensive list for production
];
```

**Validation**:
```javascript
function containsProfanity(text) {
  const normalized = normalizeProfanityCheck(text);

  for (const word of bannedWords) {
    // Substring match (catches variations)
    if (normalized.includes(word)) {
      return true;
    }
  }

  return false;
}
```

**NOTE**: The actual banned words list must be created for your implementation or use a profanity filter library.

---

### Mobile Detection:

**Detection Criteria**:
1. **Android devices**: Always mobile (User Agent detection)
2. **iOS/Other devices**: Mobile if:
   - Touch-capable AND screen width ≤ 768px, OR
   - Mobile User Agent string detected (iPhone, iPad, iPod, Mobile)

**Breakpoint**: 768px screen width

**User Agent Patterns**: Android, iPhone, iPad, iPod, Mobile

**Touch Detection**: Checks for touch capability (touch events or touch points)

---

### Player Icon System:

**Total Icons**: 20 icons (icon1.png through icon20.png)

**Icon Naming Convention**:
- Key format: `'icon1'`, `'icon2'`, ..., `'icon20'`
- File location: `assets/icons/icon[N].png`

**Size Specifications**:
- Desktop pixel values converted using: `pixels × 0.052vw` (1920px base width)
- Example: 140px = 7.28vw
- Common sizes:
  - Large: 140px (7.28vw)
  - Medium: 70px (3.64vw)
  - Small: 60px (3.12vw)
  - Extra small: 40px (2.08vw)

**Missing Icon Display**: Shows "?" placeholder if icon not found

---

### Color Palette:
- **Primary Yellow**: #ffd82f
- **Cream/Off-white**: #fffbbc
- **Dark Blue**: #030231
- **Dark Purple**: #26174f
- **Bright Green**: #11ffce
- **Teal**: #0e8f9f
- **Bright Red**: #fe1d4a
- **Dark Red**: #9a0a30
- **Grey (disabled)**: #7c7a61
- **Black**: #000000

### Font Families:
- **SINK**: Main UI, buttons, headers (sans-serif fallback)
- **BILLY**: Question text, medium body text (sans-serif fallback)
- **SIGNAL**: Small labels, metadata text (sans-serif fallback)
- **HAVOKS**: Room codes, special headers (sans-serif fallback)
- **HALF AWAKE**: Timer display (sans-serif fallback)

### Timer Specifications:
- **Component**: LottieTimer (React component)
- **Data**: `TimerAnim.json` (Lottie animation)
- **Variants**:
  - "default" - Standard colors (images/ folder)
  - "altcolor" - Alternative colors (images-altcolor/ folder)
- **Circle Progress**:
  - Stroke: 8px width
  - Radius: 35% of timer size
  - Linecap: round
  - Colors:
    - Normal: #11ffce (green)
    - Warning (<25%): #f7f946 (yellow)
    - Critical (<10%): #fe1d4a (red)
  - Background: rgba(76, 22, 36, 1)
- **Number Display**:
  - Font: HALF AWAKE, bold
  - Size: 20% of timer size (normal) or 8% (time's up)
  - Colors:
    - Normal: #f69736 (orange)
    - Warning: #ffa726 (lighter orange)
    - Critical: #fdbc36 (yellow-orange)
    - Expired: #ff4757 (red)
  - Shadow: 2px 2px 4px rgba(0,0,0,0.3)
  - Animations:
    - Bounce: 0.3s ease-out on each second
    - Shake: 0.1s infinite when critical
- **Glow Effect** (critical):
  - Radial gradient: rgba(255, 71, 87, 0.3) � transparent
  - Animation: Pulse 1s alternate
  - Position: -10px inset all sides

### Viewport Conversions:
- **Mobile**: Base width 375px
  - 1px = 0.267vw
  - Safe area: 90vh (accounts for mobile browser UI)
- **Desktop**: Base width 1920px
  - 1px = 0.052vw
  - Full viewport: 100vh

### Animation Keyframes:
- **fadeIn**: opacity 0 � 1
- **fadeOut**: opacity 1 � 0
- **fadeToBlack**: opacity 0 � 1
- **fadeFromBlack**: opacity 1 � 0
- **slideInFromLeft**: translateX(-100%) � 0
- **slideInFromRight**: translateX(100%) � 0
- **slideOutToLeft**: translateX(0) � translateX(-100%)
- **playerAppear**: scale(0) rotate(180deg) � scale(1) rotate(0)
- **borderPulse**: Border color pulse with box shadow
- **playerIconCascadeBounce**: scale(0) � 1.15 � 0.95 � 1
- **scaleUp**: scale(0) + opacity 0 � scale(1) + opacity 1

### Animation Timing Clarifications:

**Index Starting Points**:
ALWAYS start at index 0 for all cascade animations.

**Example Calculations**:
```javascript
// Answer reveals in EliminationScreen
// Base delay: 1000ms, stagger: 200ms per item
const revealDelay = 1000 + (index * 200);

// For 4 answers:
// Answer 0: 1000ms
// Answer 1: 1200ms
// Answer 2: 1400ms
// Answer 3: 1600ms
```

**Cumulative vs Sequential Timing**:

**QuestionScreen Card Flip** (CUMULATIVE - all delays from screen mount):
```
0ms        - Screen mounts, fade from black starts
100ms      - Robot slideout starts
1000ms     - Fade from black completes
1200ms     - Timer scales in, placeholder text fades in
3800ms     - Placeholder text fades out
4200ms     - Card flip starts
4800ms     - Question text reveals (600ms after flip start)
```

**ResultsScreen Panels** (SEQUENTIAL - each panel after previous):
```
700ms      - Initial delay
700ms      - Panel 1 starts (0.7s from start)
5700ms     - Panel 1 ends, Panel 2 starts (5.7s from start)
10700ms    - Panel 2 ends, Panel 3 starts (10.7s from start)
15700ms    - Panel 3 ends, Panel 4 starts (15.7s from start)
20700ms+   - Panel 4 displays until button clicked
```

### Transition Timings:
- **Screen transitions**: 500-1000ms
- **Button hovers**: 200-300ms
- **Fade overlays**: 500ms
- **Card flips**: 600ms
- **Icon pops**: 500ms
- **Slide animations**: 800-1000ms

### Z-Index Layers:
- **Background**: 0
- **Foreground/Robot**: 1-2
- **UI Elements**: 2
- **Overlays/Panels**: 3-4
- **Modals**: 5
- **Fade Transitions**: 9999-10000

### Sound Effects Timing:
- **button_press**: On button click
- **robot_slide_out**: When robot foreground animates in (QuestionScreen)
- **responses_swoosh**: When answer overlay appears (QuestionScreen)
- **timer_final_10sec**: At 12 seconds remaining (once)
- **input_accept**: When answer accepted (mobile)
- **player_icon_pop**: When answer accepted (desktop)
- **failure**: When duplicate or profanity detected
- **success**: When player voted correctly (mobile, VotingScreen)

### Voice Over Timing:
- **landingrules**: Landing page rules video
- **question intro**: 1s after QuestionScreen loads (desktop)
- **question nudge**: At 30s remaining (desktop, QuestionScreen)
- **time**: At 18s remaining (desktop, QuestionScreen, 500ms delay)
- **robotanswergone**: After elimination complete (desktop, EliminationScreen)
- **norobotanswergone**: After tie result (desktop, EliminationScreen)

---

## DATA SCHEMAS

### Question Data Structure:
```javascript
{
  id: string,                    // "q1_round1"
  question: string,              // "What's the best pizza topping?"
  correctAnswer: string,         // "Pepperoni"
  robotAnswer: string,           // "Motor Oil"
  type: "text" | "picture",      // Question type
  round: number,                 // 1-12
  imageUrl?: string              // Only for picture questions
}
```

### Player Data Structure:
```javascript
{
  name: string,                  // "Alice"
  icon: string,                  // "icon1" through "icon20"
  score?: number                 // Cumulative score (optional)
}
```

### Round Results Data Structure:
```javascript
{
  playerScores: {
    [playerName: string]: {
      total: number,             // Round total (+/-)
      correctAnswer: number,     // Points from correct answer (0 or 8/6)
      robotIdentified: number,   // Points from identifying robot (0 or 4/3)
      votesReceived: number,     // Count of votes received
      fooled: number,            // Penalty for being fooled (0 or -8/-6)
      icon: string               // Player icon
    }
  },
  placements: [
    {
      name: string,              // Player name
      placement: number          // 1st, 2nd, 3rd, etc.
    }
  ],
  votingResults: {
    [playerName: string]: string // Player's final vote
  },
  eliminationResults: {
    [playerName: string]: string // Player's elimination vote
  },
  playerAnswers: {
    [playerName: string]: string // Player's submitted answer
  },
  currentQuestion: QuestionData  // The question for this round
}
```

### Elimination Results Data Structure:
```javascript
{
  eliminatedAnswer: string | null,  // Answer eliminated (null if tie)
  tieOccurred: boolean,             // True if vote was tied
  voteCounts: {
    [answer: string]: number        // Vote count per answer
  },
  totalVotesCast: number            // Total votes
}
```

### Voting Results Data Structure:
```javascript
{
  voteCounts: {
    [answer: string]: number        // Vote count per answer
  },
  correctAnswer: string,            // The correct answer
  playersCorrect: string[]          // Names of players who voted correctly
}
```

---

## GENERAL NOTES

### Mobile Behavior:
- All mobile screens use 90vh height (not 100vh) to account for browser UI
- Mobile filters out player's own answer in elimination/voting
- Mobile uses flash messages instead of persistent warnings
- Mobile has simplified layouts (no desktop animations)
- Touch targets have scale feedback (0.93 on press)

### Desktop Behavior:
- Full 100vh layouts
- Complex multi-panel animations
- Keyboard navigation support (Enter/Space)
- Fade from/to black transitions between screens
- Shows all answers including player's own (marked differently)

### Synchronization:
- **HOST_SYNC_DELAY_MS**: 500ms (QuestionScreen, EliminationScreen)
- **MOBILE_FOLLOW_DELAY_MS**: 500ms (all screens)
- **Desktop host** schedules phase transitions
- **Mobile clients** follow scheduled room state updates
- Uses `schedulePhaseTransition` method for coordination

### Answer Validation:
- **Levenshtein Distance**: Calculates edit distance between strings
- **Similarity Threshold**: 85% for correct answer detection, 90% for duplicates
- **Normalization**: Lowercase, remove punctuation, normalize whitespace
- **Within One Letter**: Allows answers that differ by d1 character
- **Banned Words**: Substring match after normalization (no spaces, no special chars)

### Dynamic Sizing:
- Questions use length-based font sizing (shorter = larger)
- Answers use count-based sizing (fewer = larger)
- Mobile uses getDynamicTextStyle utility for auto-fitting
- Desktop has explicit size breakpoints per answer count

### Asset Loading:
- Backgrounds are preloaded
- Fonts are loaded via @font-face
- Images use background-image for sizing control
- Lottie animations load JSON + image assets
- Video uses HTML5 video element with autoplay handling

---

## SOCKET EVENT SYSTEM

### Complete Socket Events List:

**Connection Events**:
- `connect` - Socket connected to server
- `disconnect` - Socket disconnected

**Room Management**:
- `create-room` → Response: `{ success, roomCode }`
- `join-room` → Payload: `{ roomCode, playerName, playerIcon }`
- `leave-room` → Payload: `{ roomCode }`

**Game Flow**:
- `start-game` → Payload: `{ roomCode, gameMode: '8' | '12' }`
- `start-round` → Payload: `{ roomCode, round }`
- `round-started` ← Server broadcasts round start

**Question Phase**:
- `submit-answer` → Payload: `{ roomCode, answer }`
- `player-answered` ← Server notifies player submitted
- `all-answers-submitted` ← Server notifies all answers in

**Elimination Phase**:
- `eliminate-answer` → Payload: `{ roomCode, answer }`
- `elimination-vote-cast` ← Server notifies vote received
- `elimination-time-expired` → Host notifies time up
- `elimination-complete` ← Server provides elimination results
  - Response: `{ eliminatedAnswer, tieOccurred, voteCounts, totalVotesCast }`

**Voting Phase**:
- `start-voting-phase` → Host triggers voting start
- `submit-vote` → Payload: `{ roomCode, votedAnswer }`
- `final-voting-time-expired` → Host notifies time up
- `all-votes-submitted` ← Server provides voting results
  - Response: `{ voteCounts, correctAnswer, playersCorrect }`

**Results Phase**:
- `round-completed` → Payload: `{ roomCode, playerScores }`
- `final-round-scores` ← Server provides round results
  - Response: `{ playerScores, placements, votingResults, eliminationResults, playerAnswers, currentQuestion }`

**Special Rounds**:
- `halftime-reached` → Payload: `{ roomCode, currentScores, gameMode, currentRound }`
- `bonus-round-started` → Payload: `{ roomCode, currentScores, bonusQuestions }`
- `bonus-transition-ready` → Payload: `{ roomCode, followDelayMs }`
- `bonus-question-started` → Payload: `{ roomCode, questionNumber, questionData, currentScores, bonusQuestions }`
- `submit-bonus-vote` → Payload: `{ roomCode, votedPlayer, questionNumber }`
- `bonus-voting-time-expired` → Payload: `{ roomCode, questionNumber }`
- `bonus-votes-complete` ← Server provides bonus results
- `bonus-player-voted` ← Server notifies bonus vote cast

**End Game**:
- `game-completed` → Payload: `{ roomCode, finalScores }`

**State Sync**:
- `room-state-update` ← Server provides state updates
- `players-update` ← Server provides player list
- `scores-updated` ← Server provides score updates

---

## SHARE RESULTS POPUP COMPONENT

### Component Specification:

**Props**:
```javascript
{
  isVisible: boolean,        // Show/hide popup
  onClose: function,         // Close handler
  playerName: string,        // Player's name
  playerIcon: string,        // Player's icon key
  totalScore: number,        // Final score (raw points)
  rank: number              // Player's rank (1st, 2nd, etc.)
}
```

**Layout**:
- **Container**:
  - Position: Fixed, full screen (100vw x 100vh)
  - Background: rgba(0, 0, 0, 0.7) (dark overlay)
  - Display: flex, centered
  - Z-index: 9999

- **Modal Box**:
  - Background: #030231 (dark blue)
  - Border: 0.16vw solid #fe1d4a (red)
  - Border radius: 1.04vw
  - Padding: 2.78vh
  - Max width: 20.83vw
  - Width: 90%
  - Font: SINK, sans-serif

- **Close Button** (top-right):
  - Position: Absolute, top 1.39vh, right 0.78vw
  - Text: "×"
  - Font size: 2.22vh
  - Color: #fffbbc
  - No background/border

**Content Sections**:

1. **Congratulations Text**:
   - Text: "Congratulations on completing your test."
   - Color: #fffbbc
   - Font size: 1.67vh
   - Font weight: bold
   - Letter spacing: 0.05vw

2. **Share Section**:
   - Header: "SHARE YOUR SCORE:"
   - Color: #ffd82f (yellow)
   - Font size: 2.22vh
   - Share Button:
     - Background: #fe1d4a (red)
     - Color: #fffbbc
     - Border radius: 0.52vw
     - Padding: 1.39vh 1.56vw
     - Text: "SHARE"
     - Calls `shareScoreCard()` utility function

3. **Divider**:
   - Height: 0.09vh
   - Background: #444
   - Margin: 1.85vh 0

4. **Discount Section**:
   - Header: "Get 25% off Cosmic Meatball games"
   - Color: #fffbbc
   - Font size: 1.67vh
   - Email Input:
     - Width: 100%
     - Padding: 1.11vh
     - Font size: 1.48vh
     - Border radius: 0.42vw
     - Background: #2a2a2a (dark grey)
     - Color: #fffbbc
     - Placeholder: "Enter your email"
     - Invalid state: red border (#ff4444)
   - Submit Button:
     - Background: #fe1d4a (normal) or #666 (submitted)
     - Color: #fffbbc
     - Border radius: 0.42vw
     - Padding: 1.11vh 1.30vw
     - Text: "SUBMIT" / "Submitting..." / "Sent!"
     - Disabled when no email or already submitted

**Email Validation**:
- Regex: `/^[^\s@]+@[^\s@]+\.[^\s@]+$/`
- Invalid format message: "Please enter a valid email address"

**Email Storage**:
- Stores submitted email addresses in database
- Prevents duplicate submissions
- Auto-closes popup after successful submission

---

This specification provides all necessary information to recreate the game screens in another programming language, including exact measurements, colors, timing, animations, game logic flow, network communication, and all utility functions.
