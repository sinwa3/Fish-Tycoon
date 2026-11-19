using System;
using UnityEngine;

/// <summary> GYH : 손님 본체. 상태 머신을 돌리고 상태들이 쓰는 기능과 정보를 제공한다 </summary>
public class CustomerController : MonoBehaviour
{
    #region ===== 수치 =====

    // 대기시간 100%. 기다린 시간은 재는 값이고 이건 그 최대값
    private const float MaxWaitDuration = 20.0f;
    // 남은 대기시간 비율이 이 값 이하면 절반 배율
    private const float HalfMultiplierRatio = 0.5f;
    private const float ZeroMultiplier = 0.0f;
    private const float HalfMultiplier = 0.5f;
    private const float FullMultiplier = 1.0f;

    #endregion ===== 수치 =====

    #region ===== 행동 =====

    private readonly CustomerStateMachine _stateMachine = new();

    public string CurrentStateName => _stateMachine.CurrentStateName;

    #endregion ===== 행동 =====

    #region ===== 좌석 =====

    private CustomerSeats _customerSeats;
    private int _seatIndex = -1;

    #endregion ===== 좌석 =====

    #region ===== 음식과 대기 =====

    private bool _hasReceivedFood;

    // 주문 후 음식을 받을 때까지 걸린 시간
    private float _waitTime;

    public bool HasReceivedFood => _hasReceivedFood;
    public float WaitTime => _waitTime;

    // 자식 오브젝트의 대기 게이지
    private CustomerWaitGauge _waitGauge;

    // 자식 오브젝트의 받은 요리
    private CustomerServedFood _servedFood;

    #endregion ===== 음식과 대기 =====

    #region ===== 주문 =====

    // 자식 오브젝트의 말풍선
    private CustomerOrderBubble _orderBubble;

    // 스포너가 Spawn 때 넘겨주는 그날 메뉴
    private DailyMenu _dailyMenu;

    // 주문한 메뉴 ID, 주문 전이면 -1
    private int _orderedMenuId = -1;

    public int OrderedMenuId => _orderedMenuId;

    // 몇 번째로 주문했는지, 주문 전이면 -1
    private int _orderIndex = -1;

    public int OrderIndex => _orderIndex;

    /// <summary> KDH : 손님이 주문했을 때 발생한다. </summary>
    public event Action<int> OnOrdered;
    
    #endregion ===== 주문 =====

    #region ===== 만족도 표시 =====

    // 자식 오브젝트의 만족도 수치 UI
    private CustomerSatisfactionUI _satisfactionUI;

    #endregion ===== 만족도 표시 =====

    #region ===== 애니메이션 =====

    private Animator _animator;
    
    #endregion ===== 애니메이션 =====

    private void Awake()
    {
        InitializeComponents();
        InitializeStateMachine();
    }

    private void Update()
    {
        _stateMachine.UpdateCurrentState(Time.deltaTime);
    }

    #region ===== 초기화 =====

    /// <summary> GYH : 자식의 주문 말풍선, 대기 게이지, 받은 요리, 만족도 수치 UI, 애니메이터를 찾아 둔다 </summary>
    private void InitializeComponents()
    {
        _orderBubble = GetComponentInChildren<CustomerOrderBubble>(true);
        _waitGauge = GetComponentInChildren<CustomerWaitGauge>(true);
        _servedFood = GetComponentInChildren<CustomerServedFood>(true);
        _satisfactionUI = GetComponentInChildren<CustomerSatisfactionUI>(true);
        _animator = GetComponentInChildren<Animator>(true);

        if (_orderBubble == null)
        {
            CPrint.Warning("[CustomerController] 주문 말풍선 없음");
        }

        if (_waitGauge == null)
        {
            CPrint.Warning("[CustomerController] 게이지 없음");
        }

        if (_servedFood == null)
        {
            CPrint.Warning("[CustomerController] 받은 요리 없음");
        }

        if (_satisfactionUI == null)
        {
            CPrint.Warning("[CustomerController] 만족도 수치 없음");
        }

        if (_animator == null)
        {
            CPrint.Warning("[CustomerController] 애니메이터 없음");
        }
    }

    /// <summary> GYH : 손님 상태 7개를 상태 머신에 등록한다 </summary>
    private void InitializeStateMachine()
    {
        _stateMachine.RegisterState(new CustomerEnterState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerSitState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerOrderState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerWaitState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerEatState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerPayState(_stateMachine, this));
        _stateMachine.RegisterState(new CustomerExitState(_stateMachine, this));
    }

    #endregion ===== 초기화 =====

    #region ===== 등장과 퇴장 =====

    /// <summary> GYH : 손님이 사라지기 직전에 발행 </summary>
    public event Action<CustomerController> OnDespawned;

