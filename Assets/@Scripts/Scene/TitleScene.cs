public class TitleScene : BaseScene
{
    protected override void OnAwake()
    {
        Managers.Table.Load();
        Managers.Data.Load();
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdate()
    {
    }
}
