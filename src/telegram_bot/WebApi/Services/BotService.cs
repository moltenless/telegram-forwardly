using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Services.Interfaces;
using TelegramForwardly.WebApi.Models.Dtos;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.DataAccess.Repositories.Interfaces;
using TelegramForwardly.WebApi.Services.Interfaces.Handlers;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramForwardly.WebApi.Services;

public class BotService(
    ITelegramBotClient botClient,
    IUserService userService,
    IUserbotApiService userbotApiService,

    ICommandHandler commandHandler,
    IUserInputHandler userInputHandler,

    ILogger<BotService> logger
    ) : IBotService
{
    private readonly ITelegramBotClient botClient = botClient;
    private readonly IUserService userService = userService;
    private readonly IUserbotApiService userbotApiService = userbotApiService;

    private readonly ICommandHandler commandHandler = commandHandler;
    private readonly IUserInputHandler userInputHandler = userInputHandler;

    private readonly ILogger logger = logger;

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(update.Message!, cancellationToken),
                UpdateType.CallbackQuery => HandleCallbackQueryAsync(update.CallbackQuery!, cancellationToken),
                _ => Task.CompletedTask
            };

            await handler;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling update: {UpdateId}", update.Id);
            await HandleErrorAsync(update, ex, cancellationToken);
        }
    }

    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        var user = await userService.GetOrCreateUserAsync(
            message.From!.Id, UserState.Idle, message.From!.Username, message.From!.FirstName);

        var messageText = message.Text ?? string.Empty;

        if (messageText.StartsWith('/'))
        {
            await commandHandler.HandleCommandAsync(user, message, 
                SendTextMessageAsync, ShowMainMenuAsync, userService,
                cancellationToken);
            return;
        }

        await userInputHandler.HandleUserInputAsync(user, message, cancellationToken);
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task ShowMainMenuAsync(BotUser user, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("🔑 Setup API", "setup"),
                InlineKeyboardButton.WithCallbackData("📝 Keywords", "keywords")
            ],
            [
                InlineKeyboardButton.WithCallbackData("💬 Chats", "chats"),
                InlineKeyboardButton.WithCallbackData("📊 Status", "status"),
            ],
            [
                InlineKeyboardButton.WithCallbackData("⚙️ Settings", "settings"),
                InlineKeyboardButton.WithCallbackData("❓ Help", "help")
            ]
        ]);

        var menuText = (bool)user.IsAuthenticated!
            ? "🏠 Main Menu - You're authenticated and ready to go!"
            : "🏠 Main Menu - Please set up your API credentials first.";

        await botClient.SendMessage(
            chatId: chatId,
            text: menuText,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task SendTextMessageAsync(long chatId, string text, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to chat {ChatId}", chatId);
        }
    }

    private async Task HandleErrorAsync(Update update, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An error occurred while handling update {UpdateId}", update.Id);

        if (update.Message != null)
        {
            try
            {
                await SendTextMessageAsync(update.Message.Chat.Id,
                    "An error occurred while processing your request. Please try again later.",
                    cancellationToken);
            }
            catch
            {
                // Ignore errors when sending error messages
            }
        }
    }
}
