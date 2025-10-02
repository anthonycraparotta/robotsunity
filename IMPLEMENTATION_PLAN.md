# Unity Implementation Plan for Robots Game

## Overview
This document breaks down the implementation of the Unity game into logical, manageable sections to avoid token limit issues. Each section should be completed in order, with verification checkpoints.

---

## Implementation Phases

### Phase 0: Foundation & Global Setup
**Goal:** Set up core systems, managers, and global constants

**Tasks:**
1. Create project structure and folders
2. Implement GameConstants (scoring, timing, thresholds)
3. Implement data structures (Question, Answer, Player, RoundScore, VoteResults)
4. Create GameManager singleton
5. Create AudioManager for sound effects and voice-overs
6. Create UI utility scripts (button effects, fade transitions)
7. Set up Canvas Scaler and responsive system
8. Import and organize all assets (fonts, images, audio, video)

**Deliverables:**
- Scripts/Core/ folder with managers
- Scripts/Data/ folder with data structures
- Scripts/UI/Utilities/ folder with reusable UI components
- Assets organized in proper folders

---

### Phase 1: LandingPage Scene
**Goal:** Build the entry point screen

**Section 1.1 - Basic Elements:**
- Scene setup with Canvas
- Background image (landingbg.png)
- Fade overlay system
- Basic layout structure

**Section 1.2 - Interactive Elements:**
- Start Game button with hover effects
- Video player integration (rulesvideo.mp4)
- Keyboard input handling (Enter/Space)

**Section 1.3 - Logic & Transitions:**
- Fade in/out animations using DOTween
- Audio integration (button_press SFX, landingrules VO)
- Scene transition to Join Room
- Mobile vs Desktop responsive behavior

**Verification Checklist:**
- [ ] Scene loads with fade-in effect
- [ ] Video plays automatically on desktop
- [ ] Button hover effect works (scale 1.05)
- [ ] Button click plays SFX and fades out
- [ ] Enter/Space key triggers same behavior
- [ ] Mobile shows black screen only
- [ ] Transition to next scene works

---

### Phase 2: QuestionScreen Scene
**Goal:** Build the main gameplay screen where players submit answers

**Section 2.1 - Basic Elements (Desktop):**
- Background setup
- Top info bar (round counter, timer display)
- Question card with flip animation container
- Robot character container
- Answer submission area

**Section 2.2 - Basic Elements (Mobile):**
- Simplified background
- Round/timer header
- Question text display
- Input field and submit button
- Status text area

**Section 2.3 - Relationships & Connections:**
- Timer synchronization system
- Answer collection and validation
- Player icon grid management
- Answer overlay panel structure

**Section 2.4 - Complex Mechanisms (Desktop):**
- Card flip animation (question reveal)
- Robot slide-out animation
- Answer submission with real-time validation
- Player icon pop-in when answer submitted
- Timer visual states (normal/warning/critical)
- Keyboard input (Enter to submit)

**Section 2.5 - Complex Mechanisms (Mobile):**
- Input field validation (duplicate/correct/profanity)
- Flash warning animations
- Submit button enable/disable logic
- Timer sync with desktop

**Section 2.6 - Logic & State Management:**
- Question data loading
- Answer validation (85% similarity)
- Duplicate detection (90% similarity)
- Profanity filtering
- All players submitted check
- Timer expiration handling
- Audio cues (robot_slide_out, responses_swoosh, question intro VO, etc.)

**Verification Checklist:**
- [ ] Question displays correctly (text and image types)
- [ ] Timer counts down with correct visual states
- [ ] Card flip animation works smoothly
- [ ] Robot character slides out
- [ ] Players can submit answers
- [ ] Validation prevents duplicates/correct answer/profanity
- [ ] Player icons appear when answers submitted
- [ ] Timer stops when all players submit
- [ ] Audio cues play at correct times
- [ ] Mobile input works with touch keyboard
- [ ] Transitions to EliminationScreen when complete

---

### Phase 3: EliminationScreen Scene
**Goal:** Build the screen where players vote to eliminate the robot answer

**Section 3.1 - Basic Elements (Desktop):**
- Background
- Header with instructions
- Timer display
- Answer list panel
- Vote count display area

**Section 3.2 - Basic Elements (Mobile):**
- Header with round/timer
- Answer button grid
- Eliminate button
- Selection indicator

**Section 3.3 - Relationships & Connections:**
- Answer list population (all submitted answers)
- Vote tracking per answer
- Selected answer highlighting
- Timer synchronization

**Section 3.4 - Complex Mechanisms:**
- Answer button selection toggle
- Vote submission
- All votes collected check
- Timer expiration handling
- Result calculation (most voted answer)
- Tie vote detection
- Result display overlay ("ROBOT IDENTIFIED" or "TIE VOTE")

**Section 3.5 - Logic & State Management:**
- Vote collection and aggregation
- Elimination logic (highest vote count)
- Tie handling
- Answer removal from pool
- Audio cues (robotanswergone/norobotanswergone VO)

