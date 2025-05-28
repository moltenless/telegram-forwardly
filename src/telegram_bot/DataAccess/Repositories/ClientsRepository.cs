using Microsoft.EntityFrameworkCore;
using TelegramForwardly.DataAccess.Context;
using TelegramForwardly.DataAccess.Entities;
using TelegramForwardly.DataAccess.Repositories.Interfaces;

namespace TelegramForwardly.DataAccess.Repositories
{
    public class ClientsRepository(ForwardlyContext context) : Repository(context), IClientsRepository
    {
        public async Task<Client> GetOrCreateClientAsync(long telegramUserId, ClientCurrentState initialState)
        {
            var client = await context.Clients
                .Include(c => c.CurrentState)
                .FirstOrDefaultAsync(c => c.TelegramUserId == telegramUserId);

            if (client == null)
            {
                client = new Client
                {
                    TelegramUserId = telegramUserId,
                    CurrentStateId = initialState.Id,
                    RegistrationDataTime = DateTime.UtcNow,
                };

                context.Clients.Add(client);
                await context.SaveChangesAsync();

                client = await context.Clients
                    .Include(c => c.CurrentState)
                    .FirstAsync(c => c.TelegramUserId == telegramUserId);
            }

            return client;
        }
    }
}
