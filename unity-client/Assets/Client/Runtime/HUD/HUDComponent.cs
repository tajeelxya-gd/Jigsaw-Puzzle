using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.IoC;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class HUDComponent : MonoBehaviour, IInjectable
    {
        [SerializeField] private Button _reset;
        [SerializeField] private Toggle _cornerPieces;
        [SerializeField] private Toggle _dropPieces;
        [SerializeField] private Toggle _eye;

        private IPuzzleTray _puzzleTray;

        public void Inject(IResolver resolver)
        {
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
        }

        private void Awake() => RegisterEvents();

        private void OnDestroy() => UnRegisterEvents();

        private void RegisterEvents()
        {
            _reset.onClick.AddListener(HandleReset);
            _cornerPieces.onValueChanged.AddListener(HandleCornerPieces);
            _dropPieces.onValueChanged.AddListener(HandleDropPieces);
            _eye.onValueChanged.AddListener(HandleEye);
        }

        private void UnRegisterEvents()
        {
            _reset.onClick.RemoveListener(HandleReset);
            _cornerPieces.onValueChanged.RemoveListener(HandleCornerPieces);
            _dropPieces.onValueChanged.RemoveListener(HandleDropPieces);
            _eye.onValueChanged.RemoveListener(HandleEye);
        }

        private void HandleReset()
        {
        }

        private void HandleCornerPieces(bool value)
        {
        }

        private void HandleDropPieces(bool value) => HandleDropPiecesAsync(value, this.GetCancellationTokenOnDestroy()).Forget();

        private async UniTaskVoid HandleDropPiecesAsync(bool value, CancellationToken cToken = default)
        {
            if (value)
            {
                _puzzleTray.PickPieces();
                return;
            }

            _dropPieces.interactable = false;
            await _puzzleTray.DropPiecesAsync(cToken);
            _dropPieces.interactable = true;
        }

        private void HandleEye(bool value)
        {
        }
    }
}
