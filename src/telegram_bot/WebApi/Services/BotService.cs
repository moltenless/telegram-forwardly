using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Services.Interfaces;
using TelegramForwardly.WebApi.Models.Dtos;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.DataAccess.Repositories.Interfaces;

namespace TelegramForwardly.WebApi.Services;

public class BotService(
    ITelegramBotClient botClient,
    IUserService userService,
    IUserbotApiService userbotApiService,
    IOptions<TelegramConfig> config,
    ILogger<BotService> logger,
    IClientCurrentStatesRepository statesRepository) : IBotService
{
    private readonly ITelegramBotClient botClient = botClient;
    private readonly IUserService userService = userService;
    private readonly IUserbotApiService userbotApiService = userbotApiService;
    private readonly TelegramConfig config = config.Value;
    private readonly ILogger logger = logger;
    private readonly IClientCurrentStatesRepository statesRepository = statesRepository;

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
        string states = string.Join(", ", (await statesRepository.GetAllAsync()).Select(s => s.Value));
        await SendTextMessageAsync(message.Chat.Id,
            "This bot is currently under development. Please check back later." +
            "\nBy the way states in db are:\n" + states,
            cancellationToken);
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
