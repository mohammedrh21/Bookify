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
            return Ok(result);
        }

        /// <summary>Get all active services.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>Create a new service (Staff or Admin).</summary>
        /// <response code="201">Service created.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="409">Duplicate service name for this staff.</response>
        /// <response code="422">Business rule violation.</response>
        [HttpPost]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data },
                result);
        }

        /// <summary>Update an existing service (Staff or Admin).</summary>
        /// <response code="200">Service updated.</response>
        /// <response code="404">Service not found.</response>
        /// <response code="422">Business rule violation.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceRequest request)
        {
            request.Id = id;
            var result = await _service.UpdateAsync(request);
            return Ok(result);
        }

        /// <summary>Soft-delete a service (Staff or Admin).</summary>
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
            return Ok(result);
        }
    }
}
