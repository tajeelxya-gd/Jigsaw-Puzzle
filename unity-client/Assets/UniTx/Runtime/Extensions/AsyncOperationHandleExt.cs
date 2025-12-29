using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UniTx.Runtime.Extensions
{
    public static class AsyncOperationHandleExt
    {
        public static UniTask<T> ToUniTask<T>(this AsyncOperationHandle<T> source, IProgress<float> progress = null,
            PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cToken = default)
        {
            if (cToken.IsCancellationRequested)
            {
                return UniTask.FromCanceled<T>(cToken);
            }

            if (!source.IsValid())
            {
                return UniTask.FromException<T>(new InvalidOperationException("Invalid Addressables handle."));
            }

            if (source.IsDone)
            {
                if (source.Status == AsyncOperationStatus.Failed)
                {
                    return UniTask.FromException<T>(source.OperationException);
                }

                return UniTask.FromResult(source.Result);
            }

            return new UniTask<T>(AsyncOperationHandleConfiguredSource<T>.Create(source, timing, progress, cToken, out var token), token);
        }

        private sealed class AsyncOperationHandleConfiguredSource<T> : IUniTaskSource<T>, IPlayerLoopItem,
            ITaskPoolNode<AsyncOperationHandleConfiguredSource<T>>
        {
            private static TaskPool<AsyncOperationHandleConfiguredSource<T>> _pool;
            private AsyncOperationHandleConfiguredSource<T> _nextNode;
            public ref AsyncOperationHandleConfiguredSource<T> NextNode => ref _nextNode;

            static AsyncOperationHandleConfiguredSource()
                => TaskPool.RegisterSizeGetter(typeof(AsyncOperationHandleConfiguredSource<T>), () => _pool.Size);

            private readonly Action<AsyncOperationHandle<T>> _continuationAction;
            private AsyncOperationHandle<T> _handle;
            private CancellationToken _cancellationToken;
            private IProgress<float> _progress;
            private bool _completed;

            private UniTaskCompletionSourceCore<T> _core;

            private AsyncOperationHandleConfiguredSource() => _continuationAction = Continuation;

            public static IUniTaskSource<T> Create(AsyncOperationHandle<T> handle, PlayerLoopTiming timing,
                IProgress<float> progress, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<T>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!_pool.TryPop(out var result))
                {
                    result = new AsyncOperationHandleConfiguredSource<T>();
                }

                result._handle = handle;
                result._cancellationToken = cancellationToken;
                result._completed = false;
                result._progress = progress;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                handle.Completed += result._continuationAction;

                token = result._core.Version;
                return result;
            }

            private void Continuation(AsyncOperationHandle<T> argHandle)
            {
                _handle.Completed -= _continuationAction;

                if (_completed)
                {
                    TryReturn();
                }
                else
                {
                    _completed = true;
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        _core.TrySetCanceled(_cancellationToken);
                    }
                    else if (argHandle.Status == AsyncOperationStatus.Failed)
                    {
                        _core.TrySetException(argHandle.OperationException);
                    }
                    else
                    {
                        _core.TrySetResult(argHandle.Result);
                    }
                }
            }

            public T GetResult(short token) => _core.GetResult(token);

            void IUniTaskSource.GetResult(short token) => GetResult(token);

            public UniTaskStatus GetStatus(short token) => _core.GetStatus(token);

            public UniTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();

            public void OnCompleted(Action<object> continuation, object state, short token)
                => _core.OnCompleted(continuation, state, token);

            public bool MoveNext()
            {
                if (_completed)
                {
                    TryReturn();
                    return false;
                }

                if (_cancellationToken.IsCancellationRequested)
                {
                    _completed = true;
                    _core.TrySetCanceled(_cancellationToken);
                    return false;
                }

                if (_progress != null && _handle.IsValid())
                {
                    _progress.Report(_handle.PercentComplete);
                }

                return true;
            }

            private bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                _core.Reset();
                _handle = default;
                _progress = default;
                _cancellationToken = default;
                return _pool.TryPush(this);
            }
        }
    }
}