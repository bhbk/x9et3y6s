using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BaseController : Controller
    {
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;
        private readonly IHostedService[] _tasks;
        private readonly IJwtContext _jwt;

        protected IConfigurationRoot Conf
        {
            get
            {
                return _conf ?? (IConfigurationRoot)ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfigurationRoot>();
            }
        }

        protected IIdentityContext<AppDbContext> UoW
        {
            get
            {
                return _uow ?? (IIdentityContext<AppDbContext>)ControllerContext.HttpContext.RequestServices.GetRequiredService<IIdentityContext<AppDbContext>>();
            }
        }

        protected IHostedService[] Tasks
        {
            get
            {
                return _tasks ?? (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>();
            }
        }

        protected IJwtContext Jwt
        {
            get
            {
                return _jwt ?? (IJwtContext)ControllerContext.HttpContext.RequestServices.GetService<IJwtContext>();
            }
        }

        public BaseController() { }

        public BaseController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
        {
            if (conf == null || uow == null || tasks == null)
                throw new ArgumentNullException();

            _conf = conf;
            _uow = uow;
            _tasks = tasks;
            _jwt = new JwtContext(_conf, _uow.Situation);
        }

        [NonAction]
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

        [NonAction]
        protected Guid GetUserGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;
            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        [NonAction]
        public void SetUser(Guid userID)
        {
            var user = UoW.CustomUserMgr.Store.FindByIdAsync(userID.ToString()).Result;
            var identity = UoW.CustomUserMgr.ClaimProvider.CreateAsync(user).Result;

            ControllerContext.HttpContext = new DefaultHttpContext();
            ControllerContext.HttpContext.User = identity;
        }
    }
}
