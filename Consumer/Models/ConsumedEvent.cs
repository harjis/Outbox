using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Outbox.Consumer.Models
{
    public class ConsumedEvent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Required]
        public string Id { get; set; }

        public long TimeOfReceived { get; set; }
    }
}
