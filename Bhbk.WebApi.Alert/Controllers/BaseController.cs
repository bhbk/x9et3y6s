using Bhbk.Lib.Identity.Data.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected IUnitOfWork UoW { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); }
        protected IHostedService[] Tasks { get => (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>(); }

        public BaseController() { }

        [NonAction]
        protected Guid GetUserGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;

            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}
