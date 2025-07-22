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
        private readonly string apiKey;
        private readonly ILogger<UserbotApiService> logger;

        public UserbotApiService(
            HttpClient httpClient,
            IOptions<TelegramConfig> telegramConfig,
            ILogger<UserbotApiService> logger)
        {
            this.apiKey = telegramConfig.Value.ApiKey;
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri(telegramConfig.Value.UserbotApiBaseUrl);
            this.httpClient.DefaultRequestHeaders.Add("X-Api-Key", this.apiKey);
            this.logger = logger;
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

        public async Task<UserChatsResponse> GetUserChatsAsync(long telegramUserId)
        {
            try
            {
                var response = await httpClient.GetAsync($"api/user/{telegramUserId}/chats");
                var result = JsonSerializer.Deserialize<UserChatsResponse>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting user chats");
                return new UserChatsResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Chats = []
                };
            }
        }

        public async Task<UserChatsResponse> AddChatsAsync(long telegramUserId, List<long> addedChats)
        {
            try
            {
                var request = JsonSerializer.Serialize(new { user_id = telegramUserId, chats = addedChats });
                var content = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("api/user/chats/add", content);
                var result = JsonSerializer.Deserialize<UserChatsResponse>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding chats to user");
                return new UserChatsResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Chats = []
                };
            }
        }

        public async Task<FieldUpdateResult> RemoveChatsAsync(
            long telegramUserId, List<long> removedChats)
        {
            try
            {
                var request = JsonSerializer.Serialize(new { user_id = telegramUserId, chats = removedChats });
                var content = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("api/user/chats/remove", content);
                var result = JsonSerializer.Deserialize<FieldUpdateResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing chats from user");
                return new FieldUpdateResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<FieldUpdateResult> AddKeywordsAsync(long telegramUserId, string[] keywords)
        {
            try
            {
                var request = JsonSerializer.Serialize(new { user_id = telegramUserId, keywords });
                var content = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("api/user/keywords/add", content);
                var result = JsonSerializer.Deserialize<FieldUpdateResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding keywords to user");
                return new FieldUpdateResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FieldUpdateResult> RemoveKeywordsAsync(
            long telegramUserId,
            string[] keywordsWithoutSpecialCharacters)
        {
            try
            {
                var request = JsonSerializer.Serialize(new { user_id = telegramUserId, 
                    keywordsWithoutSpecialCharacters });
                var content = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("api/user/keywords/remove", content);
                var result = JsonSerializer.Deserialize<FieldUpdateResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing keywords from user");
                return new FieldUpdateResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FieldUpdateResult> UpdateForwardlyEnabledAsync(long telegramUserId, bool value)
        {
            try
            {
                var request = JsonSerializer.Serialize(new { user_id = telegramUserId, value });
                var content = new StringContent(request, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/user/forwardly", content);
                var result = JsonSerializer.Deserialize<FieldUpdateResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating forwardly enabled");
                return new FieldUpdateResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FieldUpdateResult> UpdateUserThresholdAsync(
            long telegramUserId,
            int limit)
        {
            try
            {
                var request = JsonSerializer.Serialize(new
                {
                    user_id = telegramUserId,
                    limit
                });
                var content = new StringContent(request, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/user/threshold", content);
                var result = JsonSerializer.Deserialize<FieldUpdateResult>(
                    await response.Content.ReadAsStringAsync());
                return result!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user threshold");
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
    }
}
