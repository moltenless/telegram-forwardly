using TelegramForwardly.WebApi.Models.Dtos;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<BotUser> GetOrCreateUserAsync(long telegramUserId,
            UserState initialStateIfNew,
            string? userNameIfNew,
            string? firstNameIfNew);
        Task<HashSet<BotUser>> GetAllUsersAsync();
        Task<HashSet<BotUser>> GetAllAuthenticatedUsersAsync();
        Task SetUserStateAsync(long telegramUserId, UserState newState);
        Task UpdateUserPhoneAsync(long telegramUserId, string phone);
        Task UpdateUserApiIdAsync(long telegramUserId, string apiId);
        Task UpdateUserApiHashAsync(long telegramUserId, string apiHash);
        Task CompleteAuthenticationAsync(long telegramUserId, string sessionString);
        Task<IEnumerable<Keyword>> GetKeywordsAsync(long telegramUserId);
        Task AddUserKeywordAsync(long telegramUserId, string keyword);
        Task RemoveUserKeywordAsync(long telegramUserId, string keyword);
        Task<IEnumerable<Chat>> GetChatsAsync(long telegramUserId);
        Task AddChatAsync(long telegramUserId, long telegramChatId);
        Task RemoveChatAsync(long telegramUserId, long telegramChatId);
        Task SetUserForumSupergroupAsync(long telegramUserId, long forumSupergroupId);
        Task SetUserGroupingModeAsync(long telegramUserId, GroupingMode mode);
        Task UpdateUserPasswordAsync(long telegramUserId, string? password);
        Task UpdateUserVerificationCodeAsync(long telegramUserId, string? verificationCode);
        Task RemoveUserVerificationCode(long telegramUserId);
    }
}
