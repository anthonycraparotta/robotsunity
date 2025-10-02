# Phase 2: QuestionScreen Scene - COMPLETE ✓

## Summary
Phase 2 has been successfully completed. The QuestionScreen is the most complex scene in the game, featuring desktop card flip animations, robot character slide-in, real-time answer validation, timer with multiple states, and mobile input with flash warnings.

---

## Deliverables Created

### UI Components (7 files)
```
Scripts/UI/
├── QuestionCard.cs            ✓ - Desktop card flip animation
├── RobotCharacter.cs          ✓ - Robot slide-in and blinking
├── TimerDisplay.cs            ✓ - Countdown timer with visual states
├── PlayerIconGrid.cs          ✓ - Player icons that pop in when answers submitted
├── MobileAnswerInput.cs       ✓ - Mobile input with flash warnings
└── AnswersOverlay.cs          ✓ - Desktop overlay showing received answers

Scripts/Screens/
└── QuestionScreenController.cs ✓ - Main scene controller
```

---

## Components Overview

### QuestionCard.cs
**Purpose**: Desktop question card with flip animation

**Features**:
- Front face with placeholder text "WRITE A CREATIVE ANSWER TO THIS QUESTION"
- Back face with actual question
- Timed animation sequence:
  - 0-800ms: Slide in
  - 1200ms: Placeholder fade in
  - 3800ms: Placeholder fade out
  - 4200ms: Card flip (600ms duration, 3D rotation)
  - 4800ms: Question text reveal
- Instant show method for skipping animation

**Key Methods**:
- `PlayCardSequence(string question)` - Start full animation
- `ShowQuestionInstant(string question)` - Skip to final state

---

### RobotCharacter.cs
**Purpose**: Robot foreground image with animations

**Features**:
- Round-specific sprites (Q1FG through Q12FG)
- Blink sprites (Q1FG-blink through Q12FG-blink)
- Slide-in from left (100ms delay, 1000ms duration)
- Automatic blinking every 5 seconds (200ms duration)
- Plays "robot_slide_out" SFX when sliding in

**Key Methods**:
- `SetupRobot(int round)` - Load sprites for round
- `SlideIn()` - Trigger slide-in animation
- `StartBlinking()` / `StopBlinking()` - Control blink loop

---

### TimerDisplay.cs
**Purpose**: Countdown timer with urgency states

**Features**:
- 60-second countdown (configurable)
- Visual states:
  - **Normal** (>25% time): White/normal color
  - **Warning** (≤25% time, 30s): Yellow color
  - **Critical** (≤18s): Red color, plays time warning VO
  - **Final** (≤12s): Red + shake animation, plays timer SFX
- Scale-in animation on start (1.2s delay, 0.5s duration)
- Pause/resume support
- Auto-expires at 0

**Audio Cues**:
- 18s: "time warning" voice-over (desktop only, 500ms delay)
- 12s: "timer_final_10sec" SFX (plays once)

**Key Methods**:
- `StartTimer(float delay)` - Begin countdown with animation
- `PauseTimer()` / `ResumeTimer()` - Control flow
- `SetTime(float seconds)` - Change total time

**Properties**:
- `TimeRemaining` - Current seconds left
- `IsRunning` - Whether countdown active
- `IsExpired` - Whether time is up

---

### PlayerIconGrid.cs
**Purpose**: Desktop grid showing player icons as they submit

**Features**:
- Dynamically creates player icon GameObjects
- Two sizes: 140px (text questions) or 70px (picture questions)
- Pop-in animation (0.5s with OutBack ease)
- Loads player icon sprites from Resources/PlayerIcons/
- Shows player name label below icon
- Plays "player_icon_pop" SFX when icon appears
- Tracks which players have submitted

**Key Methods**:
- `SetPictureQuestion(bool isPicture)` - Adjust icon size
- `ShowPlayerIcon(Player player)` - Display player's icon
- `AllPlayersSubmitted(List<Player> allPlayers)` - Check completion
- `ClearIcons()` - Remove all icons

---

### MobileAnswerInput.cs
**Purpose**: Mobile input field with validation warnings

