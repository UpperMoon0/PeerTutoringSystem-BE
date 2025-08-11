using System.Threading.Tasks;
using PeerTutoringSystem.Application.DTOs.Payment;

namespace PeerTutoringSystem.Application.Interfaces.Payment
{
    public interface IPayOSService
    {
        Task<PayOSCreatePaymentLinkResponseDto> CreatePaymentLink(PayOSCreatePaymentLinkRequestDto request, string successUrl, string cancelUrl);
    }
}