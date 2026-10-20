using UnityEngine;

/// <summary> KYH : (테스트용) Space를 누르면 대기 중인 손님에게 요리를 준 것처럼 처리해 내보낸다. 요리 쪽 연결 전 영업 종료, 정산 확인용, 에디터에서만 동작 </summary>
public class CustomerDebugInput : MonoBehaviour
{
    // 같은 오브젝트의 스포너
    private CustomerSpawner _customerSpawner;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        UpdateServeInput();
    }

    /// <summary> KYH : 같은 오브젝트의 CustomerSpawner를 찾아 둔다 </summary>
    private void InitializeComponents()
    {
        _customerSpawner = GetComponent<CustomerSpawner>();

        if (_customerSpawner == null)
        {
            CPrint.Warning("[CustomerDebugInput] CustomerSpawner 없음");
        }
    }

    /// <summary> KYH : Space를 누르면 대기 중인 손님에게 요리를 준다 (에디터에서만) </summary>
    private void UpdateServeInput()
    {
#if UNITY_EDITOR
        if (_customerSpawner == null)
        {
            return;
        }

        if (!Managers.Input.KeyDown_Space)
        {
            return;
        }

        ServeWaitingCustomers();
#endif
    }

    /// <summary> KYH : 주문한 순서대로 대기 중인 손님에게 요리를 준다. 받은 손님은 식사, 결제를 거쳐 나간다 </summary>
    private void ServeWaitingCustomers()
    {
        // 주문했는데 아직 요리를 못 받은 손님이 있을 때만 준다
        while (_customerSpawner.ServedCount < _customerSpawner.OrderedCount)
        {
            if (!_customerSpawner.DeliverFood())
            {
                break;
            }
        }
    }
}
