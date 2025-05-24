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
                new ClientCurrentState { Id = 1, Value = "Idle" },
                new ClientCurrentState { Id = 2, Value = "AwaitingPhoneNumber", },
                new ClientCurrentState { Id = 3, Value = "AwaitingApiId", },
                new ClientCurrentState { Id = 4, Value = "AwaitingApiHash", },
                new ClientCurrentState { Id = 5, Value = "AwaitingVerificationCode", },
                new ClientCurrentState { Id = 6, Value = "AwaitingPassword", },
                new ClientCurrentState { Id = 7, Value = "AwaitingEnableAllChats", },
                new ClientCurrentState { Id = 8, Value = "AwaitingChats", },
                new ClientCurrentState { Id = 9, Value = "AwaitingKeywords", },
                new ClientCurrentState { Id = 10, Value = "AwaitingForumGroup", },
                new ClientCurrentState { Id = 11, Value = "AwaitingGroupingType", },
                new ClientCurrentState { Id = 12, Value = "AwaitingEnableLoggingTopic", },
                new ClientCurrentState { Id = 13, Value = "Ready" }
            );
        }
    }
}
