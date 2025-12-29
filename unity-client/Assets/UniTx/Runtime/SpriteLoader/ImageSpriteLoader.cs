using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UniTx.Runtime.ResourceManagement;

namespace UniTx.Runtime.SpriteLoader
{
    [RequireComponent(typeof(Image))]
    public sealed class ImageSpriteLoader : MonoBehaviour
    {
        [SerializeField] private string _format;
        [SerializeField] private bool _setNativeSize;

        private Image _image;
        private Sprite _sprite;

        public async UniTask LoadSpriteAsync(string[] args, CancellationToken cToken = default)
        {
            var key = string.Format(_format, args);
            var _sprite = await UniResources.LoadAssetAsync<Sprite>(key, cToken: cToken);

            _image.sprite = _sprite;

            if (_setNativeSize)
            {
                _image.SetNativeSize();
            }

            _image.enabled = true;
        }

        public void UnloadSprite()
        {
            if (_sprite != null)
            {
                UniResources.DisposeAsset(_sprite);
                _sprite = null;
            }

            _image.sprite = null;
            _image.enabled = false;
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.enabled = false;
        }
    }
}