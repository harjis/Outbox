using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Outbox.Producer.Events;

namespace Outbox.Producer.Managers
{
    public class EventManager<TContext> where TContext : DbContext
    {
        private readonly TContext _dbContext;

        public EventManager(TContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task FireEvent<T>(IOutboxEvent<T> outboxEvent)
        {
            var outbox = outboxEvent.ToOutboxModel();
            await _dbContext.AddAsync(outbox);
            await _dbContext.SaveChangesAsync();
            // TODO Then remove it after
        }
    }
}
