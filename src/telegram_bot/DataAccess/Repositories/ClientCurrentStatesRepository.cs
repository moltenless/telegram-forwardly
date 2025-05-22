using Microsoft.EntityFrameworkCore;
using TelegramForwardly.DataAccess.Context;
using TelegramForwardly.DataAccess.Entities;
using TelegramForwardly.DataAccess.Repositories.Interfaces;

namespace TelegramForwardly.DataAccess.Repositories
{
    public class ClientCurrentStatesRepository(ForwardlyContext context) : Repository(context), IClientCurrentStatesRepository
    {
        public async Task<IEnumerable<ClientCurrentState>> GetAllAsync()
        {
            return await context.ClientCurrentStates.ToListAsync();
        }
    }
}
