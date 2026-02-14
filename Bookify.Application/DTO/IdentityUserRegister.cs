using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookify.Application.DTO
{
    public class IdentityUserRegister
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FullName { get; set; }
        [Phone]
        public required string Phone { get; set; }
    }
}
