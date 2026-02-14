using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Category;
using Bookify.Application.Interfaces.Category;
using Bookify.Domain.Contracts.Category;
using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<Guid>> CreateAsync(CreateCategoryRequest request)
        {
            if (await _repo.IsExists(request.Name))
                return ServiceResponse<Guid>.Fail("Category is already used!");

            Category category = new Category()
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };
            await _repo.AddAsync(category);
            await _repo.SaveChangesAsync();
            return ServiceResponse<Guid>.Ok(
                id: category.Id,
                message: "Category created successfully");
        }

        public async Task<ServiceResponse<CategoryResponse>> GetAsync(Guid id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category is null)
                return ServiceResponse<CategoryResponse>.Fail("Category not found!");

            return ServiceResponse<CategoryResponse>.Ok(
                data: _mapper.Map<CategoryResponse>(category));
        }

        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateCategoryRequest request)
        {
            var category = await _repo.GetByIdAsync(request.Id);
            if (category == null)
                return ServiceResponse<Guid>.Fail("Category is not found!");
            await _repo.UpdateAsync(_mapper.Map(request, category));
            return ServiceResponse<Guid>.Ok(
                id: category.Id,
                message: "Category updated successfully!");
        }
    }
}
