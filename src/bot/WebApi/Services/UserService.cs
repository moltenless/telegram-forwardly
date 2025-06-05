using System.Data;
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

        public async Task<HashSet<BotUser>> GetAllUsersAsync()
        {
            var clients = await clientsRepository.GetAllClientsAsync();
            return [.. clients.Select(BotUser.FromEntity)];
        }

        public async Task<HashSet<BotUser>> GetAllAuthenticatedUsersAsync()
        {
            var clients = await clientsRepository.GetAllAuthenticatedUsersAsync();
            return [.. clients.Select(BotUser.FromEntity)];
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

        public async Task UpdateUserPhoneAsync(long telegramUserId, string phone)
        {
            var client = await clientsRepository.GetClientOrDefaultAsync(telegramUserId);
            if (client is not null)
                await clientsRepository.UpdateClientPhoneAsync(client, phone);
        }

        public async Task UpdateUserApiIdAsync(long telegramUserId, string apiId)
        {
            var client = await clientsRepository.GetClientOrDefaultAsync(telegramUserId);
            if (client is not null)
                await clientsRepository.UpdateClientApiIdAsync(client, apiId);
        }

        public async Task UpdateUserApiHashAsync(long telegramUserId, string apiHash)
        {
            var client = await clientsRepository.GetClientOrDefaultAsync(telegramUserId);
            if (client is not null)
                await clientsRepository.UpdateClientApiHashAsync(client, apiHash);
        }

        public async Task UpdateUserSessionStringAsync(long telegramUserId, string sessionString)
        {
            var client = await clientsRepository.GetClientOrDefaultAsync(telegramUserId);
            if (client is not null)
                await clientsRepository.UpdateClientSessionStringAsync(client, sessionString);
        }







        public async Task AddChatAsync(long telegramUserId, long telegramChatId)
        {
        }

        public async Task AddUserKeywordAsync(long telegramUserId, string keyword)
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
    }
}
