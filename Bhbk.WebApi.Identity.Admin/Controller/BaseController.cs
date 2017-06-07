using Bhbk.Lib.Identity.Infrastructure;
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
        private CustomModelFactory _mf = null;
        private IUnitOfWork _uow = null;
        protected IUnitOfWork UoW
        {
            get
            {
                return _uow ?? Request.GetOwinContext().GetUserManager<IUnitOfWork>();
            }
        }
        protected CustomModelFactory ModelFactory
        {
            get
            {
                return _mf ?? new CustomModelFactory(Request.GetOwinContext().GetUserManager<IUnitOfWork>());
            }
        }

        public BaseController() { }

        public BaseController(IUnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            this._uow = uow;
            this._mf = new CustomModelFactory(this._uow);
        }

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
            var user = UoW.CustomUserManager.FindById(guid);
            var roles = UoW.CustomUserManager.GetRolesAsync(user.Id);
            var id = new GenericIdentity(BaseLib.Statics.ApiUnitTestsUserDisplayName + BaseLib.Helper.EntrophyHelper.GenerateRandomBase64(4));

            id.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            id.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            id.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

            User = new ClaimsPrincipal(new GenericPrincipal(id, roles.Result.ToArray()));
        }
    }
}
