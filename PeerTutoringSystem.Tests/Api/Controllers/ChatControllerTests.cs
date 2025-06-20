using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using PeerTutoringSystem.Api.Controllers.Chat;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Tests.Api.Controllers
{
  [TestFixture]
  public class ChatControllerTests
  {
    private Mock<IChatService> _mockChatService;
    private ChatController _controller;

    [SetUp]
    public void Setup()
    {
      _mockChatService = new Mock<IChatService>();
      _controller = new ChatController(_mockChatService.Object);
    }

    [Test]
    public async Task SendMessage_ValidMessage_ReturnsOkResult()
    {
      // Arrange
      var chatMessage = new ChatMessage { SenderId = "user1", ReceiverId = "user2", Message = "Hello" };

      _mockChatService
          .Setup(s => s.SendMessageAsync(It.IsAny<ChatMessage>()))
          .Returns(Task.CompletedTask);

      // Act
      var result = await _controller.SendMessage(chatMessage);

      // Assert
      Assert.IsInstanceOf<OkResult>(result);
    }
  }
}