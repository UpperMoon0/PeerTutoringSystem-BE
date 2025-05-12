using Microsoft.AspNetCore.Http;
using PeerTutoringSystem.Application.DTOs;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentResponseDto> UploadDocumentAsync(IFormFile file);
    }
}