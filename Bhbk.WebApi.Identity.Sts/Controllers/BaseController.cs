﻿using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BaseController : Controller
    {
        private readonly IIdentityContext _ioc;
        private readonly IHostedService[] _tasks;

        protected IIdentityContext IoC
        {
            get
            {
                return _ioc ?? (IIdentityContext)ControllerContext.HttpContext.RequestServices.GetRequiredService<IIdentityContext>();
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

        public BaseController(IIdentityContext ioc, IHostedService[] tasks)
        {
            if (ioc == null || tasks == null)
                throw new ArgumentNullException();

            _ioc = ioc;
            _tasks = tasks;
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
