using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class RestartLevelWidget : MonoBehaviour, IWidget, IInjectable
    {
        [SerializeField] private Button _restartBtn;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private TMP_Text _levelText;

        private IPuzzleService _puzzleService;

        public GameObject GameObject => gameObject;

        public Transform Transform => transform;

        public void Inject(IResolver resolver) => _puzzleService = resolver.Resolve<IPuzzleService>();

        public void Initialise()
        {
            _restartBtn.onClick.AddListener(HandleRestartLevel);
            _closeBtn.onClick.AddListener(HandleClose);
            SetLevelText();
        }

        public void Reset()
        {
            _restartBtn.onClick.RemoveListener(HandleRestartLevel);
            _closeBtn.onClick.RemoveListener(HandleClose);
        }

        private void HandleRestartLevel() => UniTask.Void(HandleRestartLevelAsync, this.GetCancellationTokenOnDestroy());

        private async UniTaskVoid HandleRestartLevelAsync(CancellationToken cToken = default)
        {
            await UniWidgets.PushAsync<LoadingWidget>(cToken);
            await _puzzleService.RestartPuzzleAsync(cToken);
            await UniWidgets.PopWidgetsStackAsync(cToken);
            await UniWidgets.PopWidgetsStackAsync(cToken);
        }

        private void HandleClose() => UniWidgets.PopWidgetsStackAsync(this.GetCancellationTokenOnDestroy()).Forget();

        private void SetLevelText()
        {
            var levelName = _puzzleService.GetCurrentLevelData().Name;
            _levelText.SetText(levelName);
        }
    }
}