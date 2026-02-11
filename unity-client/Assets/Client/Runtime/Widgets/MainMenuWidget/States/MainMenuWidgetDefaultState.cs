using Cysharp.Threading.Tasks;
using System.Threading;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public sealed class MainMenuWidgetDefaultState : WidgetState<MainMenuWidgetContext, MainMenuWidgetData, MainMenuWidgetRefs>
    {
        public override void OnEnter()
        {
            Context.Refs.PlayButton.onClick.AddListener(HandlePlayKicked);
            SetPlayButtonText();
        }

        public override void OnExit()
        {
            Context.Refs.PlayButton.onClick.RemoveListener(HandlePlayKicked);
        }

        private void HandlePlayKicked()
        {
            UniTask.Void(HandlePlayKickedAsync, Context.CancellationTokenOnDestroy);
        }

        private async UniTaskVoid HandlePlayKickedAsync(CancellationToken cToken)
        {
            await Context.PuzzleService.LoadPuzzleAsync(cToken);
            await UniWidgets.PopWidgetsStackAsync(cToken);
        }

        private void SetPlayButtonText()
        {
            var levelData = Context.PuzzleService.GetCurrentLevelData();
            Context.Refs.PlayButtonText.SetText($"Play {levelData.Name}");
        }
    }
}
