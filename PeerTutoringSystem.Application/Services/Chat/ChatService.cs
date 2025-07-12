using PeerTutoringSystem.Application.DTOs.Chat;
using PeerTutoringSystem.Application.Interfaces.Chat;
using PeerTutoringSystem.Domain.Entities.Chat;
using PeerTutoringSystem.Domain.Interfaces.Chat;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PeerTutoringSystem.Application.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ISubject<ChatMessage> _messageSubject = new ReplaySubject<ChatMessage>(1);

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ChatMessage> SendMessageAsync(ChatMessage message)
        {
            var sentMessage = await _chatRepository.SendMessageAsync(message);
            _messageSubject.OnNext(sentMessage);
            return sentMessage;
        }

        public IObservable<ChatMessage> ObserveMessages()
        {
            return _messageSubject.AsObservable();
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(string userId)
        {
            var conversations = await _chatRepository.GetConversationsAsync(userId);
            return conversations
                .Where(c => c.Participant != null)
                .Select(c => new ConversationDto
            {
                Id = c.Id,
                Participant = new ConversationParticipantDto
                {
                    Id = c.Participant.UserID.ToString(),
                    FullName = c.Participant.FullName,
                    AvatarUrl = c.Participant.AvatarUrl
                },
                LastMessage = c.LastMessage
            });
        }

        public async Task<ConversationDto> FindOrCreateConversationAsync(string userId, string participantId)
        {
            var conversation = await _chatRepository.FindOrCreateConversationAsync(userId, participantId);
            return new ConversationDto
            {
                Id = conversation.Id,
                Participant = new ConversationParticipantDto
                {
                    Id = conversation.Participant.UserID.ToString(),
                    FullName = conversation.Participant.FullName,
                    AvatarUrl = conversation.Participant.AvatarUrl
                },
                LastMessage = conversation.LastMessage
            };
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(string conversationId)
        {
            return await _chatRepository.GetMessagesAsync(conversationId);
        }
    }
}