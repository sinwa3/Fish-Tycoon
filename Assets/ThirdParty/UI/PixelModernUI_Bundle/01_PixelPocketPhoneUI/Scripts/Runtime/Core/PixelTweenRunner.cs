using System.Collections.Generic;
using UnityEngine;

namespace PixelPocketPhoneUI.Tweening
{
    /// <summary>
    /// Hidden MonoBehaviour that ticks every active tween once per frame.
    /// It is created automatically the first time a tween starts, so the user
    /// never has to add anything to the scene. Survives scene loads.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [AddComponentMenu("")] // internal/auto-created — hide from the Add Component menu
    public sealed class PixelTweenRunner : MonoBehaviour
    {
        private static PixelTweenRunner _instance;
        private static bool _quitting;

        // Reset statics when entering Play mode. Without this, projects that
        // DISABLE "Reload Domain" (Enter Play Mode Options) would keep a stale
        // _quitting = true from a previous run, breaking all tweens on the 2nd Play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            _quitting = false;
        }

        private readonly List<PixelTween> _active = new List<PixelTween>(64);
        private readonly List<PixelTween> _pending = new List<PixelTween>(16);

        /// <summary>Returns the runner, creating it on demand. Null only while quitting.</summary>
        public static PixelTweenRunner Ensure()
        {
            if (_instance != null) return _instance;
            if (_quitting) return null;

            var go = new GameObject("[PixelTweenRunner]")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _instance = go.AddComponent<PixelTweenRunner>();
            if (Application.isPlaying) DontDestroyOnLoad(go);
            return _instance;
        }

        internal void Register(PixelTween tween)
        {
            // Avoid double-registration if a handle is restarted while still queued.
            if (_pending.Contains(tween) || _active.Contains(tween)) return;
            _pending.Add(tween);
        }

        /// <summary>Stops and clears every running tween.</summary>
        public static void KillAll(bool invokeComplete = false)
        {
            if (_instance == null) return;
            foreach (var t in _instance._active) t.Kill(invokeComplete);
            foreach (var t in _instance._pending) t.Kill(invokeComplete);
            _instance._active.Clear();
            _instance._pending.Clear();
        }

        private void Update()
        {
            // Clamp dt so a huge first frame (Play-mode entry / returning from
            // a stall) can't fast-forward a whole tween in a single step.
            float sdt = Mathf.Min(Time.deltaTime, Time.maximumDeltaTime);
            float udt = Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime);

            // Tick the already-active tweens first, then promote newcomers.
            // This makes a tween's first real step happen on the frame AFTER it
            // was registered, skipping the spiky frame it started on.
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var t = _active[i];
                bool alive = t.Tick(sdt, udt);
                if (!alive)
                {
                    int last = _active.Count - 1;
                    _active[i] = _active[last];
                    _active.RemoveAt(last);
                }
            }

            if (_pending.Count > 0)
            {
                _active.AddRange(_pending);
                _pending.Clear();
            }
        }

        private void OnApplicationQuit() => _quitting = true;

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
