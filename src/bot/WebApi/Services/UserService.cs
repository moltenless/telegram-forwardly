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

        public async Task<BotUser> GetUserAsync(long telegramUserId)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
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
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            var state = await statesRepository.GetStateAsync(newState.ToString());

            await clientsRepository.SetClientStateAsync(client, state);
        }

        public async Task UpdateUserPhoneAsync(long telegramUserId, string phone)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.UpdateClientPhoneAsync(client, phone);
        }

        public async Task UpdateUserApiIdAsync(long telegramUserId, string apiId)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.UpdateClientApiIdAsync(client, apiId);
        }

        public async Task UpdateUserApiHashAsync(long telegramUserId, string apiHash)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.UpdateClientApiHashAsync(client, apiHash);
        }

        public async Task UpdateUserSessionStringAsync(long telegramUserId, string sessionString)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.UpdateClientSessionStringAsync(client, sessionString);
        }

        public async Task SetUserAuthenticatedAsync(long telegramUserId, bool isAuthenticated)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.SetClientAuthenticatedAsync(client, isAuthenticated);
        }

        public async Task UpdateUserForumIdAsync(long telegramUserId, long forumId)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.UpdateClientForumIdAsync(client, forumId);
        }

        public async Task SetUserGroupingModeAsync(long telegramUserId, GroupingMode mode)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.SetClientGroupingAsync(client, mode.ToString());
        }

        public async Task DeleteUserAsync(long telegramUserId)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.DeleteClientAsync(client);
        }

        public async Task SetUserAllChatsEnabledAsync(long telegramUserId, bool value)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.SetClientAllChatsEnabledAsync(client, value: value);
        }

        public async Task<HashSet<Chat>> GetUserChatsAsync(long telegramUserId)
        {
            HashSet<DataAccess.Entities.Chat> chats =
                    await clientsRepository.GetClientChatsAsync(telegramUserId);
            return [.. chats.Select(Chat.FromEntity)];
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
    }
}
