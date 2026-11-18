using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary> KDH : 버튼에 마우스를 올렸을 때 효과음을 재생하는 컴포넌트 </summary>
public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private SoundKey.Name.SfxType _sfxType;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button == null || !_button.IsInteractable())
        {
            return;
        }

        Managers.Sound?.PlaySfx(_sfxType);
    }
}