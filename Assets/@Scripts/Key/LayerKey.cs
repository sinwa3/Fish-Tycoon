public abstract class LayerKey
{
    public static class Idx
    {
        public const int Default = 0;
        public const int TransprentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Water = 4;
        public const int UI = 5;
        public const int Furniture = 6;
        public const int Fish = 7;
        public const int SecurityMinigame = 8;
        public const int Wall = 9;
        public const int Door = 10;
    }

    public static class Mask
    {
        public const int Default = 1 << Idx.Default;
        public const int TransprentFX = 1 << Idx.TransprentFX;
        public const int IgnoreRaycast = 1 << Idx.IgnoreRaycast;
        public const int Water = 1 << Idx.Water;
        public const int UI = 1 << Idx.UI;
        public const int Furniture = 1 << Idx.Furniture;
        public const int Fish = 1 << Idx.Fish;
        public const int SecurityMinigame = 1 << Idx.SecurityMinigame;
        public const int Wall = 1 << Idx.Wall;
        public const int Door = 1 << Idx.Door;
    }
}
