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

        public async Task<string> ConfirmWebhook()
        {
            var clientId = Environment.GetEnvironmentVariable("PayOS_Client_ID");
            var apiKey = Environment.GetEnvironmentVariable("PayOS_API_Key");
            var webhookUrl = "https://peertutoringsystem-be.onrender.com/api/webhook/payos";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey))
            {
                var errorMessage = "PayOS ClientId or ApiKey is not configured in .env file.";
                _logger.LogError(errorMessage);
                return errorMessage;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-client-id", clientId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            _logger.LogInformation($"PayOS webhook headers: x-client-id={clientId}, x-api-key={apiKey}");

            var requestBody = new { webhookUrl };
            var jsonPayload = JsonSerializer.Serialize(requestBody);
            _logger.LogInformation($"PayOS webhook request payload: {jsonPayload}");
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://api-merchant.payos.vn/confirm-webhook", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully confirmed PayOS webhook.");
                    return $"Successfully confirmed PayOS webhook. Response: {responseBody}";
                }
                else
                {
                    _logger.LogError($"Failed to confirm PayOS webhook. Status: {response.StatusCode}, Body: {responseBody}");
                    return $"Failed to confirm PayOS webhook. Status: {response.StatusCode}, Body: {responseBody}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while confirming the PayOS webhook.");
                return $"An error occurred while confirming the PayOS webhook: {ex.Message}";
            }
        }
    }
}