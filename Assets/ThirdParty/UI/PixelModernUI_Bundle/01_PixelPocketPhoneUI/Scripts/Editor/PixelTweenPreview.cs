using UnityEditor;
using UnityEngine;

namespace PixelPocketPhoneUI.Tweening.EditorTools
{
    /// <summary>
    /// Drives a single tween component in edit mode by hooking EditorApplication.update.
    /// Used by <see cref="PixelTweenComponentEditor"/>'s Play Preview button.
    /// </summary>
    public static class PixelTweenPreview
    {
        public static PixelTweenComponent Current { get; private set; }

        private static double _startTime;
        private static bool _playable;

        public static void Play(PixelTweenComponent component)
        {
            Stop();
            if (component == null) return;

            Current = component;
            _playable = component.EditorBeginPreview();
            _startTime = EditorApplication.timeSinceStartup;

            if (!_playable)
            {
                Current = null;
                return;
            }

            EditorApplication.update += Tick;
        }

        public static void Stop()
        {
            if (Current == null) return;
            EditorApplication.update -= Tick;
            Current = null;
        }

        private static void Tick()
        {
            if (Current == null)
            {
                EditorApplication.update -= Tick;
                return;
            }

            float duration = Mathf.Max(0.0001f, Current.duration);
            double elapsed = EditorApplication.timeSinceStartup - _startTime - Mathf.Max(0f, Current.delay);

            if (elapsed < 0d)
                return; // still inside the start delay

            bool loops = Current.loop != PixelLoopType.None;
            float raw = (float)elapsed / duration;
            float normalized;

            if (loops)
            {
                float cycle = raw % 1f;
                normalized = (Current.loop == PixelLoopType.PingPong && ((int)raw % 2) == 1)
                    ? 1f - cycle
                    : cycle;
            }
            else
            {
                normalized = Mathf.Clamp01(raw);
            }

            Current.EditorApplyNormalized(normalized);
            Repaint();

            if (!loops && raw >= 1f)
                Stop();
        }

        private static void Repaint()
        {
            SceneView.RepaintAll();
            // Refresh the Game view too so Canvas changes are visible.
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}
