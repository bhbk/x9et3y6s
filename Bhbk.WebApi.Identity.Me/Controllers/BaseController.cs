using AutoMapper;
using Bhbk.Lib.Identity.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Authorize(Policy = "UsersPolicy")]
    public class BaseController : Controller
    {
        protected IMapper Mapper { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IMapper>(); }
        protected IUoWService UoW { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUoWService>(); }
        protected IConfiguration Conf { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>(); }
        protected IHostedService[] Tasks { get => (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>(); }

        [NonAction]
        protected Guid GetUserGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;

            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        [NonAction]
        public void SetUser(Guid issuerID, Guid userID)
        {
            var issuer = (UoW.Issuers.GetAsync(x => x.Id == issuerID).Result).SingleOrDefault();
            var user = (UoW.Users.GetAsync(x => x.Id == userID).Result).SingleOrDefault();

            ControllerContext.HttpContext.User = UoW.Users.GenerateAccessClaimsAsync(issuer, user).Result;
        }
    }
}
