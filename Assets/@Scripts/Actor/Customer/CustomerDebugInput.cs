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

    /// <summary> KYH : 가게 안 손님에게 요리를 준다. 대기 중인 손님만 받고 식사, 결제를 거쳐 나간다 </summary>
    private void ServeWaitingCustomers()
    {
        // TODO : 요리 전달 기능이 올라가면 DeliverFood로 바꾼다 (ReceiveFood를 직접 부르면 요리 받는 순서가 꼬인다)
        foreach (CustomerController customer in _customerSpawner.ActiveCustomers)
        {
            customer.ReceiveFood();
        }
    }
}
