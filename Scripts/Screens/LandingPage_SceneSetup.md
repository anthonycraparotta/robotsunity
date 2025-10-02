# LandingPage Scene Setup Guide

This guide provides step-by-step instructions for setting up the LandingPage scene in Unity Editor.

---

## Scene Hierarchy Structure

```
LandingPage (Scene)
├── Canvas
│   ├── ResponsiveUI (Component)
│   ├── CanvasScaler (Component)
│   ├── GraphicRaycaster (Component)
│   │
│   ├── DesktopContent (GameObject)
│   │   ├── Background (Image)
│   │   ├── VideoPlayerContainer (GameObject)
│   │   │   └── VideoPlayer (Video Player + RawImage)
│   │   ├── StartButton (Button + Image)
│   │   └── InitialFadeOverlay (Image + CanvasGroup)
│   │
│   └── MobileContent (GameObject)
│       └── BlackBackground (Image)
│
├── LandingPageController (GameObject)
│   └── LandingPageController (Component)
│
├── EventSystem (if not already in scene)
│
└── Managers (if first scene)
    ├── GameManager (Optional - auto-creates)
    ├── AudioManager (Optional - auto-creates)
    └── FadeTransition (Optional - auto-creates)
```

---

## Step-by-Step Setup

### 1. Create Canvas

1. **Create Canvas**
   - Right-click in Hierarchy → UI → Canvas
   - Name it "Canvas"

2. **Configure Canvas**
   - Canvas component:
     - Render Mode: Screen Space - Overlay (or Screen Space - Camera)
     - Sort Order: 0

3. **Add CanvasScaler**
   - Should be auto-added
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
   - Match: 0.5

4. **Add ResponsiveUI Component**
   - Add Component → ResponsiveUI
   - This will auto-configure desktop/mobile settings

---

### 2. Create DesktopContent

1. **Create DesktopContent GameObject**
   - Right-click Canvas → Create Empty
   - Name: "DesktopContent"
   - RectTransform:
     - Anchors: Stretch-Stretch (min 0,0 max 1,1)
     - Left: 0, Top: 0, Right: 0, Bottom: 0

#### 2a. Background Image

1. **Create Background**
   - Right-click DesktopContent → UI → Image
   - Name: "Background"
   - RectTransform: Stretch to fill parent (0,0,0,0)
   - Image component:
     - Source Image: landingbg.png (from assets/mainscreenbg/)
     - Color: White (255, 255, 255, 255)
     - Raycast Target: Unchecked

#### 2b. Video Player

1. **Create VideoPlayerContainer**
   - Right-click DesktopContent → Create Empty
   - Name: "VideoPlayerContainer"
   - RectTransform:
     - Anchors: Middle Center
     - Width: 1210 (63vw of 1920px)
     - Height: 680 (auto-sized based on video aspect ratio)
     - Pos X: 0, Pos Y: 0

2. **Add Raw Image for Video**
   - Right-click VideoPlayerContainer → UI → Raw Image
   - Name: "VideoPlayer"
   - RectTransform: Stretch to fill parent
   - Raw Image component:
     - Color: White

3. **Add Video Player Component**
   - Select VideoPlayer GameObject
   - Add Component → Video Player
   - Video Player settings:
     - Source: Video Clip
     - Video Clip: rulesvideo.mp4 (from assets/Video/)
     - Play On Awake: True
     - Loop: True
     - Playback Speed: 1
     - Render Mode: Render Texture OR Material Override
     - Target Texture: Create new Render Texture (if Render Texture mode)
     - If using Material Override, set Target Material Override to RawImage material
   - **Recommended**: Use Render Texture mode:
     - Create Render Texture: Assets → Create → Render Texture
     - Name it "VideoRenderTexture"
     - Assign to Video Player's Target Texture
     - Assign to Raw Image's Texture

4. **Add Audio Source for Video**
   - Video Player component:
     - Audio Output Mode: Direct
     - Mute: Unchecked

#### 2c. Start Button

