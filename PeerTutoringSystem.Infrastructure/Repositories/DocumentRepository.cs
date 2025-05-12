using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities;
using PeerTutoringSystem.Domain.Interfaces;
using PeerTutoringSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Infrastructure.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;

        public DocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Document document)
        {
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Document>> GetByVerificationIdAsync(Guid verificationId)
        {
            return await _context.Documents
                .Where(d => d.VerificationID == verificationId)
                .ToListAsync();
        }
        public async Task<Document> GetByIdAsync(Guid documentId)
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d => d.DocumentID == documentId);
        }
    }
}