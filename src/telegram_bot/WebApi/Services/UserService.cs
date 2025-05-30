using TelegramForwardly.DataAccess.Repositories.Interfaces;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services
{
    public class UserService(
        IClientsRepository clientsRepository,
        IClientCurrentStatesRepository statesRepository,

        ILogger<UserService> logger
        ) : IUserService
    {
        private readonly IClientsRepository clientsRepository = clientsRepository;
        private readonly IClientCurrentStatesRepository statesRepository = statesRepository;

        private readonly ILogger<UserService> logger = logger;

        public async Task<BotUser> GetOrCreateUserAsync(long telegramUserId,
            UserState initialStateIfNew,
            string? userNameIfNew,
            string? firstNameIfNew)
        {
            var stateIfNew = await statesRepository.GetStateOrDefaultAsync(initialStateIfNew.ToString())
                ?? throw new InvalidOperationException($"{initialStateIfNew} state not found in database");

            var client = await clientsRepository.GetOrCreateClientAsync(
                telegramUserId, stateIfNew, userNameIfNew, firstNameIfNew);

            return BotUser.FromEntity(client);
        }

        public async Task SetUserStateAsync(long telegramUserId, UserState newState)
        {
            var client = await clientsRepository.GetClientOrDefaultAsync(telegramUserId);
            if (client is null) return;

            var state = await statesRepository.GetStateOrDefaultAsync(newState.ToString());

            if (state is null)
            {
                logger.LogError("State {StateName} not found in database.", newState.ToString());
                return;
            }

            await clientsRepository.SetClientStateAsync(client, state);
        }

        public async Task AddChatAsync(long telegramUserId, long telegramChatId)
        {
        }

        public async Task AddUserKeywordAsync(long telegramUserId, string keyword)
        {
        }

        public async Task CompleteAuthenticationAsync(long telegramUserId, string sessionString)
        {
        }

        public async Task<IEnumerable<Chat>> GetChatsAsync(long telegramUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Keyword>> GetKeywordsAsync(long telegramUserId)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveChatAsync(long telegramUserId, long telegramChatId)
        {
        }

        public async Task RemoveUserKeywordAsync(long telegramUserId, string keyword)
        {
        }

        public async Task SetUserForumSupergroupAsync(long telegramUserId, long forumSupergroupId)
        {
        }

        public async Task SetUserGroupingModeAsync(long telegramUserId, GroupingMode mode)
        {
        }

        public async Task UpdateUserApiHashAsync(long telegramUserId, string apiHash)
        {
        }

        public async Task UpdateUserApiIdAsync(long telegramUserId, string apiId)
        {
        }

        public async Task UpdateUserPhoneAsync(long telegramUserId, string phone)
        {
        }
    }
}