1. **Create Start Button**
   - Right-click DesktopContent → UI → Button
   - Name: "StartButton"
   - RectTransform:
     - Anchors: Bottom Center
     - Width: 319 (16.6vw of 1920px)
     - Height: 106 (9.84vh of 1080px)
     - Pos X: 0
     - Pos Y: 106 (5% from bottom + half height)

2. **Configure Button Image**
   - Select StartButton
   - Image component:
     - Source Image: startgamebutton.png (from assets/ui/)
     - Image Type: Simple
     - Preserve Aspect: Checked (optional)
     - Raycast Target: Checked
   - Remove or hide the Text child (we're using image only)

3. **Add Shadow Component** (optional for drop shadow effect)
   - Add Component → Shadow
   - Effect Color: Black with alpha 128
   - Effect Distance: X=4, Y=-4
   - Use Graphic Alpha: Checked

4. **Add ButtonEffects Component**
   - Add Component → ButtonEffects
   - Settings (auto-configured):
     - Play Sound: True
     - Enable Hover Effect: True
     - Enable Press Effect: True
     - Hover Scale: 1.05
     - Press Scale: 0.95

#### 2d. Initial Fade Overlay

1. **Create Initial Fade Overlay**
   - Right-click DesktopContent → UI → Image
   - Name: "InitialFadeOverlay"
   - RectTransform: Stretch to fill parent (0,0,0,0)
   - Image component:
     - Color: Black (0, 0, 0, 255)
     - Raycast Target: Unchecked

2. **Add CanvasGroup**
   - Add Component → CanvasGroup
   - Alpha: 1 (starts fully opaque)
   - Interactable: Unchecked
   - Blocks Raycasts: Unchecked
   - Ignore Parent Groups: Unchecked

---

### 3. Create MobileContent

1. **Create MobileContent GameObject**
   - Right-click Canvas → Create Empty
   - Name: "MobileContent"
   - RectTransform: Stretch to fill (0,0,0,0)

2. **Create Black Background**
   - Right-click MobileContent → UI → Image
   - Name: "BlackBackground"
   - RectTransform: Stretch to fill parent
   - Image component:
     - Color: Black (0, 0, 0, 255)
     - Raycast Target: Unchecked

---

### 4. Add LandingPageController

1. **Create Controller GameObject**
   - Right-click in Hierarchy → Create Empty
   - Name: "LandingPageController"
   - Position: 0, 0, 0

2. **Add LandingPageController Component**
   - Add Component → LandingPageController

3. **Assign References in Inspector**
   - Desktop Content: Drag DesktopContent GameObject
   - Video Player: Drag VideoPlayer GameObject's Video Player component
   - Start Button: Drag StartButton GameObject's Button component
   - Initial Fade Overlay: Drag InitialFadeOverlay GameObject's CanvasGroup component
   - Mobile Content: Drag MobileContent GameObject
   - Next Scene Name: "JoinRoom" (or whatever your next scene is called)

---

### 5. Configure ResponsiveUI

1. **Select Canvas GameObject**
2. **Find ResponsiveUI Component**
3. **Assign Content References**
   - Desktop Content: Drag DesktopContent GameObject
   - Mobile Content: Drag MobileContent GameObject

---

### 6. Setup Managers (If First Scene)

If this is the first scene in your game, you may want to create manager GameObjects:

1. **Create Managers Parent** (optional organization)
   - Right-click Hierarchy → Create Empty
   - Name: "Managers"
   - Mark as DontDestroyOnLoad (or let managers handle this)

2. **GameManager** (optional - auto-creates)
   - Right-click Managers → Create Empty
   - Name: "GameManager"
   - Add Component → GameManager

3. **AudioManager** (optional - auto-creates)
   - Right-click Managers → Create Empty
   - Name: "AudioManager"
   - Add Component → AudioManager
   - Assign audio clips in Inspector:
     - Button Press: button_press.wav
     - Landing Rules: landingrules.wav (or .mp3)
     - etc.

---

## Inspector Settings Reference

### Canvas
- **Canvas**:
  - Render Mode: Screen Space - Overlay
  - Pixel Perfect: Unchecked

- **CanvasScaler**:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1920 x 1080
  - Screen Match Mode: Match Width Or Height
  - Match: 0.5

- **ResponsiveUI**:
  - Desktop Content: DesktopContent
  - Mobile Content: MobileContent

### LandingPageController
- **UI References - Desktop**:
  - Desktop Content: DesktopContent GameObject
  - Video Player: VideoPlayer Video Player component
  - Start Button: StartButton Button component
  - Initial Fade Overlay: InitialFadeOverlay CanvasGroup

- **UI References - Mobile**:
  - Mobile Content: MobileContent GameObject

- **Settings**:
  - Initial Fade Delay: 0.1
  - Initial Fade Duration: 1
  - Exit Fade Delay: 1
  - Exit Fade Duration: 1
  - Next Scene Name: "JoinRoom"

### VideoPlayer Component
- **Source**: Video Clip
- **Video Clip**: rulesvideo.mp4
- **Play On Awake**: True
- **Wait For First Frame**: True
- **Loop**: True
- **Playback Speed**: 1
- **Render Mode**: Render Texture
- **Target Texture**: VideoRenderTexture (created Render Texture asset)
- **Audio Output Mode**: Direct
- **Controlled Tracks**: 1

---

## Asset Requirements

Ensure these assets exist in your project:

### Images
- `assets/mainscreenbg/landingbg.png` - Background image
- `assets/ui/startgamebutton.png` - Start button image

### Video
- `assets/Video/rulesvideo.mp4` - Rules video

### Audio
- `assets/audio/button_press.wav` (or .mp3) - Button click SFX
- `assets/audio/landingrules.wav` (or .mp3) - Voice over for rules

### Fonts
- `assets/fonts/SINK.otf` - Primary font (used for any text)

---

## Testing Checklist

Once setup is complete, test the following:

### Desktop Mode (Game view: 1920x1080)
- [ ] Background image displays correctly
- [ ] Video plays automatically on scene load
- [ ] Video loops continuously
- [ ] Start button appears at bottom center
- [ ] Start button image displays correctly
- [ ] Hover over button scales to 1.05x
- [ ] Click button plays "button_press" sound
- [ ] Click button stops voice over
- [ ] Click button triggers 1-second delay
- [ ] After delay, fade to black occurs (1 second)
- [ ] After fade, scene transitions (or logs message)
- [ ] Enter key triggers same behavior
- [ ] Space key triggers same behavior
- [ ] Initial fade-in happens on scene load

### Mobile Mode (Game view: 375x812)
- [ ] Black screen displays (no video, no button visible on mobile)
- [ ] MobileContent is active
- [ ] DesktopContent is inactive

### Console
- [ ] No errors in console
- [ ] "Platform detected: Desktop/Mobile" message appears
- [ ] Transition message appears when button clicked

---

## Common Issues & Solutions

### Video Not Playing
- Ensure Video Player component has Video Clip assigned
- Check Render Texture is created and assigned
- Verify Raw Image has Render Texture assigned
- Check video file is in correct format (H.264 recommended)

### Button Not Clickable
- Ensure Button component has Raycast Target checked
- Verify GraphicRaycaster is on Canvas
- Check EventSystem exists in scene
- Ensure no other UI is blocking (check Canvas sort order)

### Fade Not Working
- Verify FadeTransition singleton is initialized
- Check DOTween is imported
- Ensure CanvasGroup is on InitialFadeOverlay
- Check Canvas has correct sort order

### Audio Not Playing
- Assign audio clips to AudioManager in Inspector
- Or place clips in Resources/Audio/ folder
- Check AudioListener exists in scene (usually on Main Camera)
- Verify audio files are imported correctly

---

## Next Steps

After LandingPage is complete:
1. Create the JoinRoom/Lobby scene
2. Add scene to Build Settings
3. Test full flow from Landing → Lobby
4. Proceed to Phase 2: QuestionScreen

---

**Setup Complete!** The LandingPage scene is now ready for testing.
