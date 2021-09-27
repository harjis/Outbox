using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Outbox.Producer.Models
{
    public class Outbox
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None), Required]
        public string id { get; set; }

        [StringLength(255), Required] public string aggregatetype { get; set; }
        [StringLength(255), Required] public string aggregateid { get; set; }
        [StringLength(255), Required] public string type { get; set; }
        [Column(TypeName = "jsonb"), Required] public string payload { get; set; }
    }
}
