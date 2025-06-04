using TelegramForwardly.WebApi.Models.Responses;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IAuthApiService
    {
        Task<AuthenticationResult> StartAuthenticationAsync(
            long telegramUserId,
            string phone, 
            string apiId,
            string apiHash);
        Task<AuthenticationResult> VerifyCodeAsync(
            long telegramUserId,
            string verificationCode);
        Task<AuthenticationResult> VerifyPasswordAsync(
            long telegramUserId, 
            string password);
    }
}
