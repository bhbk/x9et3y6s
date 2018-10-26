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

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BaseController : Controller
    {
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;
        private readonly IJwtContext _jwt;
        private readonly IHostedService[] _tasks;

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

        protected IJwtContext Jwt
        {
            get
            {
                return _jwt ?? (IJwtContext)ControllerContext.HttpContext.RequestServices.GetRequiredService<IJwtContext>();
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
    }
}
