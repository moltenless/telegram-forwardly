using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramForwardly.DataAccess.Entities
{
    [Table("chats")]
    public class Chat
    {
        [Column("id", TypeName = "bigint")]
        [Key]
        public long Id { get; set; }

        [Column("telegram_user_id", TypeName = "bigint")]
        [ForeignKey(nameof(Client))]
        [Required]
        public long TelegramUserId { get; set; }
        public Client Client { get; set; } = null!;

        [Column("tg_chat_id", TypeName = "bigint")]
        [Required]
        public long TelegramChatId { get; set; }

        [Column("title", TypeName = "nvarchar(100)")]
        [Required]
        public string Title { get; set; } = null!;
    }
}
