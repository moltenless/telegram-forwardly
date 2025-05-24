using Microsoft.EntityFrameworkCore;
using TelegramForwardly.DataAccess.Entities;

namespace TelegramForwardly.DataAccess.Context
{
    public class ForwardlyContext(DbContextOptions<ForwardlyContext> options) : DbContext(options)
    {
        internal DbSet<Client> Clients { get; set; }
        internal DbSet<Keyword> Keywords { get; set; }
        internal DbSet<Chat> Chats { get; set; }
        internal DbSet<ClientCurrentState> ClientCurrentStates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ClientCurrentState>().HasData(
                new ClientCurrentState { Id = 1L, Value = "Idle" },
                new ClientCurrentState { Id = 2L, Value = "AwaitingPhoneNumber", },
                new ClientCurrentState { Id = 3L, Value = "AwaitingApiId", },
                new ClientCurrentState { Id = 4L, Value = "AwaitingApiHash", },
                new ClientCurrentState { Id = 5L, Value = "AwaitingVerificationCode", },
                new ClientCurrentState { Id = 6L, Value = "AwaitingPassword", },
                new ClientCurrentState { Id = 7L, Value = "AwaitingEnableAllChats", },
                new ClientCurrentState { Id = 8L, Value = "AwaitingChats", },
                new ClientCurrentState { Id = 9L, Value = "AwaitingKeywords", },
                new ClientCurrentState { Id = 10L, Value = "AwaitingForumGroup", },
                new ClientCurrentState { Id = 11L, Value = "AwaitingGroupingType", },
                new ClientCurrentState { Id = 12L, Value = "AwaitingEnableLoggingTopic", },
                new ClientCurrentState { Id = 13L, Value = "Ready" }
            );
        }
    }
}
