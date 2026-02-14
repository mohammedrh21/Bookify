using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Service;
using Bookify.Application.Interfaces.Service;
using Bookify.Domain.Contracts.Category;
using Bookify.Domain.Contracts.Service;
using Bookify.Domain.Entities;
using Bookify.Domain.Rules;

namespace Bookify.Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repo;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ServiceService(IServiceRepository repo, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _repo = repo;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<Guid>> CreateAsync(CreateServiceRequest request)
        {
            if (await _categoryRepository.GetByIdAsync(request.CategoryId) == null)
                return ServiceResponse<Guid>.Fail("Servic category not found!");

            if (await _repo.ExistsAsync(request.Name, request.StaffId))
                return ServiceResponse<Guid>.Fail("Service with the same name already exists for this staff.");

            if (!ServiceRules.CanBeAssignedToStaff(request.StaffId))
                return ServiceResponse<Guid>.Fail("StaffId is not valid");

            if (!ServiceRules.CanBeCreated(request.Name, request.Price, request.Duration, request.StaffId, request.CategoryId))
                return ServiceResponse<Guid>.Fail("Service Rules: have unique name, have at least 30 min duration, having at most 480 min");

            var service = new Service()
            {
                Name = request.Name,
                Description= request.Description,
                Duration = request.Duration,
                Price = request.Price,
                StaffId = request.StaffId,
                CategoryId = request.CategoryId,
            };

            await _repo.AddAsync(service);
            await _repo.SaveChangesAsync();

            return ServiceResponse<Guid>.Ok(service.Id, "Service created successfully");
        }

        public async Task<ServiceResponse<bool>> UpdateAsync(UpdateServiceRequest request)
        {
            var service = await _repo.GetByIdAsync(request.Id);

            if (service == null)
                return ServiceResponse<bool>.Fail("Service not found");

            if (!ServiceRules.IsValidPrice(request.Price))
                return ServiceResponse<bool>.Fail("Price is invalid");

            if (!ServiceRules.IsValidName(request.Name))
                return ServiceResponse<bool>.Fail("Service name is invalid");

            if (!ServiceRules.IsValidDuration(request.Duration))
                return ServiceResponse<bool>.Fail("Duration is invalid");

            service.Price = request.Price;
            service.Name = request.Name;
            service.Description = request.Description;
            service.Duration = request.Duration;

            await _repo.UpdateAsync(service);
            await _repo.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Service updated successfully");
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(Guid id)
        {
            var service = await _repo.GetByIdAsync(id);
            if (service == null)
                return ServiceResponse<bool>.Fail("Service not found");
            service.IsDeleted = true;
            await _repo.UpdateAsync(service);
            await _repo.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Service deleted successfully");
        }

        public async Task<ServiceResponse<ServiceResponse>> GetByIdAsync(Guid id)
        {
            var service = await _repo.GetByIdAsync(id);
            return ServiceResponse<ServiceResponse>.Ok(_mapper.Map<ServiceResponse>(service));
        }

        public async Task<ServiceResponse<IEnumerable<ServiceResponse>>> GetAllAsync()
        {
            var services = await _repo.GetAllAsync();
            return ServiceResponse<IEnumerable<ServiceResponse>>.Ok(_mapper.Map<IEnumerable<ServiceResponse>>(services));
        }
    }
}