**Features**:
- TMP_InputField with submit button
- Three flash warning types (2000ms duration each):
  - **Decoy Warning**: "CORRECT ANSWER, ENTER A DECOY" (restores text)
  - **Duplicate Warning**: "DUPLICATE RESPONSE" (clears field)
  - **Profanity Warning**: "INAPPROPRIATE CONTENT" (clears field)
- Visual states:
  - Normal: Cream background, dark blue text
  - Submitted: Dark blue background (0.25 alpha), disabled
  - Warning Flash: Red background, white text
- Submit button auto-enables when text entered
- Plays "failure" SFX on warnings
- Plays "input_accept" SFX on successful submit

**Key Methods**:
- `FlashDecoyWarning(string originalAnswer)` - Show decoy flash
- `FlashDuplicateWarning()` - Show duplicate flash
- `FlashProfanityWarning()` - Show profanity flash
- `MarkAsSubmitted()` - Disable after submission
- `ClearInput()` - Reset field

**Events**:
- `OnAnswerSubmitted(string answer)` - Fires when submitted
- `OnAnswerChanged(string answer)` - Fires on text change

---

### AnswersOverlay.cs
**Purpose**: Desktop overlay showing received answers (cascade animation)

**Features**:
- Shows after all answers received (2s delay)
- Cascade reveal: Each item appears 200ms after previous
- Items slide in from left with fade (500ms per item)
- Plays "responses_swoosh" SFX when appearing
- Displays player name + answer text
- Semi-transparent dark blue background with blur effect
- Header: "Answers Received"

**Key Methods**:
- `ShowOverlay(List<Answer> answers)` - Display answers with animation
- `HideOverlay()` - Fade out and hide
- `ClearAnswers()` - Remove all items

---

### QuestionScreenController.cs
**Purpose**: Main controller coordinating all components

**Features**:
- Platform detection (desktop vs mobile)
- Round-specific background loading (Q1BG through Q12BG)
- Answer validation pipeline:
  1. Check against correct answer (85% similarity)
  2. Check for duplicates (90% similarity)
  3. Check for profanity (banned words)
- Timer management with auto-submit on expiration
- Voice-over triggers:
  - 1s after load: Question intro
  - 30s remaining: Nudge VO (if not submitted)
- All players submitted detection
- Answer overlay trigger (desktop only)
- Scene transition after completion
- Fade in/out transitions

**Desktop Flow**:
1. Load scene → Fade in from black
2. Robot slides in (100ms delay)
3. Question card flips (4.2s animation)
4. Timer scales in (1.2s delay)
5. Question intro VO plays (1s delay)
6. Player types/submits answer
7. Player icon pops in on grid
8. When all submit → Timer pauses
9. Answers overlay appears (2s delay)
10. Wait 3s → Fade out → Next scene

**Mobile Flow**:
1. Load scene → Fade in
2. Question text fades in (1s delay)
3. Timer starts immediately
4. Player types in input field
5. Real-time validation (warnings if invalid)
6. Tap submit button
7. Input disabled, shows "Answer submitted!"
8. Wait for all players → Next scene

**Key Methods**:
- `SetupQuestion(Question question)` - Initialize scene for round
- `SubmitAnswer(string answer)` - Process player submission
- `ValidateAnswerForSubmission(string answer)` - Full validation check
- `AutoSubmitAnswer()` - Submit on timer expiration
- `CheckAllAnswersReceived()` - Trigger completion flow

---

## Technical Implementation

### Answer Validation Pipeline

Uses `AnswerValidator` from Phase 0:

```
1. Correct Answer Check (85% similarity OR ≤1 char diff)
   → Show decoy warning, prevent submission

2. Duplicate Check (90% similarity OR ≤1 char diff)
   → Compare against all submitted + robot + correct
   → Show duplicate warning, prevent submission

3. Profanity Check (substring match)
   → Load banned words from data/banned_words.txt
   → Show profanity warning, prevent submission

4. Valid Answer
   → Submit to GameManager
   → Add to allAnswers list
   → UI feedback (icon or input state)
```

### Timer Urgency States

```
Normal (>25% remaining)
  → White color
  → No audio

Warning (≤25%, ~30s for 60s timer)
  → Yellow color
  → Nudge VO plays (desktop, if not submitted)

Critical (≤18s)
  → Red color
  → Time warning VO plays (desktop, 500ms delay)

Final (≤12s)
  → Red color + shake animation
  → Timer final 10sec SFX plays once
```

