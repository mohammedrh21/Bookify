using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.Identity
{
    public class RegisterClientRequest : RegisterBaseRequest
    {
        public DateTime? DateOfBirth { get; set; }
    }
}
