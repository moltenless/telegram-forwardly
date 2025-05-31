using TelegramForwardly.WebApi.Models.Responses;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IUserbotApiService
    {
        Task<AuthenticationResult> StartAuthenticationAsync(
            long telegramUserId,
            string phone, 
            string apiId,
            string apiHash);
        Task<AuthenticationResult> VerifyCodeAsync(long telegramUserId, string verificationCode);
        Task<AuthenticationResult> VerifyPasswordAsync(long telegramUserId, string password);
        Task<IEnumerable<ChatInfo>> GetUserChatsAsync(long telegramUserId);
        Task<bool> EnableForwardlyAsync(long telegramUserId);
        Task<bool> DisableForwardlyAsync(long telegramUserId);
    }
}
