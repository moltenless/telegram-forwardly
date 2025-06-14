using Telegram.Bot.Types;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IBotService
    {
        Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
        Task SendMessageAsync(long userId, long forumId, long topicId, string textHeader, string textFooter);
    }
}
