using Moq;
using NUnit.Framework;
using PeerTutoringSystem.Application.Services.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using Firebase.Database;
using System.Threading.Tasks;
using Firebase.Database.Query;
using System.Collections.Generic;

namespace PeerTutoringSystem.Tests.Application.Services
{
  [TestFixture]
  public class ChatServiceTests
  {
    private Mock<FirebaseClient> _mockFirebaseClient;
    private ChatService _chatService;

    [SetUp]
    public void Setup()
    {
      _mockFirebaseClient = new Mock<FirebaseClient>("https://test.firebaseio.com");
      _chatService = new ChatService(_mockFirebaseClient.Object);
    }

    [Test]
    public async Task SendMessageAsync_ValidMessage_DoesNotThrow()
    {
      // Arrange
      var chatMessage = new ChatMessage { SenderId = "user1", ReceiverId = "user2", Message = "Hello" };
      var mockChild = new Mock<ChildQuery>();

      _mockFirebaseClient.Setup(c => c.Child(It.IsAny<string>())).Returns(mockChild.Object);
      mockChild.Setup(c => c.PostAsync(It.IsAny<ChatMessage>(), It.IsAny<bool>())).ReturnsAsync(default(FirebaseObject<ChatMessage>));

      // Act & Assert
      Assert.DoesNotThrowAsync(async () => await _chatService.SendMessageAsync(chatMessage));
    }
  }
}