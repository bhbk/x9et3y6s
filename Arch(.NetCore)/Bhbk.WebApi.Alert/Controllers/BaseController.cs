using AutoMapper;
using Bhbk.Lib.Identity.Data.Services;
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
        protected IMapper Mapper { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IMapper>(); }
        protected IUoWService UoW { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUoWService>(); }
        protected IHostedService[] Tasks { get => (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>(); }

        [NonAction]
        protected Guid GetIdentityGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;

            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}
