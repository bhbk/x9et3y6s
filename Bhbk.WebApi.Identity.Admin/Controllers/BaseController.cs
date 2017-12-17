using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private IIdentityContext _ioc;
        
        protected IIdentityContext IoC
        {
            get
            {
                return _ioc ?? (IIdentityContext)HttpContext.RequestServices.GetRequiredService(typeof(IIdentityContext));
            }
        }

        public BaseController() { }

        public BaseController(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _ioc = ioc;
        }

        protected IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError(error.Code, error.Description);
                }

                if (ModelState.IsValid)
                    return BadRequest();

                return BadRequest(ModelState);
            }

            return null;
        }

        protected Guid GetUserGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;
            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        public void SetUser(Guid userID)
        {
            var user = IoC.UserMgmt.Store.FindByIdAsync(userID.ToString()).Result;
            var claims = IoC.UserMgmt.CreateIdentityAsync(user, "JWT").Result;

            ControllerContext.HttpContext = new DefaultHttpContext();
            ControllerContext.HttpContext.User = new ClaimsPrincipal(claims);
        }
    }
}
