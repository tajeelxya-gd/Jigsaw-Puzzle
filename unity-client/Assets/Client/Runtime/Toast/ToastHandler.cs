using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class ToastHandler : MonoBehaviour, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private Toast _toast;
        private IVFXController _vfxController;

        public void Inject(IResolver resolver) => _vfxController = resolver.Resolve<IVFXController>();

        public void Initialise()
        {
            UniEvents.Subscribe<ShowToastEvent>(HandleShowToastEvent);
        }

        public void Reset()
        {
            UniEvents.Unsubscribe<ShowToastEvent>(HandleShowToastEvent);
        }

        private void HandleShowToastEvent(ShowToastEvent ev)
            => HandleShowToastEventAsync(ev.Data, this.GetCancellationTokenOnDestroy()).Forget();

        private async UniTaskVoid HandleShowToastEventAsync(ToastEventData data, CancellationToken cToken = default)
        {
            await _vfxController.WaitForHighlightsAsync(cToken);
            _toast.Show(data.Message);
        }
    }
}