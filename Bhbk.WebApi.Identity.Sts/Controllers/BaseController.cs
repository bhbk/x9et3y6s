using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private IIdentityContext _conext;
        protected IIdentityContext Context
        {
            get
            {
                return _conext ?? (IIdentityContext)HttpContext.RequestServices.GetService(typeof(IIdentityContext));
            }
        }

        public BaseController() { }

        public BaseController(IIdentityContext context)
        {
            if (context == null)
                throw new ArgumentNullException();

            _conext = context;
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
