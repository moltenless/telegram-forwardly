using TelegramForwardly.WebApi.Models.Responses;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IUserbotApiService
    {
        Task<IEnumerable<ChatInfo>> GetUserChatsAsync(long telegramUserId);
        Task<bool> EnableForwardlyAsync(long telegramUserId);
        Task<bool> DisableForwardlyAsync(long telegramUserId);
    }
}
