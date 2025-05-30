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
    }
}
