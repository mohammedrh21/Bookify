using Bookify.Application.Common;
using Bookify.Application.DTO.Category;
using Bookify.Application.DTO.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Interfaces.Category
{
    public interface ICategoryService
    {
        // Queries
        Task<ServiceResponse<CategoryResponse>> GetAsync(Guid id);
        Task<ServiceResponse<IEnumerable<CategoryResponse>>> GetAllAsync();

        // Commands
        Task<ServiceResponse<Guid>> CreateAsync(CreateCategoryRequest request);
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateCategoryRequest request);
        Task<ServiceResponse<bool>> DeactivateAsync(Guid id);
    }
}
