using Bookify.Application.DTO.Category;
using Bookify.Application.Interfaces.Category;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    [Route("api/category")]
    public class CategoryController:BaseController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService) 
        { 
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategory(Guid id)
        {
            var result = await _categoryService.GetAsync(id);
            return Ok(result);
        }
        
        [HttpPost("create")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
        {
            var result = await _categoryService.CreateAsync(request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryRequest request)
        {
            var result = await _categoryService.UpdateAsync(request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
