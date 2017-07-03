using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [Authorize]
    public class BaseController : ApiController
    {
        private IUnitOfWork _uow = null;
        protected IUnitOfWork UoW
        {
            get
            {
                return _uow ?? Request.GetOwinContext().GetUserManager<IUnitOfWork>();
            }
        }

        public BaseController() { }

        public BaseController(IUnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            _uow = uow;
        }

        [System.Obsolete]
        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return InternalServerError();

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                        ModelState.AddModelError("", error);
                }

                if (ModelState.IsValid)
                    return BadRequest();

                return BadRequest(ModelState);
            }

            return null;
        }

        protected Guid GetUserGUID()
        {
            var claims = User.Identity as ClaimsIdentity;
            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        public void SetUser(Guid guid)
        {
            var user = UoW.UserMgmt.FindByIdAsync(guid).Result;
            var roles = UoW.UserMgmt.GetRolesAsync(user.Id).Result;
            var claims = new GenericIdentity(BaseLib.Statics.ApiUnitTestUserDisplayName + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4));

            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            claims.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            claims.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

            User = new ClaimsPrincipal(new GenericPrincipal(claims, roles.ToArray()));
        }
    }
}