**Verification Checklist:**
- [ ] All answers display in list
- [ ] Players can select one answer
- [ ] Vote submission works
- [ ] Timer counts down correctly
- [ ] All votes trigger early transition
- [ ] Elimination calculates correctly
- [ ] Tie vote handled properly
- [ ] Result overlay shows correct message
- [ ] Audio plays based on result
- [ ] Transitions to VotingScreen

---

### Phase 4: VotingScreen Scene
**Goal:** Build the screen where players vote for the correct answer

**Section 4.1 - Basic Elements:**
- Similar structure to EliminationScreen
- Header with different instructions
- Answer list (minus eliminated answer)
- Vote display area

**Section 4.2 - Relationships & Connections:**
- Filtered answer list (no eliminated answer)
- Mobile filters out own answer
- Vote tracking per answer
- Correct answer highlighting after voting

**Section 4.3 - Complex Mechanisms:**
- Answer selection
- Vote submission
- Timer handling
- Correct answer reveal
- Vote count display on each answer
- Success sound on mobile if voted correctly

**Section 4.4 - Logic & State Management:**
- Vote collection
- Correct answer comparison
- Points calculation preview
- Audio cues (success SFX on mobile if correct)

**Verification Checklist:**
- [ ] Eliminated answer not shown
- [ ] Players can vote for remaining answers
- [ ] Correct answer highlights after voting
- [ ] Vote counts display on answers
- [ ] Mobile success sound plays if voted correctly
- [ ] Timer works correctly
- [ ] Transitions to ResultsScreen

---

### Phase 5: ResultsScreen Scene
**Goal:** Build the multi-panel results display

**Section 5.1 - Panel Structure:**
- 4-panel container system
- Navigation controls (Next Round button on desktop)
- Panel transition animations

**Section 5.2 - Panel 1: Correct Answer Panel:**
- Header "Who got the correct answer?"
- Correct answer display
- Player list who got it right
- Points awarded display (+8 or +6)
- Fallback "[No Testers!]" if nobody correct

**Section 5.3 - Panel 2: Robot Identification Panel:**
- Split layout (identified vs fooled)
- Robot answer display
- Player lists for each side
- Points display (+4/+3 for identified, -8/-6 for fooled)

**Section 5.4 - Panel 3: Player Answers Panel:**
- Grid of all player answers
- Vote count badges on each
- Visual distinction for robot/correct/player answers

**Section 5.5 - Panel 4: Standings Panel:**
- Ranked player list by score
- Player icons, names, scores
- Score change indicators (+/- from round)

**Section 5.6 - Logic & Transitions:**
- Panel auto-advance timing (3-4 seconds each)
- Next Round button enable after all panels shown
- Score calculation and updates
- Mobile auto-advance after desktop host

**Verification Checklist:**
- [ ] All 4 panels display correctly
- [ ] Panel transitions smooth
- [ ] Correct answer panel shows right players
- [ ] Robot panel splits players correctly
- [ ] Points calculations accurate
- [ ] Standings sorted by score
- [ ] Next Round button works
- [ ] Mobile follows desktop timing
- [ ] Transitions to next QuestionScreen or FinalResults

---

### Phase 6: FinalResults Scene
**Goal:** Build the final winner/loser display

**Section 6.1 - Basic Elements (Desktop):**
- Background
- Winner spotlight (left side)
- Last place spotlight (right side)
- New Game button
- Credits button

**Section 6.2 - Basic Elements (Mobile):**
- Player placement display
- Final score
- Player icon
- Share Results button
- New Games button

**Section 6.3 - Complex Mechanisms:**
- Winner/loser determination
- Spotlight animations
- Confetti or celebration effects (if any)
- Button navigation

**Section 6.4 - Logic & State Management:**
- Final score sorting
- Single player handling (winner only)
- Reset game state for New Game
- Credits screen transition (if implemented)

**Verification Checklist:**
- [ ] Winner displays correctly
- [ ] Last place displays correctly
- [ ] Single player shows winner only
- [ ] Scores accurate
- [ ] New Game button resets and returns to start
- [ ] Share/Games buttons work on mobile

---

## Implementation Order Summary

1. **Phase 0** - Foundation (1 session)
2. **Phase 1** - LandingPage (1 session)
3. **Phase 2** - QuestionScreen (3-4 sessions, largest scene)
4. **Phase 3** - EliminationScreen (2 sessions)
5. **Phase 4** - VotingScreen (1-2 sessions, similar to Elimination)
6. **Phase 5** - ResultsScreen (2-3 sessions, complex multi-panel)
7. **Phase 6** - FinalResults (1 session)

**Total Estimated Sessions:** 11-15 coding sessions

---

## Notes for Implementation

- Each phase should be completed and tested before moving to the next
- Use Unity's prefab system for reusable UI components
- Implement responsive design using Canvas Scaler (Screen Space - Canvas)
- Use DOTween for all animations
- Maintain separation between UI, logic, and data layers
- Test on both "desktop" (Game view at 1920x1080) and "mobile" (375x812) resolutions
- Audio files should be organized in Resources folder or assigned via Inspector
- Use TextMeshPro for all text elements
- Implement object pooling for dynamic UI elements (player icons, answer buttons)

---

## Current Status: Phase 0 - Ready to Start

**Next Action:** Begin Phase 0 - Foundation & Global Setup
