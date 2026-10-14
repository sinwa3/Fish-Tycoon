using System.Collections;
using UnityEngine;

namespace PixelInventoryDiaryUI
{
    /// <summary>
    /// Plays several <see cref="UITweener"/>s one after another with a small gap,
    /// for a "pop in one by one" list entrance (the cute-diary staple).
    ///
    /// Setup:
    ///  - Put this on the list/container object.
    ///  - Put a UITweener on each item (Scale 0.85→1 + Fade, ease OutBack works
    ///    great) with <b>Play On Enable OFF</b> — this component drives them.
    ///  - Leave Targets empty to auto-collect the UITweeners on the children.
    ///
    /// It works by giving each tweener its own <see cref="UITweener.Delay"/>
    /// (start + index × stagger) and playing them together, so every item snaps to
    /// its "from" pose immediately and then tweens in on cue — no layout jitter.
    /// </summary>
    [AddComponentMenu("Pixel Inventory Diary UI/UI Tween Stagger")]
    [DisallowMultipleComponent]
    public class UITweenStagger : MonoBehaviour
    {
        [Tooltip("Tweeners to play, in order. Leave empty to auto-collect the UITweeners on children.")]
        [SerializeField] private UITweener[] targets = new UITweener[0];

        [Tooltip("When Targets is empty, collect UITweeners from children automatically.")]
        [SerializeField] private bool autoCollectChildren = true;

        [Tooltip("Include inactive children when auto-collecting.")]
        [SerializeField] private bool includeInactive = true;

        [Header("Timing")]
        [Tooltip("Delay before the first item starts, in seconds.")]
        [Min(0f)] [SerializeField] private float startDelay = 0f;

        [Tooltip("Gap between consecutive items, in seconds.")]
        [Min(0f)] [SerializeField] private float stagger = 0.05f;

        [Tooltip("Play from the last child to the first.")]
        [SerializeField] private bool reverseOrder = false;

        [Header("Behaviour")]
        [Tooltip("Play automatically when this object is enabled (entrance).")]
        [SerializeField] private bool playOnEnable = true;

        private void Reset()
        {
            Collect(force: true);
        }

        private void OnEnable()
        {
            if (playOnEnable)
                StartCoroutine(PlayDeferred());
        }

        // Wait one frame so every child is fully enabled before we drive them. On
        // SetActive(true) (tab switch) the child OnEnable order isn't guaranteed, so
        // playing immediately can hit items that aren't ready yet — which is why it
        // worked on scene start (all OnEnables at once) but not on tab re-entry.
        private IEnumerator PlayDeferred()
        {
            yield return null;
            Play();
        }

        /// <summary>Plays every target in sequence, spaced by the stagger interval.</summary>
        [ContextMenu("Play")]
        public void Play()
        {
            Collect(force: false);
            if (targets == null)
                return;

            int n = targets.Length;
            for (int order = 0; order < n; order++)
            {
                int index = reverseOrder ? n - 1 - order : order;
                UITweener tw = targets[index];
                if (tw == null)
                    continue;

                tw.Delay = startDelay + order * stagger;
                tw.Play();
            }
        }

        private void Collect(bool force)
        {
            if (!autoCollectChildren)
                return;
            if (force || targets == null || targets.Length == 0)
                targets = GetComponentsInChildren<UITweener>(includeInactive);
        }
    }
}
