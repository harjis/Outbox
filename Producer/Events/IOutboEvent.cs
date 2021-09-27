namespace Outbox.Producer.Events
{
    public interface IOutboxEvent<T>
    {
        public string Id { get; }
        public string AggregateType { get; set; }
        public string AggregateId { get; set; }
        public string Type { get; set; }
        public T Payload { get; set; }
        public Models.Outbox ToOutboxModel();
    }
}
