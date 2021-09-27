using System;
using Microsoft.EntityFrameworkCore;
using Outbox.Consumer.Events;
using Outbox.Consumer.Models;

namespace Outbox.Consumer.Repositories
{
    public class ConsumedEventRepository<TContext, TPayload> where TContext : DbContext
    {
        private readonly TContext _dbContext;

        public ConsumedEventRepository(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(ReceivedEvent<TPayload> receivedEvent)
        {
            var consumedEvent = ReceivedEventToConsumedEvent(receivedEvent);
            _dbContext.Add(consumedEvent);
            _dbContext.SaveChanges();
        }
        
        public bool HasBeenConsumed(ReceivedEvent<TPayload> receivedEvent)
        {
            return _dbContext.Find<ConsumedEvent>(receivedEvent.Id) != null;
        }

        private static ConsumedEvent ReceivedEventToConsumedEvent(ReceivedEvent<TPayload> receivedEvent)
        {
            return new()
            {
                Id = receivedEvent.Id,
                TimeOfReceived = DateTime.Now.ToFileTimeUtc()
            };
        }
    }
}