    /// <summary> GYH : 배정받은 좌석에 손님을 세우고 입장 상태부터 시작한다 </summary>
    public void Spawn(CustomerSeats customerSeats, int seatIndex, DailyMenu dailyMenu)
    {
        if (customerSeats == null)
        {
            CPrint.Error("[CustomerController] CustomerSeats 없음");

            return;
        }

        _customerSeats = customerSeats;
        _seatIndex = seatIndex;
        _dailyMenu = dailyMenu;

        _hasReceivedFood = false;
        _waitTime = 0.0f;
        _orderedMenuId = -1;
        _orderIndex = -1;

        HideOrderBubble();
        HideWaitGauge();
        HideServedFood();

        Transform seatTransform = _customerSeats.GetSeat(_seatIndex);

        if (seatTransform == null)
        {
            CPrint.Warning($"[CustomerController] {_seatIndex} 자리 없음");

            return;
        }

        transform.position = seatTransform.position;
        _stateMachine.ChangeState<CustomerEnterState>();
    }

    /// <summary> GYH : 좌석을 반납한다 (퇴장 상태에서 호출) </summary>
    public void LeaveSeat()
    {
        if (_customerSeats == null)
        {
            return;
        }

        _customerSeats.ReleaseSeat(_seatIndex);
        _seatIndex = -1;
    }

    /// <summary> GYH : 손님을 없앤다 (지금은 Destroy) </summary>
    public void Despawn()
    {
        _stateMachine.Clear();
        OnDespawned?.Invoke(this);
        Destroy(gameObject);
    }

    #endregion ===== 등장과 퇴장 =====

    #region ===== 주문 처리 =====

    /// <summary> GYH : 그날 메뉴에서 하나를 주문하고 말풍선에 띄운다 (주문 상태에서 호출) </summary>
    public void Order()
    {
        if (_dailyMenu == null)
        {
            CPrint.Warning("[CustomerController] 그날 메뉴 없음");

            return;
        }

        _orderIndex = _dailyMenu.OrderedCount;
        _orderedMenuId = _dailyMenu.TakeNextMenu();

        OnOrdered?.Invoke(_dailyMenu.OrderedCount);
        
        if (_orderBubble == null)
        {
            return;
        }

        _orderBubble.Show(_orderedMenuId);
    }

    /// <summary> GYH : 주문 말풍선을 끈다 (대기 상태가 끝날 때 호출) </summary>
    public void HideOrderBubble()
    {
        if (_orderBubble == null)
        {
            return;
        }

        _orderBubble.Hide();
    }

    #endregion ===== 주문 처리 =====

    #region ===== 음식 =====

    /// <summary> GYH : 대기 상태일 때만 요리를 받는다. 받으면 true </summary>
    public bool ReceiveFood()
    {
        if (!_stateMachine.IsCurrentState<CustomerWaitState>())
        {
            return false;
        }

        _hasReceivedFood = true;

        return true;
    }

    /// <summary> GYH : 대기 상태가 끝날 때 잰 대기 시간을 기록한다 </summary>
    public void RecordWaitTime(float waitTime)
    {
        _waitTime = waitTime;
    }

    /// <summary> GYH : 주문한 메뉴를 손님 앞에 놓는다 (식사 상태에 들어올 때 호출) </summary>
    public void ShowServedFood()
    {
        if (_servedFood == null)
        {
            return;
        }

        _servedFood.Show(_orderedMenuId);
    }

    /// <summary> GYH : 손님 앞 요리를 치운다 (식사 상태가 끝날 때 호출) </summary>
    public void HideServedFood()
    {
        if (_servedFood == null)
        {
            return;
        }

        _servedFood.Hide();
    }

    #endregion ===== 음식 =====

    #region ===== 대기 게이지 =====

    /// <summary> GYH : 대기 게이지를 가득 찬 상태로 켠다 (대기 상태가 시작될 때 호출) </summary>
    public void ShowWaitGauge()
    {
        if (_waitGauge == null)
        {
            return;
        }

        _waitGauge.Show();
    }

    /// <summary> GYH : 지금까지 기다린 시간으로 게이지를 줄인다 (대기 상태 Update에서 호출) </summary>
    public void UpdateWaitGauge(float waitTime)
    {
        if (_waitGauge == null)
        {
            return;
        }

        float ratio = GetRemainingWaitRatio(waitTime);
        _waitGauge.SetRatio(ratio);
    }

    /// <summary> GYH : 대기 게이지를 끈다 (대기 상태가 끝날 때 호출) </summary>
    public void HideWaitGauge()
    {
        if (_waitGauge == null)
        {
            return;
        }

        _waitGauge.Hide();
    }

    #endregion ===== 대기 게이지 =====

    #region ===== 결제 =====

    /// <summary> GYH : 손님이 계산할 때 만족도, 손님 위치, 음식 가격과 함께 발행 </summary>
    public event Action<int, Vector3, int> OnPaid;

