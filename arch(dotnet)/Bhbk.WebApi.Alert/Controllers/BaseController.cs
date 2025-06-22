using AutoMapper;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Primitives.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Authorize(Policy = PolicyConstants.AlertViewerPolicy)]
    public class BaseController : Controller
    {
        protected IMapper map { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IMapper>(); }
        protected IUnitOfWork uow { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); }

        [NonAction]
        protected Guid GetIdentityGUID()
        {
            var claims = ControllerContext.HttpContext.User.Identity as ClaimsIdentity;

            return Guid.Parse(claims.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}
