/// <summary> KYH : 소비 아이템을 썼을 때의 효과 </summary>
public interface IConsumableEffect
{
    /// <summary> KYH : 효과를 내고 성공하면 true </summary>
    bool Use(Item item);
}