    /// <summary> GYH : 주문한 음식과 기다린 시간으로 만족도를 계산해 띄우고 알린다 </summary>
    public void Pay()
    {
        if (!TryGetOrderedFood(out FoodData foodData))
        {
            return;
        }

        float remainingRatio = GetRemainingWaitRatio(_waitTime);
        SatisfactionGrade grade = GetSatisfactionGrade(remainingRatio);
        int satisfaction = CalculateSatisfaction(foodData, grade);

        ShowSatisfaction(satisfaction, grade);
        OnPaid?.Invoke(satisfaction, transform.position, foodData.Price);
    }

    /// <summary> GYH : 기다린 시간으로 남은 대기시간 비율(0 ~ 1)을 구한다 </summary>
    private float GetRemainingWaitRatio(float waitTime)
    {
        return Mathf.Clamp01(1.0f - (waitTime / MaxWaitDuration));
    }

    /// <summary> GYH : 남은 대기시간 비율로 만족 단계를 정한다 </summary>
    private SatisfactionGrade GetSatisfactionGrade(float remainingRatio)
    {
        SatisfactionGrade grade = remainingRatio switch
        {
            <= 0.0f => SatisfactionGrade.Unhappy,
            <= HalfMultiplierRatio => SatisfactionGrade.Normal,
            _ => SatisfactionGrade.Happy
        };

        return grade;
    }

    /// <summary> GYH : 만족 단계에 맞는 배율을 가져온다 </summary>
    private float GetWaitMultiplier(SatisfactionGrade grade)
    {
        float multiplier = grade switch
        {
            SatisfactionGrade.Unhappy => ZeroMultiplier,
            SatisfactionGrade.Normal => HalfMultiplier,
            SatisfactionGrade.Happy => FullMultiplier,
            _ => FullMultiplier
        };

        return multiplier;
    }

    /// <summary> GYH : 기본 만족도에 대기시간 배율을 곱한 만족도를 구한다 (최소 0) </summary>
    private int CalculateSatisfaction(FoodData food, SatisfactionGrade grade)
    {
        float waitMultiplier = GetWaitMultiplier(grade);
        int satisfaction = (int)(food.Satisfaction * waitMultiplier);

        return Mathf.Max(0, satisfaction);
    }

    /// <summary> GYH : 주문한 메뉴 ID로 음식 테이블에서 데이터를 찾는다. 못 찾으면 false </summary>
    private bool TryGetOrderedFood(out FoodData food)
    {
        food = null;

        if (!Managers.Table.IsLoaded)
        {
            CPrint.Warning("[CustomerController] 테이블이 아직 로드되지 않음");

            return false;
        }

        if (!Managers.Table.Food.Map.TryGetValue(_orderedMenuId, out food))
        {
            CPrint.Warning($"[CustomerController] Id {_orderedMenuId} 음식 테이블에 없음");

            return false;
        }

        return true;
    }

    #endregion ===== 결제 =====

    #region ===== 만족도 표시 처리 =====

    /// <summary> GYH : 얻은 만족도와 만족 단계를 손님 위에 띄운다 (결제할 때) </summary>
    private void ShowSatisfaction(int satisfaction, SatisfactionGrade grade)
    {
        if (_satisfactionUI == null)
        {
            return;
        }

        _satisfactionUI.Play(satisfaction, grade);
    }

    #endregion ===== 만족도 표시 처리 =====

    #region ===== 애니메이션 처리 =====

    /// <summary> GYH : 대기 애니메이션을 재생한다 (입장, 결제 상태가 시작될 때 호출) </summary>
    public void PlayIdle()
    {
        PlayAnimation(AnimatorKey.Hash.Idle);
    }

    /// <summary> GYH : 식사 애니메이션을 재생한다 (식사 상태가 시작될 때 호출) </summary>
    public void PlayEating()
    {
        PlayAnimation(AnimatorKey.Hash.Eating);
    }

    /// <summary> GYH : 퇴장 애니메이션을 재생한다 (퇴장 상태가 시작될 때 호출) </summary>
    public void PlayExiting()
    {
        PlayAnimation(AnimatorKey.Hash.Exiting);
    }

    /// <summary> GYH : 퇴장 애니메이션이 끝까지 재생됐는지 확인한다 </summary>
    public bool IsExitAnimationFinished()
    {
        if (_animator == null)
        {
            return true;
        }

        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);

        return info.shortNameHash == AnimatorKey.Hash.Exiting && info.normalizedTime >= 1.0f;
    }

    /// <summary> GYH : 애니메이터 상태를 처음부터 재생한다 </summary>
    private void PlayAnimation(int stateHash)
    {
        if (_animator == null)
        {
            return;
        }

        _animator.Play(stateHash, 0, 0.0f);
    }

    #endregion ===== 애니메이션 처리 =====
}
