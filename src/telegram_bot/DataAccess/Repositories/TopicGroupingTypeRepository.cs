using Microsoft.EntityFrameworkCore;
using TelegramForwardly.DataAccess.Context;
using TelegramForwardly.DataAccess.Entities;
using TelegramForwardly.DataAccess.Repositories.Interfaces;

namespace TelegramForwardly.DataAccess.Repositories
{
    public class TopicGroupingTypeRepository(ForwardlyContext context) : Repository(context), ITopicGroupingTypeRepository
    {
        public async Task<IEnumerable<TopicGroupingType>> GetAllAsync()
        {
            return await context.TopicGroupingTypes.ToListAsync();
        }
    }
}
