using UnityEngine;
using UnityEngine.UI;

public class AiProfilePicture : MonoBehaviour, IAiProfilePicture
{
    [SerializeField] private Image _profilePicture;
    
    public void SetProfilePicture(Sprite profileSprite)
    {
        _profilePicture.sprite = profileSprite;
    }
    
    public Sprite GetProfilePicture()
    {
        return _profilePicture.sprite;
    }
}

public interface IAiProfilePicture
{
    public void SetProfilePicture(Sprite profileSprite);
    public Sprite GetProfilePicture();
}
