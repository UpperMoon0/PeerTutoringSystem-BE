using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using PeerTutoringSystem.Api.Controllers.Payment;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeerTutoringSystem.Tests.Api.Controllers
{
  public class WebhookControllerTests
  {
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly WebhookController _controller;

    public WebhookControllerTests()
    {
      _mockConfig = new Mock<IConfiguration>();
      _mockPaymentService = new Mock<IPaymentService>();
      _controller = new WebhookController(_mockConfig.Object, _mockPaymentService.Object);
    }

    private SePayWebhookData CreateValidWebhookData()
    {
      return new SePayWebhookData
      {
        Id = 12345,
        Gateway = "TestBank",
        TransactionDate = System.DateTime.UtcNow,
        AccountNumber = "1234567890",
        Content = "Test payment",
        TransferType = "in",
        TransferAmount = 100000,
        Accumulated = 500000,
        ReferenceCode = "REF123",
        Description = "Test Description",
        PaymentStatus = PaymentStatus.Pending
      };
    }

    [Test]
    public async Task HandleSePayWebhook_ValidData_ReturnsOkResultWithSuccessTrue()
    {
      // Arrange
      var webhookData = CreateValidWebhookData();
      _mockPaymentService.Setup(s => s.ProcessPaymentWebhook(It.IsAny<SePayWebhookData>()))
          .Returns(Task.CompletedTask);

      // Act
      var result = await _controller.HandleSePayWebhook(webhookData);

      // Assert
      Assert.That(result, Is.InstanceOf<OkObjectResult>());
      var okResult = (OkObjectResult)result;
      var returnValue = okResult.Value;
      Assert.NotNull(returnValue);
      var successProperty = returnValue.GetType().GetProperty("success");
      Assert.NotNull(successProperty);
      Assert.That(successProperty.GetValue(returnValue, null), Is.EqualTo(true));
      _mockPaymentService.Verify(s => s.ProcessPaymentWebhook(webhookData), Times.Once);
    }

    [Test]
    public async Task HandleSePayWebhook_NullData_ReturnsBadRequest()
    {
      // Arrange
      SePayWebhookData? webhookData = null;

      // Act
      var result = await _controller.HandleSePayWebhook(webhookData!);

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
    public async Task HandleSePayWebhook_PaymentServiceThrowsException_ReturnsOkResultWithSuccessFalse()
    {
      // Arrange
      var webhookData = CreateValidWebhookData();
      _mockPaymentService.Setup(s => s.ProcessPaymentWebhook(It.IsAny<SePayWebhookData>()))
          .ThrowsAsync(new System.Exception("Service error"));

      // Act
      var result = await _controller.HandleSePayWebhook(webhookData);

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