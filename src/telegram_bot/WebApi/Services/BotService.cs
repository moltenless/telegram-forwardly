using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Services.Interfaces;
using TelegramForwardly.WebApi.Models.Dtos;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.WebApi.Services.Interfaces.Handlers;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramForwardly.WebApi.Services;

public class BotService(
    ITelegramBotClient botClient,
    IUserService userService,
    IUserbotApiService userbotApiService,

    ICommandHandler commandHandler,
    ICallbackHandler callbackHandler,
    IUserInputHandler userInputHandler,

    ILogger<BotService> logger
    ) : IBotService
{
    private readonly ITelegramBotClient botClient = botClient;
    private readonly IUserService userService = userService;
    private readonly IUserbotApiService userbotApiService = userbotApiService;

    private readonly ICommandHandler commandHandler = commandHandler;
    private readonly ICallbackHandler callbackHandler = callbackHandler;
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
                SendTextMessageAsync, ShowMainMenuAsync, 
                botClient, userService,
                cancellationToken);
            return;
        }

        await userInputHandler.HandleUserInputAsync(user, message, cancellationToken);
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var user = await userService.GetOrCreateUserAsync(
            callbackQuery.From!.Id, UserState.Idle, callbackQuery.From!.Username, callbackQuery.From!.FirstName);
        var data = callbackQuery.Data!;

        await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

        await callbackHandler.HandleCallbackAsync(
            user, data, callbackQuery, commandHandler, cancellationToken);
    }

    private async Task ShowMainMenuAsync(BotUser user, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("🔑 Setup credentials", "setup"),
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
            : "🏠 Main Menu - Please set up your credentials first.";

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
                text: EscapeMarkdownV2(text),
                //replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2,
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

    public static string EscapeMarkdownV2(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var specialChars = new[] { '\\', '_', /*'*',*/ '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

        var builder = new System.Text.StringBuilder();

        foreach (var ch in text)
        {
            if (Array.IndexOf(specialChars, ch) != -1)
                builder.Append('\\');
            builder.Append(ch);
        }

        return builder.ToString();
    }
}
