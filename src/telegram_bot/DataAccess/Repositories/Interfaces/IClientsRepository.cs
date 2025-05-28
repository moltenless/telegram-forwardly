using TelegramForwardly.DataAccess.Entities;

namespace TelegramForwardly.DataAccess.Repositories.Interfaces
{
    public interface IClientsRepository
    {
        Task<Client> GetOrCreateClientAsync(long telegramUserId, ClientCurrentState initialState);
    }
}
