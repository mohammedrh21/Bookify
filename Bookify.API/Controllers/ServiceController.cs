using Bookify.Application.DTO.Category;
using Bookify.Application.DTO.Service;
using Bookify.Application.Interfaces.Category;
using Bookify.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    [Route("api/services")]
    [Authorize]
    public class ServiceController : BaseController
    {
        private readonly IServiceService _service;
        private readonly ICategoryService _category;

        public ServiceController(IServiceService service, ICategoryService category)
        { 
            _service = service; 
            _category = category;
        }

        [HttpPost]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<IActionResult> Create(CreateServiceRequest request)
        {
            var result = await _service.CreateAsync(request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<IActionResult> Update(UpdateServiceRequest request)
        {
            var result = await _service.UpdateAsync(request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
    }
}