using PeerTutoringSystem.Application.DTOs.Payment;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Interfaces.Payment
{
    public interface IVietQrService
    {
        Task<VietQrResponseDto> GenerateQrCode(VietQrRequestDto request);
    }
}