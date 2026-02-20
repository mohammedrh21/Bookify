using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.Category
{
    public class UpdateCategoryRequest
    {
        /// <summary>Populated from route param by the controller – do not send in body.</summary>
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;

        public bool IsActive { get; set; } = true;
    }
}
