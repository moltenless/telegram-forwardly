using Microsoft.Extensions.Options;
using System.Net.Http;
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
        private readonly ILogger<UserbotApiService> logger;

        public UserbotApiService(
            HttpClient httpClient,
            IOptions<TelegramConfig> telegramConfig,
            ILogger<UserbotApiService> logger)
        {
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri(telegramConfig.Value.UserbotApiBaseUrl);
            this.logger = logger;
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
    }
}
