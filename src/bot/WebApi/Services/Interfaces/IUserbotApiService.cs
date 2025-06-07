using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IUserbotApiService
    {
        Task<LaunchResult> LaunchUserAsync(BotUser user);
        Task<IEnumerable<ChatInfo>> GetUserChatsAsync(long telegramUserId);
        Task<FieldUpdateResult> UpdateUserForumAsync(long telegramUserId, long forumId);
        Task<FieldUpdateResult> UpdateUserGroupingAsync(long telegramUserId, GroupingMode mode);
        Task<bool> EnableForwardlyAsync(long telegramUserId);
        Task<bool> DisableForwardlyAsync(long telegramUserId);
        Task<FieldUpdateResult> DeleteUserAsync(long telegramUserId);
    }
}
