using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Domain.Rules
{
    public static class ServiceRules
    {
        public static bool IsValidPrice(decimal price)
        {
            return price > 0;
        }

        public static bool IsValidDuration(int durationInMinutes)
        {
            return durationInMinutes is >= 30 and <= 480;
        }

        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 5;
        }

        public static bool CanBeAssignedToStaff(Guid staffId)
        {
            return staffId != Guid.Empty;
        }

        public static bool CanBeCreated(
            string name,
            decimal price,
            int duration,
            Guid staffId,
            Guid categoryId)
        {
            return
                IsValidName(name) &&
                IsValidPrice(price) &&
                IsValidDuration(duration) &&
                staffId != Guid.Empty &&
                categoryId != Guid.Empty;
        }
    }
}
