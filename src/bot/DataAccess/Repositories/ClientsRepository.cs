using Microsoft.EntityFrameworkCore;
using TelegramForwardly.DataAccess.Context;
using TelegramForwardly.DataAccess.Entities;
using TelegramForwardly.DataAccess.Repositories.Interfaces;

namespace TelegramForwardly.DataAccess.Repositories
{
    public class ClientsRepository(ForwardlyContext context) : Repository(context), IClientsRepository
    {
        public async Task<Client> GetOrCreateClientAsync(
            long telegramUserId
            , ClientCurrentState initialStateIfNew
            , string? userNameIfNew
            , string? firstNameIfNew)
        {
            var client = await GetClientOrDefaultAsync(telegramUserId);

            if (client == null)
            {
                client = new Client
                {
                    TelegramUserId = telegramUserId,
                    CurrentStateId = initialStateIfNew.Id,
                    UserName = userNameIfNew,
                    FirstName = firstNameIfNew,
                    RegistrationDataTime = DateTime.UtcNow,
                    IsAuthenticated = false,
                    ForwardlyEnabled = false,
                    TopicGrouping = "ByKeyword",
                    AllChatsFilteringEnabled = false,
                };

                context.Clients.Add(client);
                await context.SaveChangesAsync();

                client = await GetClientAsync(telegramUserId);
            }

            return client;
        }

        public async Task<HashSet<Client>> GetAllClientsAsync()
        {
            return await context.Clients
                .Include(c => c.CurrentState)
                .Include(c => c.Keywords)
                .Include(c => c.Chats)
                .AsSplitQuery()
                .ToHashSetAsync();
        }

        public async Task<HashSet<Client>> GetAllAuthenticatedUsersAsync()
        {
            return await context.Clients
                .Include(c => c.CurrentState)
                .Include(c => c.Keywords)
                .Include(c => c.Chats)
                .AsSplitQuery()
                .Where(c => c.IsAuthenticated == true)
                .ToHashSetAsync();
        }

        public async Task<Client> GetClientAsync(long telegramUserId)
        {
            var entity = await context.Clients
                .Include(c => c.CurrentState)
                .Include(c => c.Keywords)
                .Include(c => c.Chats)
                .AsSplitQuery()
                .FirstAsync(c => c.TelegramUserId == telegramUserId);
            await context.Entry(entity).ReloadAsync();
            return entity; 
        }

        public async Task<Client?> GetClientOrDefaultAsync(long telegramUserId)
        {
            var entity = await context.Clients
                .Include(c => c.CurrentState)
                .Include(c => c.Keywords)
                .Include(c => c.Chats)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.TelegramUserId == telegramUserId);
            if (entity != null)
                await context.Entry(entity).ReloadAsync();
            return entity;
        }

        public async Task SetClientStateAsync(Client client, ClientCurrentState newState)
        {
            if (client.CurrentStateId == newState.Id) return;
            client.CurrentStateId = newState.Id;
            await context.SaveChangesAsync();
        }

        public async Task UpdateClientPhoneAsync(Client client, string phone)
        {
            if (client.Phone == phone) return;
            client.Phone = phone;
            await context.SaveChangesAsync();
        }

        public async Task UpdateClientApiIdAsync(Client client, string apiId)
        {
            if (client.ApiId == apiId) return;
            client.ApiId = apiId;
            await context.SaveChangesAsync();
        }

        public async Task UpdateClientApiHashAsync(Client client, string apiHash)
        {
            if (client.ApiHash == apiHash) return;
            client.ApiHash = apiHash;
            await context.SaveChangesAsync();
        }
    }
}