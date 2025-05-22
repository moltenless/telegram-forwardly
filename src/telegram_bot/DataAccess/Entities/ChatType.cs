using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramForwardly.DataAccess.Entities
{
    [Table("chat_types")]
    public class ChatType
    {
        [Column("id", TypeName = "int")]
        [Key]
        public int Id { get; set; }

        [Column("value", TypeName = "varchar(64)")]
        [Required]
        public string Value { get; set; } = null!;

        public ICollection<Chat> Chats { get; set; } = new HashSet<Chat>();
    }
}
