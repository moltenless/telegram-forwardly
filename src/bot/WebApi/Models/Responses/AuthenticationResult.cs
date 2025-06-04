namespace TelegramForwardly.WebApi.Models.Responses
{
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public bool RequiresPassword { get; set; }
        public string? SessionString { get; set; }
    }
}
