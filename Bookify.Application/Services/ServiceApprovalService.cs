using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.Service;
using Bookify.Application.Interfaces.Service;
using Bookify.Domain.Contracts.Category;
using Bookify.Domain.Contracts.Service;
using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Domain.Exceptions;
using Bookify.Domain.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bookify.Application.Interfaces.Notification;

namespace Bookify.Application.Services
{
    public class ServiceApprovalService : IServiceApprovalService
    {
        private readonly IServiceApprovalRepository _approvalRepo;
        private readonly IServiceService _serviceService;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public ServiceApprovalService(
            IServiceApprovalRepository approvalRepo,
            IServiceService serviceService,
            ICategoryRepository categoryRepo,
            IMapper mapper,
            INotificationService notificationService)
        {
            _approvalRepo = approvalRepo;
            _serviceService = serviceService;
            _categoryRepo = categoryRepo;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<ServiceResponse<Guid>> SubmitCreateRequestAsync(CreateServiceRequest request)
        {
            if (!ServiceRules.CanBeCreated(request.Name, request.Price, request.Duration, request.StaffId, request.CategoryId))
            {
                throw new BusinessRuleException("Service does not satisfy business rules.");
            }

            // Create inactive service directly!
            var serviceResult = await _serviceService.CreateAsync(request);
            var serviceId = serviceResult.Data;

            var approvalRequest = new ServiceApprovalRequest
            {
                Id = Guid.NewGuid(),
                ServiceId = serviceId,
                StaffId = request.StaffId,
                Type = ApprovalRequestType.Create,
                Status = ApprovalStatus.Pending,
                ProposedData = JsonSerializer.Serialize(request),
                CreatedAt = DateTime.UtcNow
            };

            await _approvalRepo.AddAsync(approvalRequest);
            await _approvalRepo.SaveChangesAsync();

            return ServiceResponse<Guid>.Ok(approvalRequest.Id, "Service creation submitted for approval.");
        }

        public async Task<ServiceResponse<Guid>> SubmitUpdateRequestAsync(UpdateServiceRequest request)
        {
            if (!ServiceRules.IsValidName(request.Name) || !ServiceRules.IsValidPrice(request.Price) || !ServiceRules.IsValidDuration(request.Duration))
            {
                throw new BusinessRuleException("Service does not satisfy business rules.");
            }

            var liveServiceResponse = await _serviceService.GetByIdAsync(request.Id);
            var liveService = liveServiceResponse.Data;

            if (liveService == null)
                throw new NotFoundException("Service", request.Id);

            var approvalRequest = new ServiceApprovalRequest
            {
                Id = Guid.NewGuid(),
                ServiceId = request.Id,
                StaffId = liveService.StaffId,
                Type = ApprovalRequestType.Update,
                Status = ApprovalStatus.Pending,
                ProposedData = JsonSerializer.Serialize(request),
                CreatedAt = DateTime.UtcNow
            };

            await _approvalRepo.AddAsync(approvalRequest);
            await _approvalRepo.SaveChangesAsync();

            return ServiceResponse<Guid>.Ok(approvalRequest.Id, "Service update submitted for approval.");
        }

        public async Task<ServiceResponse<IEnumerable<ServiceApprovalRequestResponse>>> GetAllRequestsAsync()
        {
            var requests = await _approvalRepo.GetAllAsync();
            var responseList = await MapRequestsToResponseAsync(requests);
            return ServiceResponse<IEnumerable<ServiceApprovalRequestResponse>>.Ok(responseList);
        }

        public async Task<ServiceResponse<IEnumerable<ServiceApprovalRequestResponse>>> GetRequestsByStaffIdAsync(Guid staffId)
        {
            var requests = await _approvalRepo.GetByStaffIdAsync(staffId);
            var responseList = await MapRequestsToResponseAsync(requests);
            return ServiceResponse<IEnumerable<ServiceApprovalRequestResponse>>.Ok(responseList);
        }

        private async Task<IEnumerable<ServiceApprovalRequestResponse>> MapRequestsToResponseAsync(IEnumerable<ServiceApprovalRequest> requests)
        {
            var responseList = new List<ServiceApprovalRequestResponse>();

            foreach (var req in requests)
            {
                var response = new ServiceApprovalRequestResponse
                {
                    Id = req.Id,
                    ServiceId = req.ServiceId,
                    StaffId = req.StaffId,
                    StaffName = req.Staff?.FullName ?? "Unknown",
                    Type = req.Type,
                    Status = req.Status,
                    CreatedAt = req.CreatedAt,
                    AdminComment = req.AdminComment
                };

                if (req.Type == ApprovalRequestType.Create)
                {
                    var proposed = JsonSerializer.Deserialize<CreateServiceRequest>(req.ProposedData);
                    if (proposed != null)
                    {
                        var category = await _categoryRepo.GetByIdAsync(proposed.CategoryId);
                        response.ProposedDetails = new ServiceApprovalProposedDetailsDto
                        {
                            Name = proposed.Name,
                            Description = proposed.Description ?? string.Empty,
                            Price = proposed.Price,
                            Duration = proposed.Duration,
                            TimeStart = proposed.TimeStart,
                            TimeEnd = proposed.TimeEnd,
                            CategoryId = proposed.CategoryId,
                            CategoryName = category?.Name ?? "Unknown"
                        };
                    }
                }
                else if (req.Type == ApprovalRequestType.Update)
                {
                    var proposed = JsonSerializer.Deserialize<UpdateServiceRequest>(req.ProposedData);
                    if (proposed != null)
                    {
                        var liveServiceResponse = await _serviceService.GetByIdAsync(req.ServiceId!.Value);
                        response.CurrentDetails = liveServiceResponse.Data;

                        response.ProposedDetails = new ServiceApprovalProposedDetailsDto
                        {
                            Name = proposed.Name,
                            Description = proposed.Description ?? string.Empty,
                            Price = proposed.Price,
                            Duration = proposed.Duration,
                            TimeStart = proposed.TimeStart,
                            TimeEnd = proposed.TimeEnd,
                            CategoryId = response.CurrentDetails?.CategoryId ?? Guid.Empty, // Add Guid.Empty checks
                            CategoryName = response.CurrentDetails?.CategoryName ?? "Unknown"
                        };
                    }
                }

                responseList.Add(response);
            }

            return responseList;
        }

        public async Task<ServiceResponse<Guid>> ApproveRequestAsync(Guid requestId, Guid adminId)
        {
            var request = await _approvalRepo.GetByIdAsync(requestId);
            if (request == null)
                throw new NotFoundException("ApprovalRequest", requestId);

            if (request.Status != ApprovalStatus.Pending)
                throw new BusinessRuleException("Request is already processed.");

            if (request.Type == ApprovalRequestType.Create)
            {
                if (!request.ServiceId.HasValue) 
                    throw new BusinessRuleException("Service ID is missing on the approval request.");

                await _serviceService.ActivateAsync(request.ServiceId.Value);
            }
            else if (request.Type == ApprovalRequestType.Update)
            {
                var proposed = JsonSerializer.Deserialize<UpdateServiceRequest>(request.ProposedData);
                if (proposed == null) throw new BusinessRuleException("Failed to deserialize proposed data.");

                await _serviceService.UpdateAsync(proposed);
            }

            request.Status = ApprovalStatus.Approved;
            request.ActionedAt = DateTime.UtcNow;
            request.ActionedBy = adminId;

            await _approvalRepo.UpdateAsync(request);
            await _approvalRepo.SaveChangesAsync();

            // Notify the staff member
            await _notificationService.CreateAsync(
                request.StaffId,
                "Service Request Approved",
                $"Your service request has been approved by an administrator.",
                NotificationType.ServiceApproved,
                request.ServiceId,
                "/services/my-service");

            return ServiceResponse<Guid>.Ok(request.Id, "Request approved successfully.");
        }

        public async Task<ServiceResponse<Guid>> RejectRequestAsync(Guid requestId, Guid adminId, string comment)
        {
            var request = await _approvalRepo.GetByIdAsync(requestId);
            if (request == null)
                throw new NotFoundException("ApprovalRequest", requestId);

            if (request.Status != ApprovalStatus.Pending)
                throw new BusinessRuleException("Request is already processed.");

            if (request.Type == ApprovalRequestType.Create && request.ServiceId.HasValue)
            {
                await _serviceService.HardDeleteAsync(request.ServiceId.Value);
            }

            request.Status = ApprovalStatus.Rejected;
            request.ActionedAt = DateTime.UtcNow;
            request.ActionedBy = adminId;
            request.AdminComment = comment;

            await _approvalRepo.UpdateAsync(request);
            await _approvalRepo.SaveChangesAsync();

            // Notify the staff member
            await _notificationService.CreateAsync(
                request.StaffId,
                "Service Request Rejected",
                $"Your service request has been rejected.{(string.IsNullOrEmpty(comment) ? "" : $" Reason: {comment}")}",
                NotificationType.ServiceRejected,
                request.ServiceId,
                "/services/my-service");

            return ServiceResponse<Guid>.Ok(request.Id, "Request rejected.");
        }
    }
}
