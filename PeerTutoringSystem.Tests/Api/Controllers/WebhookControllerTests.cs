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
        ReferenceCode = "REF123"
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
      Assert.AreEqual(true, successProperty.GetValue(returnValue, null));
      _mockPaymentService.Verify(s => s.ProcessPaymentWebhook(webhookData), Times.Once);
    }

    [Test]
    public async Task HandleSePayWebhook_NullData_ReturnsBadRequest()
    {
      // Arrange
      SePayWebhookData webhookData = null;

      // Act
      var result = await _controller.HandleSePayWebhook(webhookData);

      // Assert
      Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
      var badRequestResult = (BadRequestObjectResult)result;
      var returnValue = badRequestResult.Value;
      Assert.NotNull(returnValue);
      var successProperty = returnValue.GetType().GetProperty("success");
      Assert.NotNull(successProperty);
      Assert.AreEqual(false, successProperty.GetValue(returnValue, null));
      var messageProperty = returnValue.GetType().GetProperty("message");
      Assert.NotNull(messageProperty);
      Assert.AreEqual("Invalid payload.", messageProperty.GetValue(returnValue, null));
    }

    [Test]
    public async Task HandleSePayWebhook_InvalidModelState_ReturnsBadRequest()
    {
      // Arrange
      var webhookData = CreateValidWebhookData();
      _controller.ModelState.AddModelError("Error", "Sample model error");

      // Act
      var result = await _controller.HandleSePayWebhook(webhookData);

      // Assert
      Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
      var badRequestResult = (BadRequestObjectResult)result;
      var returnValue = badRequestResult.Value;
      Assert.NotNull(returnValue);
      var successProperty = returnValue.GetType().GetProperty("success");
      Assert.NotNull(successProperty);
      Assert.AreEqual(false, successProperty.GetValue(returnValue, null));
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
      Assert.That(result, Is.InstanceOf<OkObjectResult>()); // Controller catches exception and returns Ok
      var okResult = (OkObjectResult)result;
      var returnValue = okResult.Value;
      Assert.NotNull(returnValue);
      var successProperty = returnValue.GetType().GetProperty("success");
      Assert.NotNull(successProperty);
      Assert.AreEqual(false, successProperty.GetValue(returnValue, null));
      var messageProperty = returnValue.GetType().GetProperty("message");
      Assert.NotNull(messageProperty);
      Assert.AreEqual("An error occurred while processing the webhook.", messageProperty.GetValue(returnValue, null));
    }
  }
}