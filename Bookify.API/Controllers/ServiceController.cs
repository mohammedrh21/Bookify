using Bookify.Application.DTO.Category;
using Bookify.Application.DTO.Service;
using Bookify.Application.Interfaces.Category;
using Bookify.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    /// <summary>
    /// Manages bookable services.
    /// </summary>
    [Route("api/services")]
    [Authorize]
    [Produces("application/json")]
    public class ServiceController : BaseController
    {
        private readonly IServiceService _service;

        public ServiceController(IServiceService service)
        {
            _service = service;
        }

        /// <summary>Get a service by Staff ID.</summary>
        /// <response code="200">Service found.</response>
        /// <response code="404">Service not found or deleted.</response>
        [HttpGet("staff/{staffId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByStaffId(Guid staffId)
        {
            var result = await _service.GetByStaffIdAsync(staffId);
            return HandleResult(result);
        }

        /// <summary>Get a service by ID.</summary>
        /// <response code="200">Service found.</response>
        /// <response code="404">Service not found or deleted.</response>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return HandleResult(result);
        }

        /// <summary>Get all active services.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllAsync(search, page, pageSize);
            return HandleResult(result);
        }

        /// <summary>Create a new service (Staff or Admin).</summary>
        /// <response code="201">Service created.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="409">Duplicate service name for this staff.</response>
        /// <response code="422">Business rule violation.</response>
        [HttpPost]
        [Authorize(Policy = "StaffOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
        {
            var result = await _service.CreateAsync(request);
            if (result.Success)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Data },
                    result);
            }
            return HandleResult(result);
        }

        /// <summary>Update an existing service (Staff).</summary>
        /// <response code="200">Service updated.</response>
        /// <response code="404">Service not found.</response>
        /// <response code="422">Business rule violation.</response>
        [HttpPut]
        [Authorize(Policy = "StaffOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update([FromBody] UpdateServiceRequest request)
        {
            var result = await _service.UpdateAsync(request);
            return HandleResult(result);
        }

        /// <summary>Soft-delete a service (Staff Admin).</summary>
        /// <response code="200">Service deleted.</response>
        /// <response code="404">Service not found.</response>
        /// <response code="422">Service has active bookings.</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            return HandleResult(result);
        }

        /// <summary>Upload a cover image for a service (Staff).</summary>
        /// <response code="200">Image uploaded.</response>
        /// <response code="400">Invalid file.</response>
        /// <response code="404">Service not found.</response>
        [HttpPost("{id:guid}/upload-image")]
        [Authorize(Policy = "StaffOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { error = "No file provided." });

            var result = await _service.UploadServiceImageAsync(id, file);
            if (!result.Success)
                return BadRequest(new { error = result.Message });

            return Ok(new { imageUrl = result.Data, message = result.Message });
        }

        [HttpDelete("{id:guid}/image")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> RemoveImage(Guid id)
        {
            var result = await _service.RemoveServiceImageAsync(id);
            return HandleResult(result);
        }
    }
}
