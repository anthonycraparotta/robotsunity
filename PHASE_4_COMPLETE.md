# Phase 4: VotingScreen Scene - COMPLETE ✓

## Summary
Phase 4 has been successfully completed. The VotingScreen allows players to vote for the correct answer from remaining options, with correct answer reveal and success feedback.

---

## Deliverables Created

### UI Components (2 files)
```
Scripts/UI/
└── VotingAnswerList.cs            ✓ - Answer list with correct answer reveal

Scripts/Screens/
└── VotingScreenController.cs      ✓ - Main scene controller
```

---

## Components Overview

### VotingAnswerList.cs
**Purpose**: Answer list for voting on correct answer

**Features**:
- Similar to EliminationAnswerList but for voting FOR correct
- All answers revealed immediately (no cascade)
- Desktop: Shows all answers including own (blocked)
- Mobile: Filters out own answer
- Click/tap to select (green background)
- Correct answer highlight after voting:
  - Red background
  - Cream text color
  - Scale 1.02x
  - Label appended: "(TRUE ANSWER)"
  - Optional check icon
- Non-correct answers greyed out
- Success sound on mobile if player voted correctly
- Shuffle answers for randomness

**Key Methods**:
- `SetupAnswers(answers, ownAnswer, filterOwn)` - Populate list
- `HighlightCorrectAnswer(correctAnswer, playerGotItRight)` - Show result
- `DisableVoting()` - Lock after vote submitted
- `ClearSelection()` / `ClearAnswers()` - Reset state

**Events**:
- `OnAnswerSelected(string answer)` - Fires when answer clicked

