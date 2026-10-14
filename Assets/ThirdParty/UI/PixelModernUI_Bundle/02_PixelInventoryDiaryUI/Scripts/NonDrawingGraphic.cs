using UnityEngine;
using UnityEngine.UI;

namespace PixelInventoryDiaryUI
{
    /// <summary>
    /// An invisible UI graphic that still receives raycasts. Use it as a click
    /// area — e.g. a large transparent button or an expanded hit-box — without the
    /// cost of a transparent Image.
    ///
    /// Unlike an Image with alpha 0 (which still builds a quad, adds overdraw and
    /// can break batching), this generates <b>no geometry at all</b>: zero draw
    /// calls, while "Raycast Target" keeps it clickable.
    ///
    /// Usage: add this to a button's root instead of a transparent Image, and set
    /// it as the Button's Target Graphic. The RectTransform's size is the hit area.
    /// </summary>
    [AddComponentMenu("Pixel Inventory Diary UI/Non-Drawing Graphic")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class NonDrawingGraphic : Graphic
    {
        // Nothing visual is ever rebuilt.
        public override void SetMaterialDirty() { }

        public override void SetVerticesDirty() { }

        // No geometry.
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        // Critically: give the CanvasRenderer NO material, so it never tries to bind a
        // texture. Without this, an active NonDrawingGraphic makes d3d12 log
        // "Attempting to bind a NULL or empty texture" every time it is drawn — on
        // scene start/stop and whenever it is (re)activated (e.g. a shared Prev/Next
        // button showing on a tab change) — even though it has no visible mesh. The
        // base Graphic still registers for raycasts, so clicks keep working.
        protected override void UpdateMaterial()
        {
            if (!IsActive())
                return;

            canvasRenderer.materialCount = 0;
        }
    }
}
