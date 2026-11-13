#if UNITY_EDITOR || DEVELOPMENT_BUILD

/// <summary> 세이브 파일을 지운다 (한 번 더 눌러야 지운다) </summary>
public class SaveResetCheat : BaseCheat
{
    private bool _isConfirming;

    public override string Title => "세이브";

    public override bool IsAvailable => Managers.Data != null;

    public override void DrawButtons()
    {
        if (!_isConfirming)
        {
            DrawButton("세이브 파일 지우기", () => _isConfirming = true);

            return;
        }

        DrawButton("정말 지우기", DeleteSaveData);
        DrawButton("취소", () => _isConfirming = false);
    }

    private void DeleteSaveData()
    {
        _isConfirming = false;
        Managers.Data?.DeleteSaveData();
    }
}
#endif
