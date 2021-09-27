namespace Outbox.Consumer.Events
{
    public class ReceivedEvent<TPayload>
    {
        public string Id { get; }
        public string Type { get; }
        public TPayload Payload { get; }

        public ReceivedEvent(string id, string type, TPayload payload)
        {
            Id = id;
            Type = type;
            Payload = payload;
        }
    }
}
