using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interface;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("audience")]
    public class AudienceController : BaseController
    {
        public AudienceController() { }

        public AudienceController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1"), HttpPost]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> CreateAudience(AudienceCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var audience = await UoW.AudienceMgmt.FindByNameAsync(model.Name);

            if (audience == null)
            {
                var create = UoW.AudienceMgmt.Store.Mf.Create.DoIt(model);
                var result = await UoW.AudienceMgmt.CreateAsync(create);

                return Ok(result);
            }
            else
                return BadRequest(BaseLib.Statics.MsgAudienceAlreadyExists);
        }

        [Route("v1/{audienceID}"), HttpDelete]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> DeleteAudience(Guid audienceID)
        {
            var audience = await UoW.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else if (audience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            if (!await UoW.AudienceMgmt.DeleteAsync(audienceID))
                return InternalServerError();

            else
                return Ok();
        }

        [Route("v1")]
        public async Task<IHttpActionResult> GetAudiences()
        {
            return Ok(await UoW.AudienceMgmt.GetListAsync());
        }

        [Route("v1/{audienceID}"), HttpGet]
        public async Task<IHttpActionResult> GetAudience(Guid audienceID)
        {
            var audience = await UoW.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else
                return Ok(audience);
        }

        [Route("v1/{audienceID}/roles"), HttpGet]
        public async Task<IHttpActionResult> GetAudienceRoles(Guid audienceID)
        {
            var audience = await UoW.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else
                return Ok(await UoW.AudienceMgmt.GetRoleListAsync(audienceID));
        }

        [Route("v1/{audienceID}"), HttpPut]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IHttpActionResult> UpdateAudience(Guid audienceID, AudienceUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            else if (audienceID != model.Id)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            var audience = await UoW.AudienceMgmt.FindByIdAsync(audienceID);

            if (audience == null)
                return BadRequest(BaseLib.Statics.MsgAudienceNotExist);

            else if (audience.Immutable)
                return BadRequest(BaseLib.Statics.MsgAudienceImmutable);

            else
            {
                var update = UoW.AudienceMgmt.Store.Mf.Update.DoIt(model);
                var result = await UoW.AudienceMgmt.UpdateAsync(update);

                return Ok(result);
            }
        }
    }
}