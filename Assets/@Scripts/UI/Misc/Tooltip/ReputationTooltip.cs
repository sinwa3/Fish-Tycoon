using TMPro;
using UnityEngine;

/// <summary> KDH : 평판 아이콘에 올렸을 때 평판에 대한 정보를 보여준다. </summary>
public class ReputationTooltip : BaseTooltip
{
    public enum Texts
    {
        NameText,
        DescText,
    }
    
    private string _prevReputationLevel;
    private ReputationData _reputationData;
    
    protected override void OnAwake()
    {
        Bind<TMP_Text>(typeof(Texts));
    }

    protected override void UpdateContent()
    {
    }
    
    /// <summary> KDH : 평판 정보를 보여준다. </summary>
    public void Show(string reputationLevel, RectTransform trigger)
    {
        if (_reputationData == null || !_prevReputationLevel.Equals(reputationLevel))
        {
            if (!int.TryParse(reputationLevel, out int level))
            {
                level = 0;
            }
            
            _reputationData = Managers.Table.Reputation.GetReputationByGrade(level);
            _prevReputationLevel = reputationLevel;
        }

        Get<TMP_Text>(Texts.NameText).SetText(_reputationData?.ReputationName);
        Get<TMP_Text>(Texts.DescText).SetText(_reputationData?.ReputationDesc);

        base.Show(trigger);
    }
}