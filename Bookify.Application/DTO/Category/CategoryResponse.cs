using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.Category
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; }
        public ICollection<Domain.Entities.Service>? Services { get; set; }
    }
}
