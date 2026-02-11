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
    internal sealed class UniWidgetsManager : MonoBehaviour, IWidgetsManager
    {
        [SerializeField] private PushLayerTransform[] _layerTransforms;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Dictionary<PushLayer, Transform> _layerRegistry = new();
        private readonly Stack<IWidget> _stack = new();
        private IResolver _resolver;
        private AssetData _assetData;

        public event Action<Type> OnPush;
        public event Action<Type> OnPop;

        public void Inject(IResolver resolver) => _resolver = resolver;

        public async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            foreach (var layerTransform in _layerTransforms)
            {
                _layerRegistry.Add(layerTransform.Layer, layerTransform.Transform);
            }

            var key = UniStatics.Config.WidgetsAssetDataKey;
            _assetData = await UniResources.LoadAssetAsync<AssetData>(key, cToken: cToken);
        }

        public UniTask PushAsync<TWidgetType>(PushLayer layer, CancellationToken cToken = default)
            where TWidgetType : IWidget
            => PushInternalAsync<TWidgetType>(null, layer, cToken);

        public UniTask PushAsync<TWidgetType>(IWidgetData widgetData, PushLayer layer, CancellationToken cToken = default)
            where TWidgetType : IWidget, IWidgetDataReceiver
            => PushInternalAsync<TWidgetType>(widgetData, layer, cToken);

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

        private async UniTask PushInternalAsync<TWidgetType>(IWidgetData widgetData, PushLayer layer, CancellationToken cToken = default)
            where TWidgetType : IWidget
        {
            await _semaphore.WaitAsync(cToken);
            try
            {
                var widgetType = typeof(TWidgetType);
                var asset = _assetData.GetAsset(widgetType.Name);
                var widget = await UniResources.CreateInstanceAsync<IWidget>(asset.RuntimeKey, _layerRegistry[layer], null, cToken);

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
    }
}