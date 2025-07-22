using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<BotUser> GetOrCreateUserAsync(long telegramUserId,
            UserState initialStateIfNew,
            string? userNameIfNew,
            string? firstNameIfNew);
        Task<BotUser> GetUserAsync(long telegramUserId);

        Task<HashSet<BotUser>> GetAllUsersAsync();
        Task<HashSet<BotUser>> GetAllAuthenticatedUsersAsync();

        Task SetUserStateAsync(long telegramUserId, UserState newState);
        Task UpdateUserPhoneAsync(long telegramUserId, string phone);
        Task UpdateUserApiIdAsync(long telegramUserId, string apiId);
        Task UpdateUserApiHashAsync(long telegramUserId, string apiHash);
        Task UpdateUserSessionStringAsync(long telegramUserId, string sessionString);
        Task SetUserAuthenticatedAsync(long telegramUserId, bool isAuthenticated);
        Task UpdateUserForumIdAsync(long telegramUserId, long forumId);
        Task SetUserGroupingModeAsync(long telegramUserId, GroupingMode mode);
        Task DeleteUserAsync(long telegramUserId);
        Task SetUserAllChatsEnabledAsync(long telegramUserId, bool value);

        Task<HashSet<Chat>> GetUserChatsAsync(long telegramUserId);
        Task AddUserChatsAsync(long telegramUserId, List<ChatInfo> chats);
        Task RemoveUserChatsAsync(long telegramUserId, List<long> removedChats);

        Task<HashSet<Keyword>> GetUserKeywordsAsync(long telegramUserId);
        Task AddUserKeywordsAsync(long telegramUserId, string[] keywords);
        Task RemoveKeywordsAsync(long telegramUserId, string[] keywordsWithoutSpecialCharacters,
            Func<string, string> specialCharactersRemover);

        Task UpdateUserDateAsync(long telegramUserId);

        Task ToggleForwardlyEnabledAsync(long telegramUserId, bool value);
        Task SetUserThresholdAsync(long telegramUserId, int limit);
    }
}
