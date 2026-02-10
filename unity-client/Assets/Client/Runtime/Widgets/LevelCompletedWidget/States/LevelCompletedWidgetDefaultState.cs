using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Audio;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public class LevelCompletedWidgetDefaultState : WidgetState<LevelCompletedWidgetContext, LevelCompletedWidgetData, LevelCompletedWidgetRefs>
    {
        public override void OnEnter()
        {
            Context.Refs.NextLevelBtn.onClick.AddListener(HandleNextLevel);
            SetLevelText();
            UniAudio.Play2D((IAudioConfig)Context.Refs.LevelCompleted);
        }

        public override void OnExit()
        {
            Context.Refs.NextLevelBtn.onClick.RemoveListener(HandleNextLevel);
        }

        private void HandleNextLevel() => UniTask.Void(HandleNextLevelAsync, Context.CancellationToken);

        private async UniTaskVoid HandleNextLevelAsync(CancellationToken cToken)
        {
            await UniWidgets.PushAsync<LoadingWidget>(cToken);
            Context.PuzzleService.UnLoadPuzzle();
            await Context.PuzzleService.LoadPuzzleAsync(cToken);
            await UniWidgets.PopWidgetsStackAsync(cToken);
            await UniWidgets.PopWidgetsStackAsync(cToken);
        }

        private void SetLevelText()
        {
            var levelName = Context.PuzzleService.GetNextLevelData().Name;
            Context.Refs.NextLevelText.SetText($"Next Level: {levelName}");
        }
    }
}