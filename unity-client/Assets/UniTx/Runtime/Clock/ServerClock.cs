using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UniTx.Runtime.Extensions;
using UniTx.Runtime.UnityEventListener;

namespace UniTx.Runtime.Clock
{
    public sealed class ServerClock : IClock, IInitialisableAsync, IResettable
    {
        private readonly IUnityEventListener _listener;
        private DateTime _retrievedTime;
        private double _realTimeWhenRetrieved;
        private string _timeServerUrl;

        public DateTime UtcNow { get; private set; }

        public long UnixTimestampNow { get; private set; } = 0;

        public ServerClock()
        {
            _listener = UniStatics.Resolver.Resolve<IUnityEventListener>();
        }

        public async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            _timeServerUrl = UniStatics.Config.TimeServerUrl;
            await SetUtcAsync(cToken);
            _listener.OnUpdate += OnUpdate;
        }

        public void Reset()
        {
            _listener.OnUpdate -= OnUpdate;
            _retrievedTime = default;
            _realTimeWhenRetrieved = 0;
        }

        private void OnUpdate()
        {
            UtcNow = _retrievedTime.AddSeconds(Time.realtimeSinceStartupAsDouble - _realTimeWhenRetrieved);
            UnixTimestampNow = UtcNow.ToUnixTimestamp();
        }

        private async UniTask SetUtcAsync(CancellationToken cToken = default)
        {
            for (var i = 0; ; i++)
            {
                try
                {
                    var asyncOp = UnityWebRequest.Get(_timeServerUrl).SendWebRequest();
                    var webRequest = await asyncOp.ToUniTask(cancellationToken: cToken);
                    var json = webRequest.downloadHandler.text;
                    var response = JsonUtility.FromJson<DateUrlJson>(json);
                    var dateTime = DateTime.Parse(response.dateTime);
                    _retrievedTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    _realTimeWhenRetrieved = Time.realtimeSinceStartupAsDouble;
                    break;
                }
                catch (Exception ex)
                {
                    UniStatics.LogInfo($"Failed to fetch at {_timeServerUrl}.\n{ex.Message}. Retry: {i}.", this, Color.red);
                    await UniStatics.RetryDelayAsync(i, cToken);
                }
            }
        }

        [Serializable]
        private sealed class DateUrlJson
        {
            public string dateTime;
        }
    }
}