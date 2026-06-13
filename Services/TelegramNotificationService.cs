using System.Text;
using System.Text.Json;
using backend_api_dotnet9.Infrastructure;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace backend_api_dotnet9.Services;

public class TelegramNotificationService(
    IHttpClientFactory httpClientFactory,
    IOptions<TelegramOptions> options,
    ILogger<TelegramNotificationService> logger) : ITelegramNotificationService
{
    public void SendOrderCreatedNotification(OrderDetailDto order)
    {
        var telegramOptions = options.Value;
        if (string.IsNullOrWhiteSpace(telegramOptions.BotToken) ||
            string.IsNullOrWhiteSpace(telegramOptions.ChatId))
        {
            logger.LogWarning("Telegram settings not configured, skipping notification");
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var message = BuildOrderMessage(order);
                var url = $"https://api.telegram.org/bot{telegramOptions.BotToken}/sendMessage";

                var payload = new
                {
                    chat_id = telegramOptions.ChatId,
                    text = message,
                    parse_mode = "HTML"
                };

                var client = httpClientFactory.CreateClient("Telegram");
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    logger.LogError("Telegram API error {StatusCode}: {Body}",
                        response.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send Telegram notification for order {OrderId}",
                    order.Id);
            }
        });
    }

    private static string BuildOrderMessage(OrderDetailDto order)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"🛒 <b>Đơn hàng mới #{order.Id}</b>");
        sb.AppendLine();
        sb.AppendLine($"👤 Khách hàng: {Escape(order.CustomerName)}");
        sb.AppendLine($"📞 SĐT: {Escape(order.CustomerPhone)}");

        if (!string.IsNullOrWhiteSpace(order.CustomerEmail))
            sb.AppendLine($"📧 Email: {Escape(order.CustomerEmail)}");

        sb.AppendLine();
        sb.AppendLine("📦 <b>Sản phẩm:</b>");

        foreach (var item in order.Items)
        {
            sb.AppendLine($"  • {Escape(item.ProductName)} — SL: {item.Quantity} — Giá: {item.UnitPrice:#,0}đ");
        }

        sb.AppendLine();
        sb.AppendLine($"💰 <b>Tổng: {order.TotalAmount:#,0}đ</b>");

        if (!string.IsNullOrWhiteSpace(order.Note))
        {
            sb.AppendLine();
            sb.AppendLine($"📝 Ghi chú: {Escape(order.Note)}");
        }

        return sb.ToString().TrimEnd();
    }

    private static string Escape(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
