using PeerTutoringSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces
{
    public interface IDocumentRepository
    {
        Task AddAsync(Document document);
        Task<IEnumerable<Document>> GetByVerificationIdAsync(Guid verificationId);
        Task<Document> GetByIdAsync(Guid documentId);
    }
}