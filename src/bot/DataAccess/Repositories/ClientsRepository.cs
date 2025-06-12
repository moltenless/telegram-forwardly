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
                    ForwardlyEnabled = true,
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

        public async Task UpdateClientSessionStringAsync(Client client, string sessionString)
        {
            if (client.SessionString == sessionString) return;
            client.SessionString = sessionString;
            await context.SaveChangesAsync();
        }

        public async Task SetClientAuthenticatedAsync(Client client, bool isAuthenticated)
        {
            if (client.IsAuthenticated == isAuthenticated) return;
            client.IsAuthenticated = isAuthenticated;
            await context.SaveChangesAsync();
        }

        public async Task UpdateClientForumIdAsync(Client client, long forumId)
        {
            if (client.ForumSupergroupId == forumId) return;
            client.ForumSupergroupId = forumId;
            await context.SaveChangesAsync();
        }

        public async Task SetClientGroupingAsync(Client client, string groupingType)
        {
            if (client.TopicGrouping == groupingType) return;
            client.TopicGrouping = groupingType;
            await context.SaveChangesAsync();
        }

        public async Task DeleteClientAsync(Client client)
        {
            context.Clients.Remove(client);
            await context.SaveChangesAsync();
        }

        public async Task SetClientAllChatsEnabledAsync(Client client, bool value)
        {
            if (client.AllChatsFilteringEnabled == value) return;
            client.AllChatsFilteringEnabled = value;
            await context.SaveChangesAsync();
        }

        public async Task<HashSet<Chat>> GetClientChatsAsync(long telegramUserId)
        {
            var client = await GetClientAsync(telegramUserId);

            return [.. client.Chats];
        }

        public async Task AddChatAsync(Client client, long telegramChatId, string title)
        {
            if (client.Chats.Any(c => c.TelegramChatId == telegramChatId)) return;
            var chat = new Chat
            {
                TelegramUserId = client.TelegramUserId,
                TelegramChatId = telegramChatId,
                Title = title,
            };
            context.Chats.Add(chat);
            await context.SaveChangesAsync();
        }

        public async Task RemoveChatAsync(Client client, long chatId)
        {
            var chat = await context.Chats
                .FirstOrDefaultAsync(c => c.TelegramUserId == client.TelegramUserId && c.TelegramChatId == chatId);
            if (chat == null) return;
            context.Chats.Remove(chat);
            await context.SaveChangesAsync();
        }

        public async Task<HashSet<Keyword>> GetClientKeywordsAsync(long telegramUserId)
        {
            var client = await GetClientAsync(telegramUserId);

            return [.. client.Keywords];
        }

        public async Task AddKeywordAsync(Client client, string keyword)
        {
            if (client.Keywords.Any(k => k.Value == keyword)) return;
            var newKeyword = new Keyword
            {
                TelegramUserId = client.TelegramUserId,
                Value = keyword,
            };
            context.Keywords.Add(newKeyword);
            await context.SaveChangesAsync();
        }

        public async Task RemoveKeywordAsync(Client client, string keywordWithoutSpecialCharacters,
            Func<string, string> specialCharactersRemover)
        {
            var clientKeywords = await context.Keywords.Where(k =>
                k.TelegramUserId == client.TelegramUserId).ToListAsync();

            var keywordEntity = clientKeywords.FirstOrDefault(
                k => k.Value == keywordWithoutSpecialCharacters
                || specialCharactersRemover(k.Value) == keywordWithoutSpecialCharacters);

            if (keywordEntity == null) return;

            context.Keywords.Remove(keywordEntity);
            await context.SaveChangesAsync();
        }




        public async Task UpdateClientDateAsync(Client client)
        {
            client.RegistrationDataTime = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}