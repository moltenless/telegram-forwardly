using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
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

        public async Task<AuthenticationResult> StartAuthenticationAsync(
            long telegramUserId,
            string phone,
            string apiId,
            string apiHash)
        {
            try
            {
                var request = new {
                    telegram_user_id = telegramUserId,
                    phone,
                    api_id = apiId,
                    api_hash = apiHash
                };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/api/auth/start", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return new AuthenticationResult { Success = true };

                var errorResult = JsonSerializer.Deserialize<AuthenticationResult>(responseContent);
                return errorResult ?? new AuthenticationResult { Success = false, ErrorMessage = "Unknown error occurred." };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting authentication for user {TelegramUserId}", telegramUserId);
                return new AuthenticationResult { Success = false, ErrorMessage = "Connection error" };
            }
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
