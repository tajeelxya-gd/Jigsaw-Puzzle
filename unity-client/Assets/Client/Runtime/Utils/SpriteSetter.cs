using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    [RequireComponent(typeof(Image))]
    public class SpriteSetter : MonoBehaviour
    {
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Sprite _unSelectedSprite;
        private Image _image;

        private void Awake() => _image = GetComponent<Image>();

        public void SetSelectedSprite() => _image.sprite = _selectedSprite;
        public void SetUnSelectedSprite() => _image.sprite = _unSelectedSprite;
    }
}
