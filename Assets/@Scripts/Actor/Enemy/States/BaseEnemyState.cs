/// <summary> GYH : 집주인 상태 머신에서 사용하는 모든 상태의 기본 클래스 </summary>
public abstract class BaseEnemyState
{
    protected readonly EnemyStateMachine StateMachine;

    public EnemyController Enemy { get; private set; }

    /// <summary> GYH : 상태가 할 일을 마쳤는지 (다음 상태는 컨트롤러가 고른다) </summary>
    public bool IsFinished { get; private set; }

    protected BaseEnemyState(EnemyStateMachine stateMachine, EnemyController enemy)
    {
        StateMachine = stateMachine;
        Enemy = enemy;
    }

    /// <summary> GYH : 상태에 들어올 때 끝남 표시를 지우고 Enter를 호출한다 </summary>
    public void BeginState()
    {
        IsFinished = false;
        Enter();
    }

    /// <summary> GYH : 할 일 마친 걸 알린다. 상태 자신만 부른다 </summary>
    protected void Finish()
    {
        IsFinished = true;
    }

    /// <summary> GYH : 현재 상태에 진입할 때 호출한다 (BeginState를 거쳐서만 불린다) </summary>
    protected abstract void Enter();

    /// <summary> GYH : 현재 상태를 갱신한다 </summary>
    public abstract void Update(float deltaTime);

    /// <summary> GYH : 현재 상태를 종료할 때 호출한다 </summary>
    public abstract void Exit();
}
