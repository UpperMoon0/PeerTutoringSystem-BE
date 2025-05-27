using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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

    [HttpPost("sepay")]
    public async Task<IActionResult> HandleSePayWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var webhookBody = await reader.ReadToEndAsync();

            // Verify webhook signature (important for security)
            if (!VerifyWebhookSignature(webhookBody, Request.Headers))
            {
                return Unauthorized();
            }

            var webhookData = JsonSerializer.Deserialize<SePayWebhookData>(webhookBody);

            // Process the payment update
            await _paymentService.ProcessPaymentWebhook(webhookData);

            return Ok(); // Must return 200 OK for SePay
        }
        catch (Exception ex)
        {
            // Log error but still return OK to prevent webhook retries
            // Handle internally
            return Ok();
        }
    }

    private bool VerifyWebhookSignature(string body, IHeaderDictionary headers)
    {
        // Implement signature verification based on SePay docs
        var signature = headers["X-SePay-Signature"].FirstOrDefault();
        var secret = _config["SePay:WebhookSecret"];

        // Generate expected signature using HMAC-SHA256
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expectedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

        return signature == expectedSignature;
    }
}
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

    [HttpPost("sepay")]
    public async Task<IActionResult> HandleSePayWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var webhookBody = await reader.ReadToEndAsync();

            // Verify webhook signature (important for security)
            if (!VerifyWebhookSignature(webhookBody, Request.Headers))
            {
                return Unauthorized();
            }

            var webhookData = JsonSerializer.Deserialize<SePayWebhookData>(webhookBody);

            // Process the payment update
            await _paymentService.ProcessPaymentWebhook(webhookData);

            return Ok(); // Must return 200 OK for SePay
        }
        catch (Exception ex)
        {
            // Log error but still return OK to prevent webhook retries
            // Handle internally
            return Ok();
        }
    }

    private bool VerifyWebhookSignature(string body, IHeaderDictionary headers)
    {
        // Implement signature verification based on SePay docs
        var signature = headers["X-SePay-Signature"].FirstOrDefault();
        var secret = _config["SePay:WebhookSecret"];

        // Generate expected signature using HMAC-SHA256
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expectedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

        return signature == expectedSignature;
    }
}