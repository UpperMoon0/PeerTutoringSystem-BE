using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Domain.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace PeerTutoringSystem.Application.Services.Payment
{
    public class WithdrawService : IWithdrawService
    {
        private readonly IWithdrawRequestRepository _withdrawRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WithdrawService(IWithdrawRequestRepository withdrawRequestRepository, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _withdrawRequestRepository = withdrawRequestRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<WithdrawRequestDto>> GetMyWithdrawRequests()
        {
            var tutorId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var withdrawRequests = await _withdrawRequestRepository.FindAsync(x => x.TutorId == tutorId);

            return withdrawRequests.Select(withdrawRequest => new WithdrawRequestDto
            {
                Id = withdrawRequest.Id,
                TutorId = withdrawRequest.TutorId,
                Amount = withdrawRequest.Amount,
                BankName = withdrawRequest.BankName,
                AccountNumber = withdrawRequest.AccountNumber,
                RequestDate = withdrawRequest.RequestDate,
                Status = withdrawRequest.Status
            });
        }

        public async Task<IEnumerable<WithdrawRequestDto>> GetWithdrawRequests()
        {
            var withdrawRequests = await _withdrawRequestRepository.GetAllAsync();

            return withdrawRequests.Select(withdrawRequest => new WithdrawRequestDto
            {
                Id = withdrawRequest.Id,
                TutorId = withdrawRequest.TutorId,
                Amount = withdrawRequest.Amount,
                BankName = withdrawRequest.BankName,
                AccountNumber = withdrawRequest.AccountNumber,
                RequestDate = withdrawRequest.RequestDate,
                Status = withdrawRequest.Status
            });
        }

        public async Task<WithdrawRequestDto> CreateWithdrawRequest(CreateWithdrawRequestDto createWithdrawRequestDto)
        {
            var tutorId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var tutor = await _userRepository.GetByIdAsync(tutorId);

            if (tutor.AccountBalance < createWithdrawRequestDto.Amount)
            {
                throw new Exception("Insufficient balance");
            }

            tutor.AccountBalance -= createWithdrawRequestDto.Amount;
            await _userRepository.UpdateAsync(tutor);

            var withdrawRequest = new WithdrawRequest
            {
                TutorId = tutorId,
                Amount = createWithdrawRequestDto.Amount,
                BankName = createWithdrawRequestDto.BankName,
                AccountNumber = createWithdrawRequestDto.AccountNumber,
            };

            await _withdrawRequestRepository.AddAsync(withdrawRequest);

            return new WithdrawRequestDto
            {
                Id = withdrawRequest.Id,
                TutorId = withdrawRequest.TutorId,
                Amount = withdrawRequest.Amount,
                BankName = withdrawRequest.BankName,
                AccountNumber = withdrawRequest.AccountNumber,
                RequestDate = withdrawRequest.RequestDate,
                Status = withdrawRequest.Status
            };
        }

        public async Task<WithdrawRequestDto> CancelWithdrawRequest(Guid id)
        {
            var withdrawRequest = await _withdrawRequestRepository.GetByIdAsync(id);
            if (withdrawRequest == null)
            {
                throw new Exception("Withdraw request not found");
            }

            var currentUserId = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (withdrawRequest.TutorId != currentUserId)
            {
                throw new Exception("You are not authorized to cancel this withdraw request");
            }

            if (withdrawRequest.Status != WithdrawRequestStatus.Pending)
            {
                throw new Exception("Only pending withdraw requests can be canceled");
            }

            withdrawRequest.Status = WithdrawRequestStatus.Canceled;
            await _withdrawRequestRepository.UpdateAsync(withdrawRequest);

            var tutor = await _userRepository.GetByIdAsync(withdrawRequest.TutorId);
            tutor.AccountBalance += withdrawRequest.Amount;
            await _userRepository.UpdateAsync(tutor);

            return new WithdrawRequestDto
            {
                Id = withdrawRequest.Id,
                TutorId = withdrawRequest.TutorId,
                Amount = withdrawRequest.Amount,
                BankName = withdrawRequest.BankName,
                AccountNumber = withdrawRequest.AccountNumber,
                RequestDate = withdrawRequest.RequestDate,
                Status = withdrawRequest.Status
            };
        }

        public async Task<WithdrawRequestDto> ApproveWithdrawRequest(Guid id)
        {
            var withdrawRequest = await _withdrawRequestRepository.GetByIdAsync(id);
            if (withdrawRequest == null)
            {
                throw new Exception("Withdraw request not found");
            }

            if (withdrawRequest.Status != WithdrawRequestStatus.Pending)
            {
                throw new Exception("Only pending withdraw requests can be approved");
            }

            withdrawRequest.Status = WithdrawRequestStatus.Approved;
            await _withdrawRequestRepository.UpdateAsync(withdrawRequest);

            return new WithdrawRequestDto
            {
                Id = withdrawRequest.Id,
                TutorId = withdrawRequest.TutorId,
                Amount = withdrawRequest.Amount,
                BankName = withdrawRequest.BankName,
                AccountNumber = withdrawRequest.AccountNumber,
                RequestDate = withdrawRequest.RequestDate,
                Status = withdrawRequest.Status
            };
        }

        public async Task<WithdrawRequestDto> RejectWithdrawRequest(Guid id)
        {
            var withdrawRequest = await _withdrawRequestRepository.GetByIdAsync(id);
            if (withdrawRequest == null)
            {
                throw new Exception("Withdraw request not found");
            }

            if (withdrawRequest.Status != WithdrawRequestStatus.Pending)
            {
                throw new Exception("Only pending withdraw requests can be rejected");
            }

            withdrawRequest.Status = WithdrawRequestStatus.Rejected;
            await _withdrawRequestRepository.UpdateAsync(withdrawRequest);

            var tutor = await _userRepository.GetByIdAsync(withdrawRequest.TutorId);
            tutor.AccountBalance += withdrawRequest.Amount;
            await _userRepository.UpdateAsync(tutor);

            return new WithdrawRequestDto
            {
                Id = withdrawRequest.Id,
                TutorId = withdrawRequest.TutorId,
                Amount = withdrawRequest.Amount,
                BankName = withdrawRequest.BankName,
                AccountNumber = withdrawRequest.AccountNumber,
                RequestDate = withdrawRequest.RequestDate,
                Status = withdrawRequest.Status
            };
        }
    }
}