using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPocketPhoneUI.Tweening.EditorTools
{
    /// <summary>
    /// One-click helpers to wire a Button's OnClick to show/hide another object,
    /// without touching code or hand-editing the scene file. Select the button
    /// and the target together, then run the menu.
    /// </summary>
    public static class PixelWireHelper
    {
        [MenuItem("Tools/Pixel Pocket UI/Wire Button to Activate Target", priority = 40)]
        private static void WireActivate() => Wire(true);

        [MenuItem("Tools/Pixel Pocket UI/Wire Button to Deactivate Target", priority = 41)]
        private static void WireDeactivate() => Wire(false);

        private static void Wire(bool active)
        {
            Button btn = null;
            GameObject target = null;

            foreach (var go in Selection.gameObjects)
            {
                var b = go.GetComponent<Button>();
                if (b != null && btn == null) btn = b;
                else if (target == null) target = go;
            }

            if (btn == null || target == null)
            {
                EditorUtility.DisplayDialog("Pixel Pocket UI",
                    "하이어라키에서 [버튼 1개] + [켜고 끌 대상 1개]를 함께 선택한 뒤 다시 실행하세요.\n\n" +
                    "예: Btn_Alarm 과 Home 을 Ctrl-클릭으로 둘 다 선택한 상태에서 메뉴 실행.",
                    "확인");
                return;
            }

            Undo.RecordObject(btn, "Wire OnClick");
            UnityEventTools.AddBoolPersistentListener(btn.onClick, target.SetActive, active);
            EditorUtility.SetDirty(btn);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(btn.gameObject.scene);

            EditorUtility.DisplayDialog("Pixel Pocket UI",
                $"연결 완료!\n\n[{btn.name}] 클릭 → [{target.name}] SetActive({(active ? "true" : "false")})\n\n" +
                "Ctrl+S 로 저장한 뒤 Play 해서 확인하세요.",
                "좋아요");
        }
    }
}
