using UnityEngine;

/// <summary> KimYH : 맵에 놓인 생선. 지금은 종류와 개수 불러오기까지만(획득 기능은 나중에 추가) </summary>
public class FishPickup : MonoBehaviour
{
    /// <summary> KimYH : 이 생선의 ID </summary>
    public int FishID { get; private set; } = -1;

    /// <summary> KimYH : 이 자리에 놓인 생선 개수 </summary>
    public int Count { get; private set; }

    /// <summary> KimYH : 생선 종류와 개수를 넣는다. SpawnManager가 생성 직후 부른다 </summary>
    public void Setup(int fishID, int count)
    {
        FishID = fishID;
        Count = count;
    }
}
