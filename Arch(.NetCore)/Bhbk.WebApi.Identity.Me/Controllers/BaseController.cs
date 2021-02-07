using AutoMapper;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected IOAuth2JwtFactory auth { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IOAuth2JwtFactory>(); }
        protected IMapper map { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IMapper>(); }
        protected IUnitOfWork uow { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); }
        protected IConfiguration conf { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>(); }

        [NonAction]
        protected Guid GetIdentityGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;

            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }

        [NonAction]
        public void SetIdentity(Guid issuerID, Guid audienceID)
        {
            var issuer = uow.Issuers.Get(x => x.Id == issuerID).Single();
            var audience = uow.Audiences.Get(x => x.Id == audienceID).Single();
            var claims = uow.Audiences.GenerateAccessClaims(issuer, audience);

            ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        [NonAction]
        public void SetIdentity(Guid issuerID, Guid audienceID, Guid userID)
        {
            var issuer = uow.Issuers.Get(x => x.Id == issuerID).Single();
            var user = uow.Users.Get(x => x.Id == userID).Single();
            var claims = uow.Users.GenerateAccessClaims(issuer, user);

            ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        }
    }
}
