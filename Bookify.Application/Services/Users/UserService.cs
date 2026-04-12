using Bookify.Application.Common;
using Bookify.Application.DTO.Common;
using Bookify.Application.DTO.Users;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Client;
using Bookify.Application.Interfaces.Staff;
using Bookify.Application.Interfaces.Users;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Bookify.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IIdentityUserService _identityUserService;

        public UserService(IStaffRepository staffRepository, IClientRepository clientRepository, IIdentityUserService identityUserService)
        {
            _staffRepository = staffRepository;
            _clientRepository = clientRepository;
            _identityUserService = identityUserService;
        }

        public async Task<PagedResult<StaffDto>> GetStaffPaginatedAsync(int page, int pageSize)
        {
            var (items, totalCount) = await _staffRepository.GetStaffPaginatedAsync(page, pageSize);

            var dtos = items.Select(s => new StaffDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Phone = s.Phone ?? string.Empty,
                IsActive = s.IsActive,
                ServiceName = s.Service?.Name ?? "No Service",
                BookingCount = s.Service?.Bookings?.Count ?? 0,
                ImagePath = s.ImagePath
            }).ToList();

            return new PagedResult<StaffDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<ClientDto>> GetClientsPaginatedAsync(int page, int pageSize)
        {
            var (items, totalCount) = await _clientRepository.GetClientsPaginatedAsync(page, pageSize);

            var dtos = items.Select(c => new ClientDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Phone = c.Phone ?? string.Empty,
                IsActive = c.IsActive,
                BookingCount = c.Bookings?.Count ?? 0,
                ImagePath = c.ImagePath
            }).ToList();

            return new PagedResult<ClientDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<ServiceResponse<bool>> ToggleUserActiveAsync(Guid id)
        {
            bool foundAndToggled = false;
            bool newStatus = false;

            var identityResult = await _identityUserService.ToggleUserActiveAsync(id);

            var client = await _clientRepository.GetByIdAsync(id);
            if (client != null)
            {
                client.IsActive = !client.IsActive;
                await _clientRepository.UpdateAsync(client);
                foundAndToggled = true;
                newStatus = client.IsActive;
            }
            else
            {
                var staff = await _staffRepository.GetByIdAsync(id);
                if (staff != null)
                {
                    staff.IsActive = !staff.IsActive;
                    await _staffRepository.UpdateAsync(staff);
                    foundAndToggled = true;
                    newStatus = staff.IsActive;
                }
            }

            if (!foundAndToggled && !identityResult.Success)
            {
                return ServiceResponse<bool>.Fail("User not found.");
            }

            return ServiceResponse<bool>.Ok(foundAndToggled ? newStatus : identityResult.Data);
        }

        public async Task<ServiceResponse<ClientReportDto>> GetClientReportAsync(Guid id)
        {
            var client = await _clientRepository.GetClientWithBookingsAsync(id);
            if (client == null)
            {
                return ServiceResponse<ClientReportDto>.Fail("Client not found.");
            }

            var report = new ClientReportDto
            {
                FullName = client.FullName,
                Phone = client.Phone ?? string.Empty,
                IsActive = client.IsActive,
                TotalBookings = client.Bookings?.Count ?? 0,
                ImagePath = client.ImagePath,
                RecentBookings = client.Bookings?
                    .OrderByDescending(b => b.Date)
                    .Take(10)
                    .Select(b => new ClientReportBookingDto
                    {
                        Id = b.Id,
                        ServiceName = b.Service?.Name ?? "Unknown",
                        Date = b.Date,
                        Status = b.Status.ToString()
                    }).ToList() ?? new List<ClientReportBookingDto>()
            };

            return ServiceResponse<ClientReportDto>.Ok(report);
        }

        public async Task<ServiceResponse<PagedResult<StaffClientDto>>> GetStaffClientsAsync(
            Guid staffId, string? search, DateTime? dateFilter, int page, int pageSize)
        {
            var (items, totalCount) = await _clientRepository.GetStaffClientsPaginatedAsync(staffId, search, dateFilter, page, pageSize);

            return ServiceResponse<PagedResult<StaffClientDto>>.Ok(new PagedResult<StaffClientDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public async Task<ServiceResponse<StaffClientDetailsDto>> GetStaffClientDetailsAsync(Guid staffId, Guid clientId)
        {
            var result = await _clientRepository.GetStaffClientDetailsAsync(staffId, clientId);

            if (result == null)
            {
                return ServiceResponse<StaffClientDetailsDto>.Fail("Client not found or hasn't booked with this staff member.");
            }

            return ServiceResponse<StaffClientDetailsDto>.Ok(result);
        }

        public async Task<ServiceResponse<PagedResult<AdminClientDto>>> GetAdminClientsAsync(
            string? search, int page, int pageSize)
        {
            var (items, totalCount) = await _clientRepository.GetAdminClientsPaginatedAsync(search, page, pageSize);

            return ServiceResponse<PagedResult<AdminClientDto>>.Ok(new PagedResult<AdminClientDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public async Task<ServiceResponse<AdminClientDetailsDto>> GetAdminClientDetailsAsync(Guid clientId)
        {
            var result = await _clientRepository.GetAdminClientDetailsAsync(clientId);

            if (result == null)
            {
                return ServiceResponse<AdminClientDetailsDto>.Fail("Client not found.");
            }

            return ServiceResponse<AdminClientDetailsDto>.Ok(result);
        }

        // ── Admin Staff Members ────────────────────────────────────────────────

        public async Task<ServiceResponse<PagedResult<AdminStaffDto>>> GetAdminStaffAsync(
            string? search, int page, int pageSize)
        {
            var (items, totalCount) = await _staffRepository.GetAdminStaffPaginatedAsync(search, page, pageSize);

            return ServiceResponse<PagedResult<AdminStaffDto>>.Ok(new PagedResult<AdminStaffDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public async Task<ServiceResponse<AdminStaffDetailsDto>> GetAdminStaffDetailsAsync(Guid staffId)
        {
            var result = await _staffRepository.GetAdminStaffDetailsAsync(staffId);

            if (result == null)
            {
                return ServiceResponse<AdminStaffDetailsDto>.Fail("Staff member not found.");
            }

            return ServiceResponse<AdminStaffDetailsDto>.Ok(result);
        }
    }
}
