using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Interfaces
{
    public interface IDatabaseSeeder
    {
        Task SeedDatabase();
    }
}
