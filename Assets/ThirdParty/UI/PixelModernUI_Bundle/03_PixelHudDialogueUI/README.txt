================================================================
 🎮 PIXEL HUD & DIALOGUE UI
 by IndigoLay
================================================================

A cozy pixel-art HUD and dialogue UI kit for RPG, farming, and
life-sim games. Everything is hand-drawn pixel art - no AI.

Eleven ready-to-use prefabs cover a full conversation flow:
in-game HUD, dialogue box with branching choices, chat-style
history log, quest tracker, pause menu, save/load screen, four
popup types, and a component showcase panel.
A TextMeshPro font asset is included, and every sprite is imported
with pixel-perfect settings out of the box.


----------------------------------------------------------------
 ✨ FEATURES
----------------------------------------------------------------

- 🎨 100 hand-drawn pixel-art sprites
- 🧩 11 ready-to-use prefabs - drag one onto a Canvas and it works
- 🎮 Demo scene showing every screen assembled
- 🎬 Dialogue open animation included (Animator, no scripts)
- ⚙️ No C# scripts - built entirely from Unity's built-in UI
  components, so nothing conflicts with your codebase
- 📐 Works in Built-in, URP and HDRP - plain uGUI + TextMeshPro
- 🔲 47 sprites pre-configured with 9-slice borders - panels, bars,
  buttons and slots stretch to any size without distortion
- 🔍 Pixel-perfect import settings - Point filter, mipmaps off,
  no compression on the default platform
- 🔤 TextMeshPro SDF font asset bundled - crisp pixel text
- 🔘 Multi-state artwork for buttons, tabs, slots and quest items
  (Normal / Hover / Selected / Dim)


----------------------------------------------------------------
 📦 PACKAGE CONTENTS
----------------------------------------------------------------

  Sprites/
    Sample Icons ................. 11   items, tools, portraits
    UI Elements/
      Background ................. 4    backdrops, shadow overlay
      Badge ...................... 4    pins, red dot
      Buttons .................... 15   choice, tab, pause, close
      Controls ................... 4    checkbox, scrollbar
      Deco ....................... 6    lines, leaves, ornament
      Info Display ............... 14   clock, calendar, weather
      Lists ...................... 5    log messages, quest items
      Panel ...................... 17   dialogue, popup, quest, log
      Progress ................... 5    bar + 4 fill colors
      Slot ....................... 4    normal, selected, dim
      UI Icons ................... 11   chat, settings, skip, time
                                  ---
                                   100 sprites total

  Prefabs/ ....................... 11 prefabs
  Demo/ .......................... Demo.unity
  Animation/ ..................... Dialogue open animation
  Font/ .......................... Luckiest Guy + TMP asset + license
  Preview/ ....................... 11 preview images


----------------------------------------------------------------
 💻 REQUIREMENTS
----------------------------------------------------------------

  Created with Unity 6000.3.7. Unity 6 (6000.x) or later
  recommended.

  REQUIRED
    - TextMeshPro (included with Unity)

  💡 DEMO SCENE
    Built with URP + Input System. Legacy Input Manager? Select
    EventSystem in the demo scene -> remove "Input System UI Input
    Module" -> add "Standalone Input Module".


----------------------------------------------------------------
 🚀 QUICK START
----------------------------------------------------------------

  1. Open PixelHudDialogueUI/Demo/Demo.unity to see every screen
     assembled and laid out.

  2. To use in your own scene:

     1) Create a Canvas:  GameObject > UI > Canvas
     2) Set Canvas Scaler to "Scale With Screen Size" with a
        reference resolution of 1920 x 1080
     3) Drag any prefab from PixelHudDialogueUI/Prefabs/ in as a
        child of the Canvas

  3. Wire up the buttons:

     Every button is a standard Unity Button. Hook your own logic
     into the OnClick list, or - with no code at all - drag a
     target object into OnClick and choose
     GameObject > SetActive to open and close panels.


----------------------------------------------------------------
 📋 PREFABS
