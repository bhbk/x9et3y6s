using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("audience")]
    public class AudienceController : BaseController
    {
        public AudienceController() { }

        public AudienceController(IIdentityContext context)
            : base(context) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> CreateAudience(AudienceCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var audience = await Context.AudienceMgmt.FindByNameAsync(model.Name);

            if (audience != null)
                return BadRequest(BaseLib.Statics.MsgAudienceAlreadyExists);

            var create = new AudienceFactory<AudienceCreate>(model);
            var result = await Context.AudienceMgmt.CreateAsync(create.Devolve());

            return Ok(create.Evolve());
        }

        [Route("v1/{audienceID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> DeleteAudience(Guid audienceID)
        {
            var audience = await Context.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else if (audience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            if (!await Context.AudienceMgmt.DeleteAsync(audienceID))
                return StatusCode(StatusCodes.Status500InternalServerError);

            else
                return NoContent();
        }

        [Route("v1/{audienceID}"), HttpGet]
        public async Task<IActionResult> GetAudience(Guid audienceID)
        {
            var audience = await Context.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            var result = new AudienceFactory<AppAudience>(audience);

            return Ok(result.Evolve());
        }

        [Route("v1")]
        public async Task<IActionResult> GetAudiences()
        {
            IList<AudienceResult> result = new List<AudienceResult>();
            var users = await Context.AudienceMgmt.GetListAsync();

            foreach (AppAudience entry in users)
                result.Add(new AudienceFactory<AppAudience>(entry).Evolve());

            return Ok(result);
        }

        [Route("v1/{audienceID}/roles"), HttpGet]
        public async Task<IActionResult> GetAudienceRoles(Guid audienceID)
        {
            var audience = await Context.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            IList<RoleResult> result = new List<RoleResult>();
            var roles = await Context.AudienceMgmt.GetRoleListAsync(audienceID);

            foreach (AppRole entry in roles)
                result.Add(new RoleFactory<AppRole>(entry).Evolve());

            return Ok(result);
        }

        [Route("v1"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateAudience(AudienceUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var audience = await Context.AudienceMgmt.FindByIdAsync(model.Id);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else if (audience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            else
            {
                var update = new AudienceFactory<AudienceUpdate>(model);
                var result = await Context.AudienceMgmt.UpdateAsync(update.Devolve());

                return Ok(update.Evolve());
            }
        }
    }
}