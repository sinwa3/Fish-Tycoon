using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PixelInventoryDiaryUI
{
    /// <summary>Fired when the visible page changes, carrying the new page index.</summary>
    [System.Serializable]
    public class PageChangedEvent : UnityEvent<int> { }

    /// <summary>
    /// Drives a multi-page "diary": shows one page at a time and flips between them
    /// with the page-turn animation. Wire <see cref="Next"/> / <see cref="Previous"/>
    /// to your buttons.
    ///
    /// The transition cross-fades the page content: the current page fades OUT while
    /// the turn animation plays, then — once the turn finishes and the flipbook is
    /// hidden — the new page fades IN. Add more pages just by dropping their root
    /// objects into the Pages list; no code changes needed.
    ///
    /// Each page gets a CanvasGroup (added automatically) so it can fade without
    /// disturbing its own layout or children.
    /// </summary>
    [AddComponentMenu("Pixel Inventory Diary UI/Diary Pager")]
    [DisallowMultipleComponent]
    public class DiaryPager : MonoBehaviour
    {
        [Header("Pages")]
        [Tooltip("Page root objects, in order. Only the current page is left active.")]
        [SerializeField] private GameObject[] pages = new GameObject[0];

        [Tooltip("Page shown first (clamped to the page count).")]
        [SerializeField] private int startIndex = 0;

        [Tooltip("Wrap from the last page back to the first (and vice-versa).")]
        [SerializeField] private bool loop = false;

        [Header("Page-turn animation")]
        [Tooltip("Flipbook that plays the page-turn frames. Optional — without it, the cross-fade still runs.")]
        [SerializeField] private UISpriteFlipbook flipbook;

        [Tooltip("Bring the flipbook to the front of its parent while turning, so it covers same-parent siblings.")]
        [SerializeField] private bool flipbookToFrontWhileTurning = true;

        [Tooltip("Keep the flipbook hidden except while turning, so it never covers the page at rest.")]
        [SerializeField] private bool hideFlipbookWhenIdle = true;

        [Header("Cross-fade")]
        [Tooltip("Fade duration for hiding the old page / showing the new page, in seconds. 0 = instant.")]
        [Min(0f)] [SerializeField] private float fadeDuration = 0.25f;

        [Header("Routing")]
        [Tooltip("Optional shared router. If empty, one is found automatically. Lets shared Prev/Next buttons drive whichever pager is currently active.")]
        [SerializeField] private PagerRouter router;

        [Header("Events")]
        [SerializeField] private PageChangedEvent onPageChanged = new PageChangedEvent();

        private int current;
        private bool turning;

        /// <summary>Index of the currently visible page.</summary>
        public int CurrentIndex => current;

        /// <summary>Number of pages registered.</summary>
        public int PageCount => pages != null ? pages.Length : 0;

        /// <summary>True while a page turn is in progress (input is ignored meanwhile).</summary>
        public bool IsTurning => turning;

        /// <summary>Fires after the visible page changes, with the new index.</summary>
        public PageChangedEvent OnPageChanged => onPageChanged;

        private void OnEnable()
        {
            // A turn coroutine is killed if this object is deactivated mid-turn (e.g.
            // switching tabs while a page is still flipping). Reset the guard so that
            // re-enabling never leaves the pager stuck — otherwise Next/Previous would
            // early-out on `turning` forever and the pager would appear dead.
            turning = false;
            current = Mathf.Clamp(startIndex, 0, Mathf.Max(0, PageCount - 1));
            ApplyPageVisibility(current);
            if (flipbook != null && hideFlipbookWhenIdle)
                flipbook.gameObject.SetActive(false);

            // Make the shared Prev/Next buttons drive this pager while it's active.
            if (router == null)
                router = Object.FindFirstObjectByType<PagerRouter>();
            if (router != null)
                router.Register(this);
        }

        private void OnDisable()
        {
            if (router != null)
                router.Unregister(this);
        }

        /// <summary>Turns to the next page (forward flip).</summary>
        public void Next() => Go(current + 1, reverse: false);

        /// <summary>Turns to the previous page (reverse flip).</summary>
        public void Previous() => Go(current - 1, reverse: true);

        /// <summary>Jumps to a page, flipping forward if it's ahead or backward if behind.</summary>
        public void GoToPage(int index) => Go(index, reverse: index < current);

        private void Go(int index, bool reverse)
        {
            if (turning || PageCount == 0)
                return;

            index = loop
                ? ((index % PageCount) + PageCount) % PageCount
                : Mathf.Clamp(index, 0, PageCount - 1);

            if (index == current)
                return; // already at an edge; nothing to turn to

            if (isActiveAndEnabled)
                StartCoroutine(TurnRoutine(index, reverse));
            else
                ApplyPageVisibility(index); // can't run a coroutine while disabled
        }

        private IEnumerator TurnRoutine(int target, bool reverse)
        {
            turning = true;
            int prev = current;

            // Start the page-turn animation (brought to front, shown only while turning).
            GameObject fbGo = null;
            if (flipbook != null)
            {
                fbGo = flipbook.gameObject;
                if (hideFlipbookWhenIdle)
                    fbGo.SetActive(true);
                if (flipbookToFrontWhileTurning)
                    flipbook.transform.SetAsLastSibling();

                if (reverse) flipbook.PlayReverse();
                else flipbook.Play();
            }

            // Fade the current page OUT while the turn plays over it.
            if (InRange(prev) && pages[prev] != null)
            {
                CanvasGroup cg = EnsureCanvasGroup(pages[prev]);
                yield return FadeCanvas(cg, cg.alpha, 0f, fadeDuration);
            }

            // Wait out the rest of the turn (the fade already consumed some of it).
            float remainder = TurnDuration() - fadeDuration;
            if (remainder > 0f)
                yield return new WaitForSecondsRealtime(remainder);

            // Turn finished: hide the flipbook so the new page can fade in on top of nothing.
            if (fbGo != null && hideFlipbookWhenIdle)
                fbGo.SetActive(false);

            if (InRange(prev) && pages[prev] != null)
                pages[prev].SetActive(false);
            current = target;

            // Fade the new page IN.
            if (InRange(target) && pages[target] != null)
            {
                GameObject page = pages[target];
                page.SetActive(true);
                CanvasGroup cg = EnsureCanvasGroup(page);
                yield return FadeCanvas(cg, 0f, 1f, fadeDuration);
            }

            onPageChanged.Invoke(current);
            turning = false;
        }

        private float TurnDuration()
        {
            if (flipbook == null || flipbook.FrameCount <= 0)
                return 0f;
            return flipbook.FrameCount / Mathf.Max(1f, flipbook.FramesPerSecond);
        }

        /// <summary>Instantly shows only the given page (used to set the initial state).</summary>
        private void ApplyPageVisibility(int index)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] == null) continue;
                bool active = i == index;
                pages[i].SetActive(active);
                if (active)
                {
                    var cg = pages[i].GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f; // make sure the starting page is fully visible
                }
            }
            current = index;
        }

        private bool InRange(int i) => i >= 0 && i < pages.Length;

        private static CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = go.AddComponent<CanvasGroup>();
            return cg;
        }

        private static IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                cg.alpha = to;
                yield break;
            }

            float t = 0f;
            cg.alpha = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            cg.alpha = to;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            startIndex = Mathf.Max(0, startIndex);
            fadeDuration = Mathf.Max(0f, fadeDuration);
        }
#endif
    }
}
