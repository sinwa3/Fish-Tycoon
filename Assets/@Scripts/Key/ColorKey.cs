using UnityEngine;

public static class ColorKey
{
    public static readonly Color DarkBrown = new Color32(43, 28, 12, 255);
    public static readonly Color DarkGray = new Color32(90, 83, 84, 255);
    public static readonly Color DodgerBlue  = new Color(0.1176471f, 0.5647059f, 1f, 1f);
    public static readonly Color PastelYellow = new Color32(255, 213, 143, 255);
    public static readonly Color CommonColor = new Color32(204, 204, 204, 255);
    public static readonly Color RareColor = new Color32(116, 185, 255, 255);
    public static readonly Color EpicColor = new Color32(204, 160, 224, 255);
    public static readonly Color LegendaryColor = new Color32(255, 242, 204, 255);

    public static Color GetRarityColor(GradeType grade) => grade switch
    {
        GradeType.Rare => RareColor,
        GradeType.Epic => EpicColor,
        GradeType.Legendary => LegendaryColor,
        _ => CommonColor,
    };
}