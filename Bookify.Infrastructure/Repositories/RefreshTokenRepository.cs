using Bookify.Domain.Contracts.RefreshToken;
using Bookify.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;
        public RefreshTokenRepository(AppDbContext db)
        {
            _db = db;
        }


    }
}
