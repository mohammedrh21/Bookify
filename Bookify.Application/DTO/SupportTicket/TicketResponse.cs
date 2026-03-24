using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.SupportTicket
{
    public class TicketResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