### Round-Specific Assets

Expected assets per round (1-12):
- **Backgrounds**: Q1BG.png through Q12BG.png
- **Foregrounds**: Q1FG.png through Q12FG.png
- **Blink Foregrounds**: Q1FG-blink.png through Q12FG-blink.png

Mobile backgrounds:
- `mobilecontrollerbg.png` - Base background
- `questionmobile.png` - Text question overlay
- `picqmobile.png` - Picture question overlay

---

## Integration with Previous Phases

### From Phase 0:
- **GameConstants**: Timing, thresholds, colors, audio names
- **AnswerValidator**: All validation logic
- **GameManager**: Question data, player list, answer submission
- **AudioManager**: SFX and voice-over playback
- **Data Structures**: Question, Answer, Player

### From Phase 1:
- **FadeTransition**: Scene entry/exit fades
- **ResponsiveUI**: Desktop/mobile detection
- **ButtonEffects**: Submit button animations (mobile)

---

## Asset Requirements

### Images
- `assets/mainscreenbg/Q1BG.png` through `Q12BG.png` (12 files)
- `assets/mainscreenbg/Q1FG.png` through `Q12FG.png` (12 files)
- `assets/mainscreenbg/Q1FG-blink.png` through `Q12FG-blink.png` (12 files)
- `assets/mobilebg/mobilecontrollerbg.png`
- `assets/mobilebg/questionmobile.png`
- `assets/mobilebg/picqmobile.png`
- `assets/ui/submitanswerbutton.png`
- `Resources/PlayerIcons/icon1.png` through `icon20.png` (20 files)

### Audio
- **SFX**: robot_slide_out, responses_swoosh, timer_final_10sec, input_accept, player_icon_pop, failure
- **Voice-Overs**: question intro, question nudge, time warning

### Fonts
- SINK (UI text)
- BILLY (question text)
- SIGNAL (small labels)

### Data
- `data/banned_words.txt` - Profanity filter list

---

## Testing Checklist

### Desktop Mode (1920x1080)

**Scene Load**:
- [ ] Fades in from black (1s)
- [ ] Round-specific background displays
- [ ] Robot slides in from left (100ms delay, 1s duration)
- [ ] Robot slide-out SFX plays
- [ ] Question card appears and flips (4.2s animation)
- [ ] Timer scales in (1.2s delay, 0.5s duration)
- [ ] Question intro VO plays (1s delay)

**Card Flip Animation**:
- [ ] Placeholder text fades in at 1.2s
- [ ] Placeholder fades out at 3.8s
- [ ] Card rotates in 3D at 4.2s (600ms)
- [ ] Question text reveals at 4.8s
- [ ] Animation smooth and timed correctly

**Robot Blinking**:
- [ ] Blink image appears every 5 seconds
- [ ] Blink lasts 200ms
- [ ] Returns to normal foreground

**Timer States**:
- [ ] Starts at 60 (or configured time)
- [ ] Counts down correctly
- [ ] At 30s: Turns yellow, nudge VO plays (if not submitted)
- [ ] At 18s: Turns red, time warning VO plays (500ms delay)
- [ ] At 12s: Shakes, timer SFX plays once
- [ ] Timer continues shaking until 0

**Answer Submission** (desktop would have input, simplified):
- [ ] Player icon appears when answer submitted
- [ ] Icon pops in with bounce (0.5s)
- [ ] Player icon pop SFX plays
- [ ] Icon shows correct player sprite and name

**All Answers Received**:
- [ ] Timer pauses when all submit
- [ ] Answers overlay appears after 2s delay
- [ ] Responses swoosh SFX plays
- [ ] Answer items cascade in (200ms apart)
- [ ] Each item slides from left with fade
- [ ] Wait 3s then fade to black
- [ ] Transitions to next scene

### Mobile Mode (375x812)

**Scene Load**:
- [ ] Fades in from black
- [ ] Mobile background displays
- [ ] Question text fades in after 1s
- [ ] Question uppercase
- [ ] Timer starts immediately

