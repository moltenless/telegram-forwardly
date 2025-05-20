using Microsoft.EntityFrameworkCore;
using TelegramForwardly.DataAccess.Entities;

namespace TelegramForwardly.DataAccess.Context
{
    public class ForwardlyContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ClientCurrentState> ClientCurrentStates { get; set; }
        public DbSet<TopicGroupingType> TopicGroupingTypes { get; set; }
        public DbSet<ChatType> ChatTypes { get; set; }
    }
}
