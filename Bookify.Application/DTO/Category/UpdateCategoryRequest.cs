using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.Category
{
    public class UpdateCategoryRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
