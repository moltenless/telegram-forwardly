namespace TelegramForwardly.WebApi.Models.Dtos
{
    public class ClientCurrentStatesOptions
    {
        public string[] States { get; set; } = [];
        public bool RemoveOthers { get; set; }
    }
}
