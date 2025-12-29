using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace UniTx.Runtime.Serialisation
{
    public class SerialisationService : ISerialisationService, IInitialisable, IResettable
    {
        private readonly Serialiser _serialiser = new();

        private CancellationTokenSource _cts;
        private float _interval;

        public void Initialise()
        {
            _interval = UniStatics.Config.SaveInterval;
            _cts = new CancellationTokenSource();
            UniTask.Void(SaveLoopAsync, _cts.Token);
        }

        public void Reset()
        {
            _cts.Cancel();
            _cts.Dispose();
            _serialiser.Reset();
        }

        public void Save(ISavedData data)
        {
            if (data?.Id == null)
            {
                UniStatics.LogInfo("Failed to save data because data or data Id is null.", this, Color.red);
                return;
            }

            _serialiser.MarkDirty(data);
        }

        public T Load<T>(string id)
            where T : ISavedData, new()
        {
            if (id == null)
            {
                UniStatics.LogInfo("Failed to load data because data Id is null.", this, Color.red);
                throw new ArgumentNullException(nameof(id));
            }

            return _serialiser.Deserialise<T>(id);
        }

        private async UniTaskVoid SaveLoopAsync(CancellationToken cToken = default)
        {
            while (!cToken.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_interval), cancellationToken: cToken);
                _serialiser.SerialiseDirty();
            }
        }
    }
}