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

        public async Task EnsureStatesPresentAsync(IEnumerable<string> states, bool removeOthers)
        {
            var existingStates = await context.ClientCurrentStates
                .Where(state => states.Contains(state.Value))
                .ToListAsync();
            var newStates = states.Except(existingStates.Select(state => state.Value))
                .Select(state => new ClientCurrentState { Value = state });

            await context.ClientCurrentStates.AddRangeAsync(newStates);

            if (removeOthers)
            {
                var statesToRemove = context.ClientCurrentStates
                    .Where(state => !states.Contains(state.Value));
                context.ClientCurrentStates.RemoveRange(statesToRemove);
            }

            await context.SaveChangesAsync();
        }
    }
}
