using Bookify.Application.DTO.Category;
using Bookify.Application.Interfaces.Category;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    /// <summary>
    /// Manages service categories.
    /// </summary>
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>Get a single category by ID.</summary>
        /// <response code="200">Category found.</response>
        /// <response code="404">Category not found.</response>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _categoryService.GetAsync(id);
            return Ok(result);
        }

        /// <summary>Get all active categories.</summary>
        /// <response code="200">List of categories.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>Create a new category (Admin only).</summary>
        /// <response code="201">Category created.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="409">Category name already exists.</response>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var result = await _categoryService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data },
                result);
        }

        /// <summary>Update an existing category (Admin only).</summary>
        /// <response code="200">Category updated.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="409">New name already exists.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            request.Id = id;
            var result = await _categoryService.UpdateAsync(request);
            return Ok(result);
        }

        /// <summary>Deactivate a category (Admin only).</summary>
        /// <response code="200">Category deactivated.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="422">Category has active services.</response>
        [HttpPatch("{id:guid}/deactivate")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var result = await _categoryService.DeactivateAsync(id);
            return Ok(result);
        }
    }
}
