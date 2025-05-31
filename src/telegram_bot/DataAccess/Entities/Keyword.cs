using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramForwardly.DataAccess.Entities
{
    [Table("keywords")]
    public class Keyword
    {
        [Column("id", TypeName = "bigint")]
        [Key]
        public long Id { get; set; }

        [Column("telegram_user_id", TypeName = "bigint")]
        [ForeignKey(nameof(Client))]
        [Required]
        public long TelegramUserId { get; set; }
        public Client Client { get; set; } = null!;

        [Column("value", TypeName = "nvarchar(128)")]
        [Required]
        public string Value { get; set; } = null!;
    }
}
