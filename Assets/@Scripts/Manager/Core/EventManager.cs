using System;

/// <summary> KDH : 게임 전역에서 사용하는 이벤트를 관리하고 발행한다. </summary>
public class EventManager
{
    #region ===== 이벤트 =====

    public event Action OnPlayerArrested;
    public event Action OnPlayerEscaped;

    public event Action<int> OnItemUsed;
    
    public event Action OnPause;
    public event Action OnResume;

    #endregion ===== 이벤트 =====

    #region ===== 이벤트 발행 =====

    /// <summary> KDH : 플레이어가 잡혔음을 알린다. </summary>
    public void RaisePlayerArrested()
    {
        OnPlayerArrested?.Invoke();
    }
    
    /// <summary> KDH : 플레이어가 탈출했음을 알린다. </summary>
    public void RaisePlayerEscaped()
    {
        OnPlayerEscaped?.Invoke();
    }

    /// <summary> KDH : 게임이 일시정지 되었음을 알린다. </summary>
    public void RaisePause()
    {
        OnPause?.Invoke();
    }
    
    /// <summary> KDH : 게임이 일시정지 상태에서 해제되었음을 알린다. </summary>
    public void RaiseResume()
    {
        OnResume?.Invoke();
    }

    /// <summary> KDH : 해당 아이템을 사용했음을 알린다. </summary>
    public void RaiseItemUsed(int itemID)
    {
        OnItemUsed?.Invoke(itemID);
    }
    
    #endregion ===== 이벤트 발행 =====
}