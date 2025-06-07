using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IUserbotApiService
    {
        Task<LaunchResult> LaunchUserAsync(BotUser user);
        Task<IEnumerable<ChatInfo>> GetUserChatsAsync(long telegramUserId);
        Task<ForumUpdateResult> UpdateUserForumAsync(long telegramUserId, long forumId);
        Task<bool> EnableForwardlyAsync(long telegramUserId);
        Task<bool> DisableForwardlyAsync(long telegramUserId);
    }
}
