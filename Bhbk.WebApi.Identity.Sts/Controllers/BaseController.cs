using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private readonly IIdentityContext _ioc;
        private readonly IHostedService[] _tasks;

        protected IIdentityContext IoC
        {
            get
            {
                return _ioc ?? (IIdentityContext)ControllerContext.HttpContext.RequestServices.GetRequiredService(typeof(IIdentityContext));
            }
        }

        protected IHostedService[] Tasks
        {
            get
            {
                return _tasks ?? (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>();
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
    }
}
