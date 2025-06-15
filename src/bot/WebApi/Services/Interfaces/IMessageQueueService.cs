using TelegramForwardly.WebApi.Models.Requests;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IMessageQueueService
    {
        Queue<SendMessageRequest> MessageQueue { get; set; }
    }
}
