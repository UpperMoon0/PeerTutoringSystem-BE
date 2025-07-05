using Moq;
using NUnit.Framework;
using PeerTutoringSystem.Application.Services.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using PeerTutoringSystem.Domain.Interfaces.Chat;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Tests.Application.Services
{
    [TestFixture]
    public class ChatServiceTests
    {
        private Mock<IChatRepository> _mockChatRepository;
        private ChatService _chatService;

        [SetUp]
        public void Setup()
        {
            _mockChatRepository = new Mock<IChatRepository>();
            _chatService = new ChatService(_mockChatRepository.Object);
        }

        [Test]
        public async Task SendMessageAsync_ValidMessage_ReturnsSentMessage()
        {
            // Arrange
            var chatMessage = new ChatMessage { SenderId = "user1", ReceiverId = "user2", Message = "Hello" };
            _mockChatRepository.Setup(r => r.SendMessageAsync(It.IsAny<ChatMessage>()))
                .ReturnsAsync(chatMessage);

            // Act
            var result = await _chatService.SendMessageAsync(chatMessage);

            // Assert
            Assert.That(result, Is.EqualTo(chatMessage));
            _mockChatRepository.Verify(r => r.SendMessageAsync(chatMessage), Times.Once);
        }
    }
}