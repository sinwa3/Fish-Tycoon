using UnityEngine;

/// <summary>
/// YSW : 조건이 맞지 않으면 상호작용(홀드)을 시작하지 못하게 막는 상호작용 대상.
/// 막혀 있는 동안 안내 문구 대신 막힌 이유를 보여주고, 키를 눌러도 홀드 게이지가 차지 않는다.
/// </summary>
public interface IInteractBlockable
{
    /// <summary> YSW : 지금 상호작용이 막혀 있으면 true와 함께 막힌 이유(안내 문구)를 반환한다. </summary>
    public bool IsInteractBlocked(GameObject interactor, out string message);
}
