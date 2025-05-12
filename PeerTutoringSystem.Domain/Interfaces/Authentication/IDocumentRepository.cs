using PeerTutoringSystem.Domain.Entities.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Authentication
{
    public interface IDocumentRepository
    {
        Task AddAsync(Document document);
        Task<IEnumerable<Document>> GetByVerificationIdAsync(Guid verificationId);
        Task<Document> GetByIdAsync(Guid documentId);
    }
}