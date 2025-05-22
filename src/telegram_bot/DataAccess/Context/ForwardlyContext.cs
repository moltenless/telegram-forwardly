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
    }
}
