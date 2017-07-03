using Bhbk.Lib.Identity.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Store
{
    public partial class CustomUserStore : IUserSecurityStampStore<AppUser, Guid>
    {
        public Task AddRefreshTokenAsync(AppUserRefreshToken token)
        {
            var refresh = _context.AppUserRefreshToken.Where(x => x.ProtectedTicket == token.ProtectedTicket).SingleOrDefault();

            if (refresh != null)
                _context.AppUserRefreshToken.Remove(refresh);

            _context.AppUserRefreshToken.Add(token);
            _context.SaveChanges();

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<AppUserRefreshToken> FindRefreshTokenAsync(string ticket)
        {
            return Task.FromResult(_context.AppUserRefreshToken.Where(x => x.ProtectedTicket == ticket).SingleOrDefault());
        }

        public Task<AppUserRefreshToken> FindRefreshTokenByIdAsync(Guid tokenId)
        {
            return Task.FromResult(_context.AppUserRefreshToken.Where(x => x.Id == tokenId).SingleOrDefault());
        }

        public Task RemoveRefreshTokenByIdAsync(Guid tokenId)
        {
            var token = _context.AppUserRefreshToken.Where(x => x.Id == tokenId).SingleOrDefault();

            if (token == null)
                throw new ArgumentNullException();

            else
            {
                _context.AppUserRefreshToken.Remove(token);
                _context.SaveChanges();
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
