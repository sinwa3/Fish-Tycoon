/// <summary> KYH : 멈춰서 마지막 자극 쪽을 바라보는 상태 </summary>
public class EnemySuspectState : BaseEnemyState
{
    // 바라보는 자극의 시각
    private float _lookingStimulusTime;

    public EnemySuspectState(EnemyStateMachine stateMachine, EnemyController enemy) : base(stateMachine, enemy)
    {
    }

    /// <summary> KYH : 의심 상태에 들어올 때 호출한다 </summary>
    protected override void Enter()
    {
        Enemy.Movement.Stop();
        LookAtLastStimulus();
    }

    /// <summary> KYH : 의심 상태를 갱신한다 </summary>
    public override void Update(float deltaTime)
    {
        if (_lookingStimulusTime != Enemy.Memory.LastStimulusTime)
        {
            LookAtLastStimulus();
        }
    }

    /// <summary> KYH : 의심 상태를 끝낼 때 호출한다 </summary>
    public override void Exit()
    {
        Enemy.Look.StopLook();
    }

    /// <summary> KYH : 마지막 자극 쪽을 바라본다 </summary>
    private void LookAtLastStimulus()
    {
        Enemy.Look.LookAt(Enemy.Memory.LastStimulusPosition);
        _lookingStimulusTime = Enemy.Memory.LastStimulusTime;
    }
}
