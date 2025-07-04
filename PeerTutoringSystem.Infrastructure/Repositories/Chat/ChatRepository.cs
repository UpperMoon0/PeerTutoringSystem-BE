using Firebase.Database;
using Firebase.Database.Query;
using PeerTutoringSystem.Domain.Entities.Chat;
using PeerTutoringSystem.Domain.Interfaces.Chat;
using PeerTutoringSystem.Domain.Interfaces.Authentication;

namespace PeerTutoringSystem.Infrastructure.Repositories.Chat
{
    public class ChatRepository : IChatRepository
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly IUserRepository _userRepository;

        public ChatRepository(FirebaseClient firebaseClient, IUserRepository userRepository)
        {
            _firebaseClient = firebaseClient;
            _userRepository = userRepository;
        }

        public async Task<ChatMessage> SendMessageAsync(ChatMessage message)
        {
            if (message.Id == Guid.Empty)
            {
                message.Id = Guid.NewGuid();
            }
            await _firebaseClient
                .Child("chats")
                .Child(message.Id.ToString())
                .PutAsync(message);
            return message;
        }

        public async Task<IEnumerable<Conversation>> GetConversationsAsync(string userId)
        {
            var allMessages = await _firebaseClient
                .Child("chats")
                .OnceAsync<ChatMessage>();

            var conversations = allMessages
                .Select(item => item.Object)
                .Where(msg => msg.SenderId == userId || msg.ReceiverId == userId)
                .GroupBy(msg =>
                {
                    var otherParticipantId = msg.SenderId == userId ? msg.ReceiverId : msg.SenderId;
                    var ids = new List<string> { userId, otherParticipantId };
                    ids.Sort();
                    return string.Join("-", ids);
                })
                .Select(group => new Conversation
                {
                    Id = group.Key,
                    ParticipantIds = group.Key.Split('-').ToList(),
                    LastMessage = group.OrderByDescending(msg => msg.Timestamp).First()
                })
                .ToList();

            var participantIds = conversations
                .Select(c => c.ParticipantIds.FirstOrDefault(id => id != userId))
                .Where(id => id != null && Guid.TryParse(id, out _))
                .Select(id => Guid.Parse(id))
                .Distinct()
                .ToList();

            if (participantIds.Any())
            {
                var participants = await _userRepository.GetUsersByIdsAsync(participantIds);
                var participantDict = participants.ToDictionary(p => p.UserID.ToString(), p => p);

                foreach (var conversation in conversations)
                {
                    var otherParticipantId = conversation.ParticipantIds.FirstOrDefault(id => id != userId);
                    if (otherParticipantId != null && participantDict.TryGetValue(otherParticipantId, out var participant))
                    {
                        conversation.Participant = participant;
                    }
                }
            }

            return conversations;
        }

        public async Task<Conversation> FindOrCreateConversationAsync(string userId, string participantId)
        {
            var ids = new List<string> { userId, participantId };
            ids.Sort();
            var conversationId = string.Join("-", ids);

            var conversation = (await _firebaseClient
                .Child("conversations")
                .Child(conversationId)
                .OnceSingleAsync<Conversation>());

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    Id = conversationId,
                    ParticipantIds = ids
                };

                await _firebaseClient
                    .Child("conversations")
                    .Child(conversationId)
                    .PutAsync(conversation);
            }

            var otherParticipantId = conversation.ParticipantIds.FirstOrDefault(id => id != userId);
            if (otherParticipantId != null)
            {
                if (Guid.TryParse(otherParticipantId, out Guid participantGuid))
                {
                    conversation.Participant = await _userRepository.GetByIdAsync(participantGuid);
                }
            }

            return conversation;
        }
    }
}