using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookify.Domain.Entities
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        public string UserId { get; set; } = default!;

        public string TokenHash { get; set; } = default!;

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByTokenHash { get; set; }
    }
}
