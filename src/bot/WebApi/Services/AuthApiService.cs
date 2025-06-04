using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<IAuthApiService> logger;

        public AuthApiService(
            HttpClient httpClient,
            IOptions<TelegramConfig> telegramConfig,
            ILogger<AuthApiService> logger)
        {
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri(telegramConfig.Value.AuthApiBaseUrl);
            this.logger = logger;
        }

        public async Task StartAuthenticationAsync(
            long telegramUserId, 
            string phone, 
            string apiId, 
            string apiHash,
            string? password)
        {
            try
            {
                var request = new
                {
                    user_id = telegramUserId,
                    phone,
                    api_id = apiId,
                    api_hash = apiHash,
                    password = password ?? string.Empty
                };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                httpClient.Timeout = TimeSpan.FromMinutes(3);/////
                logger.LogInformation("Starting authentication for user {TelegramUserId} with phone {Phone}, API ID {ApiId}, API Hash {ApiHash}", telegramUserId, phone, apiId, apiHash);
                httpClient.PostAsync("/start", content);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting authentication for user {TelegramUserId}", telegramUserId);
            }
        }

        public async Task<AuthenticationResult> VerifyCodeAsync(
            long telegramUserId, 
            string verificationCode)
        {
            try
            {
                var request = new { user_id = telegramUserId, code = verificationCode };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/verify", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<AuthenticationResult>(responseContent);
                return result ?? new AuthenticationResult { Success = false, ErrorMessage = "Unknown error" };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error verifying code for user {UserId}", telegramUserId);
                return new AuthenticationResult { Success = false, ErrorMessage = "Connection error" };
            }
        }

        public async Task<AuthenticationResult> VerifyPasswordAsync(long telegramUserId, string password)
        {
            return null;
        }
    }
}
