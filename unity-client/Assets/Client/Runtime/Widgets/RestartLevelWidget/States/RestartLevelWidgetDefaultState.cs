using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public sealed class RestartLevelWidgetDefaultState : WidgetState<RestartLevelWidgetContext, RestartLevelWidgetData, RestartLevelWidgetRefs>
    {
        public override void OnEnter()
        {
            Context.Refs.RestartBtn.onClick.AddListener(HandleRestartLevel);
            Context.Refs.CloseBtn.onClick.AddListener(HandleClose);
            SetLevelText();
        }

        public override void OnExit()
        {
            Context.Refs.RestartBtn.onClick.RemoveListener(HandleRestartLevel);
            Context.Refs.CloseBtn.onClick.RemoveListener(HandleClose);
        }

        private void HandleRestartLevel() => UniTask.Void(HandleRestartLevelAsync, Context.CancellationTokenOnDestroy);

        private async UniTaskVoid HandleRestartLevelAsync(CancellationToken cToken)
        {
            await UniWidgets.PushAsync<LoadingWidget>(cToken);
            await Context.PuzzleService.RestartPuzzleAsync(cToken);
            await UniWidgets.PopWidgetsStackAsync(cToken);
            await UniWidgets.PopWidgetsStackAsync(cToken);
        }

        private void HandleClose() => UniWidgets.PopWidgetsStackAsync(Context.CancellationTokenOnDestroy).Forget();

        private void SetLevelText()
        {
            var levelName = Context.PuzzleService.GetCurrentLevelData().Name;
            Context.Refs.LevelText.SetText(levelName);
        }
    }
}
