using TelegramForwardly.DataAccess.Repositories.Interfaces;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services
{
    public class UserService(
        IClientsRepository clientsRepository,
        IClientCurrentStatesRepository statesRepository,

        ILogger < UserService > logger
        ) : IUserService
    {
        private readonly IClientsRepository clientsRepository = clientsRepository;
        private readonly IClientCurrentStatesRepository statesRepository = statesRepository;

        private readonly ILogger<UserService> logger = logger;


        public Task AddChatAsync(long telegramUserId, long telegramChatId)
        {
            throw new NotImplementedException();
        }

        public Task AddUserKeywordAsync(long telegramUserId, string keyword)
        {
            throw new NotImplementedException();
        }

        public Task CompleteAuthenticationAsync(long telegramUserId, string sessionString)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Chat>> GetChatsAsync(long telegramUserId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Keyword>> GetKeywordsAsync(long telegramUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<BotUser> GetOrCreateUserAsync(long telegramUserId)
        {
            var idleState = await statesRepository.GetStateAsync(nameof(UserState.Idle));
            var client = await clientsRepository.GetOrCreateClientAsync(
                telegramUserId, idleState);

            return BotUser.FromEntity(client);
        }

        public Task RemoveChatAsync(long telegramUserId, long telegramChatId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserKeywordAsync(long telegramUserId, string keyword)
        {
            throw new NotImplementedException();
        }

        public Task SetUserForumSupergroupAsync(long telegramUserId, long forumSupergroupId)
        {
            throw new NotImplementedException();
        }

        public Task SetUserGroupingModeAsync(long telegramUserId, GroupingMode mode)
        {
            throw new NotImplementedException();
        }

        public Task SetUserStateAsync(long telegramUserId, UserState state)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserApiHashAsync(long telegramUserId, string apiHash)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserApiIdAsync(long telegramUserId, string apiId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserPhoneAsync(long telegramUserId, string phone)
        {
            throw new NotImplementedException();
        }
    }
}
