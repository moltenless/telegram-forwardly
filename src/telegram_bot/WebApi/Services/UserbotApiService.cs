using Microsoft.Extensions.Options;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services
{
    public class UserbotApiService : IUserbotApiService
    {
        private readonly HttpClient httpClient;
        private readonly TelegramConfig telegramConfig;
        private readonly ILogger<UserbotApiService> logger;

        public UserbotApiService(
            HttpClient httpClient,
            IOptions<TelegramConfig> telegramConfig,
            ILogger<UserbotApiService> logger)
        {
            this.httpClient = httpClient;
            this.telegramConfig = telegramConfig.Value;
            this.logger = logger;
            httpClient.BaseAddress = new Uri(this.telegramConfig.UserbotApiBaseUrl);
        }

        public Task<AuthenticationResult> StartAuthenticationAsync(
            long telegramUserId,
            string phone,
            string api_id,
            string api_hash)
        {
            return null; 
        }

        public Task<bool> DisableForwardlyAsync(long telegramUserId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EnableForwardlyAsync(long telegramUserId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChatInfo>> GetUserChatsAsync(long telegramUserId)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticationResult> VerifyCodeAsync(long telegramUserId, string verificationCode)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticationResult> VerifyPasswordAsync(long telegramUserId, string password)
        {
            throw new NotImplementedException();
        }
    }
}
