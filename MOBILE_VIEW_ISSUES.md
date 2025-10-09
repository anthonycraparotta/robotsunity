# Mobile Scene Audit

This report summarises a quick scan of the "mobile" layouts embedded in each Unity scene.

## Global issues
* Every scene's root `Canvas` `RectTransform` is scaled to `(0,0,0)`, which collapses the entire UI hierarchy (desktop and mobile) unless a script repairs it at runtime. Examples appear in `QuestionScreen`, `BonusIntroScreen`, and `LobbyScreen`.【F:Assets/Scenes/QuestionScreen.unity†L1734-L1763】【F:Assets/Scenes/BonusIntroScreen.unity†L840-L860】【F:Assets/Scenes/LobbyScreen.unity†L936-L958】

## Scene-by-scene findings

| Scene | Mobile layout snapshot | Notes / issues |
| --- | --- | --- |
| BonusIntroScreen | `MobileDisplay` wraps a full-screen `MobileBackground` image for the intro copy.【F:Assets/Scenes/BonusIntroScreen.unity†L320-L357】【F:Assets/Scenes/BonusIntroScreen.unity†L400-L431】 | Only the global zero-scale Canvas issue detected. |
| BonusQuestionScreen | Mobile view contains a stretched background plus the bonus answer list container.【F:Assets/Scenes/BonusQuestionScreen.unity†L560-L707】【F:Assets/Scenes/BonusQuestionScreen.unity†L830-L870】 | Canvas still scaled to zero; otherwise layout wiring looks intact. |
| BonusResultsScreen | Mobile layout combines `MobileBackground` and a vertical results container.【F:Assets/Scenes/BonusResultsScreen.unity†L480-L535】 | No extra mobile-specific problems spotted beyond the zero-scale Canvas. |
| CreditsScreen | Mobile canvas includes a background and credits scroller container.【F:Assets/Scenes/CreditsScreen.unity†L320-L354】【F:Assets/Scenes/CreditsScreen.unity†L840-L868】 | Zero-scale Canvas remains; otherwise mobile layout mirrors desktop content. |
| EliminationScreen | Mobile view exposes background plus elimination list container.【F:Assets/Scenes/EliminationScreen.unity†L120-L158】【F:Assets/Scenes/EliminationScreen.unity†L470-L499】 | No additional issues besides zero-scale Canvas. |
| FinalResults | Mobile layout includes background plus winner/loser stat containers.【F:Assets/Scenes/FinalResults.unity†L572-L603】【F:Assets/Scenes/FinalResults.unity†L1114-L1143】 | Only the global zero-scale Canvas issue observed. |
| GameTerminatedScreen | Mobile display carries a background and CTA stack.【F:Assets/Scenes/GameTerminatedScreen.unity†L824-L852】【F:Assets/Scenes/GameTerminatedScreen.unity†L900-L928】 | Zero-scale Canvas persists; no other mobile anomalies. |
| HalftimeResultsScreen | Mobile view has a background and results container for the halftime leaderboard.【F:Assets/Scenes/HalftimeResultsScreen.unity†L1184-L1238】【F:Assets/Scenes/HalftimeResultsScreen.unity†L1926-L1955】 | No extra problems aside from the global Canvas scaling. |
| IntroVideoScreen | The screen controller has `mobileDisplay` and `mobileBackground` references left `null`, so the scene offers no dedicated mobile layout.【F:Assets/Scenes/IntroVideoScreen.unity†L640-L659】 | Add a mobile canvas (or disable the mobile branch) so handheld clients see something other than the desktop layout. |
| LandingScreen | Controller references for `mobileDisplay`, `joinGameButton`, and `mobileBackground` are unassigned, so the mobile UI never renders.【F:Assets/Scenes/LandingScreen.unity†L680-L707】 | Provide a mobile layout or wire up the references so the join screen works on phones. |
| LoadingScreen | Loading screen script leaves `mobileDisplay` and `mobileBackground` empty, so there is no handheld-friendly variant.【F:Assets/Scenes/LoadingScreen.unity†L340-L363】 | Build or link a mobile layout so loading messages appear on phones. |
| LobbyScreen | Mobile view exists, but the background is sized for desktop (1820×980) and the whole display is offset by `(41,6)` pixels, which can crop or misalign the phone layout.【F:Assets/Scenes/LobbyScreen.unity†L752-L778】【F:Assets/Scenes/LobbyScreen.unity†L1232-L1263】 | Re-anchor the mobile canvas at `(0,0)` and swap in portrait-friendly artwork/sizing. |
| PictureQuestionScreen | Mobile layout includes a background, prompt, answer input, and supporting elements.【F:Assets/Scenes/PictureQuestionScreen.unity†L900-L942】【F:Assets/Scenes/PictureQuestionScreen.unity†L1372-L1402】 | No additional mobile-specific issues beyond the zero-scale Canvas. |
| PlayerQuestionVideoScreen | Scene exposes only desktop transforms—no `MobileDisplay` GameObject or mobile references exist.【F:Assets/Scenes/PlayerQuestionVideoScreen.unity†L120-L173】【F:Assets/Scenes/PlayerQuestionVideoScreen.unity†L480-L548】 | Add a mobile canvas if phone users should watch these videos. |
| QuestionScreen | Mobile display hosts a background, text prompt, input, and timer elements.【F:Assets/Scenes/QuestionScreen.unity†L500-L608】【F:Assets/Scenes/QuestionScreen.unity†L3220-L3242】 | Apart from the zero-scale Canvas, the layout wiring looks correct. |
| ResultsScreen | Mobile layout mirrors the results summary with its own background and score rows.【F:Assets/Scenes/ResultsScreen.unity†L3232-L3264】【F:Assets/Scenes/ResultsScreen.unity†L2868-L2896】 | No extra mobile-specific concerns besides the global Canvas scaling. |
| RoundArtScreen | Mobile view includes a full background plus an extra `MobileBackgroundContainer` limited to 100×100, likely shrinking whatever should sit inside it.【F:Assets/Scenes/RoundArtScreen.unity†L200-L228】【F:Assets/Scenes/RoundArtScreen.unity†L1740-L1778】 | Resize `MobileBackgroundContainer` (or remove it) so mobile art fills the screen. |
| VotingScreen | Mobile layout offers a background, vertical vote list, and submit button.【F:Assets/Scenes/VotingScreen.unity†L1636-L1665】【F:Assets/Scenes/VotingScreen.unity†L1212-L1242】 | Zero-scale Canvas is the only issue spotted. |
| WinnerLoserScreen | Mobile display contains background and win/lose messaging container.【F:Assets/Scenes/WinnerLoserScreen.unity†L432-L467】【F:Assets/Scenes/WinnerLoserScreen.unity†L539-L567】 | Only the global zero-scale Canvas problem noted. |

