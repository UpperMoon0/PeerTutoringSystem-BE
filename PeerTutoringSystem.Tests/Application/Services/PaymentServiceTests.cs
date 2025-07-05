using Microsoft.Extensions.Configuration;
using Moq;
using PeerTutoringSystem.Application.Services.Payment;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeerTutoringSystem.Tests.Application.Services
{
  public class PaymentServiceTests
  {
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler; 
    private HttpClient _httpClient;
    private PaymentService _paymentService;

   [SetUp]
   public void Setup()
   {
       _mockConfiguration = new Mock<IConfiguration>();
       _mockPaymentRepository = new Mock<IPaymentRepository>();
       _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

       var inMemorySettings = new Dictionary<string, string> {
           {"SePay:BaseUrl", "http://test-sepay.com"},
       };
       IConfiguration configuration = new ConfigurationBuilder()
           .AddInMemoryCollection(inMemorySettings)
           .Build();

       _mockConfiguration.Setup(c => c["SePay:BaseUrl"]).Returns("http://test-sepay.com");

       _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
       {
           BaseAddress = new Uri("http://test-sepay.com")
       };

       _paymentService = new PaymentService(_httpClient, _mockConfiguration.Object, _mockPaymentRepository.Object);
   }

    private SePayWebhookData CreateWebhookData(long id, string transferType, decimal amount = 10000)
    {
      return new SePayWebhookData
      {
        Id = id,
        Gateway = "TestBank",
        TransactionDate = DateTime.UtcNow,
        AccountNumber = "ACC123",
        Content = "Test Transaction",
        TransferType = transferType,
        TransferAmount = amount,
        Accumulated = 50000,
        ReferenceCode = $"REF{id}"
      };
    }

    private PaymentEntity CreatePaymentEntity(string transactionId, PaymentStatus initialStatus)
    {
      return new PaymentEntity
      {
        Id = Guid.NewGuid(),
        TransactionId = transactionId,
        Amount = 10000,
        Status = initialStatus,
        CreatedAt = DateTime.UtcNow.AddMinutes(-5)
      };
    }

    [Test]
    public async Task ProcessPaymentWebhook_NewSuccessfulInTransaction_UpdatesStatusToSuccess()
    {
      // Arrange
      var webhookData = CreateWebhookData(1, "in");
      var paymentEntity = CreatePaymentEntity(webhookData.Id.ToString(), PaymentStatus.Pending);
      _mockPaymentRepository.Setup(r => r.GetPaymentByTransactionIdAsync(webhookData.Id.ToString()))
          .ReturnsAsync(paymentEntity);
      _mockPaymentRepository.Setup(r => r.UpdatePaymentAsync(It.IsAny<PaymentEntity>()))
          .ReturnsAsync((PaymentEntity pe) => pe); // Returns the passed entity, wrapped in a Task

      // Act
      await _paymentService.ProcessPaymentWebhook(webhookData);

      // Assert
      _mockPaymentRepository.Verify(r => r.GetPaymentByTransactionIdAsync(webhookData.Id.ToString()), Times.Once);
      NUnit.Framework.Assert.AreEqual(PaymentStatus.Success, paymentEntity.Status);
      Assert.NotNull(paymentEntity.UpdatedAt);
      _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(
          It.Is<PaymentEntity>(p => p.TransactionId == webhookData.Id.ToString() && p.Status == PaymentStatus.Success)),
          Times.Once);
    }

    [Test]
    public async Task ProcessPaymentWebhook_NewOutTransaction_UpdatesStatusToFailed()
    {
      // Arrange
      var webhookData = CreateWebhookData(2, "out");
      var paymentEntity = CreatePaymentEntity(webhookData.Id.ToString(), PaymentStatus.Pending);
      _mockPaymentRepository.Setup(r => r.GetPaymentByTransactionIdAsync(webhookData.Id.ToString()))
          .ReturnsAsync(paymentEntity);

      // Act
      await _paymentService.ProcessPaymentWebhook(webhookData);

      // Assert
      NUnit.Framework.Assert.AreEqual(PaymentStatus.Failed, paymentEntity.Status);
      _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(
          It.Is<PaymentEntity>(p => p.Status == PaymentStatus.Failed)),
          Times.Once);
    }

    [Test]
    public async Task ProcessPaymentWebhook_DuplicateSuccessfulInTransaction_StatusRemainsSuccess_NoUpdateCall()
    {
      // Arrange
      var webhookData = CreateWebhookData(3, "in");
      var paymentEntity = CreatePaymentEntity(webhookData.Id.ToString(), PaymentStatus.Success); // Already success
      _mockPaymentRepository.Setup(r => r.GetPaymentByTransactionIdAsync(webhookData.Id.ToString()))
         .ReturnsAsync(paymentEntity);

      var originalUpdatedAt = paymentEntity.UpdatedAt;

      // Act
      await _paymentService.ProcessPaymentWebhook(webhookData);

      // Assert
      NUnit.Framework.Assert.AreEqual(PaymentStatus.Success, paymentEntity.Status);
      NUnit.Framework.Assert.AreEqual(originalUpdatedAt, paymentEntity.UpdatedAt); // Should not change if no update
      _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(It.IsAny<PaymentEntity>()), Times.Never);
    }

    [Test]
    public async Task ProcessPaymentWebhook_InTransactionForPreviouslyFailed_UpdatesToSuccess()
    {
      // Arrange
      var webhookData = CreateWebhookData(4, "in");
      var paymentEntity = CreatePaymentEntity(webhookData.Id.ToString(), PaymentStatus.Failed); // Was failed
      _mockPaymentRepository.Setup(r => r.GetPaymentByTransactionIdAsync(webhookData.Id.ToString()))
          .ReturnsAsync(paymentEntity);

      // Act
      await _paymentService.ProcessPaymentWebhook(webhookData);

      // Assert
      NUnit.Framework.Assert.AreEqual(PaymentStatus.Success, paymentEntity.Status);
      _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(
          It.Is<PaymentEntity>(p => p.Status == PaymentStatus.Success)),
          Times.Once);
    }

    [Test]
    public async Task ProcessPaymentWebhook_TransactionIdNotFound_NoAction()
    {
      // Arrange
      var webhookData = CreateWebhookData(5, "in");
      _mockPaymentRepository.Setup(r => r.GetPaymentByTransactionIdAsync(webhookData.Id.ToString()))
          .ReturnsAsync((PaymentEntity)null);

      // Act
      await _paymentService.ProcessPaymentWebhook(webhookData);

      // Assert
      _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(It.IsAny<PaymentEntity>()), Times.Never);
    }

    [Test]
    public async Task ProcessPaymentWebhook_RepositoryGetThrowsException_ExceptionPropagates()
    {
      // Arrange
      var webhookData = CreateWebhookData(6, "in");
      _mockPaymentRepository.Setup(r => r.GetPaymentByTransactionIdAsync(webhookData.Id.ToString()))
          .ThrowsAsync(new Exception("Database connection error"));

      // Act & Assert
      Assert.ThrowsAsync<Exception>(async () => await _paymentService.ProcessPaymentWebhook(webhookData));
      _mockPaymentRepository.Verify(r => r.UpdatePaymentAsync(It.IsAny<PaymentEntity>()), Times.Never);
    }

    [Test]
    public async Task ProcessPaymentWebhook_RepositoryUpdateThrowsException_ExceptionPropagates()
    {
      // Arrange
      var webhookData = CreateWebhookData(7, "in");
      var paymentEntity = CreatePaymentEntity(webhookData.Id.ToString(), PaymentStatus.Pending);
      _mockPaymentRepository.Setup(r => r.GetPaymentByTransactionIdAsync(webhookData.Id.ToString()))
          .ReturnsAsync(paymentEntity);
      _mockPaymentRepository.Setup(r => r.UpdatePaymentAsync(It.IsAny<PaymentEntity>()))
          .ThrowsAsync(new Exception("Error during update"));

      // Act & Assert
      Assert.ThrowsAsync<Exception>(async () => await _paymentService.ProcessPaymentWebhook(webhookData));
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }
  } 
} 