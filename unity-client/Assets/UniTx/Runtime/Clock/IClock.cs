using System;

namespace UniTx.Runtime.Clock
{
    /// <summary>
    /// Provides access to the current time in different formats.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current UTC date and time.
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// Gets the current Unix timestamp in seconds.
        /// Represents the number of seconds elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        long UnixTimestampNow { get; }
    }
}