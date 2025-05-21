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
            context.AddRange(
                new TopicGroupingType { Value = "Test" }, 
                new TopicGroupingType { Value = "Test 2"}, 
                new TopicGroupingType { Value = "test 3"});
            await context.SaveChangesAsync();
            return await context.TopicGroupingTypes.ToListAsync();
        }
    }
}
