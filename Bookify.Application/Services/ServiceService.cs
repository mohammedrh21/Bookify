using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Service;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Service;
using Bookify.Domain.Contracts.Category;
using Bookify.Domain.Contracts.Service;
using Bookify.Domain.Entities;
using Bookify.Domain.Exceptions;
using Bookify.Domain.Rules;

namespace Bookify.Application.Services
{
    /// <summary>
    /// Application service for managing bookable <see cref="Service"/> entities.
    /// </summary>
    public sealed class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IMapper _mapper;
        private readonly IAppLogger<ServiceService> _logger;

        public ServiceService(
            IServiceRepository repo,
            ICategoryRepository categoryRepo,
            IMapper mapper,
            IAppLogger<ServiceService> logger)
        {
            _repo = repo;
            _categoryRepo = categoryRepo;
            _mapper = mapper;
            _logger = logger;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        /// <inheritdoc/>
        /// <exception cref="NotFoundException">When the service does not exist or is soft-deleted.</exception>
        public async Task<ServiceResponse<ServiceResponse>> GetByIdAsync(Guid id)
        {
            _logger.LogInformation($"Fetching service: {id}");

            var service = await _repo.GetByIdAsync(id);

            if (service is null || service.IsDeleted)
                throw new NotFoundException(nameof(Service), id);

            return ServiceResponse<ServiceResponse>.Ok(
                data: _mapper.Map<ServiceResponse>(service));
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse<IEnumerable<ServiceResponse>>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all active services");

            var services = await _repo.GetAllAsync();
            var active = services.Where(s => !s.IsDeleted);

            return ServiceResponse<IEnumerable<ServiceResponse>>.Ok(
                data: _mapper.Map<IEnumerable<ServiceResponse>>(active));
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        /// <inheritdoc/>
        /// <exception cref="NotFoundException">When the referenced category does not exist.</exception>
        /// <exception cref="ConflictException">When a service with the same name already exists for this staff.</exception>
        /// <exception cref="BusinessRuleException">When domain rules (duration, price, etc.) are violated.</exception>
        public async Task<ServiceResponse<Guid>> CreateAsync(CreateServiceRequest request)
        {
            _logger.LogInformation($"Creating service '{request.Name}' for staff {request.StaffId}");

            // 1. Validate category exists
            var category = await _categoryRepo.GetByIdAsync(request.CategoryId)
                ?? throw new NotFoundException("Category", request.CategoryId);

            if (!category.IsActive)
                throw new BusinessRuleException("Cannot create a service under an inactive category.");

            // 2. Duplicate check for this staff member
            if (await _repo.ExistsAsync(request.Name.Trim(), request.StaffId))
                throw new ConflictException(
                    $"Staff already has a service named '{request.Name}'. Choose a different name.");

            // 3. Domain rule guard
            if (!ServiceRules.CanBeCreated(
                    request.Name, request.Price, request.Duration,
                    request.StaffId, request.CategoryId))
            {
                throw new BusinessRuleException(
                    "Service does not satisfy business rules. " +
                    "Ensure: name ≥ 3 chars, price > 0, duration 30–480 min, valid staff and category.");
            }

            var service = new Service
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                Duration = request.Duration,
                Price = request.Price,
                StaffId = request.StaffId,
                CategoryId = request.CategoryId,
                IsDeleted = false
            };

            await _repo.AddAsync(service);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Service created: {service.Id}");

            return ServiceResponse<Guid>.Ok(service.Id, "Service created successfully.");
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException">When the service does not exist or is soft-deleted.</exception>
        /// <exception cref="BusinessRuleException">When updated values violate domain rules.</exception>
        public async Task<ServiceResponse<bool>> UpdateAsync(UpdateServiceRequest request)
        {
            _logger.LogInformation($"Updating service: {request.Id}");

            var service = await _repo.GetByIdAsync(request.Id);

            if (service is null || service.IsDeleted)
                throw new NotFoundException(nameof(Service), request.Id);

            // Validate individual rules with clear messages
            if (!ServiceRules.IsValidName(request.Name))
                throw new BusinessRuleException("Service name is invalid (3–100 characters required).");

            if (!ServiceRules.IsValidPrice(request.Price))
                throw new BusinessRuleException("Price must be greater than 0 and at most 100,000.");

            if (!ServiceRules.IsValidDuration(request.Duration))
                throw new BusinessRuleException("Duration must be between 30 and 480 minutes.");

            service.Name = request.Name.Trim();
            service.Description = request.Description?.Trim() ?? service.Description;
            service.Price = request.Price;
            service.Duration = request.Duration;

            await _repo.UpdateAsync(service);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Service updated: {service.Id}");

            return ServiceResponse<bool>.Ok(true, "Service updated successfully.");
        }

        /// <summary>
        /// Soft-deletes a service. Active bookings block deletion.
        /// </summary>
        /// <exception cref="NotFoundException">When the service does not exist or is already deleted.</exception>
        /// <exception cref="BusinessRuleException">When there are pending or confirmed bookings tied to this service.</exception>
        public async Task<ServiceResponse<bool>> DeleteAsync(Guid id)
        {
            _logger.LogInformation($"Soft-deleting service: {id}");

            var service = await _repo.GetByIdAsync(id);

            if (service is null || service.IsDeleted)
                throw new NotFoundException(nameof(Service), id);

            // Guard: block deletion when active bookings exist
            bool hasActiveBookings = service.Bookings?.Any(b =>
                b.Status == Domain.Enums.BookingStatus.Pending ||
                b.Status == Domain.Enums.BookingStatus.Approved) == true;

            if (hasActiveBookings)
                throw new BusinessRuleException(
                    "Cannot delete a service with pending or confirmed bookings. " +
                    "Please cancel or complete those bookings first.");

            service.IsDeleted = true;

            await _repo.UpdateAsync(service);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Service soft-deleted: {id}");

            return ServiceResponse<bool>.Ok(true, "Service deleted successfully.");
        }
    }
}