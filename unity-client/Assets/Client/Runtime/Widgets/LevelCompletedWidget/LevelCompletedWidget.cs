using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniTx.Runtime.Audio;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class LevelCompletedWidget : MonoBehaviour, IWidget, IInjectable
    {
        [SerializeField] private Button _nextLevelBtn;
        [SerializeField] private TMP_Text _nextLevelText;
        [SerializeField] private ScriptableObject _levelCompleted;

        private IPuzzleService _puzzleService;

        public GameObject GameObject => gameObject;

        public Transform Transform => transform;

        public void Inject(IResolver resolver) => _puzzleService = resolver.Resolve<IPuzzleService>();

        public void Initialise()
        {
            _nextLevelBtn.onClick.AddListener(HandleNextLevel);
            SetLevelText();
            UniAudio.Play2D((IAudioConfig)_levelCompleted);
        }

        public void Reset()
        {
            _nextLevelBtn.onClick.RemoveListener(HandleNextLevel);
        }

        private void HandleNextLevel() => UniTask.Void(HandleNextLevelAsync, this.GetCancellationTokenOnDestroy());

        private async UniTaskVoid HandleNextLevelAsync(CancellationToken cToken = default)
        {
            await UniWidgets.PushAsync<LoadingWidget>(cToken);
            _puzzleService.UnLoadPuzzle();
            await _puzzleService.LoadPuzzleAsync(cToken);
            await UniWidgets.PopWidgetsStackAsync(cToken);
            await UniWidgets.PopWidgetsStackAsync(cToken);
        }

        private void SetLevelText()
        {
            var levelName = _puzzleService.GetNextLevelData().Name;
            _nextLevelText.SetText($"Next Level: {levelName}");
        }
    }
}
