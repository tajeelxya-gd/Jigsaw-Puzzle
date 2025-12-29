using System;
using UniTx.Runtime.Extensions;

namespace UniTx.Runtime.Clock
{
    public sealed class LocalClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public long UnixTimestampNow => UtcNow.ToUnixTimestamp();
    }
}