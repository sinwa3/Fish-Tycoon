using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> KDH : 현재 게임 씬을 관리하고 로딩 씬을 통한 씬 전환을 처리한다. </summary>
public class ScenesManager : MonoBehaviour
{
    #region ===== 현재 씬 =====

    private BaseScene _currentScene;
    public BaseScene CurrentScene => _currentScene;

    #endregion ===== 현재 씬 =====

    #region ===== 다음 씬 =====

    private SceneType _prevScene = SceneType.None;
    public SceneType PrevScene => _prevScene;
    private SceneType _nextScene = SceneType.None;
    public SceneType NextScene => _nextScene;

    #endregion ===== 다음 씬 =====

    public event Action OnSceneLoaded;
    
    #region ===== 씬 관리 =====

    /// <summary> KDH : 현재 활성화된 BaseScene을 등록한다. </summary>
    public void SetCurrentScene(BaseScene scene)
    {
        if (scene == null)
        {
            CPrint.Error("등록할 BaseScene이 null입니다.");
            return;
        }

        CPrint.Log($"SetCurrentScene ({scene.GetType().Name})");

        _currentScene = scene;
        
        if (_currentScene.Type != SceneType.LoadingScene)
        {
            _prevScene = SceneType.None;
            _nextScene = SceneType.None;
        }
        
        OnSceneLoaded?.Invoke();
    }

    /// <summary> KDH : 로딩 씬을 거쳐 지정된 씬으로 전환한다. </summary>
    public void LoadSceneWithLoading(SceneType nextScene)
    {
        if (_currentScene == null)
        {
            CPrint.Error("현재 Scene에 BaseScene이 존재하지 않습니다.");
            return;
        }

        _prevScene = _currentScene.Type;
        CPrint.Log($"LoadSceneWithLoading [PrevScene] : ({_prevScene})");
        _nextScene = nextScene;
        CPrint.Log($"LoadSceneWithLoading [NextScene] : ({_nextScene})");

        ClearCurrentScene();

        LoadScene(SceneType.LoadingScene);
    }

    /// <summary> KDH : 로딩 씬을 거치지 않고 동기로 씬을 전환한다. </summary>
    public void LoadScene(SceneType scene)
    {
        ClearCurrentScene();
        
        SceneManager.LoadScene(scene.ToString());
    }

    /// <summary> KDH : 현재 등록된 씬을 정리한다. </summary>
    public void ClearCurrentScene()
    {
        if (_currentScene == null)
        {
            return;
        }

        _currentScene.Clear();
    }

    #endregion ===== 씬 관리 =====
}