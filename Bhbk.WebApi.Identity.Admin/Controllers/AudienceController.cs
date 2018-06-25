using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("audience")]
    public class AudienceController : BaseController
    {
        public AudienceController() { }

        public AudienceController(IIdentityContext ioc, IHostedService[] tasks)
            : base(ioc, tasks) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateAudience([FromBody] AudienceCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var audience = await IoC.AudienceMgmt.FindByNameAsync(model.Name);

            if (audience != null)
                return BadRequest(BaseLib.Statics.MsgAudienceAlreadyExists);

            BaseLib.AudienceType audienceType;

            if (!Enum.TryParse<BaseLib.AudienceType>(model.AudienceType, out audienceType))
                return BadRequest(BaseLib.Statics.MsgAudienceInvalid);

            var create = new AudienceFactory<AudienceCreate>(model);
            var result = await IoC.AudienceMgmt.CreateAsync(create.Devolve());

            return Ok(create.Evolve());
        }

        [Route("v1/{audienceID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteAudience(Guid audienceID)
        {
            var audience = await IoC.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceInvalid);

            else if (audience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            audience.ActorId = GetUserGUID();

            if (!await IoC.AudienceMgmt.DeleteAsync(audience))
                return StatusCode(StatusCodes.Status500InternalServerError);

            else
                return NoContent();
        }

        [Route("v1/{audienceID}"), HttpGet]
        public async Task<IActionResult> GetAudience(Guid audienceID)
        {
            var audience = await IoC.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceInvalid);

            var result = new AudienceFactory<AppAudience>(audience);

            return Ok(result.Evolve());
        }

        [Route("v1"), HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAudiences([FromQuery] CustomPagingModel filter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audiences = IoC.AudienceMgmt.Store.Get().AsQueryable()
                .OrderBy(filter.OrderBy)
                .Skip(Convert.ToInt32((filter.PageNumber - 1) * filter.PageSize))
                .Take(Convert.ToInt32(filter.PageSize));

            var result = audiences.Select(x => new AudienceFactory<AppAudience>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1/{audienceID}/roles"), HttpGet]
        public async Task<IActionResult> GetAudienceRoles(Guid audienceID)
        {
            var audience = await IoC.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceInvalid);

            var roles = await IoC.AudienceMgmt.GetRoleListAsync(audienceID);

            var result = roles.Select(x => new RoleFactory<AppRole>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateAudience([FromBody] AudienceUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var audience = await IoC.AudienceMgmt.FindByIdAsync(model.Id);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceInvalid);

            else if (audience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            else
            {
                var update = new AudienceFactory<AppAudience>(audience);
                update.Update(model);

                var result = await IoC.AudienceMgmt.UpdateAsync(update.Devolve());

                return Ok(update.Evolve());
            }
        }
    }
}