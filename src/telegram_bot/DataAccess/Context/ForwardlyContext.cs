using Microsoft.EntityFrameworkCore;
using TelegramForwardly.DataAccess.Entities;

namespace TelegramForwardly.DataAccess.Context
{
    internal class ForwardlyContext : DbContext
    {
        private readonly string connectionString;

        public DbSet<Client> Clients { get; set; }
        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ClientCurrentState> ClientCurrentStates { get; set; }
        public DbSet<TopicGroupingType> TopicGroupingTypes { get; set; }
        public DbSet<ChatType> ChatTypes { get; set; }

        public ForwardlyContext(string connectionString) 
            => this.connectionString = connectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
