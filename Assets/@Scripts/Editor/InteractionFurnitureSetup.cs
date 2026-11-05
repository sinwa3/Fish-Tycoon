using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> YSW : 열린 씬의 문(Door_05)을 상호작용 문 프리팹(Door_Interactive)으로 교체하는 에디터 도구 </summary>
public static class InteractionFurnitureSetup
{
    #region ===== 경로 =====

    private const string SourceDoorPrefabPath = "Assets/ThirdParty/Environment/LowPolyInterior/Prefabs/Walls/Door_05.prefab";
    private const string InteractiveDoorPrefabPath = "Assets/@Resources/Prefab/Interaction/Door_Interactive.prefab";

    #endregion ===== 경로 =====

    #region ===== 메뉴 =====

    /// <summary> YSW : 열린 씬의 Door_05 문들을 상호작용 문 프리팹으로 교체한다. (위치/회전/부모는 그대로 유지) </summary>
    [MenuItem("Tools/Interaction/Replace Doors In Open Scene", priority = 1)]
    private static void ReplaceDoorsInOpenScene()
    {
        GameObject sourceDoorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourceDoorPrefabPath);
        GameObject interactiveDoorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InteractiveDoorPrefabPath);

        if (sourceDoorPrefab == null)
        {
            Debug.LogError($"[InteractionFurnitureSetup] 원본 문 프리팹을 찾을 수 없습니다: {SourceDoorPrefabPath}");
            return;
        }

        if (interactiveDoorPrefab == null)
        {
            Debug.LogError($"[InteractionFurnitureSetup] 상호작용 문 프리팹을 찾을 수 없습니다: {InteractiveDoorPrefabPath}");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        List<GameObject> targetDoors = FindDoorInstances(scene, sourceDoorPrefab);

        foreach (GameObject targetDoor in targetDoors)
        {
            ReplaceDoor(targetDoor, interactiveDoorPrefab, scene);
        }

        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log($"[InteractionFurnitureSetup] [{scene.name}] 문 {targetDoors.Count}개를 교체했습니다. 씬을 저장해주세요.");
    }

    #endregion ===== 메뉴 =====

    #region ===== 씬 교체 =====

    /// <summary> YSW : 씬에서 아직 교체되지 않은 원본 문 프리팹 인스턴스를 모두 찾는다. </summary>
    private static List<GameObject> FindDoorInstances(Scene scene, GameObject sourceDoorPrefab)
    {
        List<GameObject> doors = new List<GameObject>();

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (Transform child in rootObject.GetComponentsInChildren<Transform>(true))
            {
                GameObject candidate = child.gameObject;

                if (!PrefabUtility.IsAnyPrefabInstanceRoot(candidate))
                {
                    continue;
                }

                if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(candidate) != sourceDoorPrefab)
                {
                    continue;
                }

                if (candidate.GetComponentInParent<InteractionFurniture>(true) != null)
                {
                    continue;
                }

                doors.Add(candidate);
            }
        }

        return doors;
    }

    /// <summary> YSW : 문 하나를 같은 자리(부모/위치/회전/크기/순서)에 상호작용 문 프리팹으로 바꾼다. </summary>
    private static void ReplaceDoor(GameObject oldDoor, GameObject interactiveDoorPrefab, Scene scene)
    {
        Transform oldTransform = oldDoor.transform;

        GameObject newDoor = (GameObject)PrefabUtility.InstantiatePrefab(interactiveDoorPrefab, scene);
        Undo.RegisterCreatedObjectUndo(newDoor, "Replace Door");

        Transform newTransform = newDoor.transform;
        newTransform.SetParent(oldTransform.parent, false);
        newTransform.localPosition = oldTransform.localPosition;
        newTransform.localRotation = oldTransform.localRotation;
        newTransform.localScale = oldTransform.localScale;
        newTransform.SetSiblingIndex(oldTransform.GetSiblingIndex());

        newDoor.name = $"{oldDoor.name} (Interactive)";

        Undo.DestroyObjectImmediate(oldDoor);
    }

    #endregion ===== 씬 교체 =====
}
