using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPaymentService _paymentService;

        public WebhookController(IConfiguration config, IPaymentService paymentService)
        {
            _config = config;
            _paymentService = paymentService;
        }

    }
}