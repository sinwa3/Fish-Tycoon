using UnityEngine;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Moves a UI element by animating its RectTransform.anchoredPosition.
    /// Default behaviour is a "slide in": leave the object at its designed
    /// position and it animates in from an offset (200px above by default).
    /// Use it for slide-in panels, drop-down notification banners and screen swaps.
    /// </summary>
    [AddComponentMenu("Pixel Pocket UI/Tween/Pixel Move Tween")]
    [RequireComponent(typeof(RectTransform))]
    public sealed class PixelMoveTween : PixelTweenComponent
    {
        [Header("End position")]
        [Tooltip("End at the object's current (designed) position. Recommended for slide-ins.")]
        public bool toCurrent = true;
        [Tooltip("Explicit end position. Used only when 'To Current' is OFF.")]
        public Vector2 to = Vector2.zero;

        [Header("Start position")]
        [Tooltip("Where the element starts from.")]
        public Vector2 from = new Vector2(0f, 200f);
        [Tooltip("Treat 'From' as an OFFSET from the end position (e.g. (0,200) = slide down from 200px above). " +
                 "Turn OFF to use 'From' as an absolute anchored position.")]
        public bool fromIsOffset = true;

        private RectTransform _rt;
        private Vector2 _start, _end;

        protected override bool CaptureTargets()
        {
            _rt = (RectTransform)transform;
            _end = toCurrent ? _rt.anchoredPosition : to;
            _start = fromIsOffset ? _end + from : from;
            return true;
        }

        protected override void ApplyProgress(float t)
        {
            if (_rt == null) return;
            _rt.anchoredPosition = Vector2.LerpUnclamped(_start, _end, t);
        }
    }
}
