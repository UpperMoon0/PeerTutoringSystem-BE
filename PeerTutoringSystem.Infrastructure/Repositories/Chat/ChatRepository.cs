using Firebase.Database;
using Firebase.Database.Query;
using PeerTutoringSystem.Domain.Entities.Chat;
using PeerTutoringSystem.Domain.Interfaces.Chat;

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
            var explicitConversations = await _firebaseClient
                .Child("conversations")
                .OnceAsync<Conversation>();

            var userConversations = explicitConversations
                .Where(c => c.Object.ParticipantIds.Contains(userId))
                .Select(c => c.Object)
                .ToDictionary(c => c.Id, c => c);

            var allMessages = await _firebaseClient
                .Child("chats")
                .OnceAsync<ChatMessage>();

            var messageBasedConversations = allMessages
                .Select(item => item.Object)
                .Where(msg => msg.SenderId == userId || msg.ReceiverId == userId)
                .GroupBy(msg =>
                {
                    var otherParticipantId = msg.SenderId == userId ? msg.ReceiverId : msg.SenderId;
                    var ids = new List<string> { userId, otherParticipantId };
                    ids.Sort();
                    return string.Join("-", ids);
                })
                .ToList();

            foreach (var group in messageBasedConversations)
            {
                if (!userConversations.ContainsKey(group.Key))
                {
                    userConversations[group.Key] = new Conversation
                    {
                        Id = group.Key,
                        ParticipantIds = new List<string> { group.Key.Substring(0, 36), group.Key.Substring(37) },
                    };
                }
                userConversations[group.Key].LastMessage = group.OrderByDescending(msg => msg.Timestamp).First();
            }

            var finalConversations = userConversations.Values.ToList();

            var participantIds = finalConversations
                .Select(c => c.ParticipantIds.FirstOrDefault(id => id != userId))
                .Where(id => id != null && Guid.TryParse(id, out _))
                .Select(id => Guid.Parse(id))
                .Distinct()
                .ToList();

            if (participantIds.Any())
            {
                var participants = await _userRepository.GetUsersByIdsAsync(participantIds);
                var participantDict = participants.ToDictionary(p => p.UserID.ToString(), p => p);

                foreach (var conversation in finalConversations)
                {
                    var otherParticipantId = conversation.ParticipantIds.FirstOrDefault(id => id != userId);
                    if (otherParticipantId != null && participantDict.TryGetValue(otherParticipantId, out var participant))
                    {
                        conversation.Participant = participant;
                    }
                }
            }

            return finalConversations;
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

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(string conversationId)
        {
            // A conversationId is composed of two GUIDs, sorted and joined by a hyphen.
            // Each GUID string is 36 characters long. Total length is 36 + 1 + 36 = 73.
            if (conversationId.Length != 73)
            {
                // Invalid conversationId format
                return Enumerable.Empty<ChatMessage>();
            }

            var id1 = conversationId.Substring(0, 36);
            var id2 = conversationId.Substring(37);
            var participantIds = new List<string> { id1, id2 };

            var allMessages = await _firebaseClient
                .Child("chats")
                .OnceAsync<ChatMessage>();

            return allMessages
                .Select(item => item.Object)
                .Where(msg =>
                    (participantIds.Contains(msg.SenderId) && participantIds.Contains(msg.ReceiverId)))
                .OrderBy(msg => msg.Timestamp)
                .ToList();
        }
    }
}