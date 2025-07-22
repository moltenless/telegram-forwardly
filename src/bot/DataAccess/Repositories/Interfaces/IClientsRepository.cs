using TelegramForwardly.DataAccess.Entities;

namespace TelegramForwardly.DataAccess.Repositories.Interfaces
{
    public interface IClientsRepository
    {
        Task<Client> GetOrCreateClientAsync(
            long telegramUserId, 
            ClientCurrentState initialStateIfNew,
            string? userNameIfNew,
            string? firstNameIfNew);

        Task<Client> GetClientAsync(long telegramUserId);
        Task<Client?> GetClientOrDefaultAsync(long telegramUserId);

        Task SetClientStateAsync(Client client, ClientCurrentState newState);

        Task UpdateClientPhoneAsync(Client client, string phone);
        Task UpdateClientApiIdAsync(Client client, string apiId);
        Task UpdateClientApiHashAsync(Client client, string apiHash);
        Task UpdateClientSessionStringAsync(Client client, string sessionString);
        Task SetClientAuthenticatedAsync(Client client, bool isAuthenticated);
        Task UpdateClientForumIdAsync(Client client, long forumId);
        Task SetClientGroupingAsync(Client client, string groupingType);

        Task<HashSet<Client>> GetAllClientsAsync();
        Task<HashSet<Client>> GetAllAuthenticatedUsersAsync();
        Task DeleteClientAsync(Client client);
        Task SetClientAllChatsEnabledAsync(Client client, bool value);

        Task<HashSet<Chat>> GetClientChatsAsync(long telegramUserId);
        Task AddChatAsync(Client client, long telegramChatId, string title);
        Task RemoveChatAsync(Client client, long chatId);

        Task<HashSet<Keyword>> GetClientKeywordsAsync(long telegramUserId);
        Task AddKeywordAsync(Client client, string keyword);
        Task RemoveKeywordAsync(Client client, string keywordWithoutSpecialCharacters, 
            Func<string, string> specialCharactersRemover);


        Task UpdateClientDateAsync(Client client);
        Task SetClientForwardlyEnabledAsync(Client client, bool value);
        Task SetClientThresholdAsync(Client client, int limit);
    }
}
