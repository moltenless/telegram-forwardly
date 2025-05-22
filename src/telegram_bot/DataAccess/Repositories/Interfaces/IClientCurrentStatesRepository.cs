using TelegramForwardly.DataAccess.Entities;

namespace TelegramForwardly.DataAccess.Repositories.Interfaces
{
    public interface IClientCurrentStatesRepository
    {
        Task<IEnumerable<ClientCurrentState>> GetAllAsync();
        Task EnsureStatesPresentAsync(IEnumerable<string> states, bool removeOthers);
    }
}
