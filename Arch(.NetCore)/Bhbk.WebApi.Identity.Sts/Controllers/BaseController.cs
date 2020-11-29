﻿using AutoMapper;
using Bhbk.Lib.Identity.Data.Infrastructure_TBL;
using Bhbk.Lib.Identity.Factories;
using Bhbk.Lib.Identity.Primitives.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Authorize(Roles = DefaultConstants.RoleForViewers_Identity + ", "
        + DefaultConstants.RoleForUsers_Identity + ", "
        + DefaultConstants.RoleForAdmins_Identity)]
    public class BaseController : Controller
    {
        protected IOAuth2JwtFactory Auth { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IOAuth2JwtFactory>(); }
        protected IMapper Mapper { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IMapper>(); }
        protected IUnitOfWork UoW { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); }
        protected IConfiguration Conf { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>(); }

        public BaseController() { }

        [NonAction]
        protected Guid GetIdentityGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;

            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}
