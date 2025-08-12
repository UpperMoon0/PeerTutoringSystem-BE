using Microsoft.AspNetCore.Http;
using PeerTutoringSystem.Application.DTOs.Payment;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Payment
{
    public interface IPayOSWebhookService
    {
        Task ProcessPayOSWebhook(PayOSWebhookData webhookData);
        Task<string> ProcessWebhook(HttpRequest request);
    }
}