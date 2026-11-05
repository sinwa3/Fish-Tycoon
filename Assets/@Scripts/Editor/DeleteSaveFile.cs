using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary> KDH : persistentDataPath에 저장된 세이브 파일을 삭제한다. </summary>
public static class DeleteSaveFile
{
    private const string SaveFileName = "SaveData.json";
    
    [MenuItem("Tools/Misc/Delete Save File", priority = 1)]
    private static void Delete()
    {
        
        string filePath = Path.Combine(Application.persistentDataPath, SaveFileName);

        if (!File.Exists(filePath))
        {
            Debug.Log($"세이브 파일이 존재하지 않습니다: {filePath}");
            return;
        }

        bool isConfirmed = EditorUtility.DisplayDialog("Delete Save File", $"세이브 파일을 삭제할까요?\n\n{filePath}",
            "삭제", "취소");

        if (!isConfirmed)
        {
            return;
        }

        File.Delete(filePath);

        Debug.Log($"세이브 파일 삭제 완료: {filePath}");
    }
}