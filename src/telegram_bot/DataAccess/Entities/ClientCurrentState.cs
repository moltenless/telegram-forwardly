using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramForwardly.DataAccess.Entities
{
    [Table("client_current_states")]
    internal class ClientCurrentState
    {
        [Column("id", TypeName = "int")]
        [Key]
        public int Id { get; set; }

        [Column("value", TypeName = "varchar(124)")]
        [Required]
        public string Value { get; set; } = null!;
    }
}
