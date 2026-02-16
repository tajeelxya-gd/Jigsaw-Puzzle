using UnityEngine;
using Sirenix.OdinInspector; // Add this

[CreateAssetMenu(fileName = "Profile-Avatars", menuName = "Scriptable Objects/AvatarsData")]
public class ProfileAvatarsData : ScriptableObject
{
    [PreviewField(60, ObjectFieldAlignment.Left)] // Shows a 60px thumbnail
    [SerializeField] private Sprite[] allProfileAvatars;

    public Sprite GetProfileAvatar(int id) => allProfileAvatars[id];
}