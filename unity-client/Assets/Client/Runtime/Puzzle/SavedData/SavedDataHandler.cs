using System;
using UniTx.Runtime;
using UniTx.Runtime.Clock;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Serialisation;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SavedDataHandler : MonoBehaviour, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private string _userSavedDataKey;

        private ISerialisationService _serialisationService;
        private IPuzzleService _puzzleService;
        private IWinConditionChecker _checker;
        private IClock _clock;
        private UserSavedData _savedData;

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
            _clock = resolver.Resolve<IClock>();
            _checker = resolver.Resolve<IWinConditionChecker>();
            _serialisationService = resolver.Resolve<ISerialisationService>();
            _savedData = _serialisationService.Load<UserSavedData>(_userSavedDataKey);
            var binder = resolver.Resolve<IBinder>();
            binder.BindAsSingleton(_savedData);
        }

        public void Initialise()
        {
            _checker.OnWin += HandleOnWin;
            UniEvents.Subscribe<LevelResetEvent>(HandleOnLevelReset);
        }

        public void Reset()
        {
            _checker.OnWin -= HandleOnWin;
            UniEvents.Unsubscribe<LevelResetEvent>(HandleOnLevelReset);
        }

        private void OnApplicationQuit()
        {
            SaveCurrentLevelState();
            _serialisationService.SaveImmediate();
        }

        private void HandleOnWin()
        {
            _savedData.CurrentLevel += 1;
            Save();
        }

        private void HandleOnLevelReset(LevelResetEvent @event)
        {
            _savedData.CurrentLevelState = null;
            Save();
        }

        private void Save()
        {
            _savedData.ModifiedTimestamp = _clock.UnixTimestampNow;
            _serialisationService.Save(_savedData);
        }

        private void SaveCurrentLevelState()
        {
            var board = _puzzleService.GetCurrentBoard();

            if (board == null) return;

            var pieces = board.Pieces;
            var pieceStates = new JigsawPieceState[pieces.Count];
            for (var i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                pieceStates[i] = new JigsawPieceState(piece.CorrectIdx, piece.CurrentIdx);
            }
            _savedData.CurrentLevelState = new JigsawLevelState(pieceStates);
            Save();
        }
    }
}