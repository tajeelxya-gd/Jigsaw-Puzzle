using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public readonly struct LevelResetEvent : IEvent
    {
        public readonly string LevelId;

        public LevelResetEvent(string levelId)
        {
            LevelId = levelId;
        }
    }
}