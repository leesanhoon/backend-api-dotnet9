namespace backend_api_dotnet9.Infrastructure;

public sealed class TelegramOptions
{
    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
}
