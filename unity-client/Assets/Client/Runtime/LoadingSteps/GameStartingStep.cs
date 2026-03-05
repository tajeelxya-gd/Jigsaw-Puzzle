using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class GameStartingStep : LoadingStepBase, IInjectable
    {
        [SerializeField] private UIController _uiController;

        private IPuzzleService _puzzleService;

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
            _uiController.Inject(resolver.Resolve<IWinConditionChecker>());
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            InputHandler.SetActive(true);

            var levelData = GetLevelData();
            _uiController.Initialize(levelData);
            return _puzzleService.LoadPuzzleAsync(cToken);
        }

        private LevelData GetLevelData()
        {
            var json = Resources.Load<TextAsset>($"Levels/Level {1}");
            var levelData = ScriptableObject.CreateInstance<LevelData>();
            JsonUtility.FromJsonOverwrite(json.text, levelData);
            return levelData;
        }
    }
}