**Color Scheme**:
- Normal: Cream background (#fffbbc)
- Selected: Bright green background (#11ffce)
- Correct: Red background (#fe1d4a), cream text
- Greyed: Light grey with 0.8 alpha

---

### VotingScreenController.cs
**Purpose**: Main controller for voting flow

**Features**:
- Platform detection (desktop vs mobile)
- Remaining answers from elimination phase
- Timer management (30 seconds)
- Vote submission tracking
- Correct answer reveal sequence:
  1. Player votes
  2. Disable voting
  3. Delay 500ms
  4. Highlight correct answer
  5. Play success SFX (mobile, if correct)
  6. Hold 4 seconds
  7. Transition to ResultsScreen
- Desktop: Auto-submits on selection (500ms delay)
- Mobile: Requires submit button tap

**Desktop Flow**:
```
Load → Fade in
    → All answers revealed (500ms delay)
    → Timer scales in (1s delay)
    → User clicks answer
    → Auto-submit after 500ms
    → Pause timer
    → Delay 500ms
    → Highlight correct answer (red)
    → Grey out others
    → Hold 4s
    → Fade to ResultsScreen
```

**Mobile Flow**:
```
Load → Fade in
    → All answers revealed (own filtered)
    → Timer starts
    → User taps answer (button enables)
    → User taps "SUBMIT FINAL ANSWER"
    → Pause timer
    → Delay 500ms
    → Highlight correct answer (red)
    → Play success SFX (if correct)
    → Grey out others
    → Hold 4s
    → Fade to ResultsScreen
```

**Key Methods**:
- `SubmitVote()` - Process player vote
- `AutoSubmitVote()` - Submit on timer expiration
- `ShowResults()` - Highlight correct answer
- `TransitionToNextScreen()` - Fade to ResultsScreen

---

## Technical Implementation

### Differences from EliminationScreen

| Feature | EliminationScreen | VotingScreen |
|---------|------------------|--------------|
| Purpose | Vote to eliminate robot | Vote for correct answer |
| Selection color | Red (#fe1d4a) | Green (#11ffce) |
| Result highlight | Red (eliminated) | Red (correct) |
| Cascade animation | Yes (1s + 200ms stagger) | No (all at 500ms) |
| Mobile button text | "ELIMINATE THIS ANSWER" | "SUBMIT FINAL ANSWER" |
| Result duration | 4s (desktop), 2s (mobile) | 4s (both) |
| Success sound | None | "success" (mobile, if correct) |
| Next scene | VotingScreen | ResultsScreen |

### Vote Tracking

Unlike elimination (which aggregates votes), voting screen:
- Tracks individual player's choice
- Reveals correct answer immediately (single-player)
- In multiplayer: Would wait for all votes, then reveal
- Success determined by: `selectedAnswer == correctAnswer`

### Answer Filtering

**Desktop**:
- Shows all remaining answers
- Blocks own answer (not clickable)

**Mobile**:
- Filters out own answer (not shown)
- Only remaining player/correct/robot answers

### Timer Integration

- Same 30-second timer as EliminationScreen
- Auto-submits selected answer on expiration
- If no selection: Player doesn't vote (no points earned)

---

## Integration with Previous Phases

### From Phase 0:
- **GameConstants**: Colors (BrightGreen for selection), timing
- **GameManager**: Question data, answers, player info
- **AudioManager**: Success SFX

### From Phase 1:
- **FadeTransition**: Scene entry/exit
- **ResponsiveUI**: Platform detection

### From Phase 2:
- **TimerDisplay**: Countdown timer (reused)

### From Phase 3:
- **Similar Structure**: Mirrors EliminationScreen pattern

---

## Asset Requirements

### Images
- `assets/mainscreenbg/eliminatevotebg.png` - Desktop background (same as Elimination)
- `assets/mobilebg/namecodeiconmobile.png` - Mobile background (same as Elimination)
- `assets/ui/votinghero.png` - Desktop hero image (top right)
- `assets/ui/checkicon.png` - Check icon for correct answer (optional)

### Audio
- **SFX**: success (mobile only, when player votes correctly)

### Fonts
- SINK (main UI)
- SIGNAL (small text)

---

## Testing Checklist

### Desktop Mode (1920x1080)

**Scene Load**:
- [ ] Fades in from black (500ms)
- [ ] Background displays
- [ ] Hero image shows (top right)
- [ ] Timer scales in (1s delay)
- [ ] Instruction text: "Answer this ordinary multiple-choice trivia question"
- [ ] All answers revealed at 500ms (no cascade)

**Answer List**:
- [ ] All remaining answers display (after elimination)
- [ ] Answers shuffled randomly
- [ ] All appear at once (not staggered)
- [ ] Font size scales based on count

**Voting**:
- [ ] Click answer → Turns green background
- [ ] Previous selection deselected
- [ ] Own answer blocked (not clickable)
- [ ] Auto-submits after 500ms
- [ ] Timer pauses

**Correct Answer Reveal**:
- [ ] Delay 500ms after vote
- [ ] Correct answer turns red background
- [ ] Correct answer text turns cream
- [ ] Correct answer scales to 1.02x
- [ ] Label appended: "(TRUE ANSWER)"
- [ ] All other answers greyed out
- [ ] Holds 4 seconds
- [ ] Fades to ResultsScreen

### Mobile Mode (375x812)

**Scene Load**:
- [ ] Fades in from black
- [ ] Mobile background displays
- [ ] Header: "CHOOSE THE RIGHT ANSWER" (green text)
- [ ] All answers revealed (own filtered)
- [ ] Timer starts immediately

**Voting**:
- [ ] Tap answer → Turns green
- [ ] Submit button enables
- [ ] Button text: "SUBMIT FINAL ANSWER"
- [ ] Button color green when enabled
- [ ] Tap submit → Vote cast

**After Vote**:
- [ ] Button disabled
- [ ] Button text: "VOTE CAST!"
- [ ] Non-selected answers grey out
- [ ] Timer pauses

**Correct Answer Reveal**:
- [ ] Delay 500ms
- [ ] Correct answer highlights red
- [ ] If player correct: Success SFX plays
- [ ] If player wrong: No sound
- [ ] Others greyed out
- [ ] Holds 4s
- [ ] Fades to ResultsScreen

### General
- [ ] Timer counts down from 30s
- [ ] Timer expiration auto-submits
- [ ] No console errors
- [ ] Animations smooth
- [ ] Audio plays correctly (mobile success)
- [ ] Scene transitions work

---

## Known Limitations

1. **Single-Player Flow**: Shows results immediately. Multiplayer would wait for all player votes.

2. **Eliminated Answer**: Currently uses all CurrentAnswers. Real game should filter out eliminated answer from previous screen.

3. **Vote Aggregation**: No server-side aggregation needed for single-player. Multiplayer would track all votes for scoring.

4. **Next Scene**: Logs transition instead of loading ResultsScreen (uncomment when scene exists).

---

## Files Summary

| File | Purpose | Lines |
|------|---------|-------|
| VotingAnswerList.cs | Answer list with correct reveal | ~370 |
| VotingScreenController.cs | Main scene controller | ~330 |
| **Total** | | **~700 lines** |

---

## Next Phase Preview

**Phase 5: ResultsScreen** - Multi-panel round results:
- 4 sequential panels with transitions
- Panel 1: Who got correct answer + points
- Panel 2: Who identified robot vs fooled
- Panel 3: Player answers with vote counts
- Panel 4: Current standings (ranked by score)
- Desktop: Next Round button
- Mobile: Auto-advances after host

Most complex UI so far due to 4-panel system.

---

**Status**: ✓ Phase 4 Complete - Ready for Unity Scene Creation or Phase 5

---

## To Create Scene in Unity

1. Create new Scene: "VotingScreen"
2. Setup Canvas with ResponsiveUI
3. Create DesktopContent:
   - Background image (eliminatevotebg.png)
   - Hero image (votinghero.png, top right)
   - Answer list container
   - Timer
   - Instruction text
4. Create MobileContent:
   - Background image (namecodeiconmobile.png)
   - Header text ("CHOOSE THE RIGHT ANSWER")
   - Answer list container
   - Submit button
   - Timer
5. Add controller GameObject with VotingScreenController
6. Assign all references in Inspector
7. Import checkicon.png sprite
8. Test both desktop and mobile views
9. Verify correct answer highlight
10. Test success sound on mobile
11. Add to Build Settings

**Next Action**: Create VotingScreen scene in Unity Editor, or proceed to Phase 5 (ResultsScreen).
