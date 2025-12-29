using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace UniTx.Runtime.Extensions
{
    public static class StringExt
    {
        /// <summary>
        /// Copies the string to the system clipboard.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyToClipboard(this string source) => GUIUtility.systemCopyBuffer = source;

        /// <summary>
        /// Returns a colourised version of the given string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string WithColor(this string source, Color color)
            => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{source}</color>";

        /// <summary>
        /// Replaces Turkish characters in the string with their English equivalents.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FixTurkishChars(this string source)
        {
            if (string.IsNullOrEmpty(source)) return source;

            ReadOnlySpan<char> turkish = "İıŞşĞğÜüÖöÇç";
            ReadOnlySpan<char> english = "IiSsGgUuOoCc";

            var sb = new StringBuilder(source.Length);
            foreach (var ch in source)
            {
                var index = turkish.IndexOf(ch);
                sb.Append(index >= 0 ? english[index] : ch);
            }

            return sb.ToString();
        }
    }
}