using TelegramForwardly.WebApi.Models.Responses;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IAuthApiService
    {
        Task StartAuthenticationAsync(
            long telegramUserId,
            string phone, 
            string apiId,
            string apiHash,
            string? password);
        Task<AuthenticationResult> VerifyCodeAsync(
            long telegramUserId,
            string verificationCode);
        Task<AuthenticationResult> VerifyPasswordAsync(
            long telegramUserId, 
            string password);
    }
}