**Input Field**:
- [ ] Placeholder: "Type your answer here..."
- [ ] Cream background, dark blue text
- [ ] Submit button disabled when empty
- [ ] Submit button enabled when text entered
- [ ] Submit button alpha 0.5 disabled, 1.0 enabled

**Validation - Decoy Warning**:
- [ ] Type correct answer
- [ ] Tap submit
- [ ] Flash shows "CORRECT ANSWER, ENTER A DECOY"
- [ ] Background red, text white
- [ ] Failure SFX plays
- [ ] After 2s: Restores original text
- [ ] Background returns to cream

**Validation - Duplicate Warning**:
- [ ] Type duplicate of robot/correct/other player
- [ ] Tap submit
- [ ] Flash shows "DUPLICATE RESPONSE"
- [ ] Background red, text white
- [ ] Failure SFX plays
- [ ] After 2s: Field clears
- [ ] Background returns to cream

**Validation - Profanity Warning**:
- [ ] Type banned word
- [ ] Tap submit
- [ ] Flash shows "INAPPROPRIATE CONTENT"
- [ ] Background red, text white
- [ ] Failure SFX plays
- [ ] After 2s: Field clears
- [ ] Background returns to cream

**Successful Submission**:
- [ ] Type valid answer
- [ ] Tap submit
- [ ] Input accept SFX plays
- [ ] Background changes to dark blue (0.25 alpha)
- [ ] Input disabled
- [ ] Placeholder changes to "Answer submitted!"
- [ ] Submit button disabled

**Timer Expiration**:
- [ ] If time runs out, auto-submits current answer
- [ ] If field empty, submits "No answer provided"
- [ ] Proceeds to next screen

### General
- [ ] No console errors
- [ ] All animations smooth (60 FPS)
- [ ] Audio plays correctly
- [ ] Scene transitions work
- [ ] GameManager receives submitted answers

---

## Known Limitations

1. **Next Scene Placeholder**: Logs transition instead of loading EliminationScreen (uncomment when scene exists)

2. **Desktop Input Field**: Desktop answer input simplified in controller (full implementation would mirror mobile validation)

3. **Player Icon Sprites**: Must be placed in `Resources/PlayerIcons/` folder with names matching icon keys (icon1.png, icon2.png, etc.)

4. **Question Data**: Must be loaded into GameManager before this scene loads

5. **Network Sync**: Controller handles single-player flow; multiplayer would need network synchronization for "all players submitted" check

---

## Files Summary

| File | Purpose | Lines |
|------|---------|-------|
| QuestionCard.cs | Card flip animation | ~170 |
| RobotCharacter.cs | Robot slide-in & blink | ~160 |
| TimerDisplay.cs | Countdown timer | ~230 |
| PlayerIconGrid.cs | Player icon grid | ~240 |
| MobileAnswerInput.cs | Mobile input with warnings | ~290 |
| AnswersOverlay.cs | Desktop answers overlay | ~240 |
| QuestionScreenController.cs | Main scene controller | ~360 |
| **Total** | | **~1,690 lines** |

---

## Next Phase Preview

**Phase 3: EliminationScreen** - Vote to eliminate robot answer:
- Desktop: Answer list with vote selection
- Mobile: Answer buttons with eliminate button
- Vote tracking and aggregation
- Elimination result calculation (handles ties)
- Result overlay: "ROBOT IDENTIFIED" or "TIE VOTE!"
- Voice-over based on result

Should be simpler than QuestionScreen (1-2 sessions).

---

**Status**: ✓ Phase 2 Complete - Ready for Unity Scene Creation or Phase 3

---

## To Create Scene in Unity

1. Create new Scene: "QuestionScreen"
2. Setup Canvas with ResponsiveUI
3. Create DesktopContent and MobileContent hierarchies
4. Add all UI components
5. Create controller GameObject with QuestionScreenController
6. Assign all references in Inspector
7. Import round-specific assets (backgrounds, foregrounds, blink images)
8. Test both desktop and mobile views
9. Verify all animations and timing
10. Test validation warnings
11. Test timer states and audio cues
12. Add to Build Settings

**Next Action**: Create QuestionScreen scene in Unity Editor, or proceed to Phase 3 (EliminationScreen).
