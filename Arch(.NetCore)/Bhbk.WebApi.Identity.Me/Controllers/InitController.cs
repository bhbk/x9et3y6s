using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Providers.Me;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Identity.Me.Controllers
{
    [Route("init")]
    public class InitController : BaseController
    {
        private InitProvider _provider;

        public InitController(IConfiguration conf, IContextService instance)
        {
            _provider = new InitProvider(conf, instance);
        }

        [Route("v1/audiences"), HttpGet]
        public IActionResult GetAudiencesV1()
        {
            var audience = UoW.Audiences.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            return Ok(Mapper.Map<AudienceModel>(audience));
        }
    }
}
