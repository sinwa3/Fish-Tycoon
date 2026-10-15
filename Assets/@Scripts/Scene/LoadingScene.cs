using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> KDH : 게임 데이터와 다음 Scene을 로드하고 로딩 순서를 관리한다. </summary>
public class LoadingScene : BaseScene
{
    #region ===== 참조 =====

    private LoadingHUD _hud;

    #endregion ===== 참조 =====

    #region ===== 상태 =====

    private bool _isContinueRequested;

    #endregion ===== 상태 =====

    protected override void OnAwake()
    {
        _hud = GetComponentInChildren<LoadingHUD>(true);

        if (_hud == null)
        {
            CPrint.Error("[LoadingScene] LoadingHUD를 찾을 수 없습니다.");
        }
    }

    protected override void OnStart()
    {
        if (Managers.Scene.NextScene == SceneType.None)
        {
            CPrint.Error("[LoadingScene] NextScene이 설정되지 않았습니다.");
            return;
        }

        LoadSceneAsync(Managers.Scene.NextScene, this.GetCancellationTokenOnDestroy()).Forget();
    }

    protected override void OnUpdate()
    {
    }

    /// <summary> KDH : 다음 Scene으로 전환하도록 요청한다. </summary>
    public void SetContinueRequested(bool isContinueRequested)
    {
        _isContinueRequested = isContinueRequested;
    }

    /// <summary> KDH : 게임 데이터와 다음 Scene을 로드한다. </summary>
    private async UniTask LoadSceneAsync(SceneType nextScene, CancellationToken cancelToken)
    {
        try
        {
            // 1. 테이블 로딩
            if (!Managers.Table.IsLoaded)
            {
                await Managers.Table.LoadAsync((message) => _hud?.SetLoadingText(message));
            }

            cancelToken.ThrowIfCancellationRequested();

            // 2. 다음 Scene 비동기 로딩
            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);

            AsyncOperation operation = SceneManager.LoadSceneAsync(nextScene.ToString());

            if (operation == null)
            {
                CPrint.Error($"[LoadingScene] Scene 로드 실패: {nextScene}");
                return;
            }

            operation.allowSceneActivation = false;

            // 3. Scene 로딩 진행률 표시
            await LoadSceneProgressAsync(operation, cancelToken);

            // 4. 씬 로딩 완료(operation.progress == 0.9)후 게이지를 100%까지 채운다.
            await _hud.FillLoadingProgressAsync(1f, cancelToken);

            // 5. 사용자 클릭 대기
            await UniTask.WaitUntil(() => _isContinueRequested, cancellationToken: cancelToken);

            Managers.Scene.ClearCurrentScene();
            // 6. 씬 활성 완료(operation.progress == 1)
            operation.allowSceneActivation = true;
        }
        catch (OperationCanceledException)
        {
            // 로딩 씬 파괴 시 (로딩 씬에서 플레이 정지)
        }
        catch (Exception exception)
        {
            CPrint.Error($"[LoadingScene] {exception}");
        }
    }

    /// <summary> KDH : Scene 로딩 진행률을 HUD에 반영한다. </summary>
    private async UniTask LoadSceneProgressAsync(AsyncOperation operation, CancellationToken cancellationToken)
    {
        while (operation.progress < 0.9f)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Unity의 Scene 진행률 0~0.9를 0~1로 변환한다.
            float sceneProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // HUD 게이지를 실제 Scene 진행률까지 증가시킨다.
            await _hud.FillLoadingProgressAsync(sceneProgress, cancellationToken);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
    }
}