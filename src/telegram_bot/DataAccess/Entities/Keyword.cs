using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramForwardly.DataAccess.Entities
{
    [Table("keywords")]
    public class Keyword
    {
        [Column("id", TypeName = "int")]
        [Key]
        public int Id { get; set; }

        [Column("user_id", TypeName = "bigint")]
        [ForeignKey(nameof(Client))]
        [Required]
        public long UserId { get; set; }
        public Client Client { get; set; } = null!;

        [Column("value", TypeName = "nvarchar(128)")]
        [Required]
        public string Value { get; set; } = null!;
    }
}
