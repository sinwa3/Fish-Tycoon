using UnityEngine;

/// <summary> YSW : 문, 서랍, 수납장처럼 열고 닫을 수 있는 오브젝트가 구현하는 인터페이스 </summary>
public interface IOpenable
{
    /// <summary> YSW : 열린 상태(또는 열리는 중)인지 여부 </summary>
    public bool IsOpen { get; }

    /// <summary> YSW : 열기 애니메이션을 재생한다. 방향이 있는 가구는 opener 반대쪽으로 열린다. </summary>
    public void PlayOpen(GameObject opener);

    /// <summary> YSW : 닫기 애니메이션을 재생한다. </summary>
    public void PlayClose();
}
