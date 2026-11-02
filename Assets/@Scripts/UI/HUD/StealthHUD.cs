using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StealthHUD : BaseHUD
{
    public enum Texts
    {
        CurrentDayText,
        PhaseText,

        GoldValueText,
        
        SubQuestDescText,
        
        NextBtnText,
    }

    public enum Images
    {
        QuickSlotBar,
        
        SubQuest
    }
    
    public enum Toggles
    {
        SubQuestToggle
    }
    
    #region ===== 참조 =====

    private StealthScene _stealthScene;

    #endregion ===== 참조 =====
    
    #region ===== 애니메이션 =====
    
    // 퀘스트
    private Coroutine _subQuestCoroutine;

    #endregion ===== 애니메이션 =====
    
    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Toggle>(typeof(Toggles));
        
        _stealthScene = gameObject.FindParent<StealthScene>();
    }

    protected override void OnStart()
    {
        SetCurrentDayText();
        
        UpdateGoldText();
        
        SubscribeGoldChanged();
        SubscribeSubQuestEvents();
        
        if (Managers.Quest != null)
        {
            UpdateSubQuest(Managers.Quest.GetCurrentQuest(SceneType.StealthScene, out var progress), progress);
        }
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
        base.OnInputKey();

        if (Managers.Input.KeyDown_Esc)
        {
            Managers.UI.OpenPopup<PausePopup>();
        }
        else if (Managers.Input.KeyDown_E)
        {
            _stealthScene?.ChangeCurrentPhase();
        }
        else if (Managers.Input.MouseDown_Wheel && Cursor.lockState == CursorLockMode.Locked)
        {
            // 커서 잠금 해제
            Managers.Input.SetCursorLock(false);
            Managers.Quest.ReportProgress(SceneType.StealthScene, QuestType.UnLockCursor);
        }
        else if (Managers.Input.MouseDown_Left && Cursor.lockState == CursorLockMode.None)
        {
            // 커서 잠금
            Managers.Input.SetCursorLock(true);
            Managers.Quest.ReportProgress(SceneType.StealthScene, QuestType.LockCursor);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeGoldChanged();
        UnsubscribeSubQuestEvents();
    }

    #region ===== 초기화 =====
    
    /// <summary> KDH : 잠입 씬 첫 진입시 현재 날짜를 세팅한다. </summary>
    private void SetCurrentDayText()
    {
        Get<TMP_Text>(Texts.CurrentDayText)?.SetText($"{Managers.Data.GetCurrentDay()}일 차");
    }
    
    #endregion ===== 초기화 =====
    
    #region ===== 업데이트 =====
    
    /// <summary> KDH : 현재 골드 값을 UI에 업데이트 한다. </summary>
    private void UpdateGoldText()
    {
        Get<TMP_Text>(Texts.GoldValueText)?.SetText($"{Managers.Data.GetCurrentGold():N0} G");
    }
    
    /// <summary> KDH : 현재 상태를 UI에 업데이트 한다. </summary>
    public void UpdatePhaseText(StealthScene.Phase phase)
    {
        string phaseStr = phase switch
        {
            StealthScene.Phase.Playing => "잠입",
            StealthScene.Phase.Settlement => "정산",
            _ => string.Empty
        };

        Get<TMP_Text>(Texts.PhaseText)?.SetText(phaseStr);
        
        // NextBtnText
        string nextBtnStr = phase switch
        {
            StealthScene.Phase.Playing => "[E] 정산하기",
            StealthScene.Phase.Settlement => "[E] 다음 날로 이동",
            _ => string.Empty
        };
        
        Get<TMP_Text>(Texts.NextBtnText)?.SetText(nextBtnStr);
    }
    
    /// <summary> KDH : 해당 퀘스트 정보로 UI를 업데이트한다. </summary>
    private void UpdateSubQuest(QuestData quest, int progress, bool isCompleted = false)
    {
        if (quest == null)
        {
            Get<Image>(Images.SubQuest)?.gameObject.SetActive(false);
            return;
        }

        Get<Image>(Images.SubQuest)?.gameObject.SetActive(true);
        Get<TMP_Text>(Texts.SubQuestDescText)
            ?.SetText($"{quest.Description}\n({Mathf.Clamp(progress, 0, quest.Goal)} / {quest.Goal})\n");
        Get<Toggle>(Toggles.SubQuestToggle)?.SetIsOnWithoutNotify(isCompleted);
    }
    
    #endregion ===== 업데이트 =====
    
    #region ===== 활성화 =====

    /// <summary> KDH : 퀵슬롯을 표시/비표시한다. </summary>
    public void SetQuickSlotVisible(bool isVisible)
    {
        Get<Image>(Images.QuickSlotBar)?.gameObject.SetActive(isVisible);
    }
    
    #endregion ===== 활성화 =====
    
    #region ===== 퀘스트 =====
    
    private void StartSubQuestCompletion(QuestProgressEvent progressEvent)
    {
        if (_subQuestCoroutine != null)
        {
            StopCoroutine(_subQuestCoroutine);
        }

        _subQuestCoroutine = StartCoroutine(Co_ShowSubQuestCompletion(progressEvent));
    }

    private IEnumerator Co_ShowSubQuestCompletion(QuestProgressEvent progressEvent)
    {
        // 완료 표시
        UpdateSubQuest(progressEvent.CompletedQuest, progressEvent.CompletedQuest.Goal, true);
        Managers.Quest.SetCompletionSequenceActive(true);
        yield return new WaitForSeconds(0.8f);

        // 보상 표시
        Get<TMP_Text>(Texts.SubQuestDescText)?.SetText($"{progressEvent.CompletedQuest.GoldReward:N0} G 획득!");
        yield return new WaitForSeconds(1.2f);

        // 다음 퀘스트 표시
        UpdateSubQuest(progressEvent.NextQuest, 0);
        Managers.Quest.SetCompletionSequenceActive(false);
        _subQuestCoroutine = null;
    }
    
    #endregion ===== 퀘스트 =====
    
    #region ===== 이벤트 =====
    
    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독 </summary>
    private void SubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged += UpdateGoldText;
        }
    }

    /// <summary> KDH : 골드 변화시 UI 업데이트를 구독해제 </summary>
    private void UnsubscribeGoldChanged()
    {
        if (Managers.Data != null)
        {
            Managers.Data.OnGoldChanged -= UpdateGoldText;
        }
    }
    
    /// <summary> KDH : 퀘스트 진행도 변화시 UI 업데이트를 구독 </summary>
    private void SubscribeSubQuestEvents()
    {
        if (Managers.Quest != null)
        {
            Managers.Quest.OnQuestProgressed += OnSubQuestProgressed;
        }
    }

    /// <summary> KDH : 퀘스트 진행도 변화시 UI 업데이트를 구독해제 </summary>
    private void UnsubscribeSubQuestEvents()
    {
        if (Managers.Quest != null)
        {
            Managers.Quest.OnQuestProgressed -= OnSubQuestProgressed;
        }
    }
    
    /// <summary> KDH : 퀘스트가 진행됐을 때 서브 퀘스트의 완료 여부에 따라 표시를 달리한다. </summary>
    private void OnSubQuestProgressed(QuestProgressEvent progressEvent)
    {
        if (progressEvent.Scene != SceneType.StealthScene)
        {
            return;
        }

        if (progressEvent.IsCompleted)
        {
            StartSubQuestCompletion(progressEvent);
            return;
        }

        UpdateSubQuest(progressEvent.CurrentQuest, progressEvent.Progress);
    }
    
    #endregion ===== 이벤트 =====
}
