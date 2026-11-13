/// <summary> KYH : 빨간 버튼을 쓰면 집에서 바로 탈출한다 </summary>
public class RedButtonEscape : IConsumableEffect
{
    /// <summary> KYH : 탈출하고 성공하면 true </summary>
    public bool Use(Item item)
    {
        Escape();

        return true;
    }

    /// <summary> KYH : 탈출 이벤트를 발생시킨다 </summary>
    private void Escape()
    {
        if (Managers.Event == null)
        {
            return;
        }

        Managers.Event.RaisePlayerEscaped();
    }
}
