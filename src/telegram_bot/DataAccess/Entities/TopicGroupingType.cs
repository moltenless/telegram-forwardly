using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramForwardly.DataAccess.Entities
{
    [Table("topic_grouping_types")]
    public class TopicGroupingType
    {
        [Column("id", TypeName = "int")]
        [Key]
        public int Id { get; set; }

        [Column("value", TypeName = "varchar(64)")]
        [Required]
        public string Value { get; set; } = null!;

        public ICollection<Client> Clients { get; set; } = new HashSet<Client>();
    }
}
