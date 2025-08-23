using PeerTutoringSystem.Application.DTOs.Payment;
using System.Collections.Generic;

namespace PeerTutoringSystem.Application.Interfaces.Payment
{
    public interface IWithdrawService
    {
        Task<IEnumerable<WithdrawRequestDto>> GetMyWithdrawRequests();
        Task<IEnumerable<WithdrawRequestDto>> GetWithdrawRequests();
        Task<WithdrawRequestDto> CreateWithdrawRequest(CreateWithdrawRequestDto createWithdrawRequestDto);
        Task<WithdrawRequestDto> CancelWithdrawRequest(Guid id);
        Task<WithdrawRequestDto> ApproveWithdrawRequest(Guid id);
        Task<WithdrawRequestDto> RejectWithdrawRequest(Guid id);
    }
}