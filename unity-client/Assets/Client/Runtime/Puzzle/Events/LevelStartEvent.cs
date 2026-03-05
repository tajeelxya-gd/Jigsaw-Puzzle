using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public readonly struct LevelStartEvent : IEvent
    {
        public readonly int LevelIndex;

        public LevelStartEvent(int levelIndex)
        {
            LevelIndex = levelIndex;
        }
    }
}