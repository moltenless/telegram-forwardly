using TelegramForwardly.WebApi.Models.Requests;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services
{
    public class MessageQueueService : IMessageQueueService
    {
        public Queue<SendMessageRequest> MessageQueue { get; set; }

        public MessageQueueService()
        {
            MessageQueue = new Queue<SendMessageRequest>();
        }
    }
}
