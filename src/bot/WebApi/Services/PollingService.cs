using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services
{
    public class PollingService(
        ITelegramBotClient botClient,
        IServiceProvider serviceProvider,
        IOptions<TelegramConfig> config,
        ILogger<PollingService> logger) : BackgroundService
    {
        private readonly ITelegramBotClient botClient = botClient;
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly TelegramConfig config = config.Value;
        private readonly ILogger<PollingService> logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (config.UseWebhook is true)
            {
                logger.LogInformation("Webhook mode enabled, polling service will not start");
                return;
            }

            logger.LogInformation("Starting Telegram bot polling service...");

            try
            {
                await botClient.DeleteWebhook(cancellationToken: stoppingToken);
                logger.LogInformation("Webhook cleared for polling mode");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to clear webhook, continuing with polling");
            }

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );


            await botClient.SendMessage(-1002852167117, "Haha", cancellationToken: stoppingToken);

            logger.LogInformation("Telegram bot polling started successfully");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Telegram bot polling stopped");
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var botService = scope.ServiceProvider.GetService<IBotService>();

                await botService!.HandleUpdateAsync(update, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling update {UpdateId} in polling mode", update.Id);
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Polling error occurred");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping Telegram bot polling...");
            await base.StopAsync(cancellationToken);
        }
    }
}
