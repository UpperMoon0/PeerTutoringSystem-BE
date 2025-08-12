using PeerTutoringSystem.Application.DTOs.Booking;
using PeerTutoringSystem.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Payment
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistory(string userId);
        Task<PayOSCreatePaymentLinkResponseDto> CreatePaymentLink(PayOSCreatePaymentLinkRequestDto request, string successUrl, string cancelUrl);
        Task<bool> ConfirmPayment(Guid bookingId);
        Task HandlePayOSWebhook(PayOSWebhookData webhookData);
        Task<BookingSessionDto> HandlePayOSReturn(long orderCode);
        Task<BookingSessionDto> HandlePayOSCancel(long orderCode);
    }
}