using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    private Button _button;
    
    public SoundKey.Name.SfxType sfxType;
    
    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_button != null)
        {
            _button.onClick.AddListener(() => Managers.Sound.PlaySfx(sfxType));
        }
    }
}
