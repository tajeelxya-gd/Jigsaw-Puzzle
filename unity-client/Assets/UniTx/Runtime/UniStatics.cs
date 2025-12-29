using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UniTx.Runtime.Extensions;
using UniTx.Runtime.IoC;

namespace UniTx.Runtime
{
    /// <summary>
    /// Provides common utility functions and platform checks.
    /// </summary>
    public static class UniStatics
    {
        internal static GameObject Root { get; set; }

        internal static UniTxConfig Config { get; set; }

        /// <summary>
        /// Gets the resolver instance that is bound in the container, used to resolve dependencies at runtime.
        /// </summary>
        internal static IResolver Resolver { get; set; }

        /// <summary>
        /// Indicates whether the application is running in the Unity Editor.
        /// </summary>
        public static bool IsEditor => Application.platform.ToString().Contains("Editor");

        /// <summary>
        /// Indicates whether the application is running on Android.
        /// </summary>
        public static bool IsAndroid => Application.platform == RuntimePlatform.Android;

        /// <summary>
        /// Indicates whether the application is running on iOS.
        /// </summary>
        public static bool IsIOS => Application.platform == RuntimePlatform.IPhonePlayer;

        /// <summary>
        /// Logs a colored message with an optional context object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogInfo(object msg, object ctx = null, Color color = default)
            => Debug.Log($"{(ctx == null ? string.Empty : $"[{ctx.GetType().Name}]")} - {msg}".WithColor(
                color == default
                    ? Color.white
                    : color));

        /// <summary>
        /// Waits using an exponential backoff delay based on retry count.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniTask RetryDelayAsync(int retryCounter, CancellationToken cToken = default)
        {
            var exponent = Math.Clamp(retryCounter, 0, 4);
            var seconds = Math.Pow(2, exponent);
            return UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cToken);
        }
    }
}