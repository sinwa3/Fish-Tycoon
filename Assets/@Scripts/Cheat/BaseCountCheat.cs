#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;

/// <summary> 목록에서 하나를 골라 개수만큼 넣고 빼는 치트의 기본 클래스 </summary>
public abstract class BaseCountCheat : BaseCheat
{
    private static readonly int[] ChangeCounts = { 1, 5 };

    // 테이블은 로딩 씬에서 불러와서 처음 그릴 때 ID를 모은다
    private readonly List<int> _ids = new();
    private int _selectedIndex;

    public override void DrawButtons()
    {
        InitializeIDs();

        if (_ids.Count == 0)
        {
            DrawLabel("고를 것이 없다");

            return;
        }

        int id = _ids[_selectedIndex];

        GUILayout.BeginHorizontal();
        DrawButton("<", () => MoveSelection(-1));
        DrawLabel($"{GetName(id)} ({GetCount(id)}개)");
        DrawButton(">", () => MoveSelection(1));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        foreach (int count in ChangeCounts)
        {
            DrawButton($"+{count}", () => Add(id, count));
        }

        foreach (int count in ChangeCounts)
        {
            DrawButton($"-{count}", () => Remove(id, count));
        }

        GUILayout.EndHorizontal();
    }

    /// <summary> 고를 수 있는 ID를 모은다 </summary>
    protected abstract void CollectIDs(List<int> ids);

    protected abstract string GetName(int id);

    /// <summary> 지금 몇 개인지 돌려준다 </summary>
    protected abstract int GetCount(int id);

    protected abstract void Add(int id, int count);

    protected abstract void Remove(int id, int count);

    private void InitializeIDs()
    {
        if (_ids.Count > 0)
        {
            return;
        }

        CollectIDs(_ids);
        _ids.Sort();
    }

    private void MoveSelection(int direction)
    {
        _selectedIndex = (_selectedIndex + direction + _ids.Count) % _ids.Count;
    }
}
#endif
