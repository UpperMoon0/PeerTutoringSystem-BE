using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;
using PeerTutoringSystem.Api.Controllers.Chat;
using PeerTutoringSystem.Api.Hubs;
using PeerTutoringSystem.Application.DTOs.Chat;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Tests.Api.Controllers
{
    [TestFixture]
    public class ChatControllerTests
    {
        private Mock<IChatService> _mockChatService;
        private Mock<IHubContext<ChatHub>> _mockHubContext;
        private ChatController _controller;

        [SetUp]
        public void Setup()
        {
            _mockChatService = new Mock<IChatService>();
            _mockHubContext = new Mock<IHubContext<ChatHub>>();
            _controller = new ChatController(_mockChatService.Object, _mockHubContext.Object);
        }

        [Test]
        public async Task SendMessage_ValidMessage_ReturnsOkResultWithSentMessage()
        {
            // Arrange
            var messageDto = new SendMessageDto { SenderId = "user1", ReceiverId = "user2", Message = "Hello" };
            var sentMessage = new ChatMessage { SenderId = "user1", ReceiverId = "user2", Message = "Hello", Timestamp = System.DateTime.UtcNow };

            _mockChatService
                .Setup(s => s.SendMessageAsync(It.IsAny<ChatMessage>()))
                .ReturnsAsync(sentMessage);

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

            // Act
            var result = await _controller.SendMessage(messageDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(sentMessage));
        }
    }
}