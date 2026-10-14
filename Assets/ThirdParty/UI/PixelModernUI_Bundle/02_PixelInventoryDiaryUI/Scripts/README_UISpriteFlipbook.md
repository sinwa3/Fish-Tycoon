# UI Sprite Flipbook

A lightweight, dependency-free component that plays a frame-by-frame sprite
animation on a UGUI `Image`. Built for the diary page-turn effect, but works for
any UI flipbook (loading spinners, animated icons, effects, etc.).

## Requirements
- Unity 2019.4+ (uses UGUI / `UnityEngine.UI`)
- No third-party dependencies

## Quick start
1. In the Hierarchy, create **UI > Image** (it goes under a Canvas).
2. Set the Image's **Source Image** to the first frame (`UI_PageAnim_0`).
3. Add the **Pixel Inventory Diary UI > UI Sprite Flipbook** component
   (Add Component menu).
4. Expand **Frames**, set the size to your frame count, and drag the sprites in
   order (`UI_PageAnim_0` … `UI_PageAnim_19`).
   - Tip: select all frames in the Project window and drag them onto the Frames
     list header to fill it in sorted order at once.
5. Set **Frames Per Second** (12–24 works well) and choose **Loop** on/off.

## Playing the animation
- **Automatically:** enable **Play On Enable**.
- **On a button:** wire the button's `OnClick` to the component's `Play()`
  (or `PlayReverse()` to turn a page backward).
- **From code:**
  ```csharp
  using PixelInventoryDiaryUI;

  public UISpriteFlipbook pageTurn;

  void NextPage()
  {
      pageTurn.OnComplete.AddListener(ShowNextPageContent);
      pageTurn.Play();
  }
  ```

## Inspector reference
| Field | Description |
|-------|-------------|
| Frames | Sprites played in order. |
| Frames Per Second | Playback speed (1–60). |
| Loop | Repeat forever instead of playing once. |
| Play On Enable | Start playing when the component is enabled. |
| Use Unscaled Time | Keep playing while `Time.timeScale = 0` (recommended for UI). |
| Reset To First Frame On Stop | Snap back to frame 0 when playback stops. |
| On Complete | Event fired when a non-looping animation finishes. |

## Public API
| Member | Purpose |
|--------|---------|
| `Play()` / `PlayReverse()` / `Play(bool)` | Start playback forward / backward. |
| `Stop()` | Stop playback. |
| `Pause()` / `Resume()` | Pause on / resume from the current frame. |
| `GoToFrame(int)` | Show a specific frame without playing. |
| `SetFrames(IEnumerable<Sprite>)` | Swap the frame list at runtime (reskin). |
| `OnComplete` | UnityEvent fired when a one-shot animation ends. |
| `IsPlaying`, `CurrentFrame`, `FrameCount` | Read-only state. |
| `FramesPerSecond`, `Loop` | Adjustable at runtime. |

## Performance tip
Pack the frames into a **Sprite Atlas** (Window > 2D > Sprite Atlas) so all
frames share one draw call.
