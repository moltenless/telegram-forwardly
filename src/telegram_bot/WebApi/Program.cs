using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.DataAccess.Context;
using TelegramForwardly.DataAccess.Repositories;
using TelegramForwardly.DataAccess.Repositories.Interfaces;
using TelegramForwardly.WebApi.Controllers;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services;
using TelegramForwardly.WebApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

builder.Services.AddDbContext<ForwardlyContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

builder.Services.Configure<TelegramConfig>(
    builder.Configuration.GetSection("TelegramBot"));

builder.Services.AddSingleton<ITelegramBotClient>(provider =>
{
    var telegramConfig = provider.GetRequiredService<IOptions<TelegramConfig>>()
        .Value ?? throw new InvalidOperationException("Telegram Bot Options are not configured");
    var botToken = telegramConfig.BotToken;

    if (string.IsNullOrEmpty(botToken))
        throw new InvalidOperationException("Telegram Bot Token is not configured");

    return new TelegramBotClient(botToken);
});

builder.Services.AddScoped<IBotService, BotService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserbotApiService, UserbotApiService>();
builder.Services.AddHttpClient<IUserbotApiService, UserbotApiService>();
builder.Services.AddHostedService<PollingService>();

builder.Services.AddScoped<IClientsRepository, ClientsRepository>();
builder.Services.AddScoped<IClientCurrentStatesRepository, ClientCurrentStatesRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider;
    var context = provider.GetRequiredService<ForwardlyContext>();
    var logger = provider.GetRequiredService<ILogger<Program>>();

    var maxRetries = 5;
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed successfully.");
            break;
        }
        catch (SqlException) when (i < maxRetries - 1)
        {
#pragma warning disable S6667 // Logging in a catch clause should pass the caught exception as a parameter.
            logger.LogInformation("Database not ready, retrying...");
#pragma warning restore S6667 // Logging in a catch clause should pass the caught exception as a parameter.
            await Task.Delay(3000);
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider;
    var telegramConfig = provider.GetRequiredService<IOptions<TelegramConfig>>().Value;
    if (telegramConfig.UseWebhook is true
        && !string.IsNullOrEmpty(telegramConfig.WebhookUrl))
    {
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        try
        {
            await botClient.SetWebhook(
                url: $"{telegramConfig.WebhookUrl}/api/telegram/webhook",
                allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery]
            );

            app.Logger.LogInformation("Webhook configured successfully: {WebhookUrl}", telegramConfig.WebhookUrl);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to configure webhook");
        }
    }
    else
    {
        app.Logger.LogInformation("Bot DOESN'T use webhook. IT IS configured for polling mode");
    }
}

await app.RunAsync();
