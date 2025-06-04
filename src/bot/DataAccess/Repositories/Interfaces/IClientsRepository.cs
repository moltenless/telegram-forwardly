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
        Task<HashSet<Client>> GetAllClientsAsync();
        Task<HashSet<Client>> GetAllAuthenticatedUsersAsync();
        Task UpdateClientPasswordAsync(Client client, string? password);
        Task UpdateClientVerificationCodeAsync(Client client, string? verificationCode);
        Task CompleteClientAuthentication(Client client, string sessionString);
    }
}
