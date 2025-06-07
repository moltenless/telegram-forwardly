using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Telegram.Bot.Types;
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
            this.logger = logger;
            this.httpClient.BaseAddress = new Uri(telegramConfig.Value.UserbotApiBaseUrl);
        }


        public async Task<LaunchResult> LaunchUserAsync(BotUser user)
        {
            try
            {
                var request = JsonSerializer.Serialize(user);
                var content = new StringContent(request, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/user/launch", content);

                var result = JsonSerializer.Deserialize<LaunchResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error launching userbot API");
                return new LaunchResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FieldUpdateResult> UpdateUserForumAsync(
            long telegramUserId,
            long forumId)
        {
            try
            {
                var request = JsonSerializer.Serialize(new { user_id = telegramUserId, forum_id = forumId });
                var content = new StringContent(request, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/user/forum", content);
                var result = JsonSerializer.Deserialize<FieldUpdateResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Checking and updating this forum have failed");
                return new FieldUpdateResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FieldUpdateResult> UpdateUserGroupingAsync(
            long telegramUserId,
            GroupingMode mode)
        {
            try
            {
                var request = JsonSerializer.Serialize(new { user_id = telegramUserId, grouping = mode.ToString() });
                var content = new StringContent(request, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/user/grouping", content);
                var result = JsonSerializer.Deserialize<FieldUpdateResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user grouping type");
                return new FieldUpdateResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FieldUpdateResult> DeleteUserAsync(long telegramUserId)
        {
            try
            {
                var request = JsonSerializer.Serialize(new { user_id = telegramUserId });
                var content = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("api/user/delete", content);
                var result = JsonSerializer.Deserialize<FieldUpdateResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting user");
                return new FieldUpdateResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FieldUpdateResult> SetAllChatsEnabledAsync(long telegramUserId, bool enableAllChats)
        {
            try
            {
                var request = JsonSerializer.Serialize(new { user_id = telegramUserId, enable_all_chats = enableAllChats });
                var content = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("api/user/chats/all", content);
                var result = JsonSerializer.Deserialize<FieldUpdateResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting enabled or disabled all chats being listened");
                return new FieldUpdateResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
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
    }
}
