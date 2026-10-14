using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 씬 로딩 진행률을 표시하고 다음 씬으로 진행할 입력을 처리하는 HUD다. </summary>
public class LoadingHUD : BaseHUD
{
    private LoadingScene _loadingScene;
    
    private enum Sliders
    {
        LoadingSlider,
    }

    private enum Texts
    {
        AlertTxt,
    }

    public float LoadingProgress => Get<Slider>(Sliders.LoadingSlider).value;

    private Tween _continueMessageTween;

    protected override void OnAwake()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
        Get<TMP_Text>(Texts.AlertTxt).gameObject.SetActive(false);
        
        if (!Managers.Scene.TryGetCurrentScene(out _loadingScene))
        {
            CPrint.Error("[LoadingHUD] No Scene");
        }
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        if (LoadingProgress < 1f)
        {
            return;
        }

        if (Managers.Input.MouseDown_Left)
        {
            _loadingScene?.SetContinueRequested(true);
        }
    }

    private void OnDisable()
    {
        _continueMessageTween?.Kill();
        _continueMessageTween = null;
    }

    /// <summary> KDH : 로딩 진행률을 설정하고 완료 시 계속 진행 안내를 표시한다. </summary>
    public void SetLoadingProgress(float progress)
    {
        Slider loadingSlider = Get<Slider>(Sliders.LoadingSlider);
        loadingSlider.value = Mathf.Clamp01(progress);

        if (loadingSlider.value >= 1f)
        {
            ShowContinueMessage();
        }
    }

    /// <summary> KDH : 클릭 시 다음 씬으로 전환한다는 문구를 보여준다. </summary>
    private void ShowContinueMessage()
    {
        TMP_Text alertText = Get<TMP_Text>(Texts.AlertTxt);

        if (alertText == null || alertText.gameObject.activeSelf)
        {
            return;
        }

        Get<Slider>(Sliders.LoadingSlider).gameObject.SetActive(false);

        _continueMessageTween?.Kill();

        alertText.alpha = 0f;
        alertText.transform.localScale = Vector3.one * 0.95f;

        Sequence showSequence = DOTween.Sequence();

        // 문구를 등장
        alertText.gameObject.SetActive(true);
        showSequence.Append(alertText.DOFade(1f, 0.8f).SetEase(Ease.OutSine));
        showSequence.Join(alertText.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBack));

        showSequence.OnComplete(() =>
        {
            _continueMessageTween = DOTween.Sequence().Append(alertText.DOFade(0.5f, 0.9f).SetEase(Ease.InOutSine))
                .Append(alertText.DOFade(1f, 0.9f).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Restart);
        });

        _continueMessageTween = showSequence;
    }
}