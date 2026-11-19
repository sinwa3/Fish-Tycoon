using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 로딩 게이지와 상태 문구를 표시하고 다음 Scene으로 진행할 입력을 처리한다. </summary>
public class LoadingHUD : BaseHUD
{
    private enum Sliders
    {
        LoadingSlider,
    }

    private enum Images
    {
        Bg,
    }

    private enum Texts
    {
        AlertText,
        StatusText,
        TipText,
    }

    #region ===== 참조 =====

    [SerializeField] private Sprite[] _titleFrames;
    [SerializeField] private Sprite[] _dayFrames;
    [SerializeField] private Sprite[] _nightFrames;
    [SerializeField, Min(0.01f)] private float _frameDuration = 0.22f;

    private Sprite[] _frames;
    private float _frameTime;
    private int _frameIndex;
    private LoadingScene _loadingScene;
    private Tween _continueMessageTween;

    #endregion ===== 참조 =====

    protected override void OnAwake()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        SetTheme(Managers.Scene.PrevScene, Managers.Scene.NextScene);
        
        _loadingScene = gameObject.FindParent<LoadingScene>();
    }

    protected override void OnStart()
    {
        Get<TMP_Text>(Texts.AlertText).gameObject.SetActive(false);
        Get<TMP_Text>(Texts.TipText).gameObject.SetActive(false);
        
        if (Managers.Scene.PrevScene == SceneType.TitleScene && Managers.Scene.NextScene == SceneType.TycoonScene)
        {
            SetLoadingTipAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    protected override void OnUpdate()
    {
        PlayBackgroundAnimation();
    }

    public override void OnInputKey()
    {
        if (!IsLoadingComplete())
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
        Get<Slider>(Sliders.LoadingSlider).value = Mathf.Clamp01(progress);

        if (Get<Slider>(Sliders.LoadingSlider).value >= 1f)
        {
            ShowContinueMessage();
        }
    }

    /// <summary> KDH : 로딩 상태 문구를 표시한다. </summary>
    public void SetLoadingText(string message)
    {
        Get<TMP_Text>(Texts.StatusText).text = message;
    }

    /// <summary> KDH : 목표 진행률까지 게이지를 증가시키며 퍼센트를 표시한다. </summary>
    public async UniTask FillLoadingProgressAsync(float targetProgress, CancellationToken cancelToken)
    {
        Slider loadingSlider = Get<Slider>(Sliders.LoadingSlider);
        targetProgress = Mathf.Clamp01(targetProgress);

        while (loadingSlider.value < targetProgress)
        {
            cancelToken.ThrowIfCancellationRequested();

            loadingSlider.value = Mathf.MoveTowards(loadingSlider.value, targetProgress, Time.deltaTime);

            SetLoadingText($"로딩 중 ({Mathf.FloorToInt(loadingSlider.value * 100f)}%)");

            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);
        }

        // 목표 진행률에 정확히 도달했는지 확인하고 마지막 값을 반영한다.
        loadingSlider.value = targetProgress;

        if (targetProgress >= 1f)
        {
            SetLoadingText("로딩 완료 (100%)");
            ShowContinueMessage();
        }
        else
        {
            SetLoadingText($"로딩 중 ({Mathf.FloorToInt(targetProgress * 100f)}%)");
        }
    }

    /// <summary> KDH : 현재 게이지가 100%인지 확인한다. </summary>
    private bool IsLoadingComplete()
    {
        return Get<Slider>(Sliders.LoadingSlider).value >= 1f;
    }

    /// <summary> KDH : 클릭 시 다음 Scene으로 전환한다는 문구를 표시한다. </summary>
    private void ShowContinueMessage()
    {
        TMP_Text alertText = Get<TMP_Text>(Texts.AlertText);

        if (alertText == null || alertText.gameObject.activeSelf)
        {
            return;
        }

        Get<Slider>(Sliders.LoadingSlider).gameObject.SetActive(false);

        _continueMessageTween?.Kill();

        alertText.alpha = 0f;
        alertText.transform.localScale = Vector3.one * 0.95f;
        alertText.gameObject.SetActive(true);

        Sequence showSequence = DOTween.Sequence();
        _continueMessageTween = showSequence;
        
        showSequence.Append(alertText.DOFade(1f, 0.8f).SetEase(Ease.OutSine));
        showSequence.Join(alertText.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBack));
        showSequence.OnComplete(() =>
        {
            _continueMessageTween = DOTween.Sequence().Append(alertText.DOFade(0.5f, 0.9f).SetEase(Ease.InOutSine))
                .Append(alertText.DOFade(1f, 0.9f).SetEase(Ease.InOutSine)).SetLoops(-1, LoopType.Restart);
        });
    }
    
    #region ===== 배경 애니메이션 =====
    
    /// <summary> KDH : BG의 스프라이트 애니메이션을 재생한다. </summary>
    private void PlayBackgroundAnimation()
    {
        Image background = Get<Image>(Images.Bg);

        if (background == null || _frames == null || _frames.Length < 2)
        {
            return;
        }

        _frameTime += Time.unscaledDeltaTime;
        
        int steps = Mathf.FloorToInt(_frameTime / _frameDuration);
        if (steps == 0)
        {
            return;
        }

        _frameTime -= steps * _frameDuration;
        _frameIndex = (_frameIndex + steps) % _frames.Length;
        background.sprite = _frames[_frameIndex];
    }

    /// <summary> KDH : 출발 씬과 목적지에 맞는 배경 프레임을 설정한다. </summary>
    private void SetTheme(SceneType previous, SceneType next)
    {
        _frames = (previous, next) switch
        {
            (SceneType.TitleScene, SceneType.TycoonScene) => _titleFrames,
            (SceneType.TycoonScene, SceneType.StealthScene) => _dayFrames,
            (SceneType.StealthScene, SceneType.TycoonScene) => _nightFrames,
            _ => null,
        };

        Image background = Get<Image>(Images.Bg);

        if (background == null)
        {
            return;
        }

        Sprite firstFrame = _frames is { Length: > 0 } ? _frames[0] : null;

        background.type = Image.Type.Simple;
        background.preserveAspect = firstFrame != null;
        background.color = firstFrame != null ? Color.white : Color.black;
        background.sprite = firstFrame;
    }

    /// <summary> KDH : 로딩 팁 테이블이 준비되면 무작위 팁을 표시한다. </summary>
    private async UniTask SetLoadingTipAsync(CancellationToken cancelToken)
    {
        TMP_Text tipText = Get<TMP_Text>(Texts.TipText);

        if (tipText == null)
        {
            return;
        }
        
        string defaultText = tipText.text;
        tipText.gameObject.SetActive(true);
        tipText.text = string.Empty;

        await UniTask.WaitUntil(() => Managers.Table.LoadingTip?.Map?.Count > 0, cancellationToken: cancelToken);
        
        int randomIndex = Random.Range(0, Managers.Table.LoadingTip.Map.Count);
        tipText.SetText(Managers.Table.LoadingTip?.Map.TryGetValue(randomIndex, out LoadingTipData value) == true
            ? $"TIP. {value.Desc}"
            : defaultText);
    }
    
    #endregion ===== 배경 애니메이션 =====
}