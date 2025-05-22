namespace TelegramForwardly.WebApi.Models.Pocos
{
    public class ClientCurrentStatesOptions
    {
        public string[] States { get; set; } = [];
        public bool RemoveOthers { get; set; }
    }
}
