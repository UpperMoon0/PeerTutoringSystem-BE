using Microsoft.Extensions.Configuration;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Application.DTOs.Payment;
using System.Web;

namespace PeerTutoringSystem.Application.Services.Payment
{
   public class PaymentService : IPaymentService
   {
       private readonly IConfiguration _config;
       private readonly IPaymentRepository _paymentRepository;
       private readonly IBookingSessionRepository _bookingRepository;
       private readonly IUserBioRepository _userBioRepository;
       private readonly ISessionRepository _sessionRepository;

       public PaymentService(
           IConfiguration config,
           IPaymentRepository paymentRepository,
           IBookingSessionRepository bookingRepository,
           IUserBioRepository userBioRepository,
           ISessionRepository sessionRepository)
       // IHubContext<PaymentHub> paymentHubContext)
       {
           _config = config ?? throw new ArgumentNullException(nameof(config));
           _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
           _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
           _userBioRepository = userBioRepository ?? throw new ArgumentNullException(nameof(userBioRepository));
           _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
           // _paymentHubContext = paymentHubContext ?? throw new ArgumentNullException(nameof(paymentHubContext));
       }

       public async Task<PaymentResponseDto> CreatePayment(CreatePaymentRequestDto request)
       {
           try
           {
               var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
               if (booking == null)
               {
                   return new PaymentResponseDto { Success = false, Message = "Booking not found." };
               }
 
               var tutorBio = await _userBioRepository.GetByUserIdAsync(booking.TutorId);
               if (tutorBio == null)
               {
                   return new PaymentResponseDto { Success = false, Message = "Tutor profile not found." };
               }
 
               var session = await _sessionRepository.GetByBookingIdAsync(request.BookingId);
               if (session == null)
               {
                   return new PaymentResponseDto { Success = false, Message = "Session not found." };
               }
               var durationHours = (session.EndTime - session.StartTime).TotalHours;
               var amount = (decimal)durationHours * tutorBio.HourlyRate;
               var description = $"Payment for booking {booking.BookingId}";
               
               var accountName = _config["ACCOUNT_NAME"];
               var accountNumber = _config["ACCOUNT_NUMBER"];
               var bankBin = _config["BANK_BIN"];
               var template = "compact2";

               // Generate QR Code
               var encodedDescription = HttpUtility.UrlEncode(description);
               var qrCodePayload = $"https://img.vietqr.io/image/{bankBin}-{accountNumber}-{template}.png?amount={amount}&addInfo={encodedDescription}&accountName={accountName}";

               var payment = new PaymentEntity
               {
                   BookingId = request.BookingId,
                   Amount = amount,
                   Description = description,
                   Status = PaymentStatus.Pending,
                   CreatedAt = DateTime.UtcNow
               };

               await _paymentRepository.CreatePaymentAsync(payment);

               return new PaymentResponseDto
               {
                   Success = true,
                   PaymentId = payment.Id.ToString(),
                   QrCode = qrCodePayload,
                   Message = "Payment created successfully"
               };
           }
           catch (Exception ex)
           {
               // Handle exceptions
               return new PaymentResponseDto
               {
                   Success = false,
                   Message = $"Failed to create payment: {ex.Message}"
               };
           }
       }

        public async Task<PaymentStatus> GetPaymentStatus(string paymentId)
        {
            try
            {
                if (Guid.TryParse(paymentId, out var id))
                {
                    var payment = await _paymentRepository.GetPaymentByIdAsync(id);
                    return payment?.Status ?? PaymentStatus.Failed;
                }

                return PaymentStatus.Failed;
            }
            catch
            {
                return PaymentStatus.Failed;
            }
        }


        private async Task NotifyPaymentStatusChange(Guid paymentId, PaymentStatus status)
        {
            // If using SignalR, notify clients about payment status change
            // await _paymentHubContext.Clients.All.SendAsync("PaymentStatusChanged", paymentId, status);

            // For now, just return a completed task if SignalR is not implemented
            await Task.CompletedTask;
        }

        public async Task<bool> ConfirmPayment(Guid bookingId)
        {
            var payment = await _paymentRepository.GetPaymentByBookingIdAsync(bookingId);
            if (payment == null)
            {
                return false;
            }

            payment.Status = PaymentStatus.Paid;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdatePaymentAsync(payment);

            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking != null)
            {
                booking.PaymentStatus = Domain.Entities.Booking.PaymentStatus.Paid;
                await _bookingRepository.UpdateAsync(booking);
            }

            return true;
        }
        public async Task<AdminFinanceDto> GetAdminFinanceDetails()
        {
            var payments = await _paymentRepository.GetAllAsync();
            var successfulPayments = payments.Where(p => p.Status == PaymentStatus.Paid).ToList();

            var totalRevenue = successfulPayments.Sum(p => (double)p.Amount);
            var totalTransactions = successfulPayments.Count;
            var averageTransactionValue = totalTransactions > 0 ? totalRevenue / totalTransactions : 0;

            var monthlyRevenue = successfulPayments
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Revenue = g.Sum(p => (double)p.Amount)
                })
                .OrderBy(m => m.Month)
                .ToList();

            var recentTransactions = successfulPayments
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new RecentTransactionDto
                {
                    TransactionId = p.TransactionId,
                    TransactionDate = p.CreatedAt,
                    Amount = (double)p.Amount,
                    Status = p.Status.ToString(),
                    BookingId = p.BookingId
                })
                .ToList();

            return new AdminFinanceDto
            {
                TotalRevenue = totalRevenue,
                AverageTransactionValue = averageTransactionValue,
                TotalTransactions = totalTransactions,
                MonthlyRevenue = monthlyRevenue,
                RecentTransactions = recentTransactions
            };
        }

        public async Task ProcessPaymentWebhook(SePayWebhookData webhookData)
        {
            if (webhookData?.Content == null)
            {
                return;
            }

            var contentParts = webhookData.Content.Split(',');
            var bookingIdPart = contentParts.FirstOrDefault(p => p.StartsWith("bookingId:"));
            if (bookingIdPart != null)
            {
                var bookingIdStr = bookingIdPart.Substring("bookingId:".Length);
                if (Guid.TryParse(bookingIdStr, out var bookingId))
                {
                    await ConfirmPayment(bookingId);
                }
            }
        }
    }
}