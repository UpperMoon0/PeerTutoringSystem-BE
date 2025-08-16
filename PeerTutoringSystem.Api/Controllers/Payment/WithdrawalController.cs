using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Payment;
using PeerTutoringSystem.Application.Interfaces.Payment;

namespace PeerTutoringSystem.Api.Controllers.Payment
{
    [Route("api/payment/withdraw")]
    [ApiController]
    [Authorize]
    public class WithdrawalController : ControllerBase
    {
        private readonly IWithdrawService _withdrawService;

        public WithdrawalController(IWithdrawService withdrawService)
        {
            _withdrawService = withdrawService;
        }

        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyWithdrawRequests()
        {
            var result = await _withdrawService.GetMyWithdrawRequests();
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWithdrawRequests()
        {
            var result = await _withdrawService.GetWithdrawRequests();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWithdrawRequest([FromBody] CreateWithdrawRequestDto createWithdrawRequestDto)
        {
            try
            {
                var result = await _withdrawService.CreateWithdrawRequest(createWithdrawRequestDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelWithdrawRequest(Guid id)
        {
            var result = await _withdrawService.CancelWithdrawRequest(id);
            return Ok(result);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveWithdrawRequest(Guid id)
        {
            var result = await _withdrawService.ApproveWithdrawRequest(id);
            return Ok(result);
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectWithdrawRequest(Guid id)
        {
            var result = await _withdrawService.RejectWithdrawRequest(id);
            return Ok(result);
        }
    }
}