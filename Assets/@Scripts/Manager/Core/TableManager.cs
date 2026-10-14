using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary> KDH : CSV 형태의 테이블들을 관리하고 로드한다. </summary>
public class TableManager
{
    #region ===== 테이블 =====

    public ItemTable Item { get; private set; }
    public ShopTable Shop { get; private set; }

    #endregion ===== 테이블 =====

    #region ===== 상태 =====

    private bool _isLoading;

    private bool _isLoaded;
    public bool IsLoaded => _isLoaded;

    #endregion ===== 상태 =====

    /// <summary> KDH : 로드할 테이블 리스트을 생성한다. [새로운 테이블을 추가할 때 다음 리스트에 추가] </summary>
    private List<(string, Action)> CreateTablesToLoad()
    {
        return new List<(string, Action)>
        {
            ("아이템테이블", () =>
            {
                Item = new ItemTable();
                Item.Load();
            }),
            ("상점테이블", () =>
            {
                Shop = new ShopTable();
                Shop.Load();
            })
        };
    }

    /// <summary> KDH : CSV 형태의 테이블들을 순서대로 로드한다. </summary>
    public async UniTask LoadAsync(Action<string> onProgress = null)
    {
        if (_isLoaded)
        {
            onProgress?.Invoke("게임 데이터 준비 완료");
            return;
        }

        if (_isLoading)
        {
            throw new InvalidOperationException("[TableManager] 테이블이 이미 로드 중입니다.");
        }

        _isLoading = true;

        try
        {
            List<(string tableName, Action load)> loadSteps = CreateTablesToLoad();

            foreach (var step in loadSteps)
            {
                CPrint.Log($"{step.tableName} 로드 전");

                step.load();

                onProgress?.Invoke($"{step.tableName} 로드 완료");
                CPrint.Log($"{step.tableName} 로드 완료");

                // UI가 문구를 표시할 수 있도록 프레임을 양보한다.
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            _isLoaded = true;

            onProgress?.Invoke("게임 데이터 준비 완료");
        }
        catch (Exception exception)
        {
            CPrint.Error($"[TableManager] 테이블 로드 실패: {exception}");
            throw;
        }
        finally
        {
            _isLoading = false;
        }
    }
}