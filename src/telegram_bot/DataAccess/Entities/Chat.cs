using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramForwardly.DataAccess.Entities
{
    [Table("chats")]
    internal class Chat
    {
        [Column("db_id", TypeName = "int")]
        [Key]
        public int Id { get; set; }

        [Column("user_id", TypeName = "bigint")]
        [ForeignKey(nameof(Client))]
        [Required]
        public long UserId { get; set; }
        public virtual Client Client { get; set; } = null!;

        [Column("tg_id", TypeName = "bigint")]
        [Required]
        public long ChatId { get; set; }

        [Column("type_id")]
        [ForeignKey(nameof(Type))]
        [Required]
        public int TypeId { get; set; }
        public ChatType Type { get; set; } = null!;
    }
}