----------------------------------------------------------------

  HUD .................. Status bar, currency counters, quest
                         tracker, clock, date, weather, location
                         plate, quick slots, shortcut buttons

  Dialogue ............. Dialogue box with nameplate, branching
                         choice buttons, Skip / Next controls.
                         Includes the open animation.

  History_Log .......... Chat-style conversation log with left and
                         right speech bubbles, round portraits and
                         date separators

  Quest ................ Quest list with tabs, scroll view and
                         on/off quest entries

  Pause ................ Pause menu panel with menu buttons

  Save_Load ............ Save / load screen with slot list and
                         scroll view

  Pop_Base ............. Base popup frame - start here for your
                         own popup types

  Pop_Notice ........... Single-button notice popup

  Pop_Confirmation ..... Confirm / cancel popup

  Pop_ItemObtained ..... Item reward popup with icon slot

  Components ........... Showcase panel laying out the UI parts in
                         one place - handy as a visual reference
                         while you build your own screens


----------------------------------------------------------------
 🛠 CUSTOMIZATION
----------------------------------------------------------------

  🎨 RECOLOR INSTANTLY
    Most sprites are drawn in neutral tones. Tint the Image.color
    field to recolor a panel, bar or button without editing any
    PNG. The four progress fill colors are provided as separate
    sprites if you prefer swapping instead of tinting.

  📐 RESIZE FREELY
    47 sprites already have 9-slice borders configured, so panels,
    list rows, bars and buttons keep their corners crisp at any
    size. Just drag the RectTransform.

  🔘 SWAP BUTTON STATES
    Buttons use Sprite Swap transitions. Replace the Normal /
    Highlighted / Pressed / Disabled sprites in the Button
    component to restyle a button set.

  🔍 PIXEL-PERFECT SCALING
    This art is pixel art - scale it by whole numbers (1x, 2x, 3x)
    to keep pixels sharp. Avoid fractional scaling.


----------------------------------------------------------------
 🎥 ANIMATION
----------------------------------------------------------------

  The Dialogue prefab ships with an open animation driven by an
  Animator - the box slides in while the text and choice buttons
  fade up. No scripts and no external tween libraries are used.

  To trigger it from your own code or from a button, call
  Play or SetTrigger on the Animator, or simply enable the object.

  To build your own: the clip animates RectTransform
  m_AnchoredPosition and the alpha of Image and TextMeshPro
  components - the same approach works for any of the popups.


----------------------------------------------------------------
 🔤 FONT LICENSE
----------------------------------------------------------------

  Font:     Luckiest Guy
  Designer: Brian J. Bonislawsky DBA Astigmatic (AOETI)
  License:  Apache License, Version 2.0
  Source:   https://fonts.google.com/specimen/Luckiest+Guy

  The font is redistributed without modification. The full license
  text is included at Font/LICENSE_LuckiestGuy.txt.

  You may replace the bundled TextMeshPro font asset with your own
  at any time - every text object uses a standard TMP font asset
  reference.


----------------------------------------------------------------
 🧩 PIXEL UI SERIES
----------------------------------------------------------------

  This pack is part of a pixel UI series sharing the same art
  style and palette. They are designed to sit side by side in one
  project - the HUD in this pack even includes shortcut buttons
  for phone and diary screens.

    - 📱 Pixel Pocket: Phone & Social Media UI Pack
    - 🎒 Pixel Inventory & Diary UI
    - 🃏 Pixel Card UI
    - 🗺️ Pixel Adventure Book UI
    - ⚔️ Pixel RPG UI Mega Pack (4-in-1 bundle)

  Each pack is sold separately. Search for "IndigoLay" to see the
  full series.


----------------------------------------------------------------
 📞 SUPPORT
----------------------------------------------------------------

  Questions, bug reports and feature requests are welcome.

    Email:  support@indigolay.com

  You can also reach out through the Asset Store publisher page.

  If you enjoy this pack, a review helps a lot - thank you!


================================================================
 🚫 All artwork is 100% hand-made pixel art. No AI-generated assets.
================================================================