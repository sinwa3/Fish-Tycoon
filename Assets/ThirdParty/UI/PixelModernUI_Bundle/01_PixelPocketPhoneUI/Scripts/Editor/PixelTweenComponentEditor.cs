using UnityEditor;
using UnityEngine;

namespace PixelPocketPhoneUI.Tweening.EditorTools
{
    /// <summary>
    /// Custom inspector shared by every tween component. Adds a live
    /// "Play Preview" button that animates the object right in the editor,
    /// without entering Play mode.
    /// </summary>
    [CustomEditor(typeof(PixelTweenComponent), true)]
    [CanEditMultipleObjects]
    public class PixelTweenComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(6);

            bool isPreviewingThis = PixelTweenPreview.Current == target;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = isPreviewingThis ? new Color(1f, 0.6f, 0.6f) : new Color(0.6f, 1f, 0.7f);
                string label = isPreviewingThis ? "■  Stop Preview" : "▶  Play Preview";
                if (GUILayout.Button(label, GUILayout.Height(28)))
                {
                    if (isPreviewingThis) PixelTweenPreview.Stop();
                    else PixelTweenPreview.Play(target as PixelTweenComponent);
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.HelpBox(
                "Preview plays in the editor (no Play mode needed). " +
                "It ends at the 'To' values — use Ctrl+Z if you want to revert.",
                MessageType.None);
        }
    }
}
