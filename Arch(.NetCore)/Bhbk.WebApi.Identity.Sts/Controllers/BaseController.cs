using AutoMapper;
using Bhbk.Lib.Identity.Data.Services;
using Bhbk.Lib.Identity.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Authorize(Policy = "UsersPolicy")]
    public class BaseController : Controller
    {
        protected IJsonWebTokenFactory Factory { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IJsonWebTokenFactory>(); }
        protected IMapper Mapper { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IMapper>(); }
        protected IUoWService UoW { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUoWService>(); }
        protected IConfiguration Conf { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>(); }
        protected IHostedService[] Tasks { get => (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>(); }

        public BaseController() { }

        [NonAction]
        protected Guid GetUserGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;

            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        [NonAction]
        public void SetUser(Guid issuerID, Guid userID)
        {
            var issuer = UoW.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            var user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();
            var claims = UoW.Users.GenerateAccessClaims(issuer, user);

            var identity = new ClaimsIdentity(claims, "Mock");

            ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
        }
    }
}
