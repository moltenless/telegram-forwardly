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
            if (await context.TopicGroupingTypes.CountAsync() > 30)
            {
                context.TopicGroupingTypes.RemoveRange(context.TopicGroupingTypes);
                await context.SaveChangesAsync();
            }

            context.AddRange(
                new TopicGroupingType { Value = $"{TimeOnly.FromDateTime(DateTime.Now)}" }); 
            await context.SaveChangesAsync();

            return await context.TopicGroupingTypes.ToListAsync();
        }
    }
}
