using PeerTutoringSystem.Application.DTOs.Payment;

namespace PeerTutoringSystem.Application.Interfaces.Payment
{
    public interface IWithdrawService
    {
        Task<WithdrawRequestDto> CreateWithdrawRequest(CreateWithdrawRequestDto createWithdrawRequestDto);
        Task<WithdrawRequestDto> CancelWithdrawRequest(Guid id);
        Task<WithdrawRequestDto> ApproveWithdrawRequest(Guid id);
        Task<WithdrawRequestDto> RejectWithdrawRequest(Guid id);
    }
}