using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
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
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("audience")]
    public class AudienceController : BaseController
    {
        public AudienceController() { }

        public AudienceController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1"), HttpPost] 
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateAudienceV1([FromBody] AudienceCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var check = await IoC.AudienceMgmt.FindByNameAsync(model.Name);

            if (check != null)
                return BadRequest(BaseLib.Statics.MsgAudienceAlreadyExists);

            BaseLib.AudienceType audienceType;

            if (!Enum.TryParse<BaseLib.AudienceType>(model.AudienceType, out audienceType))
                return BadRequest(BaseLib.Statics.MsgAudienceInvalid);

            var audience = new AudienceFactory<AudienceCreate>(model);
            var result = await IoC.AudienceMgmt.CreateAsync(audience.Devolve());

            return Ok(audience.Evolve());
        }

        [Route("v1/{audienceID:guid}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteAudienceV1([FromRoute] Guid audienceID)
        {
            var audience = await IoC.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return NotFound(BaseLib.Statics.MsgAudienceNotExist);

            else if (audience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            audience.ActorId = GetUserGUID();

            if (!await IoC.AudienceMgmt.DeleteAsync(audience))
                return StatusCode(StatusCodes.Status500InternalServerError);

            return NoContent();
        }

        [Route("v1/{audienceID:guid}"), HttpGet]
        public async Task<IActionResult> GetAudienceV1([FromRoute] Guid audienceID)
        {
            var audience = await IoC.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return NotFound(BaseLib.Statics.MsgAudienceNotExist);

            var result = new AudienceFactory<AppAudience>(audience);

            return Ok(result.Evolve());
        }

        [Route("v1/{audienceName}"), HttpGet]
        public async Task<IActionResult> GetAudienceV1([FromRoute] string audienceName)
        {
            var audience = await IoC.AudienceMgmt.FindByNameAsync(audienceName);

            if (audience == null)
                return NotFound(BaseLib.Statics.MsgAudienceNotExist);

            var result = new AudienceFactory<AppAudience>(audience);

            return Ok(result.Evolve());
        }

        [Route("v1"), HttpGet]
        public IActionResult GetAudiencesV1([FromQuery] PagingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audiences = IoC.AudienceMgmt.Store.Get()
                .OrderBy(model.OrderBy)
                .Skip(Convert.ToInt32((model.PageNumber - 1) * model.PageSize))
                .Take(Convert.ToInt32(model.PageSize));

            var result = audiences.Select(x => new AudienceFactory<AppAudience>(x).Evolve());

            return Ok(result);
        }

        [Route("v1/{audienceID:guid}/roles"), HttpGet]
        public async Task<IActionResult> GetAudienceRolesV1([FromRoute] Guid audienceID)
        {
            var audience = await IoC.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return NotFound(BaseLib.Statics.MsgAudienceNotExist);

            var roles = await IoC.AudienceMgmt.GetRoleListAsync(audienceID);

            var result = roles.Select(x => new RoleFactory<AppRole>(x).Evolve()).ToList();

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateAudienceV1([FromBody] AudienceUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ActorId = GetUserGUID();

            var audience = await IoC.AudienceMgmt.FindByIdAsync(model.Id);

            if (audience == null)
                return NotFound(BaseLib.Statics.MsgAudienceNotExist);

            else if (audience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            var update = new AudienceFactory<AppAudience>(audience);
            update.Update(model);

            var result = await IoC.AudienceMgmt.UpdateAsync(update.Devolve());

            return Ok(update.Evolve());
        }
    }
}