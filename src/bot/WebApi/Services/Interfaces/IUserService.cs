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
        Task AddUserChatsAsync(long telegramUserId, List<ChatInfo> chats);


        Task<HashSet<Chat>> GetUserChatsAsync(long telegramUserId);

        Task<IEnumerable<Keyword>> GetKeywordsAsync(long telegramUserId);
        Task AddUserKeywordAsync(long telegramUserId, string keyword);
        Task RemoveUserKeywordAsync(long telegramUserId, string keyword);

        Task<IEnumerable<Chat>> GetChatsAsync(long telegramUserId);
        Task AddChatAsync(long telegramUserId, long telegramChatId);
        Task RemoveChatAsync(long telegramUserId, long telegramChatId);
        Task RemoveUserChatsAsync(long telegramUserId, List<long> removedChats);

        Task<HashSet<Keyword>> GetUserKeywordsAsync(long telegramUserId);


        Task UpdateUserDateAsync(long telegramUserId);
    }
}
