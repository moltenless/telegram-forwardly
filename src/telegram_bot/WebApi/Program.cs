using Microsoft.EntityFrameworkCore;
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

app.UseSwagger();
app.UseSwaggerUI();

app.MapWhen(context => context.Request.Path.StartsWithSegments("/health"), appBuilder =>
{
    appBuilder.UseRouting();
    appBuilder.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/health");
    });
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider;
    var context = provider.GetRequiredService<ForwardlyContext>();
    await context.Database.MigrateAsync();
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
