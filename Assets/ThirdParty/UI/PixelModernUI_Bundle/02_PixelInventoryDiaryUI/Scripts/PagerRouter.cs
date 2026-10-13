using UnityEngine;

namespace PixelInventoryDiaryUI
{
    /// <summary>
    /// Routes shared Prev/Next buttons to whichever <see cref="DiaryPager"/> is
    /// currently active. Each pager registers itself while enabled, so one pair of
    /// buttons can drive different page sets across tabs/screens without being
    /// re-wired.
    ///
    /// Setup:
    ///  - Put this on an always-active object (e.g. the diary root).
    ///  - Wire the shared Prev/Next buttons' OnClick to <see cref="Previous"/> /
    ///    <see cref="Next"/>.
    ///  - Assign Prev/Next Button objects below so the router can auto-show them
    ///    only when the active pager actually has more than one page.
    ///  - Each DiaryPager finds and registers with this router automatically.
    /// </summary>
    [AddComponentMenu("Pixel Inventory Diary UI/Pager Router")]
    [DisallowMultipleComponent]
    public class PagerRouter : MonoBehaviour
    {
        [Tooltip("Optional: the Previous button object. Auto-hidden when the active pager has 0/1 page.")]
        [SerializeField] private GameObject previousButton;

        [Tooltip("Optional: the Next button object. Auto-hidden when the active pager has 0/1 page.")]
        [SerializeField] private GameObject nextButton;

        [Tooltip("Hide the buttons when there is no active pager, or it only has one page.")]
        [SerializeField] private bool autoToggleButtons = true;

        private DiaryPager active;

        /// <summary>The pager the buttons currently drive (may be null).</summary>
        public DiaryPager Active => active;

        // Runs after every pager's OnEnable, so the button state reflects whatever
        // (if anything) registered on start — otherwise the scene's saved state
        // (buttons visible) would linger until the first tab change.
        private void Start() => RefreshButtons();

        /// <summary>Called by a DiaryPager when it becomes active.</summary>
        public void Register(DiaryPager pager)
        {
            active = pager;
            RefreshButtons();
        }

        /// <summary>Called by a DiaryPager when it is disabled. Clears only if it's the current one.</summary>
        public void Unregister(DiaryPager pager)
        {
            if (active == pager)
            {
                active = null;
                RefreshButtons();
            }
        }

        /// <summary>Turns the active pager to its next page.</summary>
        public void Next()
        {
            if (active != null)
                active.Next();
        }

        /// <summary>Turns the active pager to its previous page.</summary>
        public void Previous()
        {
            if (active != null)
                active.Previous();
        }

        /// <summary>Shows/hides the Prev/Next buttons based on the active pager's page count.</summary>
        public void RefreshButtons()
        {
            if (!autoToggleButtons)
                return;

            bool show = active != null && active.PageCount > 1;
            if (previousButton != null) previousButton.SetActive(show);
            if (nextButton != null) nextButton.SetActive(show);
        }
    }
}
