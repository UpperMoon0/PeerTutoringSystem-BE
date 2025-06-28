using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required.", timestamp = DateTime.UtcNow });
                }

                var response = await _paymentService.CreatePayment(request);
                
                if (!response.Success)
                {
                    return BadRequest(new { error = response.Message, timestamp = DateTime.UtcNow });
                }

                return Ok(new 
                { 
                    data = response,
                    message = "PaymentEntity created successfully.", 
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, new { error = "An error occurred while creating payment.", timestamp = DateTime.UtcNow });
            }
        }

        [HttpGet("{paymentId}")]
        [Authorize(Roles = "Student,Tutor")]
        public async Task<IActionResult> GetPaymentStatus(string paymentId)
        {
            try
            {
                var status = await _paymentService.GetPaymentStatus(paymentId);
                
                return Ok(new 
                { 
                    data = new { status = status.ToString() },
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for payment ID {PaymentId}", paymentId);
                return StatusCode(500, new { error = "An error occurred while getting payment status.", timestamp = DateTime.UtcNow });
            }
        }
    }
}