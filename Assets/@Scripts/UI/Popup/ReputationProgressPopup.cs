using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> KDH : 영업 후 획득한 만족도와 평판 상승을 보여준다. </summary>
public class ReputationProgressPopup : BasePopup
{
    private enum Images
    {
        ProgressFill,
        RankStar
    }

    private enum Texts
    {
        GainText,
        RankText,
        ProgressText,
        PromotionText
    }

    private Sequence _sequence;

    protected override void OnAwake()
    {
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void OnStart()
    {
    }

    protected override void OnOpened()
    {
        base.OnOpened();
        
        Managers.Sound?.PlaySfx(SoundKey.Name.SfxType.Reputation);
    }

    protected override void OnUpdate()
    {
    }

    public override void OnInputKey()
    {
    }

    protected override void DestroyOverride()
    {
        _sequence?.Kill();
    }

    /// <summary> KDH : 등급 경계마다 게이지를 채우고 평판 상승 연출을 재생한 뒤 정산 화면으로 전환한다. </summary>
    public void Play(int before, int after, Action onClosed)
    {
        _sequence?.Kill();

        ReputationTable table = Managers.Table.Reputation;
        int grade = table.GetReputationGrade(before);
        int endGrade = table.GetReputationGrade(after);

        // 초기 평판 정보 설정
        Get<TMP_Text>(Texts.RankText).SetText($"{grade}");
        Get<TMP_Text>(Texts.GainText).SetText($"+{after - before:N0}");
        Get<TMP_Text>(Texts.PromotionText).SetText(string.Empty);
        
        ShowProgress(table, grade, before);

        // 평판 상승 애니메이션 시작
        _sequence = DOTween.Sequence().SetUpdate(true).AppendInterval(0.3f);

        int segmentStart = before;
        
        // 상승한 등급마다 게이지를 채우고 승급 연출 재생
        for (int nextGrade = grade + 1; nextGrade <= endGrade; nextGrade++)
        {
            int promotedGrade = nextGrade;
            int threshold = table.GetReputationByGrade(promotedGrade).RequiredSatisfaction;
            
            // 현재 등급의 게이지를 끝까지 채운다.
            AppendProgress(table, promotedGrade - 1, segmentStart, threshold);
            _sequence.AppendInterval(0.2f);
            
            // 다음 등급으로 전환하고 승급 메시지 표시
            _sequence.AppendCallback(() =>
            {
                Get<TMP_Text>(Texts.RankText).SetText($"{promotedGrade}");
                ShowProgress(table, promotedGrade, threshold);
                Get<TMP_Text>(Texts.PromotionText).SetText("평판 상승!");
            });
            
            // 승급 시 별 아이콘 강조
            _sequence.Append(Get<Image>(Images.RankStar).rectTransform
                .DOPunchScale(Vector3.one * 0.3f, 0.5f, 5, 0.6f));
            
            _sequence.AppendInterval(0.25f);
            
            segmentStart = threshold;
        }

        // 마지막 등급의 남은 만족도만큼 게이지를 채운다.
        if (after > segmentStart)
        {
            AppendProgress(table, endGrade, segmentStart, after);
        }

        // 연출 종료 후 팝업을 닫고 정산 화면으로 전환
        _sequence.AppendInterval(0.65f).OnComplete(() =>
        {
            _sequence = null;
            Close(onClosed);
        });
    }

    /// <summary> KDH : 지정한 평판 구간의 만족도를 증가시키며 게이지 애니메이션을 추가한다. </summary>
    private void AppendProgress(ReputationTable table, int grade, int start, int end)
    {
        _sequence.Append(DOVirtual.Int(start, end, 0.75f,
            value => ShowProgress(table, grade, value)).SetEase(Ease.OutCubic));
    }

    /// <summary> KDH : 현재 등급의 만족도 구간을 기준으로 평판 게이지와 진행 수치를 갱신한다. </summary>
    private void ShowProgress(ReputationTable table, int grade, int satisfaction)
    {
        ReputationData rank = table.GetReputationByGrade(grade);
        ReputationData next = table.GetNextReputation(grade);
        
        if (next == null)
        {
            Get<Image>(Images.ProgressFill).fillAmount = 1f;
            Get<TMP_Text>(Texts.ProgressText).SetText("최고 평판");
            return;
        }

        int start = rank?.RequiredSatisfaction ?? 0;
        int required = next.RequiredSatisfaction - start;
        int progress = Mathf.Clamp(satisfaction - start, 0, required);
        
        Get<Image>(Images.ProgressFill).fillAmount = required > 0 ? (float)progress / required : 1f;
        Get<TMP_Text>(Texts.ProgressText).SetText($"{progress:N0} / {required:N0}");
    }
}