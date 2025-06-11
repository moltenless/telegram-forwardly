using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramForwardly.DataAccess.Entities
{
    [Table("client_current_states")]
    public class ClientCurrentState
    {
        [Column("id", TypeName = "bigint")]
        [Key]
        public long Id { get; set; }

        [Column("value", TypeName = "varchar(124)")]
        [Required]
        public string Value { get; set; } = null!;

        public ICollection<Client> Clients { get; set; } = new HashSet<Client>();
    }
}
