using Microsoft.AspNetCore.Http;
using PeerTutoringSystem.Application.DTOs.Authentication;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Authentication
{
    public interface IDocumentService
    {
        Task<DocumentResponseDto> UploadDocumentAsync(IFormFile file);
        Task<DocumentDto> GetDocumentAsync(string id);
    }
}