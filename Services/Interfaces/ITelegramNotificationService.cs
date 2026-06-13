namespace backend_api_dotnet9.Services.Interfaces;

public interface ITelegramNotificationService
{
    void SendOrderCreatedNotification(OrderDetailDto order);
}
