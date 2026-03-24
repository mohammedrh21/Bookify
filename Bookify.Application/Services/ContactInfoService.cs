using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.ContactInfo;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.ContactInfo;
using Bookify.Domain.Contracts.ContactInfo;
using Bookify.Domain.Entities;
using Bookify.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bookify.Application.Services
{
    public class ContactInfoService : IContactInfoService
    {
        private readonly IContactInfoRepository _repo;
        private readonly IMapper _mapper;
        private readonly IAppLogger<ContactInfoService> _logger;

        public ContactInfoService(IContactInfoRepository repo, IMapper mapper, IAppLogger<ContactInfoService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<ContactInfoResponse>> GetAsync()
        {
            _logger.LogInformation("Fetching contact info.");
            var info = await _repo.GetAsync();
            if (info == null)
                throw new NotFoundException(nameof(ContactInfo));

            return ServiceResponse<ContactInfoResponse>.Ok(
                message: "success",
                data: _mapper.Map<ContactInfoResponse>(info),
                id: info.Id);
        }

        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateContactInfoRequest request)
        {
            _logger.LogInformation($"Updating contact info.");
            var info = await _repo.GetAsync();
            if (info == null)
                throw new NotFoundException(nameof(ContactInfo));

            info.Country = request.Country?.Trim()!;
            info.AddressLine_1 = request.AddressLine_1?.Trim();
            info.AddressLine_2 = request.AddressLine_2?.Trim();
            info.Email = request.Email?.Trim()!;
            info.PhoneNumber = request.PhoneNumber?.Trim()!;
            info.CallDayFrom = request.CallDayFrom;
            info.CallDayTo = request.CallDayTo;
            info.CallHourFrom = request.CallHourFrom;
            info.CallHourTo = request.CallHourTo;

            await _repo.UpdateAsync(info);
            await _repo.SaveChangesAsync();
            
            _logger.LogInformation($"Contact info updated successfully.");
            
            return ServiceResponse<Guid>.Ok(
                message: "Contact info updated successfully",
                data: info.Id,
                id: info.Id);
        }

        public async Task<ServiceResponse<Guid>> CreateAsync(CreateContactInfoRequest request)
        {
            _logger.LogInformation("Creating contact info");

            var info = await _repo.GetAsync();
            if (info != null)
                throw new ConflictException("Contact info already exists.");

            var contactInfo = new ContactInfo()
            {
                Id = Guid.NewGuid(),
                Country = request.Country?.Trim()!,
                AddressLine_1 = request.AddressLine_1?.Trim(),
                AddressLine_2 = request.AddressLine_2?.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim()!,
                Email = request.Email?.Trim()!,
                CallDayFrom = request.CallDayFrom,
                CallDayTo = request.CallDayTo,
                CallHourFrom = request.CallHourFrom,
                CallHourTo = request.CallHourTo
            };

            await _repo.AddAsync(contactInfo);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Contact info created successfully: {contactInfo.Id}");

            return ServiceResponse<Guid>.Ok(
                message: "Contact info created successfully",
                id: contactInfo.Id,
                data: contactInfo.Id);
        }
    }
}
