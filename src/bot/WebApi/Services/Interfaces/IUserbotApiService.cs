﻿using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IUserbotApiService
    {
        Task<LaunchResult> LaunchUserAsync(BotUser user);
        Task<FieldUpdateResult> UpdateUserForumAsync(long telegramUserId, long forumId);
        Task<FieldUpdateResult> UpdateUserGroupingAsync(long telegramUserId, GroupingMode mode);
        Task<FieldUpdateResult> DeleteUserAsync(long telegramUserId);
        Task<FieldUpdateResult> SetAllChatsEnabledAsync(long telegramUserId, bool enableAllChats);

        Task<UserChatsResponse> GetUserChatsAsync(long telegramUserId);
        Task<UserChatsResponse> AddChatsAsync(long telegramUserId, List<long> addedChats);
        Task<FieldUpdateResult> RemoveChatsAsync(long telegramUserId, List<long> removedChats);

        Task<FieldUpdateResult> AddKeywordsAsync(long telegramUserId, string[] keywords);
        Task<FieldUpdateResult> RemoveKeywordsAsync(long telegramUserId, 
            string[] keywordsWithoutSpecialCharacters);

        Task<bool> EnableForwardlyAsync(long telegramUserId);
        Task<bool> DisableForwardlyAsync(long telegramUserId);

        Task<FieldUpdateResult> UpdateForwardlyEnabledAsync(long telegramUserId, bool value);
        Task<FieldUpdateResult> UpdateUserThresholdAsync(long telegramUserId, int limit);
    }
}
