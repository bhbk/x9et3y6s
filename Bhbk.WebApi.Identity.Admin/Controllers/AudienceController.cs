using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
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

            if (audience == null)
            {
                var create = Context.AudienceMgmt.Store.Mf.Create.DoIt(model);
                var result = await Context.AudienceMgmt.CreateAsync(create);

                return Ok(result);
            }
            else
                return BadRequest(BaseLib.Statics.MsgAudienceAlreadyExists);
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
                return Ok();
        }

        [Route("v1")]
        public async Task<IActionResult> GetAudiences()
        {
            return Ok(await Context.AudienceMgmt.GetListAsync());
        }

        [Route("v1/{audienceID}"), HttpGet]
        public async Task<IActionResult> GetAudience(Guid audienceID)
        {
            var audience = await Context.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else
                return Ok(audience);
        }

        [Route("v1/{audienceID}/roles"), HttpGet]
        public async Task<IActionResult> GetAudienceRoles(Guid audienceID)
        {
            var audience = await Context.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else
                return Ok(await Context.AudienceMgmt.GetRoleListAsync(audienceID));
        }

        [Route("v1/{audienceID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> UpdateAudience(Guid audienceID, AudienceUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (audienceID != model.Id)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            var audience = await Context.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else if (audience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            else
            {
                var update = Context.AudienceMgmt.Store.Mf.Update.DoIt(model);
                var result = await Context.AudienceMgmt.UpdateAsync(update);

                return Ok(result);
            }
        }
    }
}