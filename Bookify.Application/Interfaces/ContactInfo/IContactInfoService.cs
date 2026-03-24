using Bookify.Application.Common;
using Bookify.Application.DTO.ContactInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Interfaces.ContactInfo
{
    public interface IContactInfoService
    {
        Task<ServiceResponse<ContactInfoResponse>> GetAsync();
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateContactInfoRequest request);
        Task<ServiceResponse<Guid>> CreateAsync(CreateContactInfoRequest request);
    }
}
