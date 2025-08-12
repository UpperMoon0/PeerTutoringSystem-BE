using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PeerTutoringSystem.Api.Controllers.Payment;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeerTutoringSystem.Tests.Api.Controllers
{
  public class WebhookControllerTests
  {
    private Mock<IPayOSWebhookService> _mockPayOSWebhookService;
    private Mock<ILogger<WebhookController>> _mockLogger;
    private WebhookController _controller;

    [SetUp]
    public void Setup()
    {
        _mockPayOSWebhookService = new Mock<IPayOSWebhookService>();
        _mockLogger = new Mock<ILogger<WebhookController>>();
        _controller = new WebhookController(_mockPayOSWebhookService.Object, _mockLogger.Object);
    }

    private PayOSWebhookData CreateValidWebhookData()
    {
        return new PayOSWebhookData
        {
            Code = "00",
            Description = "Success",
            Data = new PayOSWebhookInnerData
            {
                OrderCode = 12345,
                Amount = 100000,
                Description = "Test payment",
                AccountNumber = "1234567890",
                Reference = "REF123",
                TransactionDateTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                PaymentLinkId = "link123",
                Code = "00",
                Desc = "Success"
            },
            Signature = "test-signature"
        };
    }

    [Test]
    public async Task HandlePayOSWebhook_ValidData_ReturnsOkResultWithSuccessTrue()
    {
      // Arrange
      var webhookData = CreateValidWebhookData();
      _mockPayOSWebhookService.Setup(s => s.ProcessPayOSWebhook(It.IsAny<PayOSWebhookData>()))
          .Returns(Task.CompletedTask);

      // Act
      var result = await _controller.HandlePayOSWebhook(webhookData);

      // Assert
      Assert.That(result, Is.InstanceOf<OkObjectResult>());
      var okResult = (OkObjectResult)result;
      var returnValue = okResult.Value;
      Assert.NotNull(returnValue);
      var successProperty = returnValue.GetType().GetProperty("success");
      Assert.NotNull(successProperty);
      Assert.That(successProperty.GetValue(returnValue, null), Is.EqualTo(true));
      _mockPayOSWebhookService.Verify(s => s.ProcessPayOSWebhook(webhookData), Times.Once);
    }

    [Test]
    public async Task HandlePayOSWebhook_NullData_ReturnsBadRequest()
    {
      // Arrange
      PayOSWebhookData? webhookData = null;

      // Act
      var result = await _controller.HandlePayOSWebhook(webhookData!);

      // Assert
      Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
      var badRequestResult = (BadRequestObjectResult)result;
      var returnValue = badRequestResult.Value;
      Assert.NotNull(returnValue);
      var successProperty = returnValue.GetType().GetProperty("success");
      Assert.NotNull(successProperty);
      Assert.That(successProperty.GetValue(returnValue, null), Is.EqualTo(false));
      var messageProperty = returnValue.GetType().GetProperty("message");
      Assert.NotNull(messageProperty);
      Assert.That(messageProperty.GetValue(returnValue, null), Is.EqualTo("Invalid payload."));
    }


    [Test]
    public async Task HandlePayOSWebhook_PaymentServiceThrowsException_ReturnsOkResultWithSuccessFalse()
    {
      // Arrange
      var webhookData = CreateValidWebhookData();
      _mockPayOSWebhookService.Setup(s => s.ProcessPayOSWebhook(It.IsAny<PayOSWebhookData>()))
          .ThrowsAsync(new System.Exception("Service error"));

      // Act
      var result = await _controller.HandlePayOSWebhook(webhookData);

      // Assert
      Assert.That(result, Is.InstanceOf<ObjectResult>());
      var objectResult = (ObjectResult)result;
      Assert.That(objectResult.StatusCode, Is.EqualTo(500));
      var returnValue = objectResult.Value;
      Assert.NotNull(returnValue);
      var successProperty = returnValue.GetType().GetProperty("success");
      Assert.NotNull(successProperty);
      Assert.That(successProperty.GetValue(returnValue, null), Is.EqualTo(false));
      var messageProperty = returnValue.GetType().GetProperty("message");
      Assert.NotNull(messageProperty);
      Assert.That(messageProperty.GetValue(returnValue, null), Is.EqualTo("An unexpected error occurred."));
    }
  }
}