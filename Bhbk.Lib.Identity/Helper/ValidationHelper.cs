using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Bhbk.Lib.Identity.Helper
{
    public class ValidationHelper : ApiController
    {
        private IUnitOfWork _uow;

        public ValidationHelper(IUnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            this._uow = uow;
        }

        public Guid GetUserGUID()
        {
            var claims = User.Identity as ClaimsIdentity;
            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        public bool IsValidUser(Guid user)
        {
            var result = _uow.UserRepository.Get(x => x.Id == user).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        public bool IsValidUser(AppUser user)
        {
            var result = _uow.UserRepository.Get(x => x.Id == user.Id || x.UserName == user.UserName).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }

        public bool IsValidUser(Guid user, out AppUser result)
        {
            result = _uow.UserRepository.Get(x => x.Id == user).SingleOrDefault();

            if (result == null)
                return false;
            else
                return true;
        }
    }
}
