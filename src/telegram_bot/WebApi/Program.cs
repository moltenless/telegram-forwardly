using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Telegram.Bot;
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


//// Register Telegram Bot Client
//builder.Services.AddSingleton<ITelegramBotClient>(provider =>
//{
//    var botToken = builder.Configuration["TelegramBot:BotToken"];
//    if (string.IsNullOrEmpty(botToken))
//        throw new InvalidOperationException("Telegram Bot Token is not configured");

//    return new TelegramBotClient(botToken);
//});

//// Register HTTP Client for userbot communication
//builder.Services.AddHttpClient<IUserbotApiService, UserbotApiService>();

//// Register application services
//builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IUserbotApiService, UserbotApiService>();


builder.Services.AddScoped<IClientCurrentStatesRepository, ClientCurrentStatesRepository>();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddJsonConsole();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ForwardlyContext>();
    await context.Database.MigrateAsync();

    var statesRepository = services.GetRequiredService<IClientCurrentStatesRepository>();
    var initialStates = services.GetRequiredService<IOptions<ClientCurrentStatesOptions>>().Value;
    await statesRepository.EnsureStatesPresentAsync
        (initialStates.States, removeOthers: initialStates.RemoveOthers);
}

var telegramBotOptions = builder.Configuration.GetSection("TelegramBot").Get<TelegramBotOptions>();
if (telegramBotOptions?.UseWebhook is true
    && !string.IsNullOrEmpty(telegramBotOptions.WebhookUrl))
{
    using var scope = app.Services.CreateScope();
    //var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

    //try
    //{
    //    await botClient.SetWebhookAsync(
    //        url: $"{telegramOptions.WebhookUrl}/api/telegram/webhook",
    //        allowedUpdates: new[] { Telegram.Bot.Types.Enums.UpdateType.Message, Telegram.Bot.Types.Enums.UpdateType.CallbackQuery }
    //    );

    //    app.Logger.LogInformation("Webhook configured successfully: {WebhookUrl}", telegramOptions.WebhookUrl);
    //}
    //catch (Exception ex)
    //{
    //    app.Logger.LogError(ex, "Failed to configure webhook");
    //}
}

await app.RunAsync();
