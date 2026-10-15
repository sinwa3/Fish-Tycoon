public class TitleScene : BaseScene
{
    protected override void OnAwake()
    {
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdate()
    {
    }

    /// <summary> KDH : 게임 새로 시작 </summary>
    public void PlayNewGame()
    {
        // 만약 이미 데이터가 존재한다면 삭제하고 새로 할지 확인
        if (Managers.Data.IsSaved)
        {
            Managers.UI.OpenPopup<AlertPopup>().Set("저장", "이미 저장된 데이터가 존재합니다.\n새로 시작하시겠습니까?",
                true, () =>
                {
                    Managers.Data.Delete();
                    Managers.Data.Load();
                    Managers.Scene.LoadSceneWithLoading(SceneType.TycoonScene);
                });
        }
        else
        {
            Managers.Data.Load();
            Managers.Scene.LoadSceneWithLoading(SceneType.TycoonScene);
        }
    }

    /// <summary> KDH : 이어 하기 </summary>
    public void LoadGame()
    {
        if (Managers.Data.IsSaved)
        {
            Managers.Data.Load();
            Managers.Scene.LoadSceneWithLoading(SceneType.TycoonScene);
        }
    }
}