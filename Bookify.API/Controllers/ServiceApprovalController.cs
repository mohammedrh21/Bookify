using Bookify.Application.DTO.Service;
using Bookify.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bookify.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceApprovalController : BaseController
    {
        private readonly IServiceApprovalService _approvalService;

        public ServiceApprovalController(IServiceApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        /// <summary>
        /// Admin gets all requests for listing and comparison
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllRequests()
        {
            var result = await _approvalService.GetAllRequestsAsync();
            return HandleResult(result);
        }

        /// <summary>
        /// Staff gets their own requests
        /// </summary>
        [Authorize(Policy = "StaffOnly")]
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var result = await _approvalService.GetRequestsByStaffIdAsync(CurrentUserGuid);
            return HandleResult(result);
        }

        /// <summary>
        /// Staff submits creation request
        /// </summary>
        [Authorize(Policy = "StaffOnly")]
        [HttpPost("submit-create")]
        public async Task<IActionResult> SubmitCreate([FromBody] CreateServiceRequest request)
        {
            request.StaffId = CurrentUserGuid; // Secure it
            var result = await _approvalService.SubmitCreateRequestAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Staff submits update request
        /// </summary>
        [Authorize(Policy = "StaffOnly")]
        [HttpPost("submit-update")]
        public async Task<IActionResult> SubmitUpdate([FromBody] UpdateServiceRequest request)
        {
            var result = await _approvalService.SubmitUpdateRequestAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Admin approves request
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _approvalService.ApproveRequestAsync(id, CurrentUserGuid);
            return HandleResult(result);
        }

        /// <summary>
        /// Admin rejects request
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromQuery] string comment)
        {
            var result = await _approvalService.RejectRequestAsync(id, CurrentUserGuid, comment);
            return HandleResult(result);
        }
    }
}
