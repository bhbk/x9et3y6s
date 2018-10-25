using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Bhbk.Lib.Identity.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("audience")]
    public class AudienceController : BaseController
    {
        public AudienceController() { }

        public AudienceController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1"), HttpPost] 
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateAudienceV1([FromBody] AudienceCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await UoW.AudienceRepo.GetAsync(x => x.Name == model.Name);

            if (check.Any())
                return BadRequest(Strings.MsgAudienceAlreadyExists);

            Enums.AudienceType audienceType;

            if (!Enum.TryParse<Enums.AudienceType>(model.AudienceType, out audienceType))
                return BadRequest(Strings.MsgAudienceInvalid);

            var audience = new AudienceFactory<AudienceCreate>(model);
            var result = await UoW.AudienceRepo.CreateAsync(audience.ToStore());

            await UoW.CommitAsync();

            return Ok(audience.ToClient());
        }

        [Route("v1/{audienceID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteAudienceV1([FromRoute] Guid audienceID)
        {
            var audience = await UoW.AudienceRepo.GetAsync(audienceID);

            if (audience == null)
                return NotFound(Strings.MsgAudienceNotExist);

            else if (audience.Immutable)
                return BadRequest(Strings.MsgAudienceImmutable);

            audience.ActorId = GetUserGUID();

            if (!await UoW.AudienceRepo.DeleteAsync(audience))
                return StatusCode(StatusCodes.Status500InternalServerError);

            await UoW.CommitAsync();

            return NoContent();
        }

        [Route("v1/{audienceID:guid}"), HttpGet]
        public async Task<IActionResult> GetAudienceV1([FromRoute] Guid audienceID)
        {
            var audience = await UoW.AudienceRepo.GetAsync(audienceID);

            if (audience == null)
                return NotFound(Strings.MsgAudienceNotExist);

            var result = new AudienceFactory<AppAudience>(audience);

            return Ok(result.ToClient());
        }

        [Route("v1/{audienceName}"), HttpGet]
        public async Task<IActionResult> GetAudienceV1([FromRoute] string audienceName)
        {
            var audience = (await UoW.AudienceRepo.GetAsync(x => x.Name == audienceName)).SingleOrDefault();

            if (audience == null)
                return NotFound(Strings.MsgAudienceNotExist);

            var result = new AudienceFactory<AppAudience>(audience);

            return Ok(result.ToClient());
        }

        [Route("v1"), HttpGet]
        public async Task<IActionResult> GetAudiencesV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audiences = await UoW.AudienceRepo.GetAsync(x => true,
                x => x.OrderBy(model.OrderBy).Skip(model.Skip).Take(model.Take));

            var result = audiences.Select(x => new AudienceFactory<AppAudience>(x).ToClient());

            return Ok(result);
        }

        [Route("v1/{audienceID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetAudienceRolesV1([FromRoute] Guid audienceID)
        {
            var audience = await UoW.AudienceRepo.GetAsync(audienceID);

            if (audience == null)
                return NotFound(Strings.MsgAudienceNotExist);

            var roles = await UoW.AudienceRepo.GetRoleListAsync(audienceID);

            var result = roles.Select(x => new RoleFactory<AppRole>(x).ToClient()).ToList();

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateAudienceV1([FromBody] AudienceUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var audience = await UoW.AudienceRepo.GetAsync(model.Id);

            if (audience == null)
                return NotFound(Strings.MsgAudienceNotExist);

            else if (audience.Immutable)
                return BadRequest(Strings.MsgAudienceImmutable);

            var update = new AudienceFactory<AudienceUpdate>(model);
            var result = await UoW.AudienceRepo.UpdateAsync(update.ToStore());

            await UoW.CommitAsync();

            return Ok(update.ToClient());
        }
    }
}