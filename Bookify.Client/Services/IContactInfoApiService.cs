using Bookify.Client.Models;
using Bookify.Client.Models.ContactInfo;

namespace Bookify.Client.Services;

public interface IContactInfoApiService
{
    Task<ApiResult<ContactInfoModel>> GetAsync();
    Task<ApiResult<Guid>> CreateAsync(ContactInfoModel request);
    Task<ApiResult<Guid>> UpdateAsync(ContactInfoModel request);
}
