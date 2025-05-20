using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramForwardly.DataAccess.Entities
{
    [Table("clients")]
    public class Client
    {
        [Column("user_id", TypeName = "bigint")]
        [Key]
        [Required]
        public long UserId { get; set; }

        [Column("current_state_id", TypeName = "int")]
        [ForeignKey(nameof(CurrentState))]
        public int? CurrentStateId { get; set; }
        public ClientCurrentState? CurrentState { get; set; }

        [Column("api_id", TypeName = "varchar(20)")]
        public string? ApiId { get; set; }

        [Column("api_hash", TypeName = "varchar(64)")]
        public string? ApiHash { get; set; }

        [Column("session_sting", TypeName = "varchar(600)")]
        public string? SessionString { get; set; }

        [Column("phone", TypeName = "varchar(20)")]
        public string? Phone { get; set; }

        [Column("verification_code", TypeName = "varchar(16)")]
        public string? VerificationCode { get; set; }

        [Column("password", TypeName = "nvarchar(100)")]
        public string? Password { get; set; }

        [Column("registration_datetime", TypeName = "datetime")]
        public DateTime? RegistrationDataTime { get; set; }

        [Column("username", TypeName = "varchar(32)")]
        public string? UserName { get; set; }

        [Column("first_name", TypeName = "nvarchar(64)")]
        public string? FirstName { get; set; }

        [Column("forum_supergroup_id", TypeName = "bigint")]
        public long? ForumSupergroupId { get; set; }

        [Column("logging_topic_enabled", TypeName = "bit")]
        public bool? LoggingTopicEnabled { get; set; }

        [Column("topic_grouping_type_id", TypeName = "int")]
        [ForeignKey(nameof(TopicGroupingType))]
        public int? TopicGroupingTypeId { get; set; }
        public TopicGroupingType? TopicGroupingType { get; set; }

        [Column("forwardly_enabled", TypeName = "bit")]
        public bool? ForwardlyEnabled { get; set; }

        [Column("all_chats_filtering_enabled", TypeName = "bit")]
        public bool? AllChatsFilteringEnabled { get; set; }


        public ICollection<Keyword> Keywords { get; set; } = new HashSet<Keyword>();
        public ICollection<Chat> Chats { get; set; } = new HashSet<Chat>();
    }
}
