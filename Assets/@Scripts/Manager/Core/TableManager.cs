public class TableManager
{
    public ItemTable Item { get; private set; }
    
    public ShopTable Shop { get; private set; }

    private bool _isLoaded = false;
    
    /// <summary> KDH : CSV 형태의 테이블들을 로드한다. </summary>
    public void Load()
    {
        if (_isLoaded)
        {
            return;
        }
        
        // 아이템
        Item = new();
        Item.Load();
        
        // 상점
        Shop = new();
        Shop.Load();

        _isLoaded = true;
    }
}