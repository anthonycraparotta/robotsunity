# Unity Scenes Quick Reference

## Scene-by-Scene Component Summary

### LandingScreen.unity
- **GameObjects**: 12
- **Text**: 0 TextMeshProUGUI
- **Images**: 5 Image + 1 RawImage
- **Interactive**: 2 Buttons
- **Special**: VideoPlayer (rules video)
- **Key Objects**: StartTestButton, JoinGameButton, RulesVideo

---

### LobbyScreen.unity
- **GameObjects**: 39
- **Text**: 6 TextMeshProUGUI
- **Images**: 19 Image
- **Interactive**: 5 Buttons + 1 InputField
- **Special**: Icon selection, player list
- **Key Objects**: StartTestButton, 8QButton, 12Qbutton, NameInput, JoinButton, PlayerIconLobby

---

### QuestionScreen.unity
- **GameObjects**: 52
- **Text**: 6 TextMeshProUGUI
- **Images**: 34 Image
- **Interactive**: 1 Button + 1 InputField
- **Special**: 12 round backgrounds, 12 robot images, animated timer, GridLayout
- **Key Objects**: QuestionCard, AnswerInput, AnswerSubmitButton, TimerContainer, PlayerIconQuestion

---

### PictureQuestionScreen.unity
- **GameObjects**: 30
- **Text**: 5 TextMeshProUGUI
- **Images**: 13 Image
- **Interactive**: 1 Button + 1 InputField
- **Special**: Picture display, timer, GridLayout
- **Key Objects**: PictureContainer, AnswerInput, AnswerSubmitButton, PlayerIconPictureQuestion

---

### EliminationScreen.unity
- **GameObjects**: 18
- **Text**: 3 TextMeshProUGUI
- **Images**: 6 Image
- **Interactive**: 1 Button (with Outline)
- **Special**: VerticalLayoutGroup for lists, timer
- **Key Objects**: ElimSubmitButton, AnswerListContainer, ElimListMobile

---

### VotingScreen.unity
- **GameObjects**: 18
- **Text**: 3 TextMeshProUGUI
- **Images**: 6 Image
- **Interactive**: 1 Button (with Outline)
- **Special**: VerticalLayoutGroup for lists, timer
- **Key Objects**: VotingSubmitButton, AnswerListContainer, VoteListMobile

---

### ResultsScreen.unity
- **GameObjects**: 76
- **Text**: 21 TextMeshProUGUI
- **Images**: 33 Image
- **Interactive**: 2 Buttons
- **Special**: 3 panel layouts, Outline effects, player zones
- **Key Objects**: ResultsPanelContainer, PlayerIconZone-Fooled, PlayerIconZone-NotFooled, PlayerRank, Robot Answer, True Answer

---

### RoundArtScreen.unity
- **GameObjects**: 21
- **Text**: 0 TextMeshProUGUI
- **Images**: 15 Image (12 round backgrounds)
- **Interactive**: 1 Button
- **Special**: Round background display
- **Key Objects**: BackgroundContainer, Continue button

---

### HalftimeResultsScreen.unity
- **GameObjects**: 25
- **Text**: 6 TextMeshProUGUI
- **Images**: 10 Image
- **Interactive**: 1 Button
- **Special**: Winner/Loser sections, Outline effects
- **Key Objects**: WinnerSection, LoserSection, PlayerIconZone (x2)

---

### BonusIntroScreen.unity
- **GameObjects**: 9
- **Text**: 0 TextMeshProUGUI
- **Images**: 4 Image
- **Interactive**: 1 Button
- **Special**: Simple intro screen
- **Key Objects**: Continue button

---

### BonusQuestionScreen.unity
- **GameObjects**: 28
- **Text**: 6 TextMeshProUGUI
- **Images**: 11 Image
- **Interactive**: 2 Buttons (with Outline)
- **Special**: GridLayout, VerticalLayoutGroup, timer
- **Key Objects**: BonusQuestionCard, PlayerIconBonus, TimerContainer, answer buttons

---

### FinalResults.unity
- **GameObjects**: 32
- **Text**: 9 TextMeshProUGUI
- **Images**: 13 Image
- **Interactive**: 3 Buttons
- **Special**: Winner/Loser sections, Outline effects, multiple zones
- **Key Objects**: WinnerSection, LoserSection, PlayerIconZone (x3), Continue/Exit buttons

