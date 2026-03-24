using Bookify.Application.Interfaces;
using Bookify.Domain.Contracts;
using Bookify.Domain.Contracts.ContactInfo;
using Bookify.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Infrastructure.Data
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly IContactInfoRepository _infoRepo;
        public DatabaseSeeder(IContactInfoRepository infoRepo)
        {
            _infoRepo = infoRepo;
        }

        public async Task SeedDatabase()
        {
            await SeedContactInfo();
        }

        public async Task SeedContactInfo()
        {
            var info = new ContactInfo()
            {
                Id = Guid.NewGuid(),
                Country = "State of Palestine",
                AddressLine_1 = "Gaza",
                AddressLine_2 = string.Empty,
                PhoneNumber = "+970594737084",
                Email = "contact@bookify.com",
                CallDayFrom = DayOfWeek.Saturday,
                CallDayTo = DayOfWeek.Thursday,
                CallHourFrom = new TimeSpan(9, 0, 0),
                CallHourTo = new TimeSpan(16, 0, 0)
            };

            await _infoRepo.AddAsync(info);
            await _infoRepo.SaveChangesAsync();
        }
    }
}
