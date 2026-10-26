/// <summary> KDH : Resources 폴더에 있는 리소스를 가져오기 위한 키 </summary>
public abstract class ResourceKey
{
    public static class Path
    {
        public const string Prefab = "Prefab/";
        public const string Table = "Table/";
        public const string Sprite = "Sprite/";
            
        public const string Misc = Prefab + "Misc/";
        public const string Item = Prefab + "Item/";
        
        public const string UI = Prefab + "UI/";
        public const string ScreenUI = UI + "ScreenUI/";
        public const string PopupUI = UI + "PopupUI/";
        public const string OverlayUI = UI + "OverlayUI/";
        public const string SlotUI = UI + "SlotUI/";
        public const string TooltipUI = UI + "Tooltip/";
        
        public const string ItemSprite = Sprite + "Item/";
        public const string FishSprite = Sprite + "Fish/";
        public const string FoodSprite = Sprite + "Food/";
    }

    public static class Name
    {
        public const string Event = "EventSystem";
    }
}