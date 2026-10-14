using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPocketPhoneUI.Tweening.EditorTools
{
    /// <summary>
    /// Editor menu helpers that add ready-made tween animations to scene objects,
    /// so the demo (or any UI) animates without hand-wiring components.
    /// Everything is Undo-friendly (Ctrl+Z) and never stacks duplicate components.
    /// </summary>
    public static class PixelTweenDemoSetup
    {
        private const string Root = "Tools/Pixel Pocket UI/";

        // ---------------------------------------------------------------- Open scene

        [MenuItem(Root + "Apply Sample Animations to Open Scene", priority = 0)]
        private static void ApplySampleToScene()
        {
            var log = new StringBuilder();
            int icons = 0, popups = 0, screens = 0;

            // 1) App-icon grids: cascade pop-in of each icon button.
            foreach (var grid in FindByName("AppIcons"))
                icons += CascadeChildren(grid, 0.045f);

            // 2) Popups / overlays: fade + scale "pop" when shown.
            //    Demo objects are named things like "Pop", "Alarm", "Call".
            string[] popupNames = { "Pop", "Popup", "Dialog", "Alert", "Modal" };
            foreach (var name in popupNames)
                foreach (var popup in FindByName(name))
                {
                    AddPopupShow(popup);
                    popups++;
                }

            // 3) Whole screens: a gentle fade-in (InOutSine = smooth both ends).
            foreach (var screen in FindByName("Screen"))
            {
                AddFade(screen, 0.3f, 0f, 1f, PixelEaseType.InOutSine);
                screens++;
            }

            log.AppendLine($"App icons animated: {icons}");
            log.AppendLine($"Popups animated:   {popups}");
            log.AppendLine($"Screens animated:  {screens}");

            if (icons + popups + screens == 0)
            {
                EditorUtility.DisplayDialog("Pixel Pocket UI",
                    "No matching objects found in the open scene.\n\n" +
                    "Tip: select objects yourself and use\n" +
                    "Tools ▸ Pixel Pocket UI ▸ Selection ▸ ...",
                    "OK");
                return;
            }

            MarkSceneDirty();
            EditorUtility.DisplayDialog("Pixel Pocket UI — Sample Animations Applied", log.ToString(), "Nice!");
        }

        [MenuItem(Root + "Remove All Tween Components in Open Scene", priority = 1)]
        private static void RemoveAllInScene()
        {
            var all = Object.FindObjectsByType<PixelTweenComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int n = all.Length;
            foreach (var c in all)
                Undo.DestroyObjectImmediate(c);

            MarkSceneDirty();
            EditorUtility.DisplayDialog("Pixel Pocket UI", $"Removed {n} tween component(s).", "OK");
        }

        // ---------------------------------------------------------------- Selection

        [MenuItem(Root + "Selection/Add Fade-In", priority = 20)]
        private static void SelFade()
        {
            foreach (var t in Selection.transforms) AddFade(t, 0.3f, 0f, 1f);
            MarkSceneDirty();
        }

        [MenuItem(Root + "Selection/Add Scale Pop-In", priority = 21)]
        private static void SelScale()
        {
            foreach (var t in Selection.transforms) AddScalePop(t, 0f);
            MarkSceneDirty();
        }

        [MenuItem(Root + "Selection/Add Shake", priority = 22)]
        private static void SelShake()
        {
            foreach (var t in Selection.transforms)
            {
                var s = AddOrGet<PixelShakeTween>(t.gameObject);
                if (s == null) continue;
                s.playOn = PixelTweenComponent.PlayTrigger.Manual;
                s.duration = 0.4f;
                s.ease = PixelEaseType.Linear;
            }
            MarkSceneDirty();
        }

        [MenuItem(Root + "Selection/Add Cascade Pop-In to Children", priority = 23)]
        private static void SelCascade()
        {
            int n = 0;
            foreach (var t in Selection.transforms) n += CascadeChildren(t, 0.045f);
            MarkSceneDirty();
            EditorUtility.DisplayDialog("Pixel Pocket UI", $"Added cascade pop-in to {n} child object(s).", "OK");
        }

        [MenuItem(Root + "Selection/Add Cascade Pop-In to Children", validate = true)]
        private static bool SelCascadeValidate() => Selection.transforms.Length > 0;

        // ---------------------------------------------------------------- Builders

        private static int CascadeChildren(Transform parent, float stagger)
        {
            int i = 0;
            foreach (Transform child in parent)
            {
                AddScalePop(child, i * stagger);
                i++;
            }
            return i;
        }

        private static void AddScalePop(Transform t, float delay)
        {
            var st = AddOrGet<PixelScaleTween>(t.gameObject);
            if (st == null) return;
            st.from = Vector3.zero;
            st.to = t.localScale == Vector3.zero ? Vector3.one : t.localScale;
            st.fromCurrent = false;
            st.duration = 0.35f;
            st.ease = PixelEaseType.OutBack;
            st.delay = delay;
            st.playOn = PixelTweenComponent.PlayTrigger.OnEnable;
        }

        private static void AddFade(Transform t, float duration, float from, float to,
                                    PixelEaseType ease = PixelEaseType.OutCubic)
        {
            if (t.GetComponent<CanvasGroup>() == null)
                Undo.AddComponent<CanvasGroup>(t.gameObject);

            var f = AddOrGet<PixelFadeTween>(t.gameObject);
            if (f == null) return;
            f.from = from;
            f.to = to;
            f.fromCurrent = false;
            f.duration = duration;
            f.ease = ease;
            f.playOn = PixelTweenComponent.PlayTrigger.OnEnable;
        }

        private static void AddPopupShow(Transform t)
        {
            AddFade(t, 0.2f, 0f, 1f);
            var st = AddOrGet<PixelScaleTween>(t.gameObject);
            if (st == null) return;
            st.from = Vector3.one * 0.7f;
            st.to = t.localScale == Vector3.zero ? Vector3.one : t.localScale;
            st.fromCurrent = false;
            st.duration = 0.3f;
            st.ease = PixelEaseType.OutBack;
            st.playOn = PixelTweenComponent.PlayTrigger.OnEnable;
        }

        // ---------------------------------------------------------------- Utilities

        /// <summary>Adds the component (with Undo) unless one already exists, which it returns.</summary>
        private static T AddOrGet<T>(GameObject go) where T : Component
        {
            var existing = go.GetComponent<T>();
            if (existing != null) return existing;
            return Undo.AddComponent<T>(go);
        }

        private static List<Transform> FindByName(string exact)
        {
            var result = new List<Transform>();
            foreach (var t in AllTransforms())
                if (t.name == exact) result.Add(t);
            return result;
        }

        private static List<Transform> FindByNameContains(string fragment)
        {
            var result = new List<Transform>();
            string f = fragment.ToLowerInvariant();
            foreach (var t in AllTransforms())
                if (t.name.ToLowerInvariant().Contains(f)) result.Add(t);
            return result;
        }

        private static Transform[] AllTransforms()
            => Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        private static void MarkSceneDirty()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
