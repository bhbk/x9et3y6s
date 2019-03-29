using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
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
    [Authorize]
    public class BaseController : Controller
    {
        protected IConfigurationRoot Conf { get => (IConfigurationRoot)ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfigurationRoot>(); }
        protected IIdentityContext<AppDbContext> UoW { get => (IIdentityContext<AppDbContext>)ControllerContext.HttpContext.RequestServices.GetRequiredService<IIdentityContext<AppDbContext>>(); }
        protected IHostedService[] Tasks { get => (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>(); }
        protected IJwtContext Jwt { get => (IJwtContext)ControllerContext.HttpContext.RequestServices.GetService<IJwtContext>(); }

        public BaseController() { }

        [NonAction]
        protected Guid GetUserGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;

            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        [NonAction]
        public void SetUser(Guid userID)
        {
            var user = (UoW.UserRepo.GetAsync(x => x.Id == userID).Result).SingleOrDefault();

            ControllerContext.HttpContext.User = UoW.UserRepo.GenerateAccessTokenAsync(user).Result;
        }
    }
}
