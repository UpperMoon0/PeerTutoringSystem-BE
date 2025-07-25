using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class VietQrService : IVietQrService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public VietQrService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<VietQrResponseDto> GenerateQrCode(VietQrRequestDto request)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-client-id", _configuration["VietQR:ClientId"]);
            client.DefaultRequestHeaders.Add("x-api-key", _configuration["VietQR:ApiKey"]);

            var response = await client.PostAsJsonAsync("https://api.vietqr.io/v2/generate", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<VietQrResponseDto>();
            }

            // Handle error response
            return null;
        }
    }
}