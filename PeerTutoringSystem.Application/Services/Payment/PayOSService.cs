using System.Threading.Tasks;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class PayOSService : IPayOSService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PayOSService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<PayOSCreatePaymentLinkResponseDto> CreatePaymentLink(PayOSCreatePaymentLinkRequestDto request)
        {
            var client = _httpClientFactory.CreateClient();
            var clientId = _configuration["PayOS_Client_ID"];
            var apiKey = _configuration["PayOS_API_Key"];
            var checksumKey = _configuration["PayOS_Checksum_Key"];

            var dataToSign = $"amount={request.amount}&cancelUrl={request.cancelUrl}&description={request.description}&orderCode={request.orderCode}&returnUrl={request.returnUrl}";
            request.signature = CreateSignature(dataToSign, checksumKey);

            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Add("x-client-id", clientId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await client.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PayOSCreatePaymentLinkResponseDto>(jsonResponse);
            }

            // Handle error
            return null;
        }

        private string CreateSignature(string data, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}