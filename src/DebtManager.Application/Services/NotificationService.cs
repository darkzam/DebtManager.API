using DebtManager.Application.Common.Interfaces;
using DebtManager.Application.Models;
using Microsoft.Extensions.Options;

namespace DebtManager.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IOptions<NotificationSettings> _settings;
        private readonly HttpClient _httpClient;

        public NotificationService(HttpClient httpClient,
                                   IOptions<NotificationSettings> settings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task Notify(string message)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(new { message = message });
            await _httpClient.PostAsync(_settings.Value.Webhook, new StringContent(json));
        }
    }
}
