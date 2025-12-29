using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UniTx.Runtime.Database;
using UniTx.Runtime.Extensions;
using UniTx.Runtime.IoC;
using UniTx.Runtime.ResourceManagement;

namespace UniTx.Runtime.Widgets
{
    internal sealed class UniWidgetsManager : IWidgetsManager
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Stack<IWidget> _stack;
        private readonly IResolver _resolver;
        private AssetData _assetData;
        private Transform _spawnPoint;

        public event Action<Type> OnPush;
        public event Action<Type> OnPop;

        public UniWidgetsManager()
        {
            _stack = new();
            _resolver = UniStatics.Resolver;
            _spawnPoint = null;
        }

        public async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            var key = UniStatics.Config.WidgetsAssetDataKey;
            _assetData = await UniResources.LoadAssetAsync<AssetData>(key, cToken: cToken);
        }

        public UniTask PushAsync<TWidgetType>(CancellationToken cToken = default)
            where TWidgetType : IWidget
            => PushInternalAsync<TWidgetType>(null, cToken);

        public UniTask PushAsync<TWidgetType>(IWidgetData widgetData, CancellationToken cToken = default)
            where TWidgetType : IWidget, IWidgetDataReceiver
            => PushInternalAsync<TWidgetType>(widgetData, cToken);

        public async UniTask PopWidgetsStackAsync(CancellationToken cToken = default)
        {
            await _semaphore.WaitAsync(cToken);
            try
            {
                if (_stack.TryPop(out var widget))
                {
                    var widgetType = widget.GetType();
                    widget.Reset();
                    UniResources.DisposeInstance(widget.GameObject);
                    OnPop.Broadcast(widgetType);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IWidget Peek() => _stack.TryPeek(out var widget) ? widget : null;

        private async UniTask PushInternalAsync<TWidgetType>(IWidgetData widgetData, CancellationToken cToken = default)
            where TWidgetType : IWidget
        {
            await _semaphore.WaitAsync(cToken);
            try
            {
                var widgetType = typeof(TWidgetType);
                var asset = _assetData.GetAsset(widgetType.Name);
                var widget = await UniResources.CreateInstanceAsync<IWidget>(asset.RuntimeKey, GetSpawnPoint(), null, cToken);

                if (widget is IInjectable injectable)
                {
                    injectable.Inject(_resolver);
                }

                if (widgetData != null && widget is IWidgetDataReceiver dataReceiver)
                {
                    dataReceiver.SetData(widgetData);
                }

                widget.Initialise();
                _stack.Push(widget);
                OnPush.Broadcast(widgetType);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private Transform GetSpawnPoint()
        {
            if (_spawnPoint != null) return _spawnPoint;

            var parentTag = UniStatics.Config.WidgetsParentTag;
            var go = GameObject.FindGameObjectWithTag(parentTag);
            if (go == null)
            {
                UniStatics.LogInfo($"No GameObject found with tag '{parentTag}' to serve as widgets parent.", this, Color.red);
                return null;
            }

            return _spawnPoint = go.transform;
        }
    }
}