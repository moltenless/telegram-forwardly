using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.DataAccess.Context;
using TelegramForwardly.DataAccess.Repositories;
using TelegramForwardly.DataAccess.Repositories.Interfaces;
using TelegramForwardly.WebApi.Models.Pocos;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    );

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

builder.Services.Configure<ClientCurrentStatesOptions>(
    builder.Configuration.GetSection("ClientCurrentStates"));
builder.Services.Configure<TelegramBotOptions>(
    builder.Configuration.GetSection("TelegramBot"));


builder.Services.AddSingleton<ITelegramBotClient>(provider =>
{
    var options = provider.GetRequiredService<IOptions<TelegramBotOptions>>()
        .Value ?? throw new InvalidOperationException("Telegram Bot Options are not configured");
    var botToken = options.BotToken;

    if (string.IsNullOrEmpty(botToken))
        throw new InvalidOperationException("Telegram Bot Token is not configured");

    return new TelegramBotClient(botToken);
});


//builder.Services.AddHttpClient<IUserbotApiService, UserbotApiService>();

//builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IUserbotApiService, UserbotApiService>();




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddJsonConsole();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider;
    var context = provider.GetRequiredService<ForwardlyContext>();
    await context.Database.MigrateAsync();

    var statesRepository = provider.GetRequiredService<IClientCurrentStatesRepository>();
    var initialStatesOptions = provider.GetRequiredService<IOptions<ClientCurrentStatesOptions>>().Value;
    await statesRepository.EnsureStatesPresentAsync
        (initialStatesOptions.States, removeOthers: initialStatesOptions.RemoveOthers);
}

using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider;
    var telegramBotOptions = provider.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
    if (telegramBotOptions?.UseWebhook is true
        && !string.IsNullOrEmpty(telegramBotOptions.WebhookUrl))
    {
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        try
        {
            await botClient.SetWebhook(
                url: $"{telegramBotOptions.WebhookUrl}/api/telegram/webhook",
                allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery]
            );

            app.Logger.LogInformation("Webhook configured successfully: {WebhookUrl}", telegramBotOptions.WebhookUrl);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to configure webhook");
        }
    }
}

await app.RunAsync();