---

### CreditsScreen.unity
- **GameObjects**: 15
- **Text**: 0 TextMeshProUGUI
- **Images**: 7 Image + 2 RawImage
- **Interactive**: 2 Buttons
- **Special**: VideoPlayer, scroll zone
- **Key Objects**: CreditsVideo, Credits Scroll Zone, NewGameButton, WebsiteButton

---

### LoadingScreen.unity
- **GameObjects**: 9
- **Text**: 0 TextMeshProUGUI
- **Images**: 4 Image
- **Interactive**: 0
- **Special**: Simple loading display
- **Key Objects**: GameLogo, Tips, Background

---

### IntroVideoScreen.unity
- **GameObjects**: 6
- **Text**: 0 TextMeshProUGUI
- **Images**: 1 Image + 1 RawImage
- **Interactive**: 0
- **Special**: VideoPlayer
- **Key Objects**: IntroVideo, DesktopBackground

---

## Component Type Quick List

### Text Components
- **TextMeshProUGUI**: 70+ instances across all scenes
- **TMP_InputField**: 3 instances (LobbyScreen, QuestionScreen, PictureQuestionScreen)

### Image Components
- **Image**: 180+ instances
- **RawImage**: 4 instances (video display)
- **Outline**: 10+ instances (visual effects)

### Interactive Components
- **Button**: 25+ instances
- **TMP_InputField**: 3 instances

### Layout Components
- **GridLayoutGroup**: 3+ instances (player icon grids)
- **VerticalLayoutGroup**: 6+ instances (answer/vote lists)
- **LayoutElement**: 4+ instances (text layout control)
- **RectMask2D**: 3+ instances (text masking)

### Media Components
- **VideoPlayer**: 3 instances (IntroVideoScreen, CreditsScreen, LandingScreen)

### Animation Components
- **Animator (Type_95)**: Used in timer animations

### Core Components (Every Scene)
- **Canvas**: 1 per scene
- **CanvasScaler**: 1 per scene
- **GraphicRaycaster**: 1 per scene
- **EventSystem**: 1 per scene
- **StandaloneInputModule**: 1 per scene
- **Camera**: 1 per scene
- **AudioListener**: 1 per scene

---

## Common GameObject Names by Function

### Containers
- **Canvas** - Root UI container (every scene)
- **DesktopDisplay** - Desktop layout container (every scene)
- **MobileDisplay** - Mobile layout container (every scene)
- **PlayerIconContainer** - Player icon grid (multiple scenes)
- **TimerContainer** - Timer display (gameplay scenes)
- **BackgroundContainer** - Background images (QuestionScreen, RoundArtScreen)
- **AnswerListContainer** - Answer list (EliminationScreen, VotingScreen)
- **ResultsPanelContainer** - Results panels (ResultsScreen)

### Backgrounds
- **DesktopBackground** - Desktop background image (most scenes)
- **MobileBackground** - Mobile background image (most scenes)
- **Round1BG through Round12BG** - Round-specific backgrounds (QuestionScreen, RoundArtScreen)

### Buttons
- **StartTestButton** - Start game (LandingScreen, LobbyScreen)
- **JoinGameButton** - Join game (LandingScreen, LobbyScreen)
- **JoinButton** - Submit join (LobbyScreen)
- **8QButton, 12Qbutton** - Question count selection (LobbyScreen)
- **AnswerSubmitButton** - Submit answer (QuestionScreen, PictureQuestionScreen)
- **ElimSubmitButton** - Submit elimination vote (EliminationScreen)
- **VotingSubmitButton** - Submit vote (VotingScreen)
- **NewGameButton** - New game (CreditsScreen)
- **WebsiteButton** - Visit website (CreditsScreen)
- **Continue button** - Navigation (multiple screens)

### Input Fields
- **NameInput** - Player name input (LobbyScreen)
- **AnswerInput** - Answer input (QuestionScreen, PictureQuestionScreen)

### Text Elements
- **TipText** - Tip/instruction text (multiple scenes)
- **Question Text** - Question display (QuestionScreen)
- **PlayerName** - Player name in icon (multiple scenes)
- **TimerCountdown** - Timer number (gameplay scenes)
- **HeadlineText** - Header text (LobbyScreen)
- **Data, DataHeaders** - Data display (LobbyScreen)
- **Text (TMP)** - Generic text (various)

