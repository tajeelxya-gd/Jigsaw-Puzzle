using System;
using System.Runtime.CompilerServices;

namespace UniTx.Runtime.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Action"/>.
    /// </summary>
    public static class ActionExt
    {
        /// <summary>
        /// Safely invokes the action with the provided value.
        /// </summary>
        /// <param name="source">delegate to invoke</param>
        /// <param name="value">value to invoke with</param>
        /// <typeparam name="T">type param</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast<T>(this Action<T> source, T value) => source?.Invoke(value);

        /// <summary>
        /// Safely invokes the action.
        /// </summary>
        /// <param name="source">delegate to invoke</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast(this Action source) => source?.Invoke();
    }
}