using UnityEngine;

/// <summary> YSW : 플레이어가 바라보고 E 키로 상호작용할 수 있는 오브젝트가 구현하는 인터페이스 </summary>
public interface IInteractable
{
    /// <summary> YSW : 화면에 표시할 상호작용 안내 문구 (예: 숨기, 줍기) </summary>
    public string InteractionText { get; }

    /// <summary> YSW : 지정한 상호작용 주체가 지금 상호작용할 수 있는지 반환한다. </summary>
    public bool CanInteract(GameObject interactor);

    /// <summary> YSW : 지정한 상호작용 주체와 상호작용을 실행한다. </summary>
    public void Interact(GameObject interactor);
}
