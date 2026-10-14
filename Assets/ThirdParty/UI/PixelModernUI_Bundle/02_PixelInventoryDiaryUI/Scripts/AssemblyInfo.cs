using System.Runtime.CompilerServices;

// Lets the editor assembly (PixelInventoryDiaryUI.Editor) reach the internal
// Edit-mode preview hooks on UITweener without exposing them as public runtime API.
[assembly: InternalsVisibleTo("PixelInventoryDiaryUI.Editor")]
