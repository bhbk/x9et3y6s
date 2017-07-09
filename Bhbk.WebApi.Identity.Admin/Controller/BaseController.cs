using Bhbk.Lib.Identity.Interface;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

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
            var user = UoW.UserMgmt.Store.FindById(guid);
            var claims = UoW.UserMgmt.CreateIdentityAsync(user, "JWT").Result;

            User = new ClaimsPrincipal(claims);
        }
    }
}
