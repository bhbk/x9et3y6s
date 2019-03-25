using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected IConfigurationRoot Conf { get => (IConfigurationRoot)ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfigurationRoot>(); }
        protected IIdentityContext<AppDbContext> UoW { get => (IIdentityContext<AppDbContext>)ControllerContext.HttpContext.RequestServices.GetRequiredService<IIdentityContext<AppDbContext>>(); }
        protected IHostedService[] Tasks { get => (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>(); }
        protected IJwtContext Jwt { get => (IJwtContext)ControllerContext.HttpContext.RequestServices.GetService<IJwtContext>(); }

        public BaseController() { }
    }
}
