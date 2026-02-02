using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public readonly struct ShowToastEvent : IEvent
    {
        public readonly ToastEventData Data;

        public ShowToastEvent(string message) => Data = new(message);
    }

    public readonly struct ToastEventData
    {
        public readonly string Message;

        public ToastEventData(string message) => Message = message;
    }
}