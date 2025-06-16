using System.Data;
using Telegram.Bot.Types;
using TelegramForwardly.DataAccess.Repositories.Interfaces;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services
{
    public class UserService(
        IClientsRepository clientsRepository,
        IClientCurrentStatesRepository statesRepository
        ) : IUserService
    {
        private readonly IClientsRepository clientsRepository = clientsRepository;
        private readonly IClientCurrentStatesRepository statesRepository = statesRepository;

        public async Task<BotUser> GetOrCreateUserAsync(long telegramUserId,
            UserState initialStateIfNew,
            string? userNameIfNew,
            string? firstNameIfNew)
        {
            var stateIfNew = await statesRepository.GetStateOrDefaultAsync(initialStateIfNew.ToString())
                ?? throw new InvalidOperationException($"{initialStateIfNew} state not found in database");

            var client = await clientsRepository.GetOrCreateClientAsync(
                telegramUserId, stateIfNew, userNameIfNew, firstNameIfNew);

            await UpdateUserDateAsync(telegramUserId);
            return BotUser.FromEntity(client);
        }

        public async Task<BotUser> GetUserAsync(long telegramUserId)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await UpdateUserDateAsync(telegramUserId);
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

        public async Task<HashSet<Models.Dtos.Chat>> GetUserChatsAsync(long telegramUserId)
        {
            var chats = await clientsRepository.GetClientChatsAsync(telegramUserId);
            return [.. chats.Select(Models.Dtos.Chat.FromEntity)];
        }

        public async Task AddUserChatsAsync(long telegramUserId, List<ChatInfo> chats)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            foreach (var chat in chats)
                await clientsRepository.AddChatAsync(client, chat.Id, chat.Title);
        }

        public async Task RemoveUserChatsAsync(long telegramUserId, List<long> removedChats)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            foreach (var chatId in removedChats)
                await clientsRepository.RemoveChatAsync(client, chatId);
        }


        public async Task<HashSet<Keyword>> GetUserKeywordsAsync(long telegramUserId)
        {
            var keywords = await clientsRepository.GetClientKeywordsAsync(telegramUserId);
            return [.. keywords.Select(Keyword.FromEntity)];
        }

        public async Task AddUserKeywordsAsync(long telegramUserId, string[] keywords)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    continue;
                await clientsRepository.AddKeywordAsync(client, keyword);
            }   
        }

        public async Task RemoveKeywordsAsync(
            long telegramUserId, 
            string[] keywordsWithoutSpecialCharacters,
            Func<string, string> specialCharactersRemover)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            foreach (var keywordWithoutSpecialCharacters in keywordsWithoutSpecialCharacters)
                await clientsRepository.RemoveKeywordAsync(client, 
                    keywordWithoutSpecialCharacters,
                    specialCharactersRemover);
        }


        public async Task UpdateUserDateAsync(long telegramUserId)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.UpdateClientDateAsync(client);
        }

        public async Task ToggleForwardlyEnabledAsync(long telegramUserId, bool value)
        {
            var client = await clientsRepository.GetClientAsync(telegramUserId);
            await clientsRepository.SetClientForwardlyEnabledAsync(client, value);
        }
    }
}
