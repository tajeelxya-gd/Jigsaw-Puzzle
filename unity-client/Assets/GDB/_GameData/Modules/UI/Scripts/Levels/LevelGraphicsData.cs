using UnityEngine;


[CreateAssetMenu(fileName = "LevelsThemeGraphicsUI", menuName = "Scriptable Objects/LevelsThemeGraphic")]
public class LevelGraphicsData : ScriptableObject
{
    [SerializeField] private LevelType levelType;
    public LevelType LevelType => levelType;

    [SerializeField] private Sprite bgSprite;
    public Sprite BgSprite => bgSprite;

    [SerializeField] private Texture2D towerSprite;
    public Texture2D TowerSprite => towerSprite;
    
    [SerializeField] private Texture2D towerSpriteBack;
    public Texture2D TowerSpriteBackground => towerSpriteBack;
    
    
    
}
