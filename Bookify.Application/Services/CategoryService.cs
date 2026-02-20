using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Category;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Category;
using Bookify.Domain.Contracts.Category;
using Bookify.Domain.Entities;
using Bookify.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Services
{
    /// <summary>
    /// Application service for managing <see cref="Category"/> entities.
    /// All mutating operations throw <see cref="DomainException"/>-derived exceptions
    /// that are handled centrally by <c>GlobalExceptionMiddleware</c>.
    /// </summary>
    public sealed class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;
        private readonly IAppLogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository repo,
            IMapper mapper,
            IAppLogger<CategoryService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        /// <summary>Returns a single category by ID.</summary>
        /// <exception cref="NotFoundException">When the category does not exist.</exception>
        public async Task<ServiceResponse<CategoryResponse>> GetAsync(Guid id)
        {
            _logger.LogInformation($"Fetching category with ID {id}");

            var category = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Category), id);

            return ServiceResponse<CategoryResponse>.Ok(
                data: _mapper.Map<CategoryResponse>(category));
        }

        /// <summary>Returns all active categories.</summary>
        public async Task<ServiceResponse<IEnumerable<CategoryResponse>>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all categories");

            var categories = await _repo.GetAllAsync();
            return ServiceResponse<IEnumerable<CategoryResponse>>.Ok(
                data: _mapper.Map<IEnumerable<CategoryResponse>>(categories));
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        /// <summary>Creates a new category.</summary>
        /// <exception cref="ConflictException">When a category with the same name already exists.</exception>
        public async Task<ServiceResponse<Guid>> CreateAsync(CreateCategoryRequest request)
        {
            _logger.LogInformation($"Creating category: {request.Name}");

            // Normalize for comparison
            var normalizedName = request.Name.Trim();

            if (await _repo.IsExists(normalizedName))
                throw new ConflictException($"A category named '{normalizedName}' already exists.");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                IsActive = true
            };

            await _repo.AddAsync(category);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Category created: {category.Id} – {category.Name}");

            return ServiceResponse<Guid>.Ok(
                id: category.Id,
                message: "Category created successfully.");
        }

        /// <summary>Updates an existing category's name or active status.</summary>
        /// <exception cref="NotFoundException">When the category does not exist.</exception>
        /// <exception cref="ConflictException">When the new name is already taken by another category.</exception>
        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateCategoryRequest request)
        {
            _logger.LogInformation($"Updating category: {request.Id}");

            var category = await _repo.GetByIdAsync(request.Id)
                ?? throw new NotFoundException(nameof(Category), request.Id);

            var normalizedName = request.Name.Trim();

            // Name collision check – exclude current category
            if (!string.Equals(category.Name, normalizedName, StringComparison.OrdinalIgnoreCase)
                && await _repo.IsExists(normalizedName))
            {
                throw new ConflictException($"A category named '{normalizedName}' already exists.");
            }

            category.Name = normalizedName;
            category.IsActive = request.IsActive;

            await _repo.UpdateAsync(category);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Category updated: {category.Id}");

            return ServiceResponse<Guid>.Ok(
                id: category.Id,
                message: "Category updated successfully.");
        }

        /// <summary>Deactivates a category (soft-delete via IsActive flag).</summary>
        /// <exception cref="NotFoundException">When the category does not exist.</exception>
        /// <exception cref="BusinessRuleException">When the category still has active services.</exception>
        public async Task<ServiceResponse<bool>> DeactivateAsync(Guid id)
        {
            _logger.LogInformation($"Deactivating category: {id}");

            var category = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Category), id);

            if (!category.IsActive)
                throw new BusinessRuleException("Category is already inactive.");

            bool hasActiveServices = category.Services?.Any(s => !s.IsDeleted) == true;
            if (hasActiveServices)
                throw new BusinessRuleException(
                    "Cannot deactivate a category that still has active services. " +
                    "Please reassign or delete those services first.");

            category.IsActive = false;
            await _repo.UpdateAsync(category);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Category deactivated: {id}");

            return ServiceResponse<bool>.Ok(true, "Category deactivated successfully.");
        }
    }
}
