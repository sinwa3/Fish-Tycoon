using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureInteractable_Test : MonoBehaviour, IInteractable
{
    [SerializeField] private string _interactionText = "열기";
    [SerializeField] private bool _canInteract = true;

    /// <summary>SJY : 플레이어 화면에 표시할 상호작용 문구를 반환한다.</summary>
    public string InteractionText => _interactionText;

    /// <summary>SJY : 현재 가구에 상호작용할 수 있는지 확인한다.</summary>
    public bool CanInteract(GameObject interactor)
    {
        return _canInteract;
    }

    /// <summary>SJY : E키로 상호작용했을 때 테스트 로그를 출력한다.</summary>
    public void Interact(GameObject interactor)
    {
        Debug.Log($"[FurnitureInteractable] {name} 상호작용 실행", this);
    }
}
