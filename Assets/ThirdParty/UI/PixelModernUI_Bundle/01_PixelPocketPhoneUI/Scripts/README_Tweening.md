# Pixel Pocket — Tween Animation System

A tiny, self-contained tween engine for the Pixel Pocket Phone UI.

- **No external dependencies.** Pure C# + Unity. No DOTween, no packages to import.
- **No setup.** A hidden runner is created automatically the first time a tween plays.
- **Two ways to use it:** drop-in Inspector components *or* one-line code calls.
- **UI-friendly defaults:** unscaled time (keeps animating while the game is paused).

Namespace: `PixelPocketPhoneUI.Tweening`

---

## 1. Inspector components (no code)

Add one of these to any UI GameObject (`Add Component ▸ Pixel Pocket UI ▸ Tween`):

| Component | What it animates | Typical use |
|-----------|------------------|-------------|
| **Fade Tween** | CanvasGroup alpha | popups, alerts, screen fades |
| **Scale Tween** | localScale | pop-in, button press, heartbeat |
| **Move Tween** | RectTransform anchoredPosition | slide-in panels, notification banners |
| **Rotate Tween** | euler angles | loading spinner, icon spin |
| **Color Tween** | Image / Text color | highlight flash, state change |
| **Shake Tween** | position (decaying) | error feedback, call buzz |

Shared settings on every component:

- **Play On** — `OnEnable` (default), `OnStart`, or `Manual`
- **Delay / Duration** — seconds
- **Ease** — 20+ presets, or `Custom` to use an AnimationCurve
- **Loop** — `None`, `Restart`, or `PingPong` (+ loop count, -1 = forever)
- **Use Unscaled Time** — keep animating when `Time.timeScale = 0`
- **Play Reversed** — run end → start
- **On Complete** — UnityEvent fired when it finishes

> Tip: a **Scale Tween** with `From = 0.6`, `To = 1`, ease `OutBack` gives the classic
> popup "pop". A **Rotate Tween** with `To = (0,0,-360)`, ease `Linear`,
> loop `Restart` is a loading spinner.

### Triggering manually
Set **Play On = Manual**, then call `Play()` from a Button's OnClick, a UnityEvent,
or code. `Stop()` and `TogglePause()` are also available.

---

## 2. From code

Extension methods return a handle you can configure fluently:

```csharp
using PixelPocketPhoneUI.Tweening;

// Fade a popup in
canvasGroup.TweenFade(1f, 0.25f).SetEase(PixelEaseType.OutCubic);

// Pop a panel in with a bounce
panel.transform.TweenScale(Vector3.one, 0.35f).SetEase(PixelEaseType.OutBack);

// Slide a banner down, then fire a callback
banner.TweenAnchoredMove(Vector2.zero, 0.3f)
      .SetDelay(0.1f)
      .OnComplete(() => Debug.Log("arrived"));

// Looping spinner
icon.TweenRotate(new Vector3(0, 0, -360f), 1f)
    .SetEase(PixelEaseType.Linear)
    .SetLoop(PixelLoopType.Restart);

// Flash a label
label.TweenColor(Color.red, 0.2f).SetLoop(PixelLoopType.PingPong, 2);
```

Available extensions:
`TweenFade`, `TweenScale`, `TweenMove`, `TweenLocalMove`, `TweenRotate`,
`TweenAnchoredMove`, `TweenColor`, `TweenAlpha`, `TweenFillAmount`.

### Fully generic tween
For anything not covered, drive a value yourself:

```csharp
PixelTween.To(t => myValue = Mathf.LerpUnclamped(0f, 100f, t), 0.5f)
          .SetEase(PixelEaseType.InOutQuad);
```

### Handle control
```csharp
var tween = cg.TweenFade(0f, 0.4f);
tween.Pause();
tween.Resume();
tween.Complete();   // snap to end + fire OnComplete
tween.Kill();       // stop now

PixelTweenRunner.KillAll();   // stop every tween
```

---

## Preset components

Ready-made effects built on the tween system — just add and go:

- **Pixel Heart Burst** — little hearts float up and fade out (SNS "like").
  Triggers: Toggle ON, Play On Enable, Repeat (timer), or call `Burst()`.
- **Pixel Star Burst** — fireworks: stars shoot out in all directions, spin and
  fall (gravity), then fade. Great for rewards / level-ups / celebrations.
  Same trigger options as Heart Burst.

Both support: a color gradient (2+ colors = random blend), per-particle color
variation, size range, and a Spawn Parent override (leave empty to spawn on the
object itself; set a top-level Canvas to avoid ScrollRect mask clipping).

---

## 3. Editor tools

**Inspector preview** — every tween component has a **▶ Play Preview** button.
It animates the object directly in the editor (no Play mode). Press again to stop.
The object ends at the *To* values; use Ctrl+Z to revert.

**Menu helpers** — `Tools ▸ Pixel Pocket UI`:
- **Apply Sample Animations to Open Scene** — cascades a pop-in over app icons,
  adds fade+pop to popups, fades in screens.
- **Remove All Tween Components in Open Scene** — clean slate.
- **Selection ▸ Add Fade-In / Scale Pop-In / Shake / Cascade Pop-In to Children**
  — apply effects to whatever you've selected. All Undo-friendly.

---

## Notes
- Tweens auto-cancel if their target object is destroyed — no null-ref errors.
- `LerpUnclamped` is used everywhere, so overshoot eases (Back / Elastic) work correctly.
- The runner lives on a hidden `[PixelTweenRunner]` object with `DontDestroyOnLoad`.
