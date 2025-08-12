using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class PayOSWebhookService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayOSWebhookService> _logger;

        public PayOSWebhookService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PayOSWebhookService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ConfirmWebhook()
        {
            var clientId = Environment.GetEnvironmentVariable("PayOS_Client_ID");
            var apiKey = Environment.GetEnvironmentVariable("PayOS_API_Key");
            var webhookUrl = "https://peertutoringsystem-be.onrender.com/api/webhook/payos";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("PayOS ClientId or ApiKey is not configured in .env file.");
                return;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-client-id", clientId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var requestBody = new { webhookUrl };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://api-merchant.payos.vn/confirm-webhook", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully confirmed PayOS webhook.");
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to confirm PayOS webhook. Status: {response.StatusCode}, Body: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while confirming the PayOS webhook.");
            }
        }
    }
}