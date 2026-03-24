using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Common;
using Bookify.Application.DTO.Service;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Service;
using Bookify.Domain.Contracts.Category;
using Bookify.Domain.Contracts.Service;
using Bookify.Domain.Entities;
using Bookify.Domain.Exceptions;
using Bookify.Domain.Rules;
using Microsoft.AspNetCore.Http;

using Bookify.Application.Interfaces.Auth;

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
        private readonly IFileService _fileService;
        private readonly ICurrentUserService _currentUserService;

        public ServiceService(
            IServiceRepository repo,
            ICategoryRepository categoryRepo,
            IMapper mapper,
            IAppLogger<ServiceService> logger,
            IFileService fileService,
            ICurrentUserService currentUserService)
        {
            _repo = repo;
            _categoryRepo = categoryRepo;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
            _currentUserService = currentUserService;
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

        public async Task<ServiceResponse<ServiceResponse>> GetByStaffIdAsync(Guid staffId)
        {
            _logger.LogInformation($"Fetching service by staffId: {staffId}");

            var service = await _repo.GetByStaffIdAsync(staffId);

            if (service == null)
                throw new NotFoundException(nameof(Service), staffId);

            return ServiceResponse<ServiceResponse>.Ok(
                data: _mapper.Map<ServiceResponse>(service));
        }

        /// <inheritdoc/>
        public async Task<ServiceResponse<PagedResult<ServiceResponse>>> GetAllAsync(string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            _logger.LogInformation($"Fetching active services (page {page}, search: {searchTerm})");
            var skip = (page - 1) * pageSize;
            var services = await _repo.GetAllAsync(searchTerm, skip, pageSize);
            var total = await _repo.GetCountAsync(searchTerm);

            var paged = new PagedResult<ServiceResponse>
            {
                Items = _mapper.Map<IEnumerable<ServiceResponse>>(services),
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            };

            return ServiceResponse<PagedResult<ServiceResponse>>.Ok(paged);
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

            if (_currentUserService.IsStaff && request.StaffId != _currentUserService.UserId)
                throw new ForbiddenException("You cannot create services for another staff member.");

            // 1. Validate category exists
            var category = await _categoryRepo.GetByIdAsync(request.CategoryId)
                ?? throw new NotFoundException("Category", request.CategoryId);

            var existedservice = await _repo.GetByStaffIdAsync(request.StaffId);
            if (existedservice != null)
            {
                if (!existedservice.IsDeleted)
                    throw new ConflictException("Staff already has an active service.");

                if (!existedservice.DeletedAt.HasValue || (DateTime.UtcNow - existedservice.DeletedAt.Value).TotalDays < 14)
                    throw new ConflictException($"You must wait 14 days after deletion before registering a new service.");

                // If 14 days passed, hard delete the old one to make room for the new one!
                await _repo.RemoveAsync(existedservice);
                // The new service will be added in this same context transaction
            }

            if (!category.IsActive)
                throw new BusinessRuleException("Cannot create a service under an inactive category.");

            // Duplicate check for this staff member
            if (await _repo.ExistsAsync(request.Name, request.StaffId))
                throw new ConflictException(
                    $"Staff already has a service named '{request.Name}'. Choose a different name.");

            //  Domain rule guard
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
                TimeStart = request.TimeStart,
                TimeEnd = request.TimeEnd,
                IsDeleted = false,
                IsActive = false
            };

            await _repo.AddAsync(service);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Service created: {service.Id}");

            return ServiceResponse<Guid>.Ok(data: service.Id, id: service.Id, message: "Service created successfully.");
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException">When the service does not exist or is soft-deleted.</exception>
        /// <exception cref="BusinessRuleException">When updated values violate domain rules.</exception>
        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateServiceRequest request)
        {
            _logger.LogInformation($"Updating service: {request.Id}");

            var service = await _repo.GetByIdAsync(request.Id);

            if (service is null || service.IsDeleted)
                throw new NotFoundException(nameof(Service), request.Id);

            if (!_currentUserService.IsAdmin && service.StaffId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to update this service.");

            // Validate individual rules with clear messages
            if (!ServiceRules.IsValidName(request.Name))
                throw new BusinessRuleException("Service name is invalid (3–100 characters required).");

            if (!ServiceRules.IsValidPrice(request.Price))
                throw new BusinessRuleException("Price must be greater than 0 and at most 10,000.");

            if (!ServiceRules.IsValidDuration(request.Duration))
                throw new BusinessRuleException("Duration must be between 30 and 480 minutes.");

            service.Name = request.Name.Trim();
            service.Description = request.Description?.Trim() ?? service.Description;
            service.Price = request.Price;
            service.Duration = request.Duration;
            service.TimeStart = request.TimeStart;
            service.TimeEnd = request.TimeEnd;
            await _repo.UpdateAsync(service);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Service updated: {service.Id}");

            return ServiceResponse<Guid>.Ok(data: service.Id, id: service.Id, message: "Service updated successfully.");
        }

        /// <summary>
        /// Soft-deletes a service. Active bookings block deletion.
        /// </summary>
        /// <exception cref="NotFoundException">When the service does not exist or is already deleted.</exception>
        /// <exception cref="BusinessRuleException">When there are pending or confirmed bookings tied to this service.</exception>
        public async Task<ServiceResponse<Guid>> DeleteAsync(Guid id)
        {
            _logger.LogInformation($"Soft-deleting service: {id}");

            var service = await _repo.GetByIdAsync(id);

            if (service is null || service.IsDeleted)
                throw new NotFoundException(nameof(Service), id);

            if (!_currentUserService.IsAdmin && service.StaffId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to delete this service.");

            // Guard: block deletion when active bookings exist
            bool hasActiveBookings = service.Bookings?.Any(b =>
                b.Status == Domain.Enums.BookingStatus.Pending ||
                b.Status == Domain.Enums.BookingStatus.Approved) == true;

            if (hasActiveBookings)
                throw new BusinessRuleException(
                    "Cannot delete a service with pending or confirmed bookings. " +
                    "Please cancel or complete those bookings first.");

            service.IsDeleted = true;
            service.DeletedAt = DateTime.UtcNow;
            service.IsActive = false;

            await _repo.UpdateAsync(service);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Service soft-deleted: {id}");

            return ServiceResponse<Guid>.Ok(data: id, id: id, message: "Service deleted successfully.");
        }

        /// <summary>
        /// Uploads a cover image for the service to Cloudinary and persists the URL.
        /// </summary>
        public async Task<Common.ServiceResponse<string>> UploadServiceImageAsync(Guid serviceId, IFormFile file)
        {
            _logger.LogInformation($"Uploading image for service: {serviceId}");

            var service = await _repo.GetByIdAsync(serviceId);
            if (service is null || service.IsDeleted)
                throw new NotFoundException(nameof(Service), serviceId);

            if (!_currentUserService.IsAdmin && service.StaffId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to manage images for this service.");

            var extension = Path.GetExtension(file.FileName);
            var customFileName = $"{serviceId}{extension}";

            var imageUrl = await _fileService.Upload(file, "Service", customFileName);

            service.ImagePath = imageUrl;
            await _repo.UpdateAsync(service);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Service image uploaded: {imageUrl}");

            return Common.ServiceResponse<string>.Ok(imageUrl, "Service image updated successfully.");
        }

        /// <summary>
        /// Removes a cover image from Cloudinary and clears the URL from the database.
        /// </summary>
        public async Task<Common.ServiceResponse<bool>> RemoveServiceImageAsync(Guid serviceId)
        {
            _logger.LogInformation($"Removing image for service: {serviceId}");

            var service = await _repo.GetByIdAsync(serviceId);
            if (service is null || service.IsDeleted)
                throw new NotFoundException(nameof(Service), serviceId);

            if (!_currentUserService.IsAdmin && service.StaffId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have permission to manage images for this service.");

            if (string.IsNullOrEmpty(service.ImagePath))
                return Common.ServiceResponse<bool>.Ok(true, "No image to remove.");

            try
            {
                // The filename in Cloudinary is `<serviceId>.<extension>`
                // We uploaded it using just the IFormFile, so let's extract the extension from the URL if possible
                // Or try to delete without extension because Cloudinary public_id usually doesn't need extension
                // But our IFileService.Delete takes fileName. Our FileService uses parameter `fileName`
                // as publicId = $"{folderName}/{Path.GetFileNameWithoutExtension(fileName)}".
                // So any extension works, or we can just send the ID.
                var fileName = $"{serviceId}"; 
                
                await _fileService.Delete(fileName, "Service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete image from Cloudinary for service {serviceId}");
                // We'll proceed to clear it from the DB even if Cloudinary fails, so the user isn't stuck.
            }

            service.ImagePath = null;
            await _repo.UpdateAsync(service);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Service image removed: {serviceId}");

            return Common.ServiceResponse<bool>.Ok(true, "Service image removed successfully.");
        }

        public async Task<ServiceResponse<bool>> ActivateAsync(Guid id)
        {
            _logger.LogInformation($"Activating service: {id}");

            var service = await _repo.GetByIdAsync(id);
            if (service == null)
                throw new NotFoundException(nameof(Service), id);

            service.IsActive = true;
            await _repo.UpdateAsync(service);
            await _repo.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Service activated successfully.");
        }

        public async Task<ServiceResponse<bool>> HardDeleteAsync(Guid id)
        {
            _logger.LogInformation($"Hard deleting service: {id}");

            var service = await _repo.GetByIdAsync(id);
            if (service == null)
                return ServiceResponse<bool>.Ok(true, "Service already deleted.");

            await _repo.RemoveAsync(service);
            await _repo.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Service hard deleted successfully.");
        }
    }
}