### Player Icons
- **PlayerIconLobby** - Lobby player icon (LobbyScreen)
- **PlayerIconQuestion** - Question screen icon (QuestionScreen)
- **PlayerIconPictureQuestion** - Picture question icon (PictureQuestionScreen)
- **PlayerIconBonus** - Bonus round icon (BonusQuestionScreen)
- **Circle, Namebar, PlayerIcon** - Icon sub-components (all player icons)

### Timer Elements
- **TimerAnimationContainer** - Timer animation (gameplay scenes)
- **Timer Circle, Timer Blob** - Timer visual elements (gameplay scenes)
- **TimerCountdown** - Timer text (gameplay scenes)

### Media Elements
- **RulesVideo** - Rules video (LandingScreen)
- **CreditsVideo** - Credits video (CreditsScreen)
- **IntroVideo** - Intro video (IntroVideoScreen)

### Screens/Sections
- **JoinScreen** - Join interface (LandingScreen, LobbyScreen)
- **JoinForm** - Join form (LobbyScreen)
- **JoinWait** - Waiting screen (LobbyScreen)
- **WinnerSection** - Winner display (HalftimeResultsScreen, FinalResults)
- **LoserSection** - Loser display (HalftimeResultsScreen, FinalResults)
- **QuestionCard** - Question card (QuestionScreen)
- **BonusQuestionCard** - Bonus question card (BonusQuestionScreen)

### Special Objects
- **RobotForegrounds** - Robot images container (QuestionScreen)
- **Robot1 through Robot12** - Individual robots (QuestionScreen)
- **PictureContainer** - Picture display (PictureQuestionScreen)
- **SelectedIconContainer** - Selected icon (LobbyScreen)
- **ScrollingPlayerIconContainer** - Scrolling icons (LobbyScreen)
- **PlayerIconZone** - Player zone (ResultsScreen, HalftimeResultsScreen, FinalResults)
- **PlayerIconZone-Fooled, PlayerIconZone-NotFooled** - Categorized zones (ResultsScreen)
- **Credits Scroll Zone** - Credits scroll area (CreditsScreen)
- **Hero** - Hero image (EliminationScreen)
- **GameLogo** - Game logo (LoadingScreen)
- **Tips** - Tips display (LoadingScreen)

---

## Layout Patterns by Scene Type

### Gameplay Screens (Question, PictureQuestion, BonusQuestion)
- Timer at top
- Question/card display center
- Player icons in grid
- Mobile: answer input + submit button

### Results Screens (Results, HalftimeResults, FinalResults)
- Multiple display panels
- Winner/Loser sections
- Player icon zones
- Extensive text displays
- Continue/navigation buttons

### Voting Screens (Elimination, Voting)
- Timer display
- Vertical layout lists (desktop & mobile)
- Submit button with outline
- Mobile vertical list

### Menu Screens (Landing, Lobby)
- Join functionality
- Multiple buttons
- Icon selection (Lobby)
- Player name input (Lobby)
- Video display (Landing)

### Transition Screens (Loading, RoundArt, BonusIntro, IntroVideo)
- Minimal interaction
- Image/video focus
- Simple navigation

---

## File Paths

### Scene Files
```
C:\Users\User\Robots Wearing Moustaches\Assets\Scenes\
├── LandingScreen.unity
├── LobbyScreen.unity
├── QuestionScreen.unity
├── PictureQuestionScreen.unity
├── EliminationScreen.unity
├── VotingScreen.unity
├── ResultsScreen.unity
├── RoundArtScreen.unity
├── HalftimeResultsScreen.unity
├── BonusIntroScreen.unity
├── BonusQuestionScreen.unity
├── FinalResults.unity
├── CreditsScreen.unity
├── LoadingScreen.unity
└── IntroVideoScreen.unity
```

### Analysis Reports
```
C:\Users\User\Robots Wearing Moustaches\
├── unity_scene_analysis.txt (basic analysis)
├── unity_detailed_scene_report.txt (complete hierarchy)
├── UNITY_SCENES_COMPLETE_ANALYSIS.md (comprehensive report)
└── QUICK_REFERENCE_SCENES.md (this file)
```

---

*Quick reference guide for Unity scene components and GameObjects*
