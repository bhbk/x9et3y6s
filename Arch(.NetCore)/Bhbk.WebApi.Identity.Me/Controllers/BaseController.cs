using AutoMapper;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure;
using Bhbk.Lib.Identity.Factories;
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
        protected IOAuth2JwtFactory Auth { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IOAuth2JwtFactory>(); }
        protected IMapper Mapper { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IMapper>(); }
        protected IUnitOfWork UoW { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); }
        protected IConfiguration Conf { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>(); }
        protected IHostedService[] Tasks { get => (IHostedService[])ControllerContext.HttpContext.RequestServices.GetServices<IHostedService>(); }

        [NonAction]
        protected Guid GetIdentityGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;

            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        [NonAction]
        public void SetIdentity(Guid issuerID, Guid audienceID)
        {
            var issuer = UoW.Issuers.Get(x => x.Id == issuerID).Single();
            var audience = UoW.Audiences.Get(x => x.Id == audienceID).Single();
            var claims = UoW.Audiences.GenerateAccessClaims(issuer, audience);

            ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        [NonAction]
        public void SetIdentity(Guid issuerID, Guid audienceID, Guid userID)
        {
            var issuer = UoW.Issuers.Get(x => x.Id == issuerID).Single();
            var user = UoW.Users.Get(x => x.Id == userID).Single();
            var claims = UoW.Users.GenerateAccessClaims(issuer, user);

            ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        }
    }
}
