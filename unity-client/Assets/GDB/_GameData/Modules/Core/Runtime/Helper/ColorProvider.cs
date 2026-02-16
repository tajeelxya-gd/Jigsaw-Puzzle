using System.Linq;
using UnityEngine;

public static class ColorProvider
{
    private static ColorData _colorData;
    public static Color HiddenColor
    {
        get
        {
            return new Color(0.5f, 0.5f, 0.5f, 1);
        }
    }
    public static void Initialize()
    {
        if (!_colorData)
            _colorData = Resources.Load<ColorData>("ColorData");
    }
    public static Color GetColor(ColorType colorType)
    {
        return _colorData._ColorInfo
                    .FirstOrDefault(x => x.colorType == colorType)?.color
                    ?? Color.white;
    }
    public static Texture GetQuestionMarkTexture(ShooterType shooterType)
    {
        return shooterType == ShooterType.SimpleCannon ? _colorData.QuestionMarkTexture : _colorData.BigCannonQuestionMarkTexture;
    }
    public static Material GetMaterial(ColorType colorType, EnemyType enemyType)
    {
        return _colorData.GetMaterial(colorType, enemyType);
    }
}