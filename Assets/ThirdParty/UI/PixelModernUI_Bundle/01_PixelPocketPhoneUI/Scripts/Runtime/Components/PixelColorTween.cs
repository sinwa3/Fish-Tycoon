using UnityEngine;
using UnityEngine.UI;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Tweens the color (or just the alpha) of any UI Graphic — Image, RawImage,
    /// or TextMeshPro text. Handy for highlight flashes and state changes.
    /// </summary>
    [AddComponentMenu("Pixel Pocket UI/Tween/Pixel Color Tween")]
    public sealed class PixelColorTween : PixelTweenComponent
    {
        [Header("Color")]
        public Color from = Color.white;
        public Color to = new Color(1f, 0.85f, 0.3f, 1f);
        [Tooltip("Start from the graphic's current color instead of the 'From' value.")]
        public bool fromCurrent = true;
        [Tooltip("Only animate the alpha channel, keep the existing RGB.")]
        public bool alphaOnly = false;

        private Graphic _g;
        private Color _start, _end;

        protected override bool CaptureTargets()
        {
            _g = GetComponent<Graphic>();
            if (_g == null)
            {
                Debug.LogWarning("[PixelColorTween] No Graphic (Image / Text) found on this object.", this);
                return false;
            }
            _start = fromCurrent ? _g.color : from;
            _end = to;
            if (alphaOnly)
            {
                _end = _start;
                _end.a = to.a;
            }
            return true;
        }

        protected override void ApplyProgress(float t)
        {
            if (_g == null) return;
            _g.color = Color.LerpUnclamped(_start, _end, t);
        }
    }
}
