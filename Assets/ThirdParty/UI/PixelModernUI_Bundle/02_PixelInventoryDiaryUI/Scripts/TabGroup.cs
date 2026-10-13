using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PixelInventoryDiaryUI
{
    /// <summary>
    /// A simple tab switcher. Each tab pairs a Button with the objects it should
    /// show (its content panel, plus any tab-specific extras like page arrows).
    /// Selecting a tab activates that tab's objects, hides every other tab's
    /// objects, and resizes the buttons (active vs inactive width).
    ///
    /// Wire nothing on the buttons themselves — TabGroup hooks their onClick
    /// automatically. Add more tabs just by growing the Tabs list.
    /// </summary>
    [AddComponentMenu("Pixel Inventory Diary UI/Tab Group")]
    [DisallowMultipleComponent]
    public class TabGroup : MonoBehaviour
    {
        [Serializable]
        public class Tab
        {
            [Tooltip("The tab button.")]
            public Button button;

            [Tooltip("Objects shown while this tab is selected (content panel + any tab-only extras such as Prev/Next arrows). Everything here is hidden for other tabs.")]
            public GameObject[] show = new GameObject[0];
        }

        [SerializeField] private Tab[] tabs = new Tab[0];

        [Tooltip("Tab selected on enable.")]
        [SerializeField] private int startTab = 0;

        [Header("Button Width")]
        [Tooltip("Width (RectTransform sizeDelta.x) of the selected tab button.")]
        [SerializeField] private float activeWidth = 114f;

        [Tooltip("Width of the non-selected tab buttons.")]
        [SerializeField] private float inactiveWidth = 84f;

        [Header("Events")]
        [Tooltip("Invoked after a tab is selected, with its index.")]
        [SerializeField] private UnityEvent<int> onTabSelected = new UnityEvent<int>();

        private int current = -1;
        private bool hooked;

        /// <summary>Index of the currently selected tab.</summary>
        public int CurrentTab => current;

        /// <summary>Number of tabs.</summary>
        public int TabCount => tabs != null ? tabs.Length : 0;

        /// <summary>Fires after a tab is selected, with its index.</summary>
        public UnityEvent<int> OnTabSelected => onTabSelected;

        private void Awake() => HookButtons();

        private void OnEnable()
        {
            HookButtons();
            // deferEvent: when the diary is turned on (SetActive), the start tab is
            // selected here — but an object listening on onTabSelected (e.g. Tabs_R's
            // decorative button tween in another branch) may not be active yet this
            // frame. Defer just the event so its feedback still plays. See Select().
            Select(current >= 0 ? current : startTab, deferEvent: true);
        }

        private void HookButtons()
        {
            if (hooked) return;
            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i; // capture per-iteration
                if (tabs[i].button != null)
                    tabs[i].button.onClick.AddListener(() => Select(index));
            }
            hooked = true;
        }

        /// <summary>Selects a tab: shows its objects, hides the others, resizes buttons.</summary>
        public void Select(int index) => Select(index, deferEvent: false);

        private void Select(int index, bool deferEvent)
        {
            if (TabCount == 0) return;
            index = Mathf.Clamp(index, 0, TabCount - 1);
            current = index;

            for (int i = 0; i < tabs.Length; i++)
            {
                bool on = i == index;

                // Show/hide this tab's objects. Re-activating a panel re-fires its
                // OnEnable, so any UITweener with Play On Enable plays its entrance.
                var show = tabs[i].show;
                if (show != null)
                {
                    for (int j = 0; j < show.Length; j++)
                        if (show[j] != null)
                            show[j].SetActive(on);
                }

                // Resize the button.
                if (tabs[i].button != null)
                {
                    var rt = tabs[i].button.transform as RectTransform;
                    if (rt != null)
                    {
                        Vector2 size = rt.sizeDelta;
                        size.x = on ? activeWidth : inactiveWidth;
                        rt.sizeDelta = size;
                    }

                    // Play the selected tab button's tween (click / select feedback).
                    if (on)
                    {
                        var tw = tabs[i].button.GetComponent<UITweener>();
                        if (tw != null)
                            tw.Play();
                    }
                }
            }

            // When called from OnEnable (deferEvent), fire the event one frame later:
            // on SetActive(true) the child OnEnable order isn't guaranteed, so a target
            // driven by onTabSelected (e.g. a decorative button's UITweener in another
            // hierarchy branch) may still be inactive this frame and UITweener.Play()
            // would be ignored. Content/buttons above already updated, so no flicker.
            if (deferEvent && isActiveAndEnabled)
                StartCoroutine(InvokeSelectedNextFrame());
            else
                onTabSelected.Invoke(index);
        }

        private IEnumerator InvokeSelectedNextFrame()
        {
            yield return null;
            // Fire the live `current`, not the index captured on enable: if the user
            // clicked another tab during the one-frame wait, `current` already reflects
            // it, so the event never reports a stale index.
            onTabSelected.Invoke(current);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            startTab = Mathf.Max(0, startTab);
            activeWidth = Mathf.Max(0f, activeWidth);
            inactiveWidth = Mathf.Max(0f, inactiveWidth);
        }
#endif
    }